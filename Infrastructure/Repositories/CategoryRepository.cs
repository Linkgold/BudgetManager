using Infrastructure.Data;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Domain.Interfaces;

namespace Infrastructure.Repositories
{
    public class CategoryRepository : ICategoryRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly DbSet<Category> _dbSet;

        public CategoryRepository(ApplicationDbContext context)
        {
            ArgumentNullException.ThrowIfNull(context);

            _context = context;
            _dbSet = context.Set<Category>();
        }

        // ==================== CONSULTAS ====================

        public async Task<Category?> GetByIdAsync(int userId, int id, bool withTracking = false)
        {
            if (userId <= 0) throw new ArgumentException("Invalid user ID", nameof(userId));
            if (id <= 0) throw new ArgumentException("Invalid category ID", nameof(id));

            IQueryable<Category> query = _dbSet;

            // 🔥 Solo aplicar AsNoTracking() si NO se pide tracking
            if (!withTracking)
            {
                query = query.AsNoTracking();
            }

            Category? category = await query.FirstOrDefaultAsync(category => category.Id == id && category.UserId == userId);

            return category;
        }

        public async Task<IEnumerable<Category>> GetAllAsync(int userId)
        {
            if (userId <= 0) throw new ArgumentException("Invalid user ID", nameof(userId));

            List<Category> categories = await _dbSet
                .AsNoTracking()
                .Where(category => category.UserId == userId)
                .OrderBy(category => category.Info.Name)
                .ToListAsync();

            return categories;
        }

        public async Task<Category?> GetByNameAsync(int userId, string name)
        {
            if (userId <= 0) throw new ArgumentException("Invalid user ID", nameof(userId));
            ArgumentException.ThrowIfNullOrWhiteSpace(name);

            Category? category = await _dbSet
                .AsNoTracking()
                .Where(category => category.UserId == userId)
                .FirstOrDefaultAsync(category => category.Info.Name.ToLower() == name.ToLower());

            return category;
        }

        public async Task<IEnumerable<Category>> GetActiveCategoriesAsync(int userId)
        {
            if (userId <= 0) throw new ArgumentException("Invalid user ID", nameof(userId));

            List<Category> categories = await _dbSet
                .AsNoTracking()
                .Where(category => category.IsActive == true && category.UserId == userId)
                .OrderBy(category => category.Info.Name)
                .ToListAsync();

            return categories;
        }

        // ==================== COMANDOS ====================

        public async Task AddAsync(Category category)
        {
            ArgumentNullException.ThrowIfNull(category);

            await _dbSet.AddAsync(category);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Category category)
        {
            ArgumentNullException.ThrowIfNull(category);

            _dbSet.Update(category);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int userId, int id)
        {
            if (userId <= 0) throw new ArgumentException("Invalid user ID", nameof(userId));
            if (id <= 0) throw new ArgumentException("Invalid category ID", nameof(id));

            Category? category = await _dbSet.FirstOrDefaultAsync(category => category.Id == id && category.UserId == userId);
            if (category == null) throw new KeyNotFoundException($"Category with ID {id} not found");

            _dbSet.Remove(category);
            await _context.SaveChangesAsync();
        }

        // ==================== VALIDACIONES ====================

        public async Task<bool> ExistsAsync(int userId, int id)
        {
            if (userId <= 0) throw new ArgumentException("Invalid user ID", nameof(userId));
            if (id <= 0) return false;

            bool exists = await _dbSet
                .AsNoTracking()
                .AnyAsync(category => category.Id == id && category.UserId == userId);

            return exists;
        }

        public async Task<bool> ExistsByNameAsync(int userId, string name)
        {
            if (userId <= 0) throw new ArgumentException("Invalid user ID", nameof(userId));
            if (string.IsNullOrWhiteSpace(name)) return false;

            bool exists = await _dbSet
                .AsNoTracking()
                .Where(category => category.UserId == userId)
                .AnyAsync(category => category.Info.Name.ToLower() == name.ToLower());

            return exists;
        }

        public async Task<bool> HasDependenciesAsync(int userId, int categoryId)
        {
            if (userId <= 0) throw new ArgumentException("Invalid user ID", nameof(userId));
            if (categoryId <= 0) throw new ArgumentException("Invalid category ID", nameof(categoryId));

            // Verificar en FixedExpenses
            bool hasFixedExpenses = await _context.FixedExpenses
                .AsNoTracking()
                .AnyAsync(f => f.CategoryId == categoryId && f.UserId == userId);

            if (hasFixedExpenses) return true;

            // 🔥 Verificar en Budgets
            bool hasBudgets = await _context.Budgets
                .AsNoTracking()
                .AnyAsync(b => b.CategoryId == categoryId && b.UserId == userId);

            if (hasBudgets) return true;

            // 🔥 Verificar en Transactions
            bool hasTransactions = await _context.Transactions
                .AsNoTracking()
                .AnyAsync(t => t.CategoryId == categoryId && t.UserId == userId);

            if (hasTransactions) return true;

            return false;
        }
    }
}