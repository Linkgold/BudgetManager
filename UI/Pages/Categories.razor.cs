using Contracts.Enums;
using Microsoft.AspNetCore.Components;
using Shared.DTOs.Request;
using Shared.DTOs.Response;
using UI.Extensions;
using UI.Models;
using UI.Models.Forms;
using UI.Services.API;
using UI.Services.Interfaces;
using UI.Shared;

namespace UI.Pages
{
    public partial class Categories : BasePage
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

        private List<CategoryModel> _categories = new();
        private List<CategoryModel> _filteredCategories = new();
        private CategoryFormModel _categoryForm = new();

        // ================================================================
        // 3. FILTROS Y PROPIEDADES CON SETTER
        // ================================================================

        private string _searchTerm = string.Empty;
        private CategoryNatureEnum? _selectedNatureFilter = null;

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

        private CategoryNatureEnum? selectedNatureFilter
        {
            get => _selectedNatureFilter;
            set
            {
                if (_selectedNatureFilter != value)
                {
                    _selectedNatureFilter = value;
                    ApplyFilters();
                }
            }
        }

        // ================================================================
        // 4. CICLO DE VIDA
        // ================================================================

        protected override async Task OnInitializedAsync()
        {
            await LoadCategories();
        }

        // ================================================================
        // 5. CARGA DE DATOS
        // ================================================================

        private async Task LoadCategories()
        {
            try
            {
                _categories = await APIService.GetCategoriesAsync() ?? new List<CategoryModel>();
            }
            catch (Exception ex)
            {
                await LogService.LogErrorAsync($"Error al cargar categorías", ex);

                ToastService.ShowError("Error al cargar las categorías.");
                _categories = new List<CategoryModel>();
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
            _filteredCategories = _categories
                .Where(c => string.IsNullOrEmpty(searchTerm) ||
                             c.Name.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ||
                             (c.Description?.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ?? false))
                .Where(c => !selectedNatureFilter.HasValue || c.Nature == selectedNatureFilter.Value)
                .OrderBy(c => c.Id)
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

        private async Task SaveCategory()
        {
            try
            {
                if (_categoryForm.IsDeleting)
                {
                    await DeleteCategoryAsync();
                }
                else if (_categoryForm.IsEditing)
                {
                    await UpdateCategoryAsync();
                }
                else
                {
                    await CreateCategoryAsync();
                }

                _categoryForm.IsModalOpen = false;
                _categoryForm.IsDeleting = false;

                await LoadCategories();
                await InvokeAsync(StateHasChanged);
            }
            catch (Exception ex)
            {
                await LogService.LogErrorAsync($"Error en SaveCategory", ex);
                ToastService.ShowError("Ocurrió un error inesperado.");
            }
        }

        private async Task CreateCategoryAsync()
        {
            CreateCategoryRequestDTO request = new()
            {
                Name = _categoryForm.Name,
                Description = _categoryForm.Description,
                Nature = _categoryForm.Nature
            };

            CategoryModel? result = await APIService.CreateCategoryAsync(request);

            if (result == null)
            {
                ToastService.ShowError($"Error al crear la categoría [{_categoryForm.Name}].");

                return;
            }

            ToastService.ShowSuccess($"Categoría [{_categoryForm.Name}] creada correctamente.");
        }

        private async Task UpdateCategoryAsync()
        {
            UpdateCategoryRequestDTO request = new()
            {
                Name = _categoryForm.Name,
                Description = _categoryForm.Description,
                Nature = _categoryForm.Nature
            };

            CategoryModel? result = await APIService.UpdateCategoryAsync(_categoryForm.Id, request);

            if (result == null)
            {
                ToastService.ShowError($"Error al actualizar la categoría [{_categoryForm.Name}].");

                return;
            }

            ToastService.ShowSuccess($"Categoría [{_categoryForm.Name}] actualizada correctamente.");
        }

        private async Task DeleteCategoryAsync()
        {
            bool success = await APIService.DeleteCategoryAsync(_categoryForm.Id, _categoryForm.Name);

            if (!success)
            {
                ToastService.ShowError($"Error al eliminar la categoría [{_categoryForm.Name}].");

                return;
            }

            ToastService.ShowSuccess($"Categoría [{_categoryForm.Name}] eliminada correctamente.");
        }

        // ================================================================
        // 8. APERTURA DE MODALES
        // ================================================================

        private void OpenCreateModal()
        {
            FillFormFromModel(new CategoryModel { Nature = CategoryNatureEnum.Expense }, FormMode.Create);

            InvokeAsync(StateHasChanged);
        }

        private void OpenEditModal(CategoryModel category)
        {
            FillFormFromModel(category, FormMode.Edit);

            InvokeAsync(StateHasChanged);
        }

        private async Task OpenDeleteModal(CategoryModel category)
        {
            FillFormFromModel(category, FormMode.Delete);

            StateHasChanged();
        }

        private void FillFormFromModel(CategoryModel model, FormMode mode)
        {
            _categoryForm = new CategoryFormModel
            {
                Id = model.Id,
                Name = model.Name,
                Description = model.Description,
                Nature = model.Nature,
                IsModalOpen = true,
                IsEditing = mode == FormMode.Edit,
                IsDeleting = mode == FormMode.Delete
            };
        }

        private void CloseModal()
        {
            _categoryForm.IsModalOpen = false;
            _categoryForm.IsEditing = false;
            _categoryForm.IsDeleting = false;

            InvokeAsync(StateHasChanged);
        }
    }
}