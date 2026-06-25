using Infrastructure.Data;
using Domain.Entities;
using Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Domain.Interfaces;

namespace Infrastructure.Repositories
{
    public class ExpenseRepository : IExpenseRepository
    {
        private readonly ApplicationDbContext _context;

        public ExpenseRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Expense?> GetByIdAsync(int id)
        {
            return await _context.Expenses
                .Include(e => e.Budget)
                .FirstOrDefaultAsync(e => e.Id == id);
        }

        public async Task<List<Expense>> GetAllAsync()
        {
            return await _context.Expenses
                .Include(e => e.Budget)
                .ToListAsync();
        }

        public async Task<List<Expense>> GetByBudgetIdAsync(int budgetId)
        {
            return await _context.Expenses
                .Where(e => e.BudgetId == budgetId)
                .Include(e => e.Budget)
                .ToListAsync();
        }

        public async Task<List<Expense>> GetByPeriodAsync(Period period)
        {
            return await _context.Expenses
                .Where(e => e.DateTime.Month == period.Month &&
                           e.DateTime.Year == period.Year)
                .Include(e => e.Budget)
                .ToListAsync();
        }

        public async Task<List<Expense>> GetByPeriodAndBudgetAsync(Period period, int budgetId)
        {
            return await _context.Expenses
                .Where(e => e.BudgetId == budgetId &&
                           e.DateTime.Month == period.Month &&
                           e.DateTime.Year == period.Year)
                .Include(e => e.Budget)
                .ToListAsync();
        }

        public async Task<List<Expense>> GetByCategoryAsync(string category)
        {
            return await _context.Expenses
                .Where(e => e.Category == category)
                .Include(e => e.Budget)
                .ToListAsync();
        }

        public async Task<decimal> GetTotalByBudgetAsync(int budgetId)
        {
            return await _context.Expenses
                .Where(e => e.BudgetId == budgetId)
                .SumAsync(e => e.Amount.Value);
        }

        public async Task<decimal> GetTotalByPeriodAsync(Period period)
        {
            return await _context.Expenses
                .Where(e => e.DateTime.Month == period.Month &&
                           e.DateTime.Year == period.Year)
                .SumAsync(e => e.Amount.Value);
        }

        public async Task AddAsync(Expense expense)
        {
            await _context.Expenses.AddAsync(expense);
        }

        public void Update(Expense expense)
        {
            _context.Expenses.Update(expense);
        }

        public void Delete(Expense expense)
        {
            _context.Expenses.Remove(expense);
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}
