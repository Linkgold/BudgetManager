using Contracts.Enums;

namespace UI.Models.Dashboard
{
    public class DashboardCategoryRow
    {
        public int CategoryId { get; set; }
        public string CategoryName { get; set; } = string.Empty;
        public CategoryNatureEnum Nature { get; set; }
        public Dictionary<int, DashboardMonthData> MonthlyData { get; set; } = new();
        public decimal TotalBudget { get; set; }
        public decimal TotalSpent { get; set; }
        public decimal TotalDisplaySpent { get; set; }
        public decimal TotalDifference => TotalBudget - TotalSpent;
    }
}