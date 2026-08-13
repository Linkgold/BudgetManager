namespace Shared.DTOs.Response.Dashboard
{
    public class DashboardMonthTotalsDTO
    {
        public int Month { get; set; }
        public decimal TotalBudget { get; set; }
        public decimal TotalSpent { get; set; }
        public decimal AccumulatedBudget { get; set; }
        public decimal AccumulatedSpent { get; set; }
    }
}