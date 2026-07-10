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
        Task<FixedExpense?> GetByIdAsync(int id, int userId);
        Task<IEnumerable<FixedExpense>> GetAllAsync(int userId);
        Task<IEnumerable<FixedExpense>> GetByCategoryAsync(int categoryId, int userId);
        Task<IEnumerable<FixedExpense>> GetActiveAsync(int userId);
        Task<IEnumerable<FixedExpense>> GetActiveByCategoryAsync(int categoryId, int userId);

        // Métodos de período (para calcular gastos fijos en un mes concreto)
        Task<IEnumerable<FixedExpense>> GetActiveForPeriodAsync(MonthlyPeriod period, int userId);
        Task<IEnumerable<FixedExpense>> GetActiveForPeriodByCategoryAsync(int categoryId, MonthlyPeriod period, int userId);

        // Métodos de agregación
        Task<decimal> GetTotalByCategoryAndPeriodAsync(int categoryId, MonthlyPeriod period, int userId);
        Task<decimal> GetTotalActiveAsync(int userId);
        Task<decimal> GetTotalActiveByCategoryAsync(int categoryId, int userId);

        // Métodos de negocio
        Task<bool> ExistsAsync(int id, int userId);
        Task<bool> IsActiveAsync(int id, int userId);

        // Métodos de escritura
        Task AddAsync(FixedExpense fixedExpense);
        Task UpdateAsync(FixedExpense fixedExpense);
        Task DeleteAsync(int id, int userId);
        Task ActivateAsync(int id, int userId);
        Task DeactivateAsync(int id, int userId);
    }
}