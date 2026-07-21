namespace Shared.DTOs.Request
{
    /// <summary>
    /// DTO para crear un grupo de presupuestos por año
    /// </summary>
    public class CreateBulkBudgetRequestDTO: BulkBudgetRequestDTOBase
    {
        public List<MonthlyBudgetDTO> MonthlyBudgets { get; set; } = new();
    }
}