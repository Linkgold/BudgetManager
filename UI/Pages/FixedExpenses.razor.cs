using Microsoft.AspNetCore.Components;
using Shared.DTOs.Request;
using UI.Extensions;
using UI.Extensions.Mappings;
using UI.Models;
using UI.Models.Cache;
using UI.Models.Forms;
using UI.Services;
using UI.Services.API;
using UI.Shared;

namespace UI.Pages
{
    public partial class FixedExpenses : BasePage
    {
        // ================================================================
        // 1. INYECCIONES
        // ================================================================

        [Inject]
        private HasDataService HasDataService { get; set; } = default!;

        // ================================================================
        // 2. MODELOS Y ESTADO
        // ================================================================
        
        private readonly CacheDictionary<int, FixedExpenseModel> _fixedExpensesCache = new();
        private List<FixedExpenseModel> _fixedExpenses = new();
        private List<FixedExpenseModel> _filteredFixedExpenses = new();
        private List<CategoryModel> _categories = new();
        private FixedExpenseFormModel _fixedExpenseForm = new();
        private List<int> _years = new();
        private HasDataModel? _hasData;
        private bool _isLoading = true;
        private string _suggestedName = "Ej. IBI";

        // ================================================================
        // 3. FILTROS Y PROPIEDADES CON SETTER
        // ================================================================

        private string _searchTerm = string.Empty;
        private int _selectedCategoryId = 0;
        private int _selectedYear = DateTime.Now.Year;

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

        private bool HasExpenseOrMixedCategories => _categories.Count != 0;

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
                // Cargar categorías
                List<CategoryModel>? categoriesResult = await APIService.GetCategoriesAsync();
                _categories = categoriesResult?.ToExpenseMixedCategoryModelList() ?? new List<CategoryModel>();


                // Cargar gastos fijos
                await LoadFixedExpenses(_selectedYear);

