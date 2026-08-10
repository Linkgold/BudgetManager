using Contracts.Enums;
using Microsoft.AspNetCore.Components;
using Shared.DTOs.Request;
using UI.Extensions;
using UI.Helpers;
using UI.Models;
using UI.Models.Cache;
using UI.Models.Forms;
using UI.Services;
using UI.Services.API;
using UI.Shared;

namespace UI.Pages
{
    public partial class Transactions : BasePage
    {
        // ================================================================
        // 1. INYECCIONES
        // ================================================================

        [Inject]
        private HasDataService HasDataService { get; set; } = default!;

        // ================================================================
        // 2. MODELOS Y ESTADO
        // ================================================================

        private readonly CacheDictionary<int, TransactionModel> _transactionsCache = new();
        private List<TransactionModel> _transactions = new();
        private List<TransactionModel> _filteredTransactions = new();
        private List<CategoryModel> _categories = new();
        private TransactionFormModel _transactionForm = new();
        private List<int> _years = new();
        private HasDataModel? _hasData;

        private decimal _totalAmount = 0m;

        // ================================================================
        // 3. FILTROS Y PROPIEDADES CON SETTER
        // ================================================================

        private string _searchTerm = string.Empty;
        private int _selectedCategoryId = 0;
        private string _selectedType = string.Empty;
        private int _selectedMonth = DateTime.Now.Month;
        private int _selectedYear = DateTime.Now.Year;
        private int _categoryId = 0;

        private string searchTerm
        {
            get => _searchTerm;
            set
            {
                if (_searchTerm != value)
                {
                    _searchTerm = value;
                    ApplyFilters();
                }
            }
        }

        private int selectedCategoryId
        {
            get => _selectedCategoryId;
            set
            {
                if (_selectedCategoryId != value)
                {
                    _selectedCategoryId = value;
                    ApplyFilters();
                    UpdateTypeSelector();
                }
            }
        }

        private string selectedType
        {
            get => _selectedType;
            set
            {
                if (_selectedType != value)
                {
                    _selectedType = value;
                    ApplyFilters();
                }
            }
        }

        private int selectedMonth
        {
            get => _selectedMonth;
            set
            {
                if (_selectedMonth != value)
                {
                    _selectedMonth = value;
                    ApplyFilters();
                }
            }
        }

        private int selectedYear
        {
            get => _selectedYear;
            set
            {
                if (_selectedYear != value)
                {
                    _selectedYear = value;
                }
            }
        }

        private int CategoryId
        {
            get => _categoryId;
            set
            {
                if (_categoryId != value)
                {
                    _categoryId = value;
                    _transactionForm.CategoryId = value;
                    OnCategoryChanged();
                }
            }
        }

        private bool ShowTypeSelector
        {
            get
            {
                if (_transactionForm.IsDeleting) return false;
                if (_transactionForm.CategoryId <= 0) return false;

                return _transactionForm.CategoryNature == CategoryNatureEnum.Mixed;
            }
        }

        // ================================================================
        // 4. CICLO DE VIDA
        // ================================================================

        protected override async Task OnInitializedAsync()
        {
            await LoadData();
            _hasData = await HasDataService.GetDataAsync();
        }

        // ================================================================
        // 5. CARGA DE DATOS
        // ================================================================
        private async Task LoadData()
        {
            try
            {
                _categories = await APIService.GetCategoriesAsync() ?? new List<CategoryModel>();

                await LoadTransactions(_selectedYear);

                _years = new List<int>();
                for (int year = 2020; year <= 2050; year++)
                {
                    _years.Add(year);
                }
            }
            catch (Exception ex)
            {
                await LogService.LogErrorAsync($"Error al cargar datos de transacciones", ex);
                ToastService.ShowError("Error al cargar los datos.");

                _transactions = new List<TransactionModel>();
                _categories = new List<CategoryModel>();
            }
            finally
            {
                ApplyFilters();
            }
        }

        private async Task LoadTransactions(int year)
        {
            if (_transactionsCache.TryGetValue(year, out List<TransactionModel>? cached))
            {
                _transactions = cached!;
                return;
            }

            List<TransactionModel> transactions = await APIService.GetTransactionsAsync(year) ?? new List<TransactionModel>();

            _transactionsCache.Add(year, transactions);

            _transactions = transactions;
        }

