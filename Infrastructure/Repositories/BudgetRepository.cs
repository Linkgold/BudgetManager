using Domain.Entities;
using Domain.Interfaces;
using Domain.ValueObjects;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories
{
    /// <summary>
    /// Implementación del repositorio de presupuestos
    /// </summary>
    public class BudgetRepository : IBudgetRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly DbSet<Budget> _dbSet;

        public BudgetRepository(ApplicationDbContext context)
        {
            ArgumentNullException.ThrowIfNull(context);

            _context = context;
            _dbSet = context.Set<Budget>();
        }

        // ==================== CONSULTAS ====================

        public async Task<Budget?> GetByIdAsync(int id)
        {
            if (id <= 0) throw new ArgumentException("Invalid budget ID", nameof(id));

            Budget? budget = await _dbSet
                .AsNoTracking()
                .Include(b => b.Category)
                .FirstOrDefaultAsync(b => b.Id == id);

            return budget;
        }

        public async Task<IEnumerable<Budget>> GetAllAsync()
        {
            IEnumerable<Budget> budgets = await _dbSet
                .AsNoTracking()
                .Include(b => b.Category)
                .ToListAsync();

            return budgets
                .OrderBy(b => b.Period.Year)
                .ThenBy(b => b.Period.Month)
                .ThenBy(b => b.Category.Info.Name);
        }

        public async Task<IEnumerable<Budget>> GetByCategoryIdAsync(int categoryId)
        {
            if (categoryId <= 0) throw new ArgumentException("Invalid category ID", nameof(categoryId));

            IEnumerable<Budget> budgets = await _dbSet
                .AsNoTracking()
                .Include(b => b.Category)
                .Where(b => b.CategoryId == categoryId)
                
                .ToListAsync();

            return budgets
                .OrderBy(b => b.Period.Year)
                .ThenBy(b => b.Period.Month);
        }

        public async Task<Budget?> GetByCategoryAndPeriodAsync(int categoryId, Period period)
        {
            if (categoryId <= 0) throw new ArgumentException("Invalid category ID", nameof(categoryId));

            ArgumentNullException.ThrowIfNull(period);

            Budget? budget = await _dbSet
                .AsNoTracking()
                .Include(b => b.Category)
                .FirstOrDefaultAsync(b => b.CategoryId == categoryId &&
                                          b.Period.Year == period.Year &&
                                          b.Period.Month == period.Month);

            return budget;
        }

        public async Task<IEnumerable<Budget>> GetByPeriodAsync(Period period)
        {
            ArgumentNullException.ThrowIfNull(period);

            IEnumerable<Budget> budgets = await _dbSet
                .AsNoTracking()
                .Include(b => b.Category)
                .Where(b => b.Period.Year == period.Year &&
                            b.Period.Month == period.Month)
                .ToListAsync();

            return budgets.OrderBy(b => b.Category.Info.Name);
        }

        // ==================== VERIFICACIONES ====================

        public async Task<bool> ExistsAsync(int id)
        {
            if (id <= 0) return false;

            bool exists = await _dbSet
                .AsNoTracking()
                .AnyAsync(b => b.Id == id);

            return exists;
        }

        public async Task<bool> ExistsForCategoryAndPeriodAsync(int categoryId, Period period)
        {
            if (categoryId <= 0) return false;

            ArgumentNullException.ThrowIfNull(period);

            bool exists = await _dbSet
                .AsNoTracking()
                .AnyAsync(b => b.CategoryId == categoryId &&
                               b.Period.Year == period.Year &&
                               b.Period.Month == period.Month);

            return exists;
        }

        // ==================== COMANDOS ====================

        public async Task AddAsync(Budget budget)
        {
            ArgumentNullException.ThrowIfNull(budget);

            await _dbSet.AddAsync(budget);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Budget budget)
        {
            ArgumentNullException.ThrowIfNull(budget);

            _dbSet.Update(budget);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            if (id <= 0) throw new ArgumentException("Invalid budget ID", nameof(id));

            Budget? budget = await _dbSet.FindAsync(id);

            if (budget == null) throw new KeyNotFoundException($"Budget with ID {id} not found");

            _dbSet.Remove(budget);
            await _context.SaveChangesAsync();
        }
    }
}