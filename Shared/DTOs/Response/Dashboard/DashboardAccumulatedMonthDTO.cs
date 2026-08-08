namespace Shared.DTOs.Response.Dashboard
{
    public class DashboardAccumulatedMonthDTO
    {
        public int Month { get; set; }
        public decimal AcumuladoBudget { get; set; }
        public decimal AcumuladoSpent { get; set; }
    }
}