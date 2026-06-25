using Domain.Entities;
using Domain.Enums;
using Domain.ValueObjects;

namespace Domain.Interfaces
{
    public interface IBudgetRepository
    {
        // CRUD Básico
        Task<Budget> GetByIdAsync(int id);
        Task<Budget> GetByIdWithCategoryAsync(int id);
        Task<IEnumerable<Budget>> GetAllAsync();
        Task<IEnumerable<Budget>> GetByCategoryAsync(int categoryId);
        Task<Budget> GetByCategoryAndPeriodAsync(int categoryId, Period period);

        // Métodos de búsqueda
        Task<IEnumerable<Budget>> GetByPeriodAsync(Period period);
        Task<IEnumerable<Budget>> GetByYearAsync(int year);
        Task<IEnumerable<Budget>> GetOverBudgetAsync();
        Task<IEnumerable<Budget>> GetByStatusAsync(BudgetStatus status);

        // Métodos de agregación
        Task<decimal> GetTotalBudgetForPeriodAsync(Period period);
        Task<decimal> GetTotalBudgetForCategoryAsync(int categoryId);

        // Métodos de negocio
        Task<bool> ExistsAsync(int categoryId, Period period);
        Task<bool> IsOverBudgetAsync(int categoryId, Period period);
        Task<IEnumerable<Budget>> GetExpiringBudgetsAsync(int daysThreshold = 30);

        // Métodos de escritura
        Task AddAsync(Budget budget);
        Task UpdateAsync(Budget budget);
        Task DeleteAsync(int id);

        // Métodos batch
        Task AddRangeAsync(IEnumerable<Budget> budgets);
        Task UpdateRangeAsync(IEnumerable<Budget> budgets);
    }
}
