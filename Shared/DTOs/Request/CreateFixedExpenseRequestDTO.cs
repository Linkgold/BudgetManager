namespace Shared.DTOs.Request
{
    /// <summary>
    /// DTO para crear un nuevo gasto fijo
    /// </summary>
    public class CreateFixedExpenseRequestDTO
    {
        public int CategoryId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public int Month { get; set; }
        public int Year { get; set; }
    }
}