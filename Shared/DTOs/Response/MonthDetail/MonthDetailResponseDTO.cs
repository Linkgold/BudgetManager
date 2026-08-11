namespace Shared.DTOs.Response.MonthDetail
{
    public class MonthDetailResponseDTO
    {
        public int Month { get; set; }
        public List<MonthDetailCategoryDTO> Categories { get; set; } = new();
        public decimal TotalBudget { get; set; }
        public decimal TotalSpent { get; set; }
    }
}