namespace Shared.DTOs.Request
{
    /// <summary>
    /// DTO para crear un grupo de presupuestos por año
    /// </summary>
    public class CreateBulkBudgetRequestDTO
    {
        public int CategoryId { get; set; }
        public int Year { get; set; }
        public List<MonthlyBudgetDTO> MonthlyBudgets { get; set; } = new();
    }

    public class MonthlyBudgetDTO
    {
        public int Month { get; set; }
        public decimal Amount { get; set; }
    }
}