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
        Task<List<FixedExpense>> GetAllAsync();
        Task<List<FixedExpense>> GetByCategoryAsync(int categoryId);
        Task<List<FixedExpense>> GetActiveAsync();
        Task<List<FixedExpense>> GetActiveByCategoryAsync(int categoryId);

        // Métodos de período (para calcular gastos fijos en un mes concreto)
        Task<List<FixedExpense>> GetActiveForPeriodAsync(Period period);
        Task<List<FixedExpense>> GetActiveForPeriodByCategoryAsync(int categoryId, Period period);

        // Métodos de agregación
        Task<decimal> GetTotalByCategoryAndPeriodAsync(int categoryId, Period period);
        Task<decimal> GetTotalActiveAsync();
        Task<decimal> GetTotalActiveByCategoryAsync(int categoryId);

        // Métodos de negocio
        Task<bool> ExistsAsync(int id);
        Task<bool> IsActiveAsync(int id);

        // Métodos de escritura
        Task AddAsync(FixedExpense fixedExpense);
        Task UpdateAsync(FixedExpense fixedExpense);
        Task DeleteAsync(int id);
        Task ActivateAsync(int id);
        Task DeactivateAsync(int id);
    }
}