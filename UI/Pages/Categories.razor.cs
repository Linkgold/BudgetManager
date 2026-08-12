using Contracts.Enums;
using Shared.DTOs.Request;
using UI.Extensions;
using UI.Models;
using UI.Models.Forms;
using UI.Services.API;
using UI.Shared;

namespace UI.Pages
{
    public partial class Categories : BasePage
    {
        // ================================================================
        // 1. MODELOS Y ESTADO
        // ================================================================

        private List<CategoryModel> _categories = new();
        private List<CategoryModel> _filteredCategories = new();
        private CategoryFormModel _categoryForm = new();

        // ================================================================
        // 2. FILTROS Y PROPIEDADES CON SETTER
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
        // 5. FILTRADO
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
        // 6. OPERACIONES CRUD (SAVE)
        // ================================================================

        private async Task SaveCategory()
        {
            try
            {
                bool success = false;

                if (_categoryForm.IsDeleting)
                {
                    success = await DeleteCategoryAsync();
                }
                else if (_categoryForm.IsEditing)
                {
                    success = await UpdateCategoryAsync();
                }
                else
                {
                    success = await CreateCategoryAsync();
                }

                if (success)
                {
                    _categoryForm.IsModalOpen = false;
                    _categoryForm.IsEditing = false;
                    _categoryForm.IsDeleting = false;

                    await LoadData();
                    await InvokeAsync(StateHasChanged);
                }
            }
            catch (Exception ex)
            {
                await LogService.LogErrorAsync($"Error en SaveCategory", ex);
                ToastService.ShowError("Ocurrió un error inesperado.");
            }
        }

        private async Task<bool> CreateCategoryAsync()
        {
            CreateCategoryRequestDTO request = new()
            {
                Name = _categoryForm.Name,
                Description = _categoryForm.Description,
                Nature = _categoryForm.Nature
            };

            CategoryModel? result = await APIService.CreateCategoryAsync(request);

            if (result != null)
            {
                ToastService.ShowSuccess($"Categoría [{_categoryForm.Name}] creada correctamente.");

                return true;
            }

            return false;
        }

        private async Task<bool> UpdateCategoryAsync()
        {
            UpdateCategoryRequestDTO request = new()
            {
                Name = _categoryForm.Name,
                Description = _categoryForm.Description,
                Nature = _categoryForm.Nature
            };

            CategoryModel? result = await APIService.UpdateCategoryAsync(_categoryForm.Id, request);

            if (result != null)
            {
                ToastService.ShowSuccess($"Categoría [{_categoryForm.Name}] actualizada correctamente.");

                return true;
            }

            return false;
        }

        private async Task<bool> DeleteCategoryAsync()
        {
            bool success = await APIService.DeleteCategoryAsync(_categoryForm.Id, _categoryForm.Name);

            if (success)
            {
                ToastService.ShowSuccess($"Categoría [{_categoryForm.Name}] eliminada correctamente.");

                return true;
            }

            return false;
        }

        // ================================================================
        // 7. APERTURA DE MODALES
        // ================================================================

        private void OpenCreateModal()
        {
            FillFormFromModel(new CategoryModel { Nature = CategoryNatureEnum.Expense }, FormModeEnum.Create);

            InvokeAsync(StateHasChanged);
        }

        private void OpenEditModal(CategoryModel category)
        {
            FillFormFromModel(category, FormModeEnum.Edit);

            InvokeAsync(StateHasChanged);
        }

        private async Task OpenDeleteModal(CategoryModel category)
        {
            FillFormFromModel(category, FormModeEnum.Delete);

            StateHasChanged();
        }

        private void FillFormFromModel(CategoryModel model, FormModeEnum mode)
        {
            _categoryForm = new CategoryFormModel
            {
                Id = model.Id,
                Name = model.Name,
                Description = model.Description,
                Nature = model.Nature,
                IsModalOpen = true,
                IsEditing = mode == FormModeEnum.Edit,
                IsDeleting = mode == FormModeEnum.Delete
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