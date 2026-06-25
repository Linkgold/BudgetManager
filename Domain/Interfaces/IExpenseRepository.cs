using Domain.Entities;
using Domain.ValueObjects;

namespace Domain.Interfaces
{
    public interface IExpenseRepository
    {
        // CRUD Básico
        Task<Expense?> GetByIdAsync(int id);
        Task<List<Expense>> GetAllAsync();
        //Task<List<Expense>> GetByCategoryAsync(int categoryId);
        Task<List<Expense>> GetByPeriodAsync(Period period);
        //Task<List<Expense>> GetByCategoryAndPeriodAsync(int categoryId, Period period);

        // Métodos de agregación (sumas, conteos)
        //Task<decimal> GetTotalByCategoryAndPeriodAsync(int categoryId, Period period);
        Task<decimal> GetTotalByPeriodAsync(Period period);
        //Task<int> GetCountByCategoryAndPeriodAsync(int categoryId, Period period);

        // Métodos con paginación y ordenamiento
        //Task<IEnumerable<Expense>> GetByCategoryAndPeriodAsync(int categoryId, Period period, int take, int skip = 0, string orderBy = "Date DESC");

        //Task<IEnumerable<Expense>> GetRecentExpensesAsync(int categoryId, Period period, int top = 10);

        // Métodos de búsqueda
        //Task<IEnumerable<Expense>> SearchAsync(string description, int? categoryId = null, Period? period = null, decimal? minAmount = null, decimal? maxAmount = null);

        // Métodos de negocio
        //Task<bool> ExistsAsync(int id);
        //Task<decimal> GetAverageMonthlyExpenseAsync(int categoryId, int lastMonths = 6);

        // Métodos de escritura
        Task AddAsync(Expense expense);
        void Update(Expense expense);
        void Delete(Expense expense);

        // Métodos batch (para operaciones masivas)
        //Task AddRangeAsync(IEnumerable<Expense> expenses);
        //Task<IEnumerable<Expense>> GetExpensesBetweenDatesAsync(int categoryId, DateTime startDate, DateTime endDate);
    }
}