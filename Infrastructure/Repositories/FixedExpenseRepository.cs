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

        public async Task<IEnumerable<FixedExpense>> GetAllByYearAsync(int userId, int year)
        {
            if (userId <= 0) throw new ArgumentException("Invalid user ID", nameof(userId));
            if (year < 1900 || year > 2100) throw new ArgumentException("Year must be between 1900 and 2100", nameof(year));

            List<FixedExpense> fixedExpenses = await _dbSet
                .AsNoTracking()
                .Include(fixedExpense => fixedExpense.Category)
                .Where(fixedExpense => fixedExpense.UserId == userId && fixedExpense.ChargePeriod.Year == year)
                .OrderBy(fixedExpense => fixedExpense.Info.Name)
                .ToListAsync();

            return fixedExpenses;
        }

        public async Task<IEnumerable<int>> GetDistinctYearsAsync(int userId)
        {
            if (userId <= 0) throw new ArgumentException("Invalid user ID", nameof(userId));

            List<int> years = await _dbSet
                .AsNoTracking()
                .Where(f => f.UserId == userId)
                .Select(f => f.ChargePeriod.Year)
                .Distinct()
                .Order()
                .ToListAsync();

            return years;
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

        public async Task<bool> ExistsByCategoryNameMonthYearAsync(int userId, int categoryId, string name, int month, int year, int? excludeId = null)
        {
            if (userId <= 0) throw new ArgumentException("Invalid user ID", nameof(userId));
            if (categoryId <= 0) throw new ArgumentException("Invalid category ID", nameof(categoryId));
            if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("Name cannot be empty", nameof(name));

            IQueryable<FixedExpense> query = _dbSet
                .AsNoTracking()
                .Where(f => f.UserId == userId &&
                            f.CategoryId == categoryId &&
                            f.Info.Name == name &&
                            f.ChargePeriod.Month == month &&
                            f.ChargePeriod.Year == year);

            // ✅ Excluir el ID actual para evitar que se detecte a sí mismo en caso de update
            if (excludeId.HasValue)
            {
                query = query.Where(f => f.Id != excludeId.Value);
            }

            return await query.AnyAsync();
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