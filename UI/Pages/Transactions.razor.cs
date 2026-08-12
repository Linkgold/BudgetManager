using Contracts.Enums;
using Microsoft.AspNetCore.Components;
using MudBlazor;
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
        private List<int> _years = new();
        private HasDataModel? _hasData;
        private bool _isLoading = true;

        private decimal _totalAmount = 0m;

        // Modal
        private bool _isTransactionModalOpen = false;
        private FormModeEnum _modalMode;
        private TransactionModel? _transactionToModal = null;

        // ================================================================
        // 3. FILTROS Y PROPIEDADES CON SETTER
        // ================================================================

        private string _searchTerm = string.Empty;
        private int _selectedCategoryId = 0;
        private string _selectedType = string.Empty;
        private int _selectedMonth = DateTime.Now.Month;
        private int _selectedYear = DateTime.Now.Year;

        private bool HasCategories => _categories.Count != 0;

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

        // ================================================================
        // 4. CICLO DE VIDA
        // ================================================================

        protected override async Task OnInitializedAsync()
        {
            await LoadData();
            _hasData = await HasDataService.GetDataAsync();
            _isLoading = false;
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

        private async Task LoadTransactions(int year, bool forceRefresh = false)
        {
            if (!forceRefresh && _transactionsCache.TryGetValue(year, out List<TransactionModel>? cached))
            {
                _transactions = cached!;
                return;
            }

            _transactionsCache.Remove(year);

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

        // ================================================================
        // 7. APERTURA DE MODALES
        // ================================================================

        private void OpenCreateModal()
        {
            CategoryModel? defaultCategory = _categories.OrderBy(c => c.Name).FirstOrDefault();

            // ✅ Determinar la fecha por defecto según los filtros
            DateTime defaultDate = MonthHelper.GetDefaultDate(_selectedMonth, _selectedYear);

            _transactionToModal = new TransactionModel
            {
                CategoryId = defaultCategory?.Id ?? 0,
                Date = defaultDate,
                Amount = 0m
            };

            _modalMode = FormModeEnum.Create;

            _isTransactionModalOpen = true;

            InvokeAsync(StateHasChanged);
        }

        private void OpenEditModal(TransactionModel transaction)
        {
            _transactionToModal = transaction;
            _modalMode = FormModeEnum.Edit;
            _isTransactionModalOpen = true;
            InvokeAsync(StateHasChanged);
        }

        private void OpenDeleteModal(TransactionModel transaction)
        {
            _transactionToModal = transaction;
            _modalMode = FormModeEnum.Delete;
            _isTransactionModalOpen = true;
            InvokeAsync(StateHasChanged);
        }

        private async Task OnTransactionModalSaved()
        {
            _isTransactionModalOpen = false;
            _transactionToModal = null;

            // Refrescar caché de HasData
            await HasDataService.RefreshAsync();
            _hasData = await HasDataService.GetDataAsync();

            // Recargar datos
            await LoadTransactions(_selectedYear, true);

            ApplyFilters();
            StateHasChanged();
        }

        private async Task OnTransactionModalCancelled()
        {
            _isTransactionModalOpen = false;
            _transactionToModal = null;

            StateHasChanged();
        }

        // ================================================================
        // 8. MÉTODOS AUXILIARES
        // ================================================================

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
                await LogService.LogErrorAsync("Error al cargar las transacciones del año seleccionado", ex);

                ToastService.ShowError("Error al cargar las transacciones.");

                _transactions = new List<TransactionModel>();
                ApplyFilters();
            }

            await InvokeAsync(StateHasChanged);
        }
    }
}