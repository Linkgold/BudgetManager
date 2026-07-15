using Domain.Entities;
using Domain.ValueObjects;

namespace Domain.Interfaces
{
    public interface ITransactionRepository
    {
        // Consultas
        Task<Transaction?> GetByIdAsync(int userId, int id);
        Task<IEnumerable<Transaction>> GetAllAsync(int userId);
        Task<IEnumerable<Transaction>> GetByCategoryIdAsync(int userId, int categoryId);
        Task<IEnumerable<Transaction>> GetByMonthlyPeriodAsync(int userId, MonthlyPeriod period);
        Task<IEnumerable<Transaction>> GetByCategoryAndMonthlyPeriodAsync(int userId, int categoryId, MonthlyPeriod period);
        Task<IEnumerable<Transaction>> GetByDateRangeAsync(int userId, DailyPeriod startDate, DailyPeriod endDate);
        Task<IEnumerable<Transaction>> GetByCategoryAndDateRangeAsync(int userId, int categoryId, DailyPeriod startDate, DailyPeriod endDate);
        Task<decimal> GetTotalByCategoryAndMonthlyPeriodAsync(int userId, int categoryId, MonthlyPeriod period);

        // Verificaciones
        Task<bool> ExistsAsync(int userId, int id);

        // Comandos
        Task AddAsync(Transaction expense);
        Task UpdateAsync(Transaction expense);
        Task DeleteAsync(int userId, int id);
    }
}