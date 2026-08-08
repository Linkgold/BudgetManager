namespace UI.Models.Dashboard
{
    public class DashboardMonthTotalsModel
    {
        public int Month { get; set; }
        public string Name { get; set; } = string.Empty;
        public decimal TotalBudget { get; set; }
        public decimal TotalSpent { get; set; }
        public decimal AccumulatedBudget { get; set; }
        public decimal AccumulatedSpent { get; set; }
    }
}