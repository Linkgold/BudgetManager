using Domain.Entities;
using Domain.Enums;

namespace Application.DTOs
{
    public class BudgetDTO
    {
        public int Id { get; set; }
        public decimal MonthlyAmount { get; set; }
        public int Month { get; set; }
        public int Year { get; set; }
        public int CategoryId { get; set; }
        public string? CategoryName { get; set; }
        public decimal TotalSpent { get; set; }
        public decimal Remaining { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }

    public class CreateBudgetDTO
    {
        public decimal MonthlyAmount { get; set; }
        public int Month { get; set; }
        public int Year { get; set; }
        public int CategoryId { get; set; }
    }

    public class UpdateBudgetDTO
    {
        public decimal MonthlyAmount { get; set; }
        public int Month { get; set; }
        public int Year { get; set; }
        public int CategoryId { get; set; }
    }
}