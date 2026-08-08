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

        public async Task<Transaction?> GetByIdAsync(int userId, int id)
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

        public async Task<IEnumerable<Transaction>> GetAllByYearAsync(int userId, int year)
        {
            if (userId <= 0) throw new ArgumentException("Invalid user ID", nameof(userId));
            if (year < 1900 || year > 2100) throw new ArgumentException("Year must be between 1900 and 2100", nameof(year));

            IEnumerable<Transaction> transactions = await _dbSet
                .AsNoTracking()
                .Include(transaction => transaction.Category)
                .Where(transaction => transaction.UserId == userId && transaction.Date.Year == year)
                .OrderByDescending(transaction => transaction.Date.Month)
                .ThenByDescending(transaction => transaction.Date.Day)
                .ToListAsync();

            return transactions;
        }

        // ==================== VERIFICACIONES ====================

        public async Task<bool> ExistsAsync(int userId, int id)
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

        public async Task DeleteAsync(int userId, int id)
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