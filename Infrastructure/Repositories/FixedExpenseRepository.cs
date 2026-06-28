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

        public async Task<FixedExpense> GetByIdAsync(int id)
        {
            if (id <= 0) throw new ArgumentException("Invalid fixed expense ID", nameof(id));

            FixedExpense? fixedExpense = await _dbSet
                .AsNoTracking()
                .Include(f => f.Category)
                .FirstOrDefaultAsync(f => f.Id == id);

            return fixedExpense;
        }

        public async Task<List<FixedExpense>> GetAllAsync()
        {
            List<FixedExpense> fixedExpenses = await _dbSet
                .AsNoTracking()
                .Include(f => f.Category)
                .OrderBy(f => f.Info.Name)
                .ToListAsync();

            return fixedExpenses;
        }

        public async Task<List<FixedExpense>> GetByCategoryAsync(int categoryId)
        {
            if (categoryId <= 0) throw new ArgumentException("Invalid category ID", nameof(categoryId));

            List<FixedExpense> fixedExpenses = await _dbSet
                .AsNoTracking()
                .Include(f => f.Category)
                .Where(f => f.CategoryId == categoryId)
                .OrderBy(f => f.Info.Name)
                .ToListAsync();

            return fixedExpenses;
        }

        public async Task<List<FixedExpense>> GetActiveAsync()
        {
            List<FixedExpense> fixedExpenses = await _dbSet
                .AsNoTracking()
                .Include(f => f.Category)
                .Where(f => f.IsActive == true)
                .OrderBy(f => f.Info.Name)
                .ToListAsync();

            return fixedExpenses;
        }

        public async Task<List<FixedExpense>> GetActiveByCategoryAsync(int categoryId)
        {
            if (categoryId <= 0) throw new ArgumentException("Invalid category ID", nameof(categoryId));

            List<FixedExpense> fixedExpenses = await _dbSet
                .AsNoTracking()
                .Include(f => f.Category)
                .Where(f => f.CategoryId == categoryId && f.IsActive == true)
                .OrderBy(f => f.Info.Name)
                .ToListAsync();

            return fixedExpenses;
        }

        public async Task<List<FixedExpense>> GetActiveForPeriodAsync(Period period)
        {
            ArgumentNullException.ThrowIfNull(period);

            List<FixedExpense> fixedExpenses = await _dbSet
                .AsNoTracking()
                .Include(f => f.Category)
                .Where(f => f.IsActive == true &&
                            (f.ChargePeriod.Year < period.Year ||
                             (f.ChargePeriod.Year == period.Year &&
                              f.ChargePeriod.Month <= period.Month)))
                .OrderBy(f => f.Info.Name)
                .ToListAsync();

            return fixedExpenses;
        }

        public async Task<List<FixedExpense>> GetActiveForPeriodByCategoryAsync(int categoryId, Period period)
        {
            if (categoryId <= 0) throw new ArgumentException("Invalid category ID", nameof(categoryId));

            ArgumentNullException.ThrowIfNull(period);

            List<FixedExpense> fixedExpenses = await _dbSet
                .AsNoTracking()
                .Include(f => f.Category)
                .Where(f => f.CategoryId == categoryId &&
                            f.IsActive == true &&
                            (f.ChargePeriod.Year < period.Year ||
                             (f.ChargePeriod.Year == period.Year &&
                              f.ChargePeriod.Month <= period.Month)))
                .OrderBy(f => f.Info.Name)
                .ToListAsync();

            return fixedExpenses;
        }

        // ==================== MÉTODOS DE AGREGACIÓN ====================

        public async Task<decimal> GetTotalByCategoryAndPeriodAsync(int categoryId, Period period)
        {
            if (categoryId <= 0) throw new ArgumentException("Invalid category ID", nameof(categoryId));

            ArgumentNullException.ThrowIfNull(period);

            decimal total = await _dbSet
                .AsNoTracking()
                .Where(f => f.CategoryId == categoryId &&
                            f.IsActive == true &&
                            (f.ChargePeriod.Year < period.Year ||
                             (f.ChargePeriod.Year == period.Year &&
                              f.ChargePeriod.Month <= period.Month)))
                .SumAsync(f => f.Amount.Value);

            return total;
        }

        public async Task<decimal> GetTotalActiveAsync()
        {
            decimal total = await _dbSet
                .AsNoTracking()
                .Where(f => f.IsActive == true)
                .SumAsync(f => f.Amount.Value);

            return total;
        }

        public async Task<decimal> GetTotalActiveByCategoryAsync(int categoryId)
        {
            if (categoryId <= 0) throw new ArgumentException("Invalid category ID", nameof(categoryId));

            decimal total = await _dbSet
                .AsNoTracking()
                .Where(f => f.CategoryId == categoryId && f.IsActive == true)
                .SumAsync(f => f.Amount.Value);

            return total;
        }

        // ==================== MÉTODOS DE NEGOCIO ====================

        public async Task<bool> ExistsAsync(int id)
        {
            if (id <= 0) return false;

            bool exists = await _dbSet
                .AsNoTracking()
                .AnyAsync(f => f.Id == id);

            return exists;
        }

        public async Task<bool> IsActiveAsync(int id)
        {
            if (id <= 0) throw new ArgumentException("Invalid fixed expense ID", nameof(id));

            bool isActive = await _dbSet
                .AsNoTracking()
                .Where(f => f.Id == id)
                .Select(f => f.IsActive)
                .FirstOrDefaultAsync();

            return isActive;
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

        public async Task DeleteAsync(int id)
        {
            if (id <= 0) throw new ArgumentException("Invalid fixed expense ID", nameof(id));

            FixedExpense? fixedExpense = await _dbSet.FindAsync(id);

            if (fixedExpense == null) throw new KeyNotFoundException($"Fixed expense with ID {id} not found");

            _dbSet.Remove(fixedExpense);
            await _context.SaveChangesAsync();
        }

        public async Task ActivateAsync(int id)
        {
            if (id <= 0) throw new ArgumentException("Invalid fixed expense ID", nameof(id));

            FixedExpense? fixedExpense = await _dbSet.FindAsync(id);

            if (fixedExpense == null) throw new KeyNotFoundException($"Fixed expense with ID {id} not found");

            fixedExpense.Activate();
            await _context.SaveChangesAsync();
        }

        public async Task DeactivateAsync(int id)
        {
            if (id <= 0) throw new ArgumentException("Invalid fixed expense ID", nameof(id));

            FixedExpense? fixedExpense = await _dbSet.FindAsync(id);

            if (fixedExpense == null) throw new KeyNotFoundException($"Fixed expense with ID {id} not found");

            fixedExpense.Deactivate();
            await _context.SaveChangesAsync();
        }
    }
}