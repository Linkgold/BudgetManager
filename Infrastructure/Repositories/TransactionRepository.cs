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

        public async Task<Transaction> GetByIdAsync(int id)
        {
            if (id <= 0) throw new ArgumentException("Invalid transaction ID", nameof(id));

            Transaction? transaction = await _dbSet
                .AsNoTracking()
                .Include(t => t.Category)
                .FirstOrDefaultAsync(t => t.Id == id);

            return transaction;
        }

        public async Task<IEnumerable<Transaction>> GetAllAsync()
        {
            IEnumerable<Transaction> transactions = await _dbSet
                .AsNoTracking()
                .Include(t => t.Category)
                .OrderByDescending(t => t.Date.Year)
                .ThenByDescending(t => t.Date.Month)
                .ThenByDescending(t => t.Date.Day)
                .ToListAsync();

            return transactions;
        }

        public async Task<IEnumerable<Transaction>> GetByCategoryIdAsync(int categoryId)
        {
            if (categoryId <= 0) throw new ArgumentException("Invalid category ID", nameof(categoryId));

            IEnumerable<Transaction> transactions = await _dbSet
                .AsNoTracking()
                .Include(t => t.Category)
                .Where(t => t.CategoryId == categoryId)
                .OrderByDescending(t => t.Date.Year)
                .ThenByDescending(t => t.Date.Month)
                .ThenByDescending(t => t.Date.Day)
                .ToListAsync();

            return transactions;
        }

        public async Task<IEnumerable<Transaction>> GetByMonthlyPeriodAsync(MonthlyPeriod period)
        {
            ArgumentNullException.ThrowIfNull(period);

            IEnumerable<Transaction> transactions = await _dbSet
                .AsNoTracking()
                .Include(t => t.Category)
                .Where(t => t.Date.Year == period.Year && t.Date.Month == period.Month)
                .OrderByDescending(t => t.Date.Day)
                .ToListAsync();

            return transactions;
        }

        public async Task<IEnumerable<Transaction>> GetByCategoryAndMonthlyPeriodAsync(int categoryId, MonthlyPeriod period)
        {
            if (categoryId <= 0) throw new ArgumentException("Invalid category ID", nameof(categoryId));

            ArgumentNullException.ThrowIfNull(period);

            IEnumerable<Transaction> transactions = await _dbSet
                .AsNoTracking()
                .Include(t => t.Category)
                .Where(t => t.CategoryId == categoryId &&
                            t.Date.Year == period.Year &&
                            t.Date.Month == period.Month)
                .OrderByDescending(t => t.Date.Day)
                .ToListAsync();

            return transactions;
        }

        public async Task<IEnumerable<Transaction>> GetByDateRangeAsync(DailyPeriod startDate, DailyPeriod endDate)
        {
            ArgumentNullException.ThrowIfNull(startDate);
            ArgumentNullException.ThrowIfNull(endDate);

            // Convertir a DateTime para la comparación
            DateTime start = startDate.ToDateTime();
            DateTime end = endDate.ToDateTime();

            IEnumerable<Transaction> transactions = await _dbSet
                .AsNoTracking()
                .Include(t => t.Category)
                .Where(t => t.Date.Year > start.Year ||
                           (t.Date.Year == start.Year && t.Date.Month > start.Month) ||
                           (t.Date.Year == start.Year && t.Date.Month == start.Month && t.Date.Day >= start.Day))
                .Where(t => t.Date.Year < end.Year ||
                           (t.Date.Year == end.Year && t.Date.Month < end.Month) ||
                           (t.Date.Year == end.Year && t.Date.Month == end.Month && t.Date.Day <= end.Day))
                .OrderByDescending(t => t.Date.Year)
                .ThenByDescending(t => t.Date.Month)
                .ThenByDescending(t => t.Date.Day)
                .ToListAsync();

            return transactions;
        }

        public async Task<IEnumerable<Transaction>> GetByCategoryAndDateRangeAsync(int categoryId, DailyPeriod startDate, DailyPeriod endDate)
        {
            if (categoryId <= 0)                throw new ArgumentException("Invalid category ID", nameof(categoryId));

            ArgumentNullException.ThrowIfNull(startDate);
            ArgumentNullException.ThrowIfNull(endDate);

            // Convertir a DateTime para una comparación más precisa
            DateTime start = startDate.ToDateTime();
            DateTime end = endDate.ToDateTime();

            IEnumerable<Transaction> transactions = await _dbSet
                .AsNoTracking()
                .Include(t => t.Category)
                .Where(t => t.CategoryId == categoryId)
                .Where(t => t.Date.Year > start.Year ||
                            (t.Date.Year == start.Year && t.Date.Month > start.Month) ||
                            (t.Date.Year == start.Year && t.Date.Month == start.Month && t.Date.Day >= start.Day))
                .Where(t => t.Date.Year < end.Year ||
                            (t.Date.Year == end.Year && t.Date.Month < end.Month) ||
                            (t.Date.Year == end.Year && t.Date.Month == end.Month && t.Date.Day <= end.Day))
                .OrderByDescending(t => t.Date.Year)
                .ThenByDescending(t => t.Date.Month)
                .ThenByDescending(t => t.Date.Day)
                .ToListAsync();

            return transactions;
        }

        public async Task<decimal> GetTotalByCategoryAndMonthlyPeriodAsync(int categoryId, MonthlyPeriod period)
        {
            if (categoryId <= 0) throw new ArgumentException("Invalid category ID", nameof(categoryId));

            ArgumentNullException.ThrowIfNull(period);

            decimal total = await _dbSet
                .AsNoTracking()
                .Where(t => t.CategoryId == categoryId &&
                            t.Date.Year == period.Year &&
                            t.Date.Month == period.Month)
                .SumAsync(t => t.Amount.Value);

            return total;
        }

        // ==================== VERIFICACIONES ====================

        public async Task<bool> ExistsAsync(int id)
        {
            if (id <= 0) return false;

            bool exists = await _dbSet
                .AsNoTracking()
                .AnyAsync(t => t.Id == id);

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

        public async Task DeleteAsync(int id)
        {
            if (id <= 0) throw new ArgumentException("Invalid transaction ID", nameof(id));

            Transaction? transaction = await _dbSet.FindAsync(id);

            if (transaction == null) throw new KeyNotFoundException($"Transaction with ID {id} not found");

            _dbSet.Remove(transaction);
            await _context.SaveChangesAsync();
        }
    }
}