namespace UI.Models.Dashboard
{
    public class DashboardMonthTotals
    {
        public int Month { get; set; }
        public decimal TotalBudget { get; set; }
        public decimal TotalSpent { get; set; }
        public decimal TotalDisplaySpent { get; set; }
    }
}