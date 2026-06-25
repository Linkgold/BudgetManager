using Domain.Entities;
using Domain.Enums;

namespace Application.DTOs
{
    public class BudgetDashboardDto
    {
        public Budget Budget { get; set; }
        public decimal TotalSpent { get; set; }
        public decimal TotalFixedExpenses { get; set; }
        public decimal Remaining { get; set; }
        public decimal PercentageUsed { get; set; }
        public BudgetStatus Status { get; set; }
        public List<Expense> RecentExpenses { get; set; } // Solo últimos 10
    }
}
