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
        Task<FixedExpense?> GetByIdAsync(int userId, int id);
        Task<IEnumerable<FixedExpense>> GetAllAsync(int userId);
        Task<IEnumerable<FixedExpense>> GetByCategoryAsync(int userId, int categoryId);
        Task<IEnumerable<FixedExpense>> GetActiveAsync(int userId);
        Task<IEnumerable<FixedExpense>> GetActiveByCategoryAsync(int userId, int categoryId);

        // Métodos de período (para calcular gastos fijos en un mes concreto)
        Task<IEnumerable<FixedExpense>> GetActiveForPeriodAsync(int userId, MonthlyPeriod period);
        Task<IEnumerable<FixedExpense>> GetActiveForPeriodByCategoryAsync(int userId, int categoryId, MonthlyPeriod period);

        // Métodos de agregación
        Task<decimal> GetTotalByCategoryAndPeriodAsync(int userId, int categoryId, MonthlyPeriod period);
        Task<decimal> GetTotalActiveAsync(int userId);
        Task<decimal> GetTotalActiveByCategoryAsync(int userId, int categoryId);

        // Métodos de negocio
        Task<bool> ExistsAsync(int userId, int id);
        Task<bool> IsActiveAsync(int userId, int id);

        // Métodos de escritura
        Task AddAsync(FixedExpense fixedExpense);
        Task UpdateAsync(FixedExpense fixedExpense);
        Task DeleteAsync(int userId, int id);
        Task ActivateAsync(int userId, int id);
        Task DeactivateAsync(int userId, int id);
    }
}