using Domain.Entities;
using Domain.ValueObjects;

namespace Domain.Interfaces
{
    /// <summary>
    /// Repositorio para la entidad Budget
    /// </summary>
    public interface IBudgetRepository
    {
        // ==================== CONSULTAS ====================
        Task<Budget?> GetByIdAsync(int id);
        Task<IEnumerable<Budget>> GetAllAsync();
        Task<Budget?> GetByCategoryAndPeriodAsync(int categoryId, Period period);
        Task<IEnumerable<Budget>> GetByPeriodAsync(Period period);
        Task<IEnumerable<Budget>> GetByCategoryIdAsync(int categoryId);

        // ==================== VERIFICACIONES ====================
        Task<bool> ExistsAsync(int id);
        Task<bool> ExistsForCategoryAndPeriodAsync(int categoryId, Period period);

        // ==================== COMANDOS ====================
        Task AddAsync(Budget budget);
        Task UpdateAsync(Budget budget);
        Task DeleteAsync(int id);
    }
}