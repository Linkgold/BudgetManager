using Contracts.Enums;

namespace Shared.DTOs.Response.Dashboard
{
    public class DashboardCategoryDTO
    {
        public int CategoryId { get; set; }
        public string CategoryName { get; set; } = string.Empty;
        public CategoryNatureEnum Nature { get; set; }
        public Dictionary<int, DashboardMonthDataDTO> MonthlyData { get; set; } = new();
        public decimal TotalBudget { get; set; }
        public decimal TotalSpent { get; set; }
        public decimal TotalDisplaySpent { get; set; }
    }
}