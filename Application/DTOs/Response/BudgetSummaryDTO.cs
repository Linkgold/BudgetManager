using Domain.Enums;

namespace Application.DTOs.Response
{
    /// <summary>
    /// DTO con resumen del presupuesto (incluye estado y cálculos)
    /// </summary>
    public class BudgetSummaryDTO
    {
        public int BudgetId { get; set; }
        public int CategoryId { get; set; }
        public string CategoryName { get; set; }
        public int Year { get; set; }
        public int Month { get; set; }
        public decimal BudgetAmount { get; set; }
        public decimal TotalSpent { get; set; }
        public decimal Remaining { get; set; }
        public decimal PercentageUsed { get; set; }
        public BudgetStatusEnum Status { get; set; }
        public bool IsOverBudget { get; set; }
    }
}