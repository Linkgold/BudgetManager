using Domain.Entities;

namespace Domain.Interfaces
{
    public interface ICategoryRepository
    {
        Task<Category?> GetByIdAsync(int userId, int id, bool withTracking = false);
        Task<IEnumerable<Category>> GetAllAsync(int userId);
        Task<Category?> GetByNameAsync(int userId, string name);
        Task<IEnumerable<Category>> GetActiveCategoriesAsync(int userId);

        Task AddAsync(Category category);
        Task UpdateAsync(Category category);
        Task DeleteAsync(int userId, int id);

        Task<bool> ExistsAsync(int userId, int id);
        Task<bool> ExistsByNameAsync(int userId, string name);
        Task<bool> HasDependenciesAsync(int userId, int categoryId); // Para validar eliminación
    }
}