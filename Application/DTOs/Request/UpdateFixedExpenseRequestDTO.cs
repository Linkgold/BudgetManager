namespace Application.DTOs.Request
{
    /// <summary>
    /// DTO para actualizar un gasto fijo existente
    /// </summary>
    public class UpdateFixedExpenseRequestDto
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public decimal Amount { get; set; }
        public int Year { get; set; }
        public int Month { get; set; }
    }
}
