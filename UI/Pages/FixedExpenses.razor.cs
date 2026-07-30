using Shared.DTOs.Request;
using UI.Extensions;
using UI.Extensions.Mappings;
using UI.Models;
using UI.Models.Forms;
using UI.Services.API;
using UI.Shared;

namespace UI.Pages
{
    public partial class FixedExpenses : BasePage
    {
        // ================================================================
        // 1. MODELOS Y ESTADO
        // ================================================================

        private List<FixedExpenseModel> _fixedExpenses = new();
        private List<FixedExpenseModel> _filteredFixedExpenses = new();
        private List<CategoryModel> _categories = new();
        private FixedExpenseFormModel _fixedExpenseForm = new();
        private List<int> _years = new();

        // ================================================================
        // 2. FILTROS Y PROPIEDADES CON SETTER
        // ================================================================

        private string _searchTerm = string.Empty;
        private int _selectedCategoryId = 0;
        private int _selectedYear = 0;  // ✅ 0 = todos los años

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
                    ApplyFilters();
                }
            }
        }

        // ================================================================
        // 3. CICLO DE VIDA
        // ================================================================

        protected override async Task OnInitializedAsync()
        {
            await LoadData();
        }

        // ================================================================
        // 4. CARGA DE DATOS
        // ================================================================

        private async Task LoadData()
        {
            try
            {
                // Cargar categorías
                List<CategoryModel>? categoriesResult = await APIService.GetCategoriesAsync();
                _categories = categoriesResult?.ToExpenseMixedCategoryModelList() ?? new List<CategoryModel>();


                // Cargar gastos fijos
                _fixedExpenses = await APIService.GetFixedExpensesAsync() ?? new List<FixedExpenseModel>();

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

        // ================================================================
        // 5. FILTRADO
        // ================================================================

        private void ApplyFilters()
        {
            _filteredFixedExpenses = _fixedExpenses
                .Where(f => (selectedCategoryId == 0 || f.CategoryId == selectedCategoryId))
                .Where(f => selectedYear == 0 || f.Year == selectedYear)
                .Where(f => string.IsNullOrEmpty(searchTerm) ||
                             f.Name.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ||
                             (f.Description?.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ?? false) ||
                             f.CategoryName.Contains(searchTerm, StringComparison.OrdinalIgnoreCase))
                .OrderBy(f => f.Year)
                .ThenBy(f => f.Month)
                .ThenBy(f => f.CategoryName)
                .ToList();
        }

        private void ClearSearch()
        {
            searchTerm = string.Empty;

            ApplyFilters();
        }

        // ================================================================
        // 6. OPERACIONES CRUD (SAVE)
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
                    _fixedExpenseForm.IsModalOpen = false;
                    _fixedExpenseForm.IsDeleting = false;
                    _fixedExpenseForm.IsEditing = false;

                    await LoadData();

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
                    Year = DateTime.Now.Year,
                }, FormMode.Create
            );

            InvokeAsync(StateHasChanged);
        }

        private void OpenEditModal(FixedExpenseModel fixedExpense)
        {
            FillFormFromModel(fixedExpense, FormMode.Edit);

            InvokeAsync(StateHasChanged);
        }

        private async Task OpenDeleteModal(FixedExpenseModel fixedExpense)
        {
            FillFormFromModel(fixedExpense, FormMode.Delete);

            StateHasChanged();
        }

        private void FillFormFromModel(FixedExpenseModel fixedExpenseModel, FormMode mode)
        {
            _fixedExpenseForm = new FixedExpenseFormModel
            {
                Id = fixedExpenseModel.Id,
                CategoryId = fixedExpenseModel.CategoryId,
                CategoryName = fixedExpenseModel.CategoryName,
                Name = fixedExpenseModel.Name,
                Description = fixedExpenseModel.Description,
                Amount = fixedExpenseModel.Amount,
                Month = fixedExpenseModel.Month,
                Year = fixedExpenseModel.Year,
                IsModalOpen = true,
                IsEditing = mode == FormMode.Edit,
                IsDeleting = mode == FormMode.Delete
            };
        }

        private void CloseModal()
        {
            _fixedExpenseForm.IsModalOpen = false;
            _fixedExpenseForm.IsEditing = false;
            _fixedExpenseForm.IsDeleting = false;

            InvokeAsync(StateHasChanged);
        }
    }
}