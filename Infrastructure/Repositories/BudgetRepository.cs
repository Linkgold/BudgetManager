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

        public async Task<Budget?> GetByIdAsync(int id, int userId)
        {
            if (userId <= 0) throw new ArgumentException("Invalid user ID", nameof(userId));
            if (id <= 0) throw new ArgumentException("Invalid budget ID", nameof(id));

            Budget? budget = await _dbSet
                .AsNoTracking()
                .Include(budget => budget.Category)
                .FirstOrDefaultAsync(budget => budget.Id == id && budget.UserId == userId);

            return budget;
        }

        public async Task<IEnumerable<Budget>> GetAllAsync(int userId)
        {
            if (userId <= 0) throw new ArgumentException("Invalid user ID", nameof(userId));

            IEnumerable<Budget> budgets = await _dbSet
                .AsNoTracking()
                .Include(budget => budget.Category)
                .Where(budget => budget.UserId == userId)
                .ToListAsync();

            return budgets
                .OrderBy(budget => budget.Period.Year)
                .ThenBy(budget => budget.Period.Month)
                .ThenBy(budget => budget.Category.Info.Name);
        }

        public async Task<IEnumerable<Budget>> GetByCategoryIdAsync(int categoryId, int userId)
        {
            if (userId <= 0) throw new ArgumentException("Invalid user ID", nameof(userId));
            if (categoryId <= 0) throw new ArgumentException("Invalid category ID", nameof(categoryId));

            IEnumerable<Budget> budgets = await _dbSet
                .AsNoTracking()
                .Include(budget => budget.Category)
                .Where(budget => budget.CategoryId == categoryId && budget.UserId == userId)

                .ToListAsync();

            return budgets
                .OrderBy(budget => budget.Period.Year)
                .ThenBy(budget => budget.Period.Month);
        }

        public async Task<Budget?> GetByCategoryAndPeriodAsync(int categoryId, MonthlyPeriod period, int userId)
        {
            if (userId <= 0) throw new ArgumentException("Invalid user ID", nameof(userId));
            if (categoryId <= 0) throw new ArgumentException("Invalid category ID", nameof(categoryId));

            ArgumentNullException.ThrowIfNull(period);

            Budget? budget = await _dbSet
                .AsNoTracking()
                .Include(budget => budget.Category)
                .Where(budget => budget.UserId == userId)
                .FirstOrDefaultAsync(budget => budget.CategoryId == categoryId &&
                                               budget.Period.Year == period.Year &&
                                               budget.Period.Month == period.Month);

            return budget;
        }

        public async Task<IEnumerable<Budget>> GetByPeriodAsync(MonthlyPeriod period, int userId)
        {
            if (userId <= 0) throw new ArgumentException("Invalid user ID", nameof(userId));
            ArgumentNullException.ThrowIfNull(period);

            IEnumerable<Budget> budgets = await _dbSet
                .AsNoTracking()
                .Include(budget => budget.Category)
                .Where(budget => budget.UserId == userId &&
                                 budget.Period.Year == period.Year &&
                                 budget.Period.Month == period.Month)
                .ToListAsync();

            return budgets.OrderBy(budget => budget.Category.Info.Name);
        }

        // ==================== VERIFICACIONES ====================

        public async Task<bool> ExistsAsync(int id, int userId)
        {
            if (userId <= 0) throw new ArgumentException("Invalid user ID", nameof(userId));
            if (id <= 0) return false;

            bool exists = await _dbSet
                .AsNoTracking()
                .Where(budget => budget.UserId == userId)
                .AnyAsync(budget => budget.Id == id);

            return exists;
        }

        public async Task<bool> ExistsForCategoryAndPeriodAsync(int categoryId, MonthlyPeriod period, int userId)
        {
            if (userId <= 0) throw new ArgumentException("Invalid user ID", nameof(userId));
            if (categoryId <= 0) return false;

            ArgumentNullException.ThrowIfNull(period);

            bool exists = await _dbSet
                .AsNoTracking()
                .Where(budget => budget.UserId == userId)
                .AnyAsync(budget => budget.CategoryId == categoryId &&
                                    budget.Period.Year == period.Year &&
                                    budget.Period.Month == period.Month);

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

        public async Task DeleteAsync(int id, int userId)
        {
            if (userId <= 0) throw new ArgumentException("Invalid user ID", nameof(userId));
            if (id <= 0) throw new ArgumentException("Invalid budget ID", nameof(id));

            Budget? budget = await _dbSet.FirstOrDefaultAsync(budget => budget.Id == id && budget.UserId == userId);

            if (budget == null) throw new KeyNotFoundException($"Budget with ID {id} not found");

            _dbSet.Remove(budget);
            await _context.SaveChangesAsync();
        }
    }
}