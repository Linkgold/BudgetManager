using Domain.Entities;
using Domain.ValueObjects;

namespace Domain.Interfaces
{
    public interface ICategoryRepository
    {
        Task<Category> GetByIdAsync(int id, bool withTracking = false);
        Task<List<Category>> GetAllAsync();
        Task<Category> GetByNameAsync(string name);
        Task<List<Category>> GetActiveCategoriesAsync();

        Task AddAsync(Category category);
        Task UpdateAsync(Category category);
        Task DeleteAsync(int id);

        Task<bool> ExistsAsync(int id);
        Task<bool> ExistsByNameAsync(string name);
        Task<bool> HasExpensesAsync(int categoryId); // Para validar eliminación
    }
}