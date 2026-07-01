namespace Application.DTOs.Request
{
    /// <summary>
    /// DTO para crear un nuevo gasto fijo
    /// </summary>
    public class CreateFixedExpenseRequestDTO
    {
        public int CategoryId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public decimal Amount { get; set; }
        public int Year { get; set; }
        public int Month { get; set; }
    }
}
