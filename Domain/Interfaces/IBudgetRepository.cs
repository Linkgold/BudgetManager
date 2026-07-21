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
        Task<Budget?> GetByIdAsync(int userId, int id);
        Task<IEnumerable<Budget>> GetAllAsync(int userId);
        Task<Budget?> GetByCategoryAndPeriodAsync(int userId, int categoryId, MonthlyPeriod period, bool withTracking = false);
        Task<IEnumerable<Budget>> GetByPeriodAsync(int userId, MonthlyPeriod periodm);
        Task<IEnumerable<Budget>> GetByCategoryIdAsync(int userId, int categoryId);

        // ==================== VERIFICACIONES ====================
        Task<bool> ExistsAsync(int id, int userId);
        Task<bool> ExistsForCategoryAndPeriodAsync(int userId, int categoryId, MonthlyPeriod period);

        // ==================== COMANDOS ====================
        Task AddAsync(Budget budget);
        Task UpdateAsync(Budget budget);
        Task DeleteAsync(int userId, int id);
    }
}