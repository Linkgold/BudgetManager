using Domain.Entities;

namespace Domain.Interfaces
{
    public interface ICategoryRepository
    {
        // Consultas
        Task<Category?> GetByIdAsync(int userId, int id, bool withTracking = false);
        Task<IEnumerable<Category>> GetAllAsync(int userId);
        Task<Category?> GetByNameAsync(int userId, string name);

        // Comandos
        Task AddAsync(Category category);
        Task UpdateAsync(Category category);
        Task DeleteAsync(int userId, int id);

        // Verificaciones
        Task<bool> ExistsAsync(int userId, int id);
        Task<bool> ExistsByNameAsync(int userId, string name);
        Task<bool> HasDependenciesAsync(int userId, int categoryId); // Para validar eliminación
    }
}