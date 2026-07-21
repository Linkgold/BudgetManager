namespace Shared.DTOs.Request
{
    public class UpdateBulkBudgetRequestDTO: BulkBudgetRequestDTOBase
    {
        public List<MonthlyBudgetDTO> MonthlyBudgets { get; set; } = new();
    }
}