        // ================================================================
        // 6. FILTRADO
        // ================================================================

        private void ApplyFilters()
        {
            IEnumerable<TransactionModel> query = _transactions.AsEnumerable();

            // Filtro por categoría
            if (selectedCategoryId > 0)
            {
                query = query.Where(t => t.CategoryId == selectedCategoryId);
            }

            // Filtro por tipo
            query = selectedType switch
            {
                "Income" => query.Where(t => t.GetDisplayAmount() > 0),
                "Expense" => query.Where(t => t.GetDisplayAmount() < 0),
                _ => query
            };

            // Filtro por mes y año
            query = query.Where(t => t.Date.Month == selectedMonth && t.Date.Year == selectedYear);

            // Búsqueda por concepto o descripción
            if (!string.IsNullOrEmpty(searchTerm))
            {
                query = query.Where
                (
                    t =>
                    t.Name.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ||
                    (t.Description?.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ?? false)
                );
            }

            _filteredTransactions = query.OrderByDescending(t => t.Date).ToList();

            _totalAmount = _filteredTransactions.GetTotalDisplayAmount();
        }

        private void ClearSearch()
        {
            searchTerm = string.Empty;

            ApplyFilters();
        }

        private void UpdateTypeSelector() => StateHasChanged();

        // ================================================================
        // 7. OPERACIONES CRUD (SAVE)
        // ================================================================

        private async Task SaveTransaction()
        {
            try
            {
                bool success = false;

                if (_transactionForm.IsDeleting)
                {
                    success = await DeleteTransactionAsync();
                }
                else if (_transactionForm.IsEditing)
                {
                    success = await UpdateTransactionAsync();
                }
                else
                {
                    success = await CreateTransactionAsync();
                }

                if (success)
                {
                    _transactionsCache.Remove(_transactionForm.Date.Year);

                    if (_transactionForm.IsEditing && _transactionForm.OriginalYear != _transactionForm.Date.Year)
                    {
                        _transactionsCache.Remove(_transactionForm.OriginalYear);
                    }

                    await HasDataService.RefreshAsync();

                    _transactionForm.IsModalOpen = false;
                    _transactionForm.IsEditing = false;
                    _transactionForm.IsDeleting = false;

                    await LoadTransactions(_selectedYear);

                    ApplyFilters();

                    await InvokeAsync(StateHasChanged);
                }
            }
            catch (Exception ex)
            {
                await LogService.LogErrorAsync($"Error en SaveTransaction", ex);
                ToastService.ShowError("Ocurrió un error inesperado.");
            }
        }

        private async Task<bool> CreateTransactionAsync()
        {
            decimal finalAmount = _transactionForm.Amount;

            CreateTransactionRequestDTO request = new()
            {
                CategoryId = _transactionForm.CategoryId,
                Name = _transactionForm.Name,
                Description = _transactionForm.Description,
                Amount = finalAmount,
                TransactionType = _transactionForm.TransactionType,
                Date = _transactionForm.Date
            };

            TransactionModel? result = await APIService.CreateTransactionAsync(request);
            
            if (result != null)
            {
                ToastService.ShowSuccess($"Transacción [{_transactionForm.Name}] creada correctamente.");
                
                return true;
            }

            return false;
        }

        private async Task<bool> UpdateTransactionAsync()
        {
            decimal finalAmount = _transactionForm.Amount;

            UpdateTransactionRequestDTO request = new()
            {
                CategoryId = _transactionForm.CategoryId,
                Name = _transactionForm.Name,
                Description = _transactionForm.Description,
                Amount = finalAmount,
                TransactionType = _transactionForm.TransactionType,
                Date = _transactionForm.Date
            };

            TransactionModel? result = await APIService.UpdateTransactionAsync(_transactionForm.Id, request);
            
            if (result != null)
            {
                ToastService.ShowSuccess($"Transacción [{_transactionForm.Name}] actualizada correctamente.");
                
                return true;
            }

            return false;
        }

        private async Task<bool> DeleteTransactionAsync()
        {
            bool success = await APIService.DeleteTransactionAsync(_transactionForm.Id, _transactionForm.Name);
            
            if (success)
            {
                ToastService.ShowSuccess($"Transacción [{_transactionForm.Name}] eliminada correctamente.");

                return true;
            }

            return false;
        }

        // ================================================================
        // 8. APERTURA DE MODALES
        // ================================================================

        private void OpenCreateModal()
        {
            CategoryModel? defaultCategory = _categories.OrderBy(c => c.Name).FirstOrDefault();

            // ✅ Determinar la fecha por defecto según los filtros
            DateTime defaultDate = MonthHelper.GetDefaultDate(_selectedMonth, _selectedYear);

            FillFormFromModel
            (
                new TransactionModel
                {
                    CategoryId = defaultCategory?.Id ?? 0,
                    Date = defaultDate,
                    Amount = 0m
                },
                FormMode.Create
            );

            InvokeAsync(StateHasChanged);
        }

        private void OpenEditModal(TransactionModel transaction)
        {
            FillFormFromModel(transaction, FormMode.Edit);
            InvokeAsync(StateHasChanged);
        }

        private void OpenDeleteModal(TransactionModel transaction)
        {
            FillFormFromModel(transaction, FormMode.Delete);
            InvokeAsync(StateHasChanged);
        }

        private void FillFormFromModel(TransactionModel model, FormMode mode)
        {
            CategoryModel? category = _categories.FirstOrDefault(c => c.Id == model.CategoryId);

            TransactionTypeEnum defaultType = TransactionTypeEnum.Expense;

            if (category != null)
            {
                defaultType = category.Nature.GetDefaultTransactionTypeForCategory();
            }

            _transactionForm = new TransactionFormModel
            {
                Id = model.Id,
                CategoryId = model.CategoryId,
                CategoryName = model.CategoryName,
                CategoryNature = category?.Nature ?? CategoryNatureEnum.Expense,
                Name = model.Name ?? string.Empty,
                Description = model.Description ?? string.Empty,
                Amount = Math.Abs(model.Amount),
                OriginalYear = model.Date.Year,
                Date = model.Date == DateTime.MinValue ? DateTime.Now : model.Date,
                TransactionType = defaultType,
                IsModalOpen = true,
                IsEditing = mode == FormMode.Edit,
                IsDeleting = mode == FormMode.Delete
            };

            _categoryId = model.CategoryId;
        }

        private void CloseModal()
        {
            _transactionForm.IsModalOpen = false;
            _transactionForm.IsEditing = false;
            _transactionForm.IsDeleting = false;
            _categoryId = 0;

            InvokeAsync(StateHasChanged);
        }

        // ================================================================
        // 9. MÉTODOS AUXILIARES
        // ================================================================
        private void OnCategoryChanged()
        {
            CategoryModel? category = _categories.FirstOrDefault(c => c.Id == _transactionForm.CategoryId);

            if (category != null)
            {
                _transactionForm.CategoryNature = category.Nature;
                _transactionForm.TransactionType = category.Nature.GetDefaultTransactionTypeForCategory();
            }

            StateHasChanged();
        }

        private async Task OnYearChanged()
        {
            try
            {
                await LoadTransactions(_selectedYear);

                // Comprobar si el mes seleccionado existe en el nuevo año
                bool currentMonthHasData = _transactions.HasMonthData(_selectedYear, _selectedMonth);

                if (!currentMonthHasData)
                {
                    int firstMonthWithData = _transactions.GetFirstMonthWithData(_selectedYear);

                    if (firstMonthWithData > 0)
                    {
                        _selectedMonth = firstMonthWithData;
                    }
                }

                ApplyFilters();
            }
            catch (Exception ex)
            {
                await LogService.LogErrorAsync(
                    "Error al cargar las transacciones del año seleccionado",
                    ex
                );

                ToastService.ShowError("Error al cargar las transacciones.");

                _transactions = new List<TransactionModel>();
                ApplyFilters();
            }

            await InvokeAsync(StateHasChanged);
        }
    }
}