using Domain.Entities;
using Domain.ValueObjects;

namespace Domain.Interfaces
{
    public interface IFixedExpenseRepository
    {
        // Consultas
        Task<FixedExpense?> GetByIdAsync(int userId, int id);
        Task<IEnumerable<FixedExpense>> GetAllByYearAsync(int userId, int year);

        // Comandos
        Task AddAsync(FixedExpense fixedExpense);
        Task UpdateAsync(FixedExpense fixedExpense);
        Task DeleteAsync(int userId, int id);

        // Verificaciones
        Task<bool> ExistsAsync(int userId, int id);
        Task<bool> ExistsByCategoryNameMonthYearAsync(int userId, int categoryId, string name, int month, int year, int? excludeId = null);
    }
}