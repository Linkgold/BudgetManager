using Shared.DTOs.Response.Dashboard;

namespace Shared.DTOs.Response.Data
{
    public class DashboardResponseDTO
    {
        public List<DashboardCategoryDTO> Categories { get; set; } = new();
        public List<DashboardMonthDTO> Months { get; set; } = new();
        public List<DashboardAccumulatedMonthDTO> AccumulatedMonths { get; set; } = new();
        public decimal TotalBudget { get; set; }
        public decimal TotalSpent { get; set; }
        public decimal TotalDisplaySpent { get; set; }
    }
}