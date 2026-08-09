using Domain.Entities;

namespace Domain.Interfaces
{
    public interface ITransactionRepository
    {
        // Consultas
        Task<Transaction?> GetByIdAsync(int userId, int id);
        Task<IEnumerable<Transaction>> GetAllByYearAsync(int userId, int year);
        Task<Dictionary<int, List<int>>> GetDistinctYearsAndMonthsAsync(int userId);

        // Comandos
        Task AddAsync(Transaction expense);
        Task UpdateAsync(Transaction expense);
        Task DeleteAsync(int userId, int id);

        // Verificaciones
        Task<bool> ExistsAsync(int userId, int id);
    }
}