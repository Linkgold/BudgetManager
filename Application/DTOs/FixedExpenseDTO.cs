namespace Application.DTOs
{
    public class FixedExpenseDTO
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public int ChargeMonth { get; set; }
        public int Year { get; set; }
        public int? ChargeDay { get; set; }
        public string? Description { get; set; }
        public bool IsActive { get; set; }
        public int? BudgetId { get; set; }
    }
}