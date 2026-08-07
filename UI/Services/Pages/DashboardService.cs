using Contracts.Enums;
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
                    // 1. Presupuesto (solo para Expense y Mixed)
                    decimal budget = 0m;
                    if (category.Nature != CategoryNatureEnum.Income)
                    {
                        budget = budgets?
                            .FirstOrDefault(b => b.CategoryId == category.Id && b.Month == month && b.Year == year)?
                            .Amount ?? 0m;
                    }


                    // 2. Gasto fijo (si existe en ese mes, solo para Expense y Mixed)
                    decimal fixedExpense = 0m;
                    if (category.Nature != CategoryNatureEnum.Income)
                    {
                        fixedExpense = fixedExpenses?
                            .Where(f => f.CategoryId == category.Id && f.Month == month && f.Year == year)
                            .Sum(f => f.Amount) ?? 0m;
                    }

                    // 3. Transacciones (gastos reales)
                    decimal spent = transactions?
                        .Where(t => t.CategoryId == category.Id && t.Date.Month == month && t.Date.Year == year)
                        .Sum(t => {
                            // ✅ Ingresos: suman positivos
                            // ✅ Gastos (Expense y Mixed): suman negativos (restan)
                            if (t.CategoryNature == CategoryNatureEnum.Income)
                            {
                                return t.Amount;  // Ya es positivo (suma)
                            }
                            else if (t.CategoryNature == CategoryNatureEnum.Expense || t.CategoryNature == CategoryNatureEnum.Mixed)
                            {
                                return -Math.Abs(t.Amount);  // Restamos (negativo)
                            }
                            else
                            {
                                return 0m;
                            }
                        }) ?? 0m;

                    // ✅ Total gasto real = transacciones
                    decimal totalSpent = spent;

                    // ✅ Presupuesto + gasto fijo (para mostrar como presupuesto combinado)
                    decimal totalBudget = budget + fixedExpense;

                    // ✅ Valor sin signo para UI
                    decimal displaySpent = Math.Abs(totalSpent);

                    row.MonthlyData[month] = new DashboardMonthData
                    {
                        Month = month,
                        Budget = totalBudget,        // Presupuesto + gasto fijo
                        Spent = totalSpent,          // Gasto real total
                        DisplaySpent = displaySpent  // Sin signo para UI
                    };

                    row.TotalBudget += totalBudget;
                    row.TotalSpent += totalSpent;
                    row.TotalDisplaySpent += displaySpent;
                }

                dashboard.Categories.Add(row);
                dashboard.TotalBudget += row.TotalBudget;
                dashboard.TotalSpent += row.TotalSpent;
                dashboard.TotalDisplaySpent += row.TotalDisplaySpent;
            }

            // Calcular totales por mes
            foreach (DashboardMonthColumn month in dashboard.Months)
            {
                foreach (DashboardCategoryRow row in dashboard.Categories)
                {
                    if (row.MonthlyData.TryGetValue(month.Month, out DashboardMonthData? data))
                    {
                        month.TotalBudget += data.Budget;
                        month.TotalSpent += data.Spent;
                        month.TotalDisplaySpent += data.DisplaySpent;
                    }
                }
            }

            return dashboard;
        }
    }
}