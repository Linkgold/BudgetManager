using Microsoft.JSInterop;
using Shared.DTOs.Response;
using UI.Shared;

namespace UI.Pages
{
    public partial class Categories : BasePage
    {
        private List<CategoryResponseDTO> categories = new();
        private List<CategoryResponseDTO> filteredCategories = new();

        private string _searchTerm = string.Empty;
        private bool _showInactive = false;
        private bool isModalOpen = false;
        private bool isEditing = false;
        private bool isDeleteMode = false;
        private CategoryResponseDTO categoryForm = new();

        private bool showInactive
        {
            get => _showInactive;
            set
            {
                if (_showInactive != value)
                {
                    _showInactive = value;
                    ApplyFilters();
                }
            }
        }

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

        protected override async Task OnInitializedAsync()
        {
            await LoadCategories();
        }

        private async Task LoadCategories()
        {
            // 🔥 Aquí irá la llamada a la API
            // Por ahora usamos datos de ejemplo
            categories = new List<CategoryResponseDTO>
            {
                new CategoryResponseDTO { Id = 1, Name = "Alimentación", Description = "Gastos de comida", IsActive = true },
                new CategoryResponseDTO { Id = 2, Name = "Transporte", Description = "Gastos de movilidad", IsActive = true },
                new CategoryResponseDTO { Id = 3, Name = "Ocio", Description = "Entretenimiento", IsActive = false },
                new CategoryResponseDTO { Id = 4, Name = "Vivienda", Description = "Hogar y servicios", IsActive = true },
                new CategoryResponseDTO { Id = 1, Name = "Alimentación", Description = "Gastos de comida", IsActive = true },
                new CategoryResponseDTO { Id = 2, Name = "Transporte", Description = "Gastos de movilidad", IsActive = true },
                new CategoryResponseDTO { Id = 3, Name = "Ocio", Description = "Entretenimiento", IsActive = false },
                new CategoryResponseDTO { Id = 4, Name = "Vivienda", Description = "Hogar y servicios", IsActive = true },
                new CategoryResponseDTO { Id = 1, Name = "Alimentación", Description = "Gastos de comida", IsActive = true },
                new CategoryResponseDTO { Id = 2, Name = "Transporte", Description = "Gastos de movilidad", IsActive = true },
                new CategoryResponseDTO { Id = 3, Name = "Ocio", Description = "Entretenimiento", IsActive = false },
                new CategoryResponseDTO { Id = 4, Name = "Vivienda", Description = "Hogar y servicios", IsActive = true },
                new CategoryResponseDTO { Id = 1, Name = "Alimentación", Description = "Gastos de comida", IsActive = true },
                new CategoryResponseDTO { Id = 2, Name = "Transporte", Description = "Gastos de movilidad", IsActive = true },
                new CategoryResponseDTO { Id = 3, Name = "Ocio", Description = "Entretenimiento", IsActive = false },
                new CategoryResponseDTO { Id = 4, Name = "Vivienda", Description = "Hogar y servicios", IsActive = true },
                new CategoryResponseDTO { Id = 1, Name = "Alimentación", Description = "Gastos de comida", IsActive = true },
                new CategoryResponseDTO { Id = 2, Name = "Transporte", Description = "Gastos de movilidad", IsActive = true },
                new CategoryResponseDTO { Id = 3, Name = "Ocio", Description = "Entretenimiento", IsActive = false },
                new CategoryResponseDTO { Id = 4, Name = "Vivienda", Description = "Hogar y servicios", IsActive = true }
            };

            ApplyFilters();
        }

        private string GetModalTitle()
        {
            if (isDeleteMode) return "🗑️ Eliminar Categoría";
            return isEditing ? "✏️ Editar Categoría" : "➕ Nueva Categoría";
        }

        private string GetSaveButtonClass()
        {
            return isDeleteMode ? "btn-danger" : "btn-primary";
        }

        private string GetSaveButtonText()
        {
            return isDeleteMode ? "Eliminar" : "Guardar";
        }

        private void OpenCreateModal()
        {
            isEditing = false;
            categoryForm = new CategoryResponseDTO { IsActive = true };
            isModalOpen = true;
            InvokeAsync(StateHasChanged);
        }

        private void OpenEditModal(CategoryResponseDTO category)
        {
            isEditing = true;
            isDeleteMode = false;
            categoryForm = new CategoryResponseDTO
            {
                Id = category.Id,
                Name = category.Name,
                Description = category.Description,
                IsActive = category.IsActive
            };
            isModalOpen = true;
            InvokeAsync(StateHasChanged);
        }

        private async Task DeleteCategory(int id)
        {
            // 🔥 Abrir modal en modo eliminación
            CategoryResponseDTO? categoryToDelete = categories.FirstOrDefault(c => c.Id == id);
            if (categoryToDelete != null)
            {
                isDeleteMode = true;
                categoryForm = new CategoryResponseDTO
                {
                    Id = categoryToDelete.Id,
                    Name = categoryToDelete.Name,
                    Description = categoryToDelete.Description,
                    IsActive = categoryToDelete.IsActive
                };
                isModalOpen = true;
                StateHasChanged();
            }
        }

        private async Task SaveCategory()
        {
            if (isDeleteMode)
            {
                // 🔥 Eliminar categoría
                // await Http.DeleteAsync($"/api/category/{categoryForm.Id}");
                // Mostrar mensaje de éxito
            }
            else if (isEditing)
            {
                // 🔥 Editar categoría
                // await Http.PutAsync($"/api/category/{categoryForm.Id}", ...);
            }
            else
            {
                // 🔥 Crear categoría
                // await Http.PostAsync("/api/category", ...);
            }

            // Por ahora cerramos el modal
            isModalOpen = false;
            isDeleteMode = false;
            await LoadCategories(); // Recargar lista
            await InvokeAsync(StateHasChanged);
        }

        private void CloseModal()
        {
            isModalOpen = false;
            isDeleteMode = false;
            InvokeAsync(StateHasChanged);
        }

        private void ApplyFilters()
        {
            filteredCategories = categories
                .Where(c => string.IsNullOrEmpty(searchTerm) ||
                             c.Name.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ||
                             (c.Description?.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ?? false))
                .Where(c => showInactive || c.IsActive)
                .ToList();
        }
    }
}