                // Inicializar años (2020-2050)
                _years = new List<int>();
                for (int year = 2020; year <= 2050; year++)
                {
                    _years.Add(year);
                }
            }
            catch (Exception ex)
            {
                await LogService.LogErrorAsync($"Error al cargar datos de gastos fijos", ex);

                ToastService.ShowError("Error al cargar los datos.");
                _fixedExpenses = new();
                _categories = new();
            }
            finally
            {
                ApplyFilters();
            }
        }

        private async Task LoadFixedExpenses(int year)
        {
            if (_fixedExpensesCache.TryGetValue(year, out List<FixedExpenseModel>? cached))
            {
                _fixedExpenses = cached!;
                return;
            }

            List<FixedExpenseModel> fixedExpenses = await APIService.GetFixedExpensesAsync(year) ?? new List<FixedExpenseModel>();

            _fixedExpensesCache.Add(year, fixedExpenses);

            _fixedExpenses = fixedExpenses;
        }

        // ================================================================
        // 6. FILTRADO
        // ================================================================

        private void ApplyFilters()
        {
            _filteredFixedExpenses = _fixedExpenses
                .Where(f => (selectedCategoryId == 0 || f.CategoryId == selectedCategoryId))
                .Where(f => f.Year == selectedYear)
                .Where
                (
                    f => string.IsNullOrEmpty(searchTerm) ||
                    f.Name.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ||
                    (f.Description?.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ?? false)
                )
                .OrderBy(f => f.Month)
                .ThenBy(f => f.CategoryName)
                .ToList();
        }

        private void ClearSearch()
        {
            searchTerm = string.Empty;

            ApplyFilters();
        }

        // ================================================================
        // 7. OPERACIONES CRUD (SAVE)
        // ================================================================

        private async Task SaveFixedExpense()
        {
            try
            {
                bool success = false;

                if (_fixedExpenseForm.IsDeleting)
                {
                    success = await DeleteFixedExpenseAsync();
                }
                else if (_fixedExpenseForm.IsEditing)
                {
                    success = await UpdateFixedExpenseAsync();
                }
                else
                {
                    success = await CreateFixedExpenseAsync();
                }

                if (success)
                {
                    _fixedExpensesCache.Remove(_fixedExpenseForm.Year);

                    if (_fixedExpenseForm.IsEditing && _fixedExpenseForm.OriginalYear != _fixedExpenseForm.Year)
                    {
                        _fixedExpensesCache.Remove(_fixedExpenseForm.OriginalYear);
                    }

                    await HasDataService.RefreshAsync();

                    _fixedExpenseForm.IsModalOpen = false;
                    _fixedExpenseForm.IsDeleting = false;
                    _fixedExpenseForm.IsEditing = false;

                    await LoadFixedExpenses(_selectedYear);

                    ApplyFilters();

                    await InvokeAsync(StateHasChanged);
                }
            }
            catch (Exception ex)
            {
                await LogService.LogErrorAsync($"Error en SaveFixedExpense", ex);
                ToastService.ShowError("Ocurrió un error inesperado.");
            }
        }

        private async Task<bool> CreateFixedExpenseAsync()
        {
            if (string.IsNullOrWhiteSpace(_fixedExpenseForm.Name))
            {
                CategoryModel? category = _categories.FirstOrDefault(c => c.Id == _fixedExpenseForm.CategoryId);
                _fixedExpenseForm.Name = category?.Name ?? "Sin nombre";
            }

            CreateFixedExpenseRequestDTO request = new()
            {
                CategoryId = _fixedExpenseForm.CategoryId,
                Name = _fixedExpenseForm.Name,
                Description = _fixedExpenseForm.Description,
                Amount = _fixedExpenseForm.Amount,
                Month = _fixedExpenseForm.Month,
                Year = _fixedExpenseForm.Year
            };

            FixedExpenseModel? result = await APIService.CreateFixedExpenseAsync(request);

            if (result != null)
            {
                ToastService.ShowSuccess($"Gasto fijo [{_fixedExpenseForm.Name}] creado correctamente.");
                return true;
            }

            return false;
        }

        private async Task<bool> UpdateFixedExpenseAsync()
        {
            UpdateFixedExpenseRequestDTO request = new()
            {
                CategoryId = _fixedExpenseForm.CategoryId,
                Name = _fixedExpenseForm.Name,
                Description = _fixedExpenseForm.Description,
                Amount = _fixedExpenseForm.Amount,
                Month = _fixedExpenseForm.Month,
                Year = _fixedExpenseForm.Year
            };

            FixedExpenseModel? result = await APIService.UpdateFixedExpenseAsync(_fixedExpenseForm.Id, request);

            if (result != null)
            {
                ToastService.ShowSuccess($"Gasto fijo [{_fixedExpenseForm.Name}] actualizado correctamente.");

                return true;
            }

            return false;
        }

        private async Task<bool> DeleteFixedExpenseAsync()
        {
            bool success = await APIService.DeleteFixedExpenseAsync(_fixedExpenseForm.Id, _fixedExpenseForm.Name);

            if (success)
            {
                ToastService.ShowSuccess($"Gasto fijo [{_fixedExpenseForm.Name}] eliminado correctamente.");

                return true;
            }

            return false;
        }

        // ================================================================
        // 8. APERTURA DE MODALES
        // ================================================================

        private void OpenCreateModal()
        {
            FillFormFromModel
            (
                new FixedExpenseModel()
                {
                    CategoryId = _categories.FirstOrDefault()?.Id ?? 0,
                    Month = DateTime.Now.Month,
                    Year = _selectedYear,
                }, FormModeEnum.Create
            );

            InvokeAsync(StateHasChanged);
        }

        private void OpenEditModal(FixedExpenseModel fixedExpense)
        {
            FillFormFromModel(fixedExpense, FormModeEnum.Edit);

            InvokeAsync(StateHasChanged);
        }

        private async Task OpenDeleteModal(FixedExpenseModel fixedExpense)
        {
            FillFormFromModel(fixedExpense, FormModeEnum.Delete);

            StateHasChanged();
        }

        private void FillFormFromModel(FixedExpenseModel fixedExpenseModel, FormModeEnum mode)
        {
            // ✅ Si el nombre está vacío, sugerir el nombre de la categoría (solo en creación)
            if (string.IsNullOrEmpty(fixedExpenseModel.Name) && mode == FormModeEnum.Create)
            {
                CategoryModel? category = _categories.FirstOrDefault(c => c.Id == fixedExpenseModel.CategoryId);

                _suggestedName = category != null ? $"Ej. {category.Name}" : _suggestedName;
            }

            _fixedExpenseForm = new FixedExpenseFormModel
            {
                Id = fixedExpenseModel.Id,
                CategoryId = fixedExpenseModel.CategoryId,
                CategoryName = fixedExpenseModel.CategoryName,
                Name = fixedExpenseModel.Name,
                Description = fixedExpenseModel.Description,
                Amount = fixedExpenseModel.Amount,
                Month = fixedExpenseModel.Month,
                OriginalYear = fixedExpenseModel.Year,
                Year = fixedExpenseModel.Year,
                IsModalOpen = true,
                IsEditing = mode == FormModeEnum.Edit,
                IsDeleting = mode == FormModeEnum.Delete
            };
        }

        private void CloseModal()
        {
            _fixedExpenseForm.IsModalOpen = false;
            _fixedExpenseForm.IsEditing = false;
            _fixedExpenseForm.IsDeleting = false;
            
            _suggestedName = "Ej. IBI";

            InvokeAsync(StateHasChanged);
        }

        // ================================================================
        // 9. MÉTODOS AUXILIARES
        // ================================================================

        private void OnCategoryChanged()
        {
            CategoryModel? category = _categories.FirstOrDefault(c => c.Id == _fixedExpenseForm.CategoryId);

            if (category != null)
            {
                // Actualizar placeholder con el nombre de la categoría
                _suggestedName = $"Ej. {category.Name}";
            }

            StateHasChanged();
        }

        private async Task OnYearChanged()
        {
            try
            {
                await LoadFixedExpenses(_selectedYear);

                ApplyFilters();
            }
            catch (Exception ex)
            {
                await LogService.LogErrorAsync("Error al cargar los gastos fijos del año seleccionado", ex);

                ToastService.ShowError("Error al cargar los gastos fijos.");

                _fixedExpenses = new List<FixedExpenseModel>();

                ApplyFilters();
            }

            await InvokeAsync(StateHasChanged);
        }
    }
}