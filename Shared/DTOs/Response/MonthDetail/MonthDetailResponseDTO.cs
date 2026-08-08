namespace Shared.DTOs.Response.MonthDetail
{
    public class MonthDetailResponseDTO
    {
        public int Year { get; set; }
        public int Month { get; set; }
        public List<MonthDetailCategoryDTO> Categories { get; set; } = new();
        public decimal TotalBudget { get; set; }
        public decimal TotalSpent { get; set; }
        public decimal TotalDisplaySpent { get; set; }
        public decimal Difference => TotalBudget - TotalSpent;
        public decimal PercentageUsed => TotalBudget > 0 ? (TotalSpent / TotalBudget) * 100 : 0;
    }
}