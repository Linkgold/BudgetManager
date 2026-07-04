namespace Application.DTOs.Request
{
    /// <summary>
    /// DTO para actualizar un gasto fijo existente
    /// </summary>
    public class UpdateFixedExpenseRequestDTO
    {
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public int Month { get; set; }
        public int Year { get; set; }
    }
}