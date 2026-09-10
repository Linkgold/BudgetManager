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
            List<DashboardCategoryModel> categories = MapCategories(dto.Categories);

            // ✅ Filtrar categorías sin actividad (sin presupuesto y sin gasto en ningún mes)
            // ✅ Ordenar: Income → Mixed → Expense 
            categories = categories
                .Where(c => c.MonthlyData.Any(m => m.Budget != 0 || m.Spent != 0))
                .OrderByNatureWithIncomeBudget(c => c.Nature, c => c.TotalBudget)
                .ThenBy(c => c.CategoryName)
                .ToList();

            List<DashboardMonthTotalsModel> months = MapMonths(dto.Months);

            return new DashboardModel { Categories = categories, Months = months };
        }

        /// <summary>
        /// Mapear categorías (totales + mensuales)
        /// </summary>
        /// <param name="categoryDtos"></param>
        /// <returns></returns>
        private static List<DashboardCategoryModel> MapCategories(List<DashboardCategoryDTO> categoryDtos)
        {
            List<DashboardCategoryModel> categories = new List<DashboardCategoryModel>();

            foreach (DashboardCategoryDTO categoryDto in categoryDtos)
            {
                List<DashboardCategoryMonthModel> monthlyData = new List<DashboardCategoryMonthModel>();

                // Mapear datos mensuales
                foreach (DashboardCategoryMonthDTO monthDto in categoryDto.MonthlyData)
                {
                    monthlyData.Add(new DashboardCategoryMonthModel(
                        monthDto.Month,
                        monthDto.Budget,
                        monthDto.Spent,
                        categoryDto.Nature
                    ));
                }

                categories.Add(new DashboardCategoryModel(
                    categoryDto.CategoryName,
                    categoryDto.Nature,
                    monthlyData,
                    categoryDto.TotalBudget,
                    categoryDto.TotalSpent
                ));
            }

            return categories;
        }

        /// <summary>
        /// Mapear meses (totales + acumulados) 
        /// </summary>
        /// <param name="monthDtos"></param>
        /// <returns></returns>
        private static List<DashboardMonthTotalsModel> MapMonths(List<DashboardMonthTotalsDTO> monthDtos)
        {
            List<DashboardMonthTotalsModel> months = new List<DashboardMonthTotalsModel>();

            foreach (DashboardMonthTotalsDTO monthDto in monthDtos)
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

            return months;
        }
    }
}