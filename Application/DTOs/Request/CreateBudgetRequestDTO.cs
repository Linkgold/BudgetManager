namespace Application.DTOs.Request
{
    /// <summary>
    /// DTO para crear un nuevo presupuesto
    /// </summary>
    public class CreateBudgetRequestDTO
    {
        public int CategoryId { get; set; }
        public decimal Amount { get; set; }
        public int Month { get; set; }
        public int Year { get; set; }
    }
}