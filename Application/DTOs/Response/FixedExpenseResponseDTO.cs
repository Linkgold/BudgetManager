namespace Application.DTOs.Response
{
    /// <summary>
    /// DTO para devolver información de un gasto fijo
    /// </summary>
    public class FixedExpenseResponseDto
    {
        public int Id { get; set; }
        public int CategoryId { get; set; }
        public string CategoryName { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public decimal Amount { get; set; }
        public string Currency { get; set; }
        public int Year { get; set; }
        public int Month { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}
