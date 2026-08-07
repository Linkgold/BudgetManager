namespace UI.Models.Dashboard
{
    public class DashboardMonthColumn
    {
        public int Month { get; set; }
        public string Name { get; set; } = string.Empty;
        public decimal TotalBudget { get; set; }
        public decimal TotalSpent { get; set; }
    }
}