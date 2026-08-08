using Shared.DTOs.Response.Dashboard;
using Shared.DTOs.Response.Data;
using UI.Helpers;

namespace UI.Models.Dashboard
{
    public class DashboardModel
    {
        public List<DashboardCategoryModel> Categories { get; set; } = new();
        public List<DashboardMonthTotalsModel> Months { get; set; } = new();

        public static DashboardModel FromDTO(DashboardResponseDTO dto)
        {
            DashboardModel model = new DashboardModel();

            // 1. Mapear categorías
            foreach (DashboardCategoryDTO categoryDto in dto.Categories)
            {
                DashboardCategoryModel categoryModel = new DashboardCategoryModel
                {
                    CategoryName = categoryDto.CategoryName,
                    Nature = categoryDto.Nature,
                    TotalBudget = categoryDto.TotalBudget,
                    TotalSpent = categoryDto.TotalSpent
                };

                // Mapear datos mensuales
                foreach (DashboardCategoryMonthDTO monthDto in categoryDto.MonthlyData)
                {
                    categoryModel.MonthlyData.Add(new DashboardCategoryMonthModel());

                    categoryModel.MonthlyData[monthDto.Month - 1] = new DashboardCategoryMonthModel
                    {
                        Month = monthDto.Month,
                        Budget = monthDto.Budget,
                        Spent = monthDto.Spent
                    };
                }

                model.Categories.Add(categoryModel);
            }

            // 2. Mapear meses (totales + acumulados)
            foreach (DashboardMonthTotalsDTO monthDto in dto.Months)
            {
                // Mes con totales
                model.Months.Add(new DashboardMonthTotalsModel
                {
                    Month = monthDto.Month,
                    Name = MonthHelper.GetMonthName(monthDto.Month),
                    TotalBudget = monthDto.TotalBudget,
                    TotalSpent = monthDto.TotalSpent,
                    AccumulatedBudget = monthDto.AccumulatedBudget,
                    AccumulatedSpent = monthDto.AccumulatedSpent
                });
            }

            return model;
        }
    }
}