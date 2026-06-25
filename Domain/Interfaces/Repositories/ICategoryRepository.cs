using Domain.Entities;
using Domain.ValueObjects;

namespace Domain.Interfaces.Repositories
{
    public interface ICategoryRepository
    {
        Task<Category> GetByIdAsync(int id);
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