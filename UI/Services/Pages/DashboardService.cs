using UI.Extensions;
using UI.Helpers;
using UI.Models;
using UI.Models.Dashboard;
using UI.Services.API;

namespace UI.Services.Pages
{
    public class DashboardService
    {
        private readonly APIService _apiService;

        public DashboardService(APIService apiService)
        {
            _apiService = apiService;
        }

        public async Task<DashboardModel> GetDashboardDataAsync(int year)
        {
            DashboardModel dashboard = new DashboardModel();

            try
            {
                // ✅ Cargar datos desde la API
                List<CategoryModel>? categories = await _apiService.GetCategoriesAsync();
                List<BudgetModel>? budgets = await _apiService.GetBudgetsAsync();
                List<TransactionModel>? transactions = await _apiService.GetTransactionsAsync();

                if (categories == null || !categories.Any())
                {
                    return dashboard;
                }

                // ✅ Inicializar meses
                for (int month = 1; month <= 12; month++)
                {
                    dashboard.Months.Add(new DashboardMonthColumn
                    {
                        Month = month,
                        Name = MonthHelper.GetMonthName(month)
                    });
                }

                // ✅ Procesar cada categoría
                foreach (CategoryModel category in categories)
                {
                    DashboardCategoryRow row = new DashboardCategoryRow
                    {
                        CategoryId = category.Id,
                        CategoryName = category.Name
                    };

                    // Calcular datos por mes
                    for (int month = 1; month <= 12; month++)
                    {
                        decimal budget = budgets?
                            .FirstOrDefault(b => b.CategoryId == category.Id && b.Month == month && b.Year == year)?
                            .Amount ?? 0m;

                        decimal spent = transactions?
                            .Where(t => t.CategoryId == category.Id && t.Date.Month == month && t.Date.Year == year)
                            .Sum(t => t.Amount) ?? 0m;

                        row.MonthlyData[month] = new DashboardMonthData
                        {
                            Month = month,
                            Budget = budget,
                            Spent = spent
                        };

                        row.TotalBudget += budget;
                        row.TotalSpent += spent;
                    }

                    dashboard.Categories.Add(row);
                    dashboard.TotalBudget += row.TotalBudget;
                    dashboard.TotalSpent += row.TotalSpent;
                }

                // ✅ Calcular totales por mes
                foreach (DashboardMonthColumn month in dashboard.Months)
                {
                    foreach (DashboardCategoryRow row in dashboard.Categories)
                    {
                        if (row.MonthlyData.TryGetValue(month.Month, out DashboardMonthData? data))
                        {
                            month.TotalBudget += data.Budget;
                            month.TotalSpent += data.Spent;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // ✅ Relanzar la excepción para que la página la maneje
                throw;
            }

            return dashboard;
        }
    }
}