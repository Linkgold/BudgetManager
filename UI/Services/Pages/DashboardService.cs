using Contracts.Enums;
using UI.Extensions;
using UI.Helpers;
using UI.Models;
using UI.Models.Dashboard;
using UI.Pages;
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

            List<CategoryModel>? categories = await _apiService.GetCategoriesAsync();
            List<BudgetModel>? budgets = await _apiService.GetBudgetsAsync();
            List<TransactionModel>? transactions = await _apiService.GetTransactionsAsync();
            List<FixedExpenseModel>? fixedExpenses = await _apiService.GetFixedExpensesAsync();  // ✅ NUEVO

            if (categories == null || !categories.Any())
            {
                return dashboard;
            }

            // Inicializar meses
            for (int month = 1; month <= 12; month++)
            {
                dashboard.Months.Add(new DashboardMonthColumn
                {
                    Month = month,
                    Name = MonthHelper.GetMonthName(month)
                });
            }

            // Procesar cada categoría
            foreach (CategoryModel category in categories)
            {
                DashboardCategoryRow row = new DashboardCategoryRow
                {
                    CategoryId = category.Id,
                    CategoryName = category.Name,
                    Nature = category.Nature
                };

                // Calcular datos por mes
                for (int month = 1; month <= 12; month++)
                {
                    // 1. Presupuesto
                    decimal budget = budgets?
                        .FirstOrDefault(b => b.CategoryId == category.Id && b.Month == month && b.Year == year)?
                        .Amount ?? 0m;

                    // 2. Gasto fijo (si existe en ese mes)
                    decimal fixedExpense = fixedExpenses?
                        .Where(f => f.CategoryId == category.Id && f.Month == month && f.Year == year)
                        .Sum(f => f.Amount) ?? 0m;

                    // 3. Transacciones
                    decimal spent = transactions?
                        .Where(t => t.CategoryId == category.Id && t.Date.Month == month && t.Date.Year == year)
                        .Sum(f => f.Amount) ?? 0m;

                    decimal totalSpent = spent;
                    decimal totalBudget = budget + fixedExpense;

                    row.MonthlyData[month] = new DashboardMonthData
                    {
                        Month = month,
                        Budget = totalBudget,
                        Spent = totalSpent,
                    };

                    row.TotalBudget += totalBudget;
                    row.TotalSpent += totalSpent;
                }

                dashboard.Categories.Add(row);
                /*dashboard.TotalBudget += row.TotalBudget;
                dashboard.TotalSpent += row.TotalSpent;*/
            }

            // Calcular totales por mes
            CalculateMonthTotals(dashboard);

            dashboard.AccumulatedMonths = CalculateAccumulatedTotals(dashboard);

            return dashboard;
        }

        private void CalculateMonthTotals(DashboardModel dashboard)
        {
            foreach (DashboardMonthColumn month in dashboard.Months)
            {
                decimal monthBudgetWithSign = 0m;
                decimal monthSpentWithSign = 0m;

                foreach (DashboardCategoryRow row in dashboard.Categories)
                {
                    if (row.MonthlyData.TryGetValue(month.Month, out DashboardMonthData? data))
                    {
                        if (row.Nature == CategoryNatureEnum.Income)
                        {
                            monthBudgetWithSign += data.Budget;
                            monthSpentWithSign += data.Spent;
                        }
                        else
                        {
                            monthBudgetWithSign -= data.Budget;
                            monthSpentWithSign -= data.Spent;
                        }
                    }
                }

                month.TotalBudget = monthBudgetWithSign;
                month.TotalSpent = monthSpentWithSign;
            }
        }

        private List<DashboardAccumulatedMonth> CalculateAccumulatedTotals(DashboardModel dashboard)
        {
            List<DashboardAccumulatedMonth> accumulatedTotals = new List<DashboardAccumulatedMonth>();

            decimal acumuladoBudget = 0m;
            decimal acumuladoSpent = 0m;

            foreach (DashboardMonthColumn month in dashboard.Months)
            {
                decimal monthBudgetWithSign = 0m;
                decimal monthSpentWithSign = 0m;

                foreach (DashboardCategoryRow row in dashboard.Categories)
                {
                    if (row.MonthlyData.TryGetValue(month.Month, out DashboardMonthData? data))
                    {
                        if (row.Nature == CategoryNatureEnum.Income)
                        {
                            monthBudgetWithSign += data.Budget;
                            monthSpentWithSign += data.Spent;
                        }
                        else
                        {
                            monthBudgetWithSign -= data.Budget;
                            monthSpentWithSign -= data.Spent;
                        }
                    }
                }

                acumuladoBudget += monthBudgetWithSign;
                acumuladoSpent += monthSpentWithSign;

                accumulatedTotals.Add(new DashboardAccumulatedMonth
                {
                    Month = month.Month,
                    AcumuladoBudget = acumuladoBudget,
                    AcumuladoSpent = acumuladoSpent,
                });
            }

            return accumulatedTotals;
        }
    }
}