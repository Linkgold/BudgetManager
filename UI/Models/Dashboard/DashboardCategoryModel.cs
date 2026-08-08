using Contracts.Enums;

namespace UI.Models.Dashboard
{
    public class DashboardCategoryModel
    {
        public string CategoryName { get; set; } = string.Empty;
        public CategoryNatureEnum Nature { get; set; }
        public List<DashboardCategoryMonthModel> MonthlyData { get; set; } = new();
        public decimal TotalBudget { get; set; }
        public decimal TotalSpent { get; set; }

        public DashboardCategoryModel()
        {
            /*for (int i = 0; i <= 12; i++)
            {
                MonthlyData.Add(new DashboardCategoryMonthModel());
            }*/
        }
    }
}