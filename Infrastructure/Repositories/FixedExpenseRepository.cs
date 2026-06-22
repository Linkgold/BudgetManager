using Infrastructure.Data;
using Domain.Entities;
using Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories
{
    public class FixedExpenseRepository
    {
        private readonly ApplicationDbContext _context;

        public FixedExpenseRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<FixedExpense?> GetByIdAsync(int id)
        {
            return await _context.FixedExpenses
                .Include(fe => fe.Budget)
                .Include(fe => fe.Category)
                .FirstOrDefaultAsync(fe => fe.Id == id);
        }

        public async Task<List<FixedExpense>> GetAllAsync()
        {
            return await _context.FixedExpenses
                .Include(fe => fe.Budget)
                .Include(fe => fe.Category)
                .ToListAsync();
        }

        public async Task<List<FixedExpense>> GetActiveAsync()
        {
            return await _context.FixedExpenses
                .Where(fe => fe.IsActive)
                .Include(fe => fe.Budget)
                .Include(fe => fe.Category)
                .ToListAsync();
        }

        public async Task<List<FixedExpense>> GetInactiveAsync()
        {
            return await _context.FixedExpenses
                .Where(fe => !fe.IsActive)
                .Include(fe => fe.Budget)
                .Include(fe => fe.Category)
                .ToListAsync();
        }

        public async Task<List<FixedExpense>> GetByBudgetIdAsync(int budgetId)
        {
            return await _context.FixedExpenses
                .Where(fe => fe.Budget.Id == budgetId)
                .Include(fe => fe.Budget)
                .Include(fe => fe.Category)
                .ToListAsync();
        }

        public async Task<List<FixedExpense>> GetByCategoryIdAsync(int categoryId)
        {
            return await _context.FixedExpenses
                .Where(fe => fe.Category.Id == categoryId)
                .Include(fe => fe.Budget)
                .Include(fe => fe.Category)
                .ToListAsync();
        }

        public async Task<List<FixedExpense>> GetByPeriodAsync(Period period)
        {
            return await _context.FixedExpenses
                .Where(fe => fe.ChargeMonth == period.Month &&
                            fe.Year == period.Year)
                .Include(fe => fe.Budget)
                .Include(fe => fe.Category)
                .ToListAsync();
        }

        public async Task<List<FixedExpense>> GetByMonthAsync(int month, int year)
        {
            return await _context.FixedExpenses
                .Where(fe => fe.ChargeMonth == month &&
                            fe.Year == year)
                .Include(fe => fe.Budget)
                .Include(fe => fe.Category)
                .ToListAsync();
        }

        public async Task<decimal> GetTotalByPeriodAsync(Period period)
        {
            return await _context.FixedExpenses
                .Where(fe => fe.ChargeMonth == period.Month &&
                            fe.Year == period.Year &&
                            fe.IsActive)
                .SumAsync(fe => fe.Amount.Value);
        }

        public async Task<decimal> GetTotalByMonthAsync(int month, int year)
        {
            return await _context.FixedExpenses
                .Where(fe => fe.ChargeMonth == month &&
                            fe.Year == year &&
                            fe.IsActive)
                .SumAsync(fe => fe.Amount.Value);
        }

        public async Task AddAsync(FixedExpense fixedExpense)
        {
            await _context.FixedExpenses.AddAsync(fixedExpense);
        }

        public void Update(FixedExpense fixedExpense)
        {
            _context.FixedExpenses.Update(fixedExpense);
        }

        public void Delete(FixedExpense fixedExpense)
        {
            _context.FixedExpenses.Remove(fixedExpense);
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}
