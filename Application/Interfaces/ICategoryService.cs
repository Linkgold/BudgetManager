using Application.DTOs.Request;
using Application.DTOs.Response;

namespace Application.Interfaces
{
    public interface ICategoryService
    {
        // Consultas
        Task<CategoryResponseDto> GetByIdAsync(int id);
        Task<List<CategoryResponseDto>> GetAllAsync();
        Task<List<CategoryResponseDto>> GetActiveCategoriesAsync();
        Task<CategoryResponseDto> GetByNameAsync(string name);

        // Comandos
        Task<CategoryResponseDto> CreateAsync(CreateCategoryRequestDto request);
        Task<CategoryResponseDto> UpdateAsync(int id, UpdateCategoryRequestDto request);
        Task DeleteAsync(int id);

        // Validaciones
        Task<bool> ExistsAsync(int id);
        Task<bool> ExistsByNameAsync(string name);
        Task<bool> CanDeleteAsync(int id);
    }
}