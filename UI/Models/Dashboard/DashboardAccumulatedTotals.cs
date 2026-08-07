using UI.Pages;

namespace UI.Models.Dashboard
{
    public class DashboardAccumulatedTotals
    {
        public List<DashboardAccumulatedMonth> Monthly { get; set; } = new();
        public decimal TotalBudget { get; set; }
        public decimal TotalSpent { get; set; }
        public decimal TotalDisplaySpent { get; set; }
    }
}