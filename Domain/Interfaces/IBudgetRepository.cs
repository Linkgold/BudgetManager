using Domain.Entities;
using Domain.ValueObjects;

namespace Domain.Interfaces
{
    public interface IBudgetRepository
    {
        // Consultas
        Task<Budget?> GetByIdAsync(int userId, int id);
        Task<IEnumerable<Budget>> GetAllByYearAsync(int userId, int year);
        Task<IEnumerable<int>> GetDistinctYearsAsync(int userId);

        // Métodos de negocio
        Task<Budget?> GetByCategoryAndPeriodAsync(int userId, int categoryId, MonthlyPeriod period, bool withTracking = false);
        Task<IEnumerable<Budget>> GetByCategoryIdAsync(int userId, int categoryId);

        // Comandos
        Task AddAsync(Budget budget);
        Task UpdateAsync(Budget budget);
        Task DeleteAsync(int userId, int id);

        // Verificaciones
        Task<bool> ExistsAsync(int id, int userId);
        Task<bool> ExistsForCategoryAndPeriodAsync(int userId, int categoryId, MonthlyPeriod period);
    }
}