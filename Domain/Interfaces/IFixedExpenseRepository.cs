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

        // Métodos de período (para calcular gastos fijos en un mes concreto)
        Task<IEnumerable<FixedExpense>> GetByPeriodAsync(int userId, MonthlyPeriod period);
        Task<IEnumerable<FixedExpense>> GetByPeriodByCategoryAsync(int userId, int categoryId, MonthlyPeriod period);

        // Métodos de agregación
        Task<decimal> GetTotalByCategoryAndPeriodAsync(int userId, int categoryId, MonthlyPeriod period);
        Task<decimal> GetTotalAsync(int userId);
        Task<decimal> GetTotalByCategoryAsync(int userId, int categoryId);

        // Métodos de negocio
        Task<bool> ExistsAsync(int userId, int id);

        // Métodos de escritura
        Task AddAsync(FixedExpense fixedExpense);
        Task UpdateAsync(FixedExpense fixedExpense);
        Task DeleteAsync(int userId, int id);
    }
}