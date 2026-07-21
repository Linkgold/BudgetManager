namespace Shared.DTOs.Response
{
    /// <summary>
    /// DTO para devolver información de un gasto fijo
    /// </summary>
    public class FixedExpenseResponseDTO
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public int CategoryId { get; set; }
        public string CategoryName { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public string Currency { get; set; } = string.Empty;
        public int Year { get; set; }
        public int Month { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}
