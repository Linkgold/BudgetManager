using Contracts.Enums;
using Shared.DTOs.Response.Dashboard;
using Shared.DTOs.Response.Data;
using UI.Helpers;
using UI.Extensions;

namespace UI.Models.Dashboard
{
    public class DashboardModel
    {
        public List<DashboardCategoryModel> Categories { get; set; } = new();
        public List<DashboardMonthTotalsModel> Months { get; set; } = new();

        public bool HasData => Categories.Any(c => c.MonthlyData.Any(m => m.Budget != 0 || m.Spent != 0));

        public static DashboardModel FromDTO(DashboardResponseDTO dto)
        {
            // 1. Mapear categorías
            List<DashboardCategoryModel> categories = new List<DashboardCategoryModel>();

            foreach (DashboardCategoryDTO categoryDto in dto.Categories)
            {
                List<DashboardCategoryMonthModel> monthlyData = new List<DashboardCategoryMonthModel>();
                
                // Mapear datos mensuales
                foreach (DashboardCategoryMonthDTO monthDto in categoryDto.MonthlyData)
                {
                    monthlyData.Add
                    (
                        new DashboardCategoryMonthModel
                        (
                            monthDto.Month, 
                            monthDto.Budget, 
                            monthDto.Spent, 
                            categoryDto.Nature
                        )
                    );
                }

                categories.Add
                (
                    new DashboardCategoryModel
                    (
                        categoryDto.CategoryName, 
                        categoryDto.Nature, 
                        monthlyData, 
                        categoryDto.TotalBudget, 
                        categoryDto.TotalSpent
                    )
                );
            }

            // ✅ Ordenar: Income → Mixed → Expense 
            categories = categories
                .OrderByNatureWithIncomeBudget(c => c.Nature, c => c.TotalBudget)
                .ThenBy(c => c.CategoryName)
                .ToList();

            // 2. Mapear meses (totales + acumulados)
            List<DashboardMonthTotalsModel> months = new List<DashboardMonthTotalsModel>();

            foreach (DashboardMonthTotalsDTO monthDto in dto.Months)
            {
                months.Add(new DashboardMonthTotalsModel(
                    monthDto.Month,
                    MonthHelper.GetMonthName(monthDto.Month),
                    monthDto.TotalBudget,
                    monthDto.TotalSpent,
                    monthDto.AccumulatedBudget,
                    monthDto.AccumulatedSpent
                ));
            }

            return new DashboardModel { Categories = categories, Months = months };
        }
    }
}