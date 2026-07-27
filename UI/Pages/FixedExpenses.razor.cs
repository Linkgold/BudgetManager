using Microsoft.AspNetCore.Components;
using Shared.DTOs.Request;
using Shared.DTOs.Response;
using UI.Extensions;
using UI.Models;
using UI.Services;
using UI.Services.Interfaces;
using UI.Shared;

namespace UI.Pages
{
    public partial class FixedExpenses : BasePage
    {
        // ================================================================
        // 1. INYECCIONES DE DEPENDENCIAS
        // ================================================================

        [Inject]
        private IToastService ToastService { get; set; } = default!;

        [Inject]
        private APIService APIService { get; set; } = default!;

        // ================================================================
        // 2. MODELOS Y ESTADO
        // ================================================================

        private List<FixedExpenseModel> fixedExpenses = new();
        private List<FixedExpenseModel> filteredFixedExpenses = new();
        private List<CategoryModel> categories = new();
        private FixedExpenseFormModel fixedExpenseForm = new();
        private List<int> years = new();

        // ================================================================
        // 3. FILTROS Y PROPIEDADES CON SETTER
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
        // 4. CICLO DE VIDA
        // ================================================================

        protected override async Task OnInitializedAsync()
        {
            await LoadData();
        }

        // ================================================================
        // 5. CARGA DE DATOS
        // ================================================================

        private async Task LoadData()
        {
            try
            {
                // Cargar categorías
                List<CategoryResponseDTO>? result = await APIService.GetCategoriesAsync();
                categories = result?.ToExpenseMixedCategoryModelList() ?? new List<CategoryModel>();


                // Cargar gastos fijos
                List<FixedExpenseResponseDTO>? dtoList = await APIService.GetFixedExpensesAsync();
                fixedExpenses = dtoList?.ToFixedExpenseModelList() ?? new List<FixedExpenseModel>();

                // Inicializar años (2020-2050)
                years = new List<int>();
                for (int year = 2020; year <= 2050; year++)
                {
                    years.Add(year);
                }
            }
            catch (Exception ex)
            {
                await LogService.LogErrorAsync($"Error al cargar datos de gastos fijos", ex);

                ToastService.ShowError("Error al cargar los datos.");
                fixedExpenses = new();
                categories = new();
            }
            finally
            {
                ApplyFilters();
            }
        }

        // ================================================================
        // 6. FILTRADO
        // ================================================================

        private void ApplyFilters()
        {
            filteredFixedExpenses = fixedExpenses
                .Where(f => (selectedCategoryId == 0 || f.CategoryId == selectedCategoryId))
                .Where(f => selectedYear == 0 || f.Year == selectedYear)
                .Where(f => string.IsNullOrEmpty(searchTerm) ||
                             f.Name.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ||
                             (f.Description?.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ?? false) ||
                             f.CategoryName.Contains(searchTerm, StringComparison.OrdinalIgnoreCase))
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
                if (fixedExpenseForm.IsDeleting)
                {
                    await DeleteFixedExpenseAsync();
                }
                else if (fixedExpenseForm.IsEditing)
                {
                    await UpdateFixedExpenseAsync();
                }
                else
                {
                    await CreateFixedExpenseAsync();
                }

                fixedExpenseForm.IsModalOpen = false;
                fixedExpenseForm.IsDeleting = false;
                fixedExpenseForm.IsEditing = false;

                await LoadData();

                await InvokeAsync(StateHasChanged);
            }
            catch (Exception ex)
            {
                await LogService.LogErrorAsync($"Error en SaveFixedExpense", ex);
                ToastService.ShowError("Ocurrió un error inesperado.");
            }
        }

        private async Task CreateFixedExpenseAsync()
        {
            CreateFixedExpenseRequestDTO request = new()
            {
                CategoryId = fixedExpenseForm.CategoryId,
                Name = fixedExpenseForm.Name,
                Description = fixedExpenseForm.Description,
                Amount = fixedExpenseForm.Amount,
                Month = fixedExpenseForm.Month,
                Year = fixedExpenseForm.Year
            };

            FixedExpenseResponseDTO? result = await APIService.CreateFixedExpenseAsync(request);

            if (result == null)
            {
                ToastService.ShowError($"Error al crear el gasto fijo [{fixedExpenseForm.Name}].");

                return;
            }

            ToastService.ShowSuccess($"Gasto fijo [{fixedExpenseForm.Name}] creado correctamente.");
        }

        private async Task UpdateFixedExpenseAsync()
        {
            UpdateFixedExpenseRequestDTO request = new()
            {
                Name = fixedExpenseForm.Name,
                Description = fixedExpenseForm.Description,
                Amount = fixedExpenseForm.Amount,
                Month = fixedExpenseForm.Month,
                Year = fixedExpenseForm.Year
            };

            FixedExpenseResponseDTO? result = await APIService.UpdateFixedExpenseAsync(fixedExpenseForm.Id, request);

            if (result == null)
            {
                ToastService.ShowError($"Error al actualizar el gasto fijo [{fixedExpenseForm.Name}].");

                return;
            }

            ToastService.ShowSuccess($"Gasto fijo [{fixedExpenseForm.Name}] actualizado correctamente.");
        }

        private async Task DeleteFixedExpenseAsync()
        {
            bool success = await APIService.DeleteFixedExpenseAsync(fixedExpenseForm.Id);

            if (!success)
            {
                ToastService.ShowError($"Error al eliminar el gasto fijo [{fixedExpenseForm.Name}].");

                return;
            }

            ToastService.ShowSuccess($"Gasto fijo [{fixedExpenseForm.Name}] eliminado correctamente.");
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
                    CategoryId = categories.FirstOrDefault()?.Id ?? 0,
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
            fixedExpenseForm = new FixedExpenseFormModel
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
            fixedExpenseForm.IsModalOpen = false;
            fixedExpenseForm.IsEditing = false;
            fixedExpenseForm.IsDeleting = false;

            InvokeAsync(StateHasChanged);
        }
    }
}