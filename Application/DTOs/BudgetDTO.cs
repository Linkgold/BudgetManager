using Domain.Entities;
using Domain.Enums;

namespace Application.DTOs
{
    public class BudgetDTO
    {
        public int Id { get; set; }
        public Category Category { get; set; }
        public decimal MonthlyAmount { get; set; }
        public int Month { get; set; }
        public int Year { get; set; }
        public decimal TotalSpent { get; set; }
        public decimal Remaining { get; set; }
        public BudgetStatus Status { get; set; }
        public List<ExpenseDTO> Expenses { get; set; } = new();
    }
}