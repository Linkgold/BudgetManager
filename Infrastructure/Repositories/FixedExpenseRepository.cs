using Domain.Entities;
using Domain.Interfaces;
using Domain.ValueObjects;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories
{
    public class FixedExpenseRepository : IFixedExpenseRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly DbSet<FixedExpense> _dbSet;

        public FixedExpenseRepository(ApplicationDbContext context)
        {
            ArgumentNullException.ThrowIfNull(context);

            _context = context;
            _dbSet = context.Set<FixedExpense>();
        }

        // ==================== CONSULTAS ====================

        public async Task<FixedExpense?> GetByIdAsync(int userId, int id)
        {
            if (userId <= 0) throw new ArgumentException("Invalid user ID", nameof(userId));
            if (id <= 0) throw new ArgumentException("Invalid fixed expense ID", nameof(id));

            FixedExpense? fixedExpense = await _dbSet
                .AsNoTracking()
                .Include(fixedExpense => fixedExpense.Category)
                .Where(fixedExpense => fixedExpense.UserId == userId)
                .FirstOrDefaultAsync(fixedExpense => fixedExpense.Id == id);

            return fixedExpense;
        }

        public async Task<IEnumerable<FixedExpense>> GetAllAsync(int userId)
        {
            if (userId <= 0) throw new ArgumentException("Invalid user ID", nameof(userId));

            List<FixedExpense> fixedExpenses = await _dbSet
                .AsNoTracking()
                .Include(fixedExpense => fixedExpense.Category)
                .Where(fixedExpense => fixedExpense.UserId == userId)
                .OrderBy(fixedExpense => fixedExpense.Info.Name)
                .ToListAsync();

            return fixedExpenses;
        }

        public async Task<IEnumerable<FixedExpense>> GetByCategoryAsync(int userId, int categoryId)
        {
            if (userId <= 0) throw new ArgumentException("Invalid user ID", nameof(userId));
            if (categoryId <= 0) throw new ArgumentException("Invalid category ID", nameof(categoryId));

            List<FixedExpense> fixedExpenses = await _dbSet
                .AsNoTracking()
                .Include(fixedExpense => fixedExpense.Category)
                .Where(fixedExpense => fixedExpense.CategoryId == categoryId && fixedExpense.UserId == userId)
                .OrderBy(fixedExpense => fixedExpense.Info.Name)
                .ToListAsync();

            return fixedExpenses;
        }

        public async Task<IEnumerable<FixedExpense>> GetByPeriodAsync(int userId, MonthlyPeriod period)
        {
            if (userId <= 0) throw new ArgumentException("Invalid user ID", nameof(userId));
            ArgumentNullException.ThrowIfNull(period);

            List<FixedExpense> fixedExpenses = await _dbSet
                .AsNoTracking()
                .Include(fixedExpense => fixedExpense.Category)
                .Where(fixedExpense => fixedExpense.UserId == userId &&
                                      (fixedExpense.ChargePeriod.Year < period.Year ||
                                      (fixedExpense.ChargePeriod.Year == period.Year &&
                                       fixedExpense.ChargePeriod.Month <= period.Month)))
                .OrderBy(fixedExpense => fixedExpense.Info.Name)
                .ToListAsync();

            return fixedExpenses;
        }

        public async Task<IEnumerable<FixedExpense>> GetByPeriodByCategoryAsync(int userId, int categoryId, MonthlyPeriod period)
        {
            if (userId <= 0) throw new ArgumentException("Invalid user ID", nameof(userId));
            if (categoryId <= 0) throw new ArgumentException("Invalid category ID", nameof(categoryId));

            ArgumentNullException.ThrowIfNull(period);

            List<FixedExpense> fixedExpenses = await _dbSet
                .AsNoTracking()
                .Include(fixedExpense => fixedExpense.Category)
                .Where(fixedExpense => fixedExpense.UserId == userId &&
                                       fixedExpense.CategoryId == categoryId &&
                                      (fixedExpense.ChargePeriod.Year < period.Year ||
                                      (fixedExpense.ChargePeriod.Year == period.Year &&
                                       fixedExpense.ChargePeriod.Month <= period.Month)))
                .OrderBy(fixedExpense => fixedExpense.Info.Name)
                .ToListAsync();

            return fixedExpenses;
        }

        // ==================== MÉTODOS DE AGREGACIÓN ====================

        public async Task<decimal> GetTotalByCategoryAndPeriodAsync(int userId, int categoryId, MonthlyPeriod period)
        {
            if (userId <= 0) throw new ArgumentException("Invalid user ID", nameof(userId));
            if (categoryId <= 0) throw new ArgumentException("Invalid category ID", nameof(categoryId));

            ArgumentNullException.ThrowIfNull(period);

            decimal total = await _dbSet
                .AsNoTracking()
                .Where(fixedExpense => fixedExpense.UserId == userId &&
                                       fixedExpense.CategoryId == categoryId &&
                                      (fixedExpense.ChargePeriod.Year < period.Year ||
                                      (fixedExpense.ChargePeriod.Year == period.Year &&
                                       fixedExpense.ChargePeriod.Month <= period.Month)))
                .SumAsync(fixedExpense => fixedExpense.Amount.Value);

            return total;
        }

        public async Task<decimal> GetTotalAsync(int userId)
        {
            if (userId <= 0) throw new ArgumentException("Invalid user ID", nameof(userId));

            decimal total = await _dbSet
                .AsNoTracking()
                .Where(fixedExpense => fixedExpense.UserId == userId)
                .SumAsync(fixedExpense => fixedExpense.Amount.Value);

            return total;
        }

        public async Task<decimal> GetTotalByCategoryAsync(int userId, int categoryId)
        {
            if (userId <= 0) throw new ArgumentException("Invalid user ID", nameof(userId));
            if (categoryId <= 0) throw new ArgumentException("Invalid category ID", nameof(categoryId));

            decimal total = await _dbSet
                .AsNoTracking()
                .Where(fixedExpense => fixedExpense.CategoryId == categoryId && fixedExpense.UserId == userId)
                .SumAsync(fixedExpense => fixedExpense.Amount.Value);

            return total;
        }

        // ==================== MÉTODOS DE NEGOCIO ====================

        public async Task<bool> ExistsAsync(int userId, int id)
        {
            if (userId <= 0) throw new ArgumentException("Invalid user ID", nameof(userId));
            if (id <= 0) return false;

            bool exists = await _dbSet
                .AsNoTracking()
                .Where(fixedExpense => fixedExpense.UserId == userId)
                .AnyAsync(fixedExpense => fixedExpense.Id == id);

            return exists;
        }

        // ==================== MÉTODOS DE ESCRITURA ====================

        public async Task AddAsync(FixedExpense fixedExpense)
        {
            ArgumentNullException.ThrowIfNull(fixedExpense);

            await _dbSet.AddAsync(fixedExpense);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(FixedExpense fixedExpense)
        {
            ArgumentNullException.ThrowIfNull(fixedExpense);

            _dbSet.Update(fixedExpense);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int userId, int id)
        {
            if (userId <= 0) throw new ArgumentException("Invalid user ID", nameof(userId));
            if (id <= 0) throw new ArgumentException("Invalid fixed expense ID", nameof(id));

            FixedExpense? fixedExpense = await _dbSet.FirstOrDefaultAsync(fixedExpense => fixedExpense.Id == id && fixedExpense.UserId == userId);

            if (fixedExpense == null) throw new KeyNotFoundException($"Fixed expense with ID {id} not found");

            _dbSet.Remove(fixedExpense);
            await _context.SaveChangesAsync();
        }
    }
}