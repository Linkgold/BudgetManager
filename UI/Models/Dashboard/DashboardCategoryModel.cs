using Contracts.Enums;

namespace UI.Models.Dashboard
{
    public class DashboardCategoryModel
    {
        public string CategoryName { get; } = string.Empty;
        public CategoryNatureEnum Nature { get; }
        public List<DashboardCategoryMonthModel> MonthlyData { get; } = new();
        public decimal TotalBudget { get; }
        public decimal TotalSpent { get; }

        public DashboardCategoryModel
        (
            string categoryName, 
            CategoryNatureEnum nature, 
            List<DashboardCategoryMonthModel> monthlyData, 
            decimal totalBudget, 
            decimal totalSpent
        )
        {
            CategoryName = categoryName;
            Nature = nature;
            MonthlyData = monthlyData;
            TotalBudget = totalBudget;
            TotalSpent = totalSpent;
        }
    }
}