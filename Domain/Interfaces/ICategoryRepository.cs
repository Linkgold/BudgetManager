using Domain.Entities;

namespace Domain.Interfaces
{
    public interface ICategoryRepository
    {
        Task<Category?> GetByIdAsync(int id, int userId, bool withTracking = false);
        Task<IEnumerable<Category>> GetAllAsync(int userId);
        Task<Category?> GetByNameAsync(string name, int userId);
        Task<IEnumerable<Category>> GetActiveCategoriesAsync(int userId);

        Task AddAsync(Category category);
        Task UpdateAsync(Category category);
        Task DeleteAsync(int id, int userId);

        Task<bool> ExistsAsync(int id, int userId);
        Task<bool> ExistsByNameAsync(string name, int userId);
        Task<bool> HasDependenciesAsync(int categoryId, int userId); // Para validar eliminación
    }
}