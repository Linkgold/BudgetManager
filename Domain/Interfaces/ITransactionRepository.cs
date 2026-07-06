using Domain.Entities;
using Domain.ValueObjects;

namespace Domain.Interfaces
{
    public interface ITransactionRepository
    {
        // Consultas
        Task<Transaction> GetByIdAsync(int id);
        Task<IEnumerable<Transaction>> GetAllAsync();
        Task<IEnumerable<Transaction>> GetByCategoryIdAsync(int categoryId);
        Task<IEnumerable<Transaction>> GetByMonthlyPeriodAsync(MonthlyPeriod period);
        Task<IEnumerable<Transaction>> GetByCategoryAndMonthlyPeriodAsync(int categoryId, MonthlyPeriod period);
        Task<IEnumerable<Transaction>> GetByDateRangeAsync(DailyPeriod startDate, DailyPeriod endDate);
        Task<IEnumerable<Transaction>> GetByCategoryAndDateRangeAsync(int categoryId, DailyPeriod startDate, DailyPeriod endDate);
        Task<decimal> GetTotalByCategoryAndMonthlyPeriodAsync(int categoryId, MonthlyPeriod period);

        // Verificaciones
        Task<bool> ExistsAsync(int id);

        // Comandos
        Task AddAsync(Transaction expense);
        Task UpdateAsync(Transaction expense);
        Task DeleteAsync(int id);
    }
}