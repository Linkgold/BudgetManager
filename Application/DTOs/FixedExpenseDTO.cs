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
        public int? CategoryId { get; set; }
        public string? CategoryName { get; set; }
        public int? BudgetId { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }

    public class CreateFixedExpenseDTO
    {
        public string Name { get; set; }
        public decimal Amount { get; set; }
        public int ChargeMonth { get; set; }
        public int Year { get; set; }
        public int? ChargeDay { get; set; }
        public string? Description { get; set; }
        public int? CategoryId { get; set; }
        public int? BudgetId { get; set; }
    }

    public class UpdateFixedExpenseDTO
    {
        public string Name { get; set; }
        public decimal Amount { get; set; }
        public int ChargeMonth { get; set; }
        public int Year { get; set; }
        public int? ChargeDay { get; set; }
        public string? Description { get; set; }
        public int? CategoryId { get; set; }
        public int? BudgetId { get; set; }
    }
}