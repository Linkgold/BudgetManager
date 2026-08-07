namespace UI.Models.Dashboard
{
    public class DashboardMonthData
    {
        public int Month { get; set; }
        public decimal Budget { get; set; }
        public decimal Spent { get; set; }
        public decimal DisplaySpent { get; set; }
        public decimal Difference => Budget - Spent;
        public decimal PercentageUsed => Budget > 0 ? (Spent / Budget) * 100 : 0;
    }
}