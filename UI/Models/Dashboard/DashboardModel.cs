namespace UI.Models.Dashboard
{
    public class DashboardModel
    {
        public List<DashboardCategoryRow> Categories { get; set; } = new();
        public List<DashboardMonthColumn> Months { get; set; } = new();
        public decimal TotalBudget { get; set; }
        public decimal TotalSpent { get; set; }
        public decimal TotalDisplaySpent { get; set; }
        public decimal TotalDifference => TotalBudget - TotalSpent;
        public decimal PercentageUsed => TotalBudget > 0 ? (TotalSpent / TotalBudget) * 100 : 0;
    }
}