using Shared.DTOs.Request;
using Shared.DTOs.Response;

namespace Application.Interfaces
{
    public interface ICategoryService
    {
        // Consultas
        Task<CategoryResponseDTO> GetByIdAsync(int id);
        Task<List<CategoryResponseDTO>> GetAllAsync();
        Task<List<CategoryResponseDTO>> GetActiveCategoriesAsync();
        Task<CategoryResponseDTO> GetByNameAsync(string name);

        // Comandos
        Task<CategoryResponseDTO> CreateAsync(CreateCategoryRequestDTO request);
        Task<CategoryResponseDTO> UpdateAsync(int id, UpdateCategoryRequestDTO request);
        Task DeleteAsync(int id);

        // Validaciones
        Task<bool> ExistsAsync(int id);
        Task<bool> ExistsByNameAsync(string name);
        Task<bool> CanDeleteAsync(int id);
    }
}