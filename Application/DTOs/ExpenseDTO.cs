namespace Application.DTOs
{
    public class ExpenseDTO
    {
        public int Id { get; set; }
        public string Description { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public DateTime DateTime { get; set; }
        public string? Category { get; set; }
        public string? Notes { get; set; }
        public int? BudgetId { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class CreateExpenseDTO
    {
        public string Description { get; set; }
        public decimal Amount { get; set; }
        public DateTime DateTime { get; set; }
        public string? Category { get; set; }
        public string? Notes { get; set; }
        public int? BudgetId { get; set; }
    }

    public class UpdateExpenseDTO
    {
        public string Description { get; set; }
        public decimal Amount { get; set; }
        public DateTime DateTime { get; set; }
        public string? Category { get; set; }
        public string? Notes { get; set; }
        public int? BudgetId { get; set; }
    }
}