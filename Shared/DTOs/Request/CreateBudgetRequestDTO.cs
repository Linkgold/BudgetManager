namespace Shared.DTOs.Request
{
    /// <summary>
    /// DTO para crear un nuevo presupuesto
    /// </summary>
    public class CreateBudgetRequestDTO
    {
        public int CategoryId { get; set; }
        public decimal Amount { get; set; }
        public string Currency { get; set; } = "EUR";
        public int Month { get; set; }
        public int Year { get; set; }
    }
}