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
        Task<Budget?> GetByIdAsync(int id, int userId);
        Task<IEnumerable<Budget>> GetAllAsync(int userId);
        Task<Budget?> GetByCategoryAndPeriodAsync(int categoryId, MonthlyPeriod period, int userId);
        Task<IEnumerable<Budget>> GetByPeriodAsync(MonthlyPeriod periodm, int userId);
        Task<IEnumerable<Budget>> GetByCategoryIdAsync(int categoryId, int userId);

        // ==================== VERIFICACIONES ====================
        Task<bool> ExistsAsync(int id, int userId);
        Task<bool> ExistsForCategoryAndPeriodAsync(int categoryId, MonthlyPeriod period, int userId);

        // ==================== COMANDOS ====================
        Task AddAsync(Budget budget);
        Task UpdateAsync(Budget budget);
        Task DeleteAsync(int id, int userId);
    }
}