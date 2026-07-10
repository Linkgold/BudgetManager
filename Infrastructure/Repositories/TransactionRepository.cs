using Infrastructure.Data;
using Domain.Entities;
using Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Domain.Interfaces;

namespace Infrastructure.Repositories
{
    /// <summary>
    /// Implementación del repositorio de transacciones
    /// </summary>
    public class TransactionRepository : ITransactionRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly DbSet<Transaction> _dbSet;

        public TransactionRepository(ApplicationDbContext context)
        {
            ArgumentNullException.ThrowIfNull(context);

            _context = context;
            _dbSet = context.Set<Transaction>();
        }

        // ==================== CONSULTAS ====================

        public async Task<Transaction?> GetByIdAsync(int id, int userId)
        {
            if (userId <= 0) throw new ArgumentException("Invalid user ID", nameof(userId));
            if (id <= 0) throw new ArgumentException("Invalid transaction ID", nameof(id));

            Transaction? transaction = await _dbSet
                .AsNoTracking()
                .Include(transaction => transaction.Category)
                .Where(transaction => transaction.UserId == userId)
                .FirstOrDefaultAsync(transaction => transaction.Id == id);

            return transaction;
        }

        public async Task<IEnumerable<Transaction>> GetAllAsync(int userId)
        {
            if (userId <= 0) throw new ArgumentException("Invalid user ID", nameof(userId));

            IEnumerable<Transaction> transactions = await _dbSet
                .AsNoTracking()
                .Include(transaction => transaction.Category)
                .Where(transaction => transaction.UserId == userId)
                .OrderByDescending(transaction => transaction.Date.Year)
                .ThenByDescending(transaction => transaction.Date.Month)
                .ThenByDescending(transaction => transaction.Date.Day)
                .ToListAsync();

            return transactions;
        }

        public async Task<IEnumerable<Transaction>> GetByCategoryIdAsync(int categoryId, int userId)
        {
            if (userId <= 0) throw new ArgumentException("Invalid user ID", nameof(userId));
            if (categoryId <= 0) throw new ArgumentException("Invalid category ID", nameof(categoryId));

            IEnumerable<Transaction> transactions = await _dbSet
                .AsNoTracking()
                .Include(transaction => transaction.Category)
                .Where(transaction => transaction.CategoryId == categoryId && transaction.UserId == userId)
                .OrderByDescending(transaction => transaction.Date.Year)
                .ThenByDescending(transaction => transaction.Date.Month)
                .ThenByDescending(transaction => transaction.Date.Day)
                .ToListAsync();

            return transactions;
        }

        public async Task<IEnumerable<Transaction>> GetByMonthlyPeriodAsync(MonthlyPeriod period, int userId)
        {
            if (userId <= 0) throw new ArgumentException("Invalid user ID", nameof(userId));
            ArgumentNullException.ThrowIfNull(period);

            IEnumerable<Transaction> transactions = await _dbSet
                .AsNoTracking()
                .Include(transaction => transaction.Category)
                .Where(transaction => transaction.Date.Year == period.Year && transaction.Date.Month == period.Month && transaction.UserId == userId)
                .OrderByDescending(transaction => transaction.Date.Day)
                .ToListAsync();

            return transactions;
        }

        public async Task<IEnumerable<Transaction>> GetByCategoryAndMonthlyPeriodAsync(int categoryId, MonthlyPeriod period, int userId)
        {
            if (userId <= 0) throw new ArgumentException("Invalid user ID", nameof(userId));
            if (categoryId <= 0) throw new ArgumentException("Invalid category ID", nameof(categoryId));
            ArgumentNullException.ThrowIfNull(period);

            IEnumerable<Transaction> transactions = await _dbSet
                .AsNoTracking()
                .Include(transaction => transaction.Category)
                .Where(transaction => transaction.UserId == userId &&
                                      transaction.CategoryId == categoryId &&
                                      transaction.Date.Year == period.Year &&
                                      transaction.Date.Month == period.Month)
                .OrderByDescending(transaction => transaction.Date.Day)
                .ToListAsync();

            return transactions;
        }

        public async Task<IEnumerable<Transaction>> GetByDateRangeAsync(DailyPeriod startDate, DailyPeriod endDate, int userId)
        {
            if (userId <= 0) throw new ArgumentException("Invalid user ID", nameof(userId));
            ArgumentNullException.ThrowIfNull(startDate);
            ArgumentNullException.ThrowIfNull(endDate);

            // Convertir a DateTime para la comparación
            DateTime start = startDate.ToDateTime();
            DateTime end = endDate.ToDateTime();

            IEnumerable<Transaction> transactions = await _dbSet
                .AsNoTracking()
                .Include(transaction => transaction.Category)
                .Where(transaction => transaction.UserId == userId)
                .Where(transaction => transaction.Date.Year > start.Year ||
                                     (transaction.Date.Year == start.Year && transaction.Date.Month > start.Month) ||
                                     (transaction.Date.Year == start.Year && transaction.Date.Month == start.Month && transaction.Date.Day >= start.Day))
                .Where(transaction => transaction.Date.Year < end.Year ||
                                     (transaction.Date.Year == end.Year && transaction.Date.Month < end.Month) ||
                                     (transaction.Date.Year == end.Year && transaction.Date.Month == end.Month && transaction.Date.Day <= end.Day))
                .OrderByDescending(transaction => transaction.Date.Year)
                .ThenByDescending(transaction => transaction.Date.Month)
                .ThenByDescending(transaction => transaction.Date.Day)
                .ToListAsync();

            return transactions;
        }

        public async Task<IEnumerable<Transaction>> GetByCategoryAndDateRangeAsync(int categoryId, DailyPeriod startDate, DailyPeriod endDate, int userId)
        {
            if (userId <= 0) throw new ArgumentException("Invalid user ID", nameof(userId));
            if (categoryId <= 0) throw new ArgumentException("Invalid category ID", nameof(categoryId));
            ArgumentNullException.ThrowIfNull(startDate);
            ArgumentNullException.ThrowIfNull(endDate);

            // Convertir a DateTime para una comparación más precisa
            DateTime start = startDate.ToDateTime();
            DateTime end = endDate.ToDateTime();

            IEnumerable<Transaction> transactions = await _dbSet
                .AsNoTracking()
                .Include(transaction => transaction.Category)
                .Where(transaction => transaction.CategoryId == categoryId && transaction.UserId == userId)
                .Where(transaction => transaction.Date.Year > start.Year ||
                                     (transaction.Date.Year == start.Year && transaction.Date.Month > start.Month) ||
                                     (transaction.Date.Year == start.Year && transaction.Date.Month == start.Month && transaction.Date.Day >= start.Day))
                .Where(transaction => transaction.Date.Year < end.Year ||
                                     (transaction.Date.Year == end.Year && transaction.Date.Month < end.Month) ||
                                     (transaction.Date.Year == end.Year && transaction.Date.Month == end.Month && transaction.Date.Day <= end.Day))
                .OrderByDescending(transaction => transaction.Date.Year)
                .ThenByDescending(transaction => transaction.Date.Month)
                .ThenByDescending(transaction => transaction.Date.Day)
                .ToListAsync();

            return transactions;
        }

        public async Task<decimal> GetTotalByCategoryAndMonthlyPeriodAsync(int categoryId, MonthlyPeriod period, int userId)
        {
            if (userId <= 0) throw new ArgumentException("Invalid user ID", nameof(userId));
            if (categoryId <= 0) throw new ArgumentException("Invalid category ID", nameof(categoryId));
            ArgumentNullException.ThrowIfNull(period);

            decimal total = await _dbSet
                .AsNoTracking()
                .Where(transaction => transaction.UserId == userId &&
                                      transaction.CategoryId == categoryId &&
                                      transaction.Date.Year == period.Year &&
                                      transaction.Date.Month == period.Month)
                .SumAsync(transaction => transaction.Amount.Value);

            return total;
        }

        // ==================== VERIFICACIONES ====================

        public async Task<bool> ExistsAsync(int id, int userId)
        {
            if (userId <= 0) throw new ArgumentException("Invalid user ID", nameof(userId));
            if (id <= 0) return false;

            bool exists = await _dbSet
                .AsNoTracking()
                .Where(transaction => transaction.UserId == userId)
                .AnyAsync(transaction => transaction.Id == id);

            return exists;
        }

        // ==================== COMANDOS ====================

        public async Task AddAsync(Transaction transaction)
        {
            ArgumentNullException.ThrowIfNull(transaction);

            await _dbSet.AddAsync(transaction);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Transaction transaction)
        {
            ArgumentNullException.ThrowIfNull(transaction);

            _dbSet.Update(transaction);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id, int userId)
        {
            if (userId <= 0) throw new ArgumentException("Invalid user ID", nameof(userId));
            if (id <= 0) throw new ArgumentException("Invalid transaction ID", nameof(id));

            Transaction? transaction = await _dbSet.FirstOrDefaultAsync(transaction => transaction.Id == id && transaction.UserId == userId);

            if (transaction == null) throw new KeyNotFoundException($"Transaction with ID {id} not found");

            _dbSet.Remove(transaction);
            await _context.SaveChangesAsync();
        }
    }
}