namespace Shared.DTOs.Response.Dashboard
{
    public class DashboardMonthDTO
    {
        public int Month { get; set; }
        public decimal TotalBudget { get; set; }
        public decimal TotalSpent { get; set; }
    }
}