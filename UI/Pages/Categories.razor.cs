using Microsoft.AspNetCore.Components;
using Shared.DTOs.Request;
using Shared.DTOs.Response;
using System.Net.Http.Json;
using UI.Services.Interfaces;
using UI.Shared;

namespace UI.Pages
{
    public partial class Categories : BasePage
    {
        [Inject]
        private IToastService ToastService { get; set; } = default!;

        private List<CategoryResponseDTO> categories = [];
        private List<CategoryResponseDTO> filteredCategories = [];

        private string _searchTerm = string.Empty;
        private bool isModalOpen = false;
        private bool isEditing = false;
        private bool isDeleteMode = false;
        private CategoryResponseDTO categoryForm = new();

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
            try
            {
                HttpResponseMessage response = await SendAuthenticatedRequestAsync(() => Http.GetAsync("/api/category"));

                if (response.IsSuccessStatusCode)
                {
                    List<CategoryResponseDTO>? result = await response.Content.ReadFromJsonAsync<List<CategoryResponseDTO>>();

                    categories = result ?? throw new InvalidOperationException("La respuesta de la API no contenía datos.");
                }
                else
                {
                    throw new InvalidOperationException($"Error al obtener categorías: {response.StatusCode}");
                }
            }
            catch (Exception ex)
            {
                await LogService.LogErrorAsync($"Error al cargar categorías", ex);
                ToastService.ShowError("Error al cargar las categorías.");
                categories = [];
            }
            finally
            {
                ApplyFilters();
            }
        }

        private async Task SaveCategory()
        {
            try
            {
                if (isDeleteMode)
                {
                    // 🔥 Eliminar categoría
                    HttpResponseMessage deleteResponse = await Http.DeleteAsync($"/api/category/{categoryForm.Id}");
                    if (!deleteResponse.IsSuccessStatusCode)
                    {
                        string errorContent = await deleteResponse.Content.ReadAsStringAsync();
                        await LogService.LogErrorAsync($"Error al eliminar categoría ID {categoryForm.Id}", new Exception(errorContent));
                        ToastService.ShowError($"Error al eliminar la categoría [{categoryForm.Name}].");

                        return;
                    }
                }
                else if (isEditing)
                {
                    // 🔥 Editar categoría
                    UpdateCategoryRequestDTO request = new()
                    {
                        Name = categoryForm.Name,
                        Description = categoryForm.Description
                    };

                    HttpResponseMessage updateResponse = await Http.PutAsJsonAsync($"/api/category/{categoryForm.Id}", request);
                    if (!updateResponse.IsSuccessStatusCode)
                    {
                        string errorContent = await updateResponse.Content.ReadAsStringAsync();
                        await LogService.LogErrorAsync($"Error al actualizar categoría ID {categoryForm.Id}", new Exception(errorContent));
                        ToastService.ShowError($"Error al actualizar la categoría [{categoryForm.Name}].");

                        return;
                    }
                }
                else
                {
                    // 🔥 Crear categoría
                    CreateCategoryRequestDTO request = new()
                    {
                        Name = categoryForm.Name,
                        Description = categoryForm.Description
                    };

                    HttpResponseMessage createResponse = await Http.PostAsJsonAsync("/api/category", request);
                    if (!createResponse.IsSuccessStatusCode)
                    {
                        string errorContent = await createResponse.Content.ReadAsStringAsync();
                        await LogService.LogErrorAsync($"Error al crear categoría", new Exception(errorContent));
                        ToastService.ShowError($"Error al crear la categoría [{categoryForm.Name}].");

                        return;
                    }
                }

                // Por ahora cerramos el modal
                isModalOpen = false;
                isDeleteMode = false;
                await LoadCategories(); // Recargar lista
                await InvokeAsync(StateHasChanged);
            }
            catch (Exception ex)
            {
                await LogService.LogErrorAsync($"Error en SaveCategory", ex);
                ToastService.ShowError("Ocurrió un error inesperado.");
            }
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
            categoryForm = new CategoryResponseDTO { };
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
                };
                isModalOpen = true;
                StateHasChanged();
            }
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
                .OrderBy(c => c.Id)
                .ToList();
        }
    }
}