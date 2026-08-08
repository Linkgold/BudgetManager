using Contracts.Enums;

namespace Shared.DTOs.Response.Dashboard
{
    public class DashboardCategoryDTO
    {
        public string CategoryName { get; set; } = string.Empty;
        public CategoryNatureEnum Nature { get; set; }
        public List<DashboardCategoryMonthDTO> MonthlyData { get; set; } = new();
        public decimal TotalBudget { get; set; }
        public decimal TotalSpent { get; set; }
    }
}