using Domain.Entities;
using Domain.ValueObjects;

namespace Domain.Interfaces
{
    public interface ITransactionRepository
    {
        // Consultas
        Task<Transaction?> GetByIdAsync(int id, int userId);
        Task<IEnumerable<Transaction>> GetAllAsync(int userId);
        Task<IEnumerable<Transaction>> GetByCategoryIdAsync(int categoryId, int userId);
        Task<IEnumerable<Transaction>> GetByMonthlyPeriodAsync(MonthlyPeriod period, int userId);
        Task<IEnumerable<Transaction>> GetByCategoryAndMonthlyPeriodAsync(int categoryId, MonthlyPeriod period, int userId);
        Task<IEnumerable<Transaction>> GetByDateRangeAsync(DailyPeriod startDate, DailyPeriod endDate, int userId);
        Task<IEnumerable<Transaction>> GetByCategoryAndDateRangeAsync(int categoryId, DailyPeriod startDate, DailyPeriod endDate, int userId);
        Task<decimal> GetTotalByCategoryAndMonthlyPeriodAsync(int categoryId, MonthlyPeriod period, int userId);

        // Verificaciones
        Task<bool> ExistsAsync(int id, int userId);

        // Comandos
        Task AddAsync(Transaction expense);
        Task UpdateAsync(Transaction expense);
        Task DeleteAsync(int id, int userId);
    }
}