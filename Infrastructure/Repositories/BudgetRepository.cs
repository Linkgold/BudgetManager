using Infrastructure.Data;
using Domain.Entities;
using Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories
{
    public class BudgetRepository
    {
        /*private readonly ApplicationDbContext _context;

        public BudgetRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Budget?> GetByIdAsync(int id)
        {
            return await _context.Budgets
                .FirstOrDefaultAsync(b => b.Id == id);
        }

        public async Task<List<Budget>> GetAllAsync()
        {
            return await _context.Budgets
                .Include(b => b.Category)
                .ToListAsync();
        }

        public async Task<List<Budget>> GetByPeriodAsync(Period period)
        {
            return await _context.Budgets
                .Where(b => b.Period.Month == period.Month &&
                           b.Period.Year == period.Year)
                .ToListAsync();
        }

        public async Task AddAsync(Budget budget)
        {
            await _context.Budgets.AddAsync(budget);
        }

        public void Update(Budget budget)
        {
            _context.Budgets.Update(budget);
        }

        public void Delete(Budget budget)
        {
            _context.Budgets.Remove(budget);
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }*/
    }
}
