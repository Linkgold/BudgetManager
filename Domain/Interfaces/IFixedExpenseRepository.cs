using Domain.Entities;
using Domain.ValueObjects;

namespace Domain.Interfaces
{
    /// <summary>
    /// Repositorio para gestionar gastos fijos (recurrentes)
    /// </summary>
    public interface IFixedExpenseRepository
    {
        // CRUD Básico
        Task<FixedExpense> GetByIdAsync(int id);
        Task<IEnumerable<FixedExpense>> GetAllAsync();
        Task<IEnumerable<FixedExpense>> GetByCategoryAsync(int categoryId);
        Task<IEnumerable<FixedExpense>> GetActiveAsync();
        Task<IEnumerable<FixedExpense>> GetActiveByCategoryAsync(int categoryId);

        // Métodos de período (para calcular gastos fijos en un mes concreto)
        Task<IEnumerable<FixedExpense>> GetActiveForPeriodAsync(Period period);
        Task<IEnumerable<FixedExpense>> GetActiveForPeriodByCategoryAsync(int categoryId, Period period);

        // Métodos de agregación
        Task<decimal> GetTotalByCategoryAndPeriodAsync(int categoryId, Period period);
        Task<decimal> GetTotalActiveAsync();
        Task<decimal> GetTotalActiveByCategoryAsync(int categoryId);

        // Métodos con paginación
        Task<IEnumerable<FixedExpense>> GetActiveForPeriodByCategoryAsync(int categoryId, Period period, int take, int skip = 0);

        // Métodos de negocio
        Task<bool> ExistsAsync(int id);
        Task<bool> IsActiveAsync(int id);
        Task<IEnumerable<FixedExpense>> GetExpiringSoonAsync(int daysThreshold = 30);

        // Métodos de escritura
        Task AddAsync(FixedExpense fixedExpense);
        Task UpdateAsync(FixedExpense fixedExpense);
        Task DeleteAsync(int id);
        Task ActivateAsync(int id);
        Task DeactivateAsync(int id);

        // Métodos batch
        Task AddRangeAsync(IEnumerable<FixedExpense> fixedExpenses);
        Task<IEnumerable<FixedExpense>> GetByDateRangeAsync(int categoryId, DateTime startDate, DateTime? endDate = null);

        // Método para prorratear gastos fijos (si necesitas cálculos por días)
        Task<decimal> GetProratedTotalForPeriodAsync(int categoryId, Period period);
    }
}