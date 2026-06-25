using Infrastructure.Data;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Domain.Interfaces.Repositories;

namespace Infrastructure.Repositories
{
    public class CategoryRepository : ICategoryRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly DbSet<Category> _dbSet;

        public CategoryRepository(ApplicationDbContext context)
        {
            if (context == null) throw new ArgumentNullException(nameof(context));

            _context = context;
            _dbSet = context.Set<Category>();
        }

        // ==================== CONSULTAS ====================

        public async Task<Category> GetByIdAsync(int id)
        {
            if (id <= 0) throw new ArgumentException("Invalid category ID", nameof(id));

            Category? category = await _dbSet
                .AsNoTracking()
                .FirstOrDefaultAsync(category => category.Id == id);

            return category;
        }

        public async Task<List<Category>> GetAllAsync()
        {
            List<Category> categories = await _dbSet
                .AsNoTracking()
                .OrderBy(category => category.Name)
                .ToListAsync();

            return categories;
        }

        public async Task<Category> GetByNameAsync(string name)
        {
            if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("Category name cannot be empty", nameof(name));

            Category? category = await _dbSet
                .AsNoTracking()
                .FirstOrDefaultAsync(category =>
                    category.Name.ToLower() == name.ToLower());

            return category;
        }

        public async Task<List<Category>> GetActiveCategoriesAsync()
        {
            List<Category> categories = await _dbSet
                .AsNoTracking()
                .Where(category => category.IsActive == true)
                .OrderBy(category => category.Name)
                .ToListAsync();

            return categories;
        }

        // ==================== COMANDOS ====================

        public async Task AddAsync(Category category)
        {
            if (category == null) throw new ArgumentNullException(nameof(category));

            await _dbSet.AddAsync(category);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Category category)
        {
            if (category == null) throw new ArgumentNullException(nameof(category));

            _dbSet.Update(category);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            if (id <= 0) throw new ArgumentException("Invalid category ID", nameof(id));

            Category? category = await _dbSet.FindAsync(id);
            if (category == null) throw new KeyNotFoundException($"Category with ID {id} not found");

            _dbSet.Remove(category);
            await _context.SaveChangesAsync();
        }

        // ==================== VALIDACIONES ====================

        public async Task<bool> ExistsAsync(int id)
        {
            if (id <= 0) return false;

            bool exists = await _dbSet
                .AsNoTracking()
                .AnyAsync(category => category.Id == id);

            return exists;
        }

        public async Task<bool> ExistsByNameAsync(string name)
        {
            if (string.IsNullOrWhiteSpace(name)) return false;

            bool exists = await _dbSet
                .AsNoTracking()
                .AnyAsync(category => category.Name.ToLower() == name.ToLower());

            return exists;
        }

        public async Task<bool> HasExpensesAsync(int categoryId)
        {
            if (categoryId <= 0) throw new ArgumentException("Invalid category ID", nameof(categoryId));

            // Por ahora siempre retorna false porque no tenemos la tabla Expenses aún
            // Cuando se implemente Expenses, se consultará:
            // bool hasExpenses = await _context.Expenses
            //     .AsNoTracking()
            //     .AnyAsync(expense => expense.CategoryId == categoryId);

            // return hasExpenses;

            // Temporal: retornar false (sin gastos asociados)
            return false;
        }
    }
}