namespace UI.Models.Dashboard
{
    public class DashboardMonthTotalsModel
    {
        public int Month { get; }
        public string Name { get; } = string.Empty;
        public decimal TotalBudget { get; }
        public decimal TotalSpent { get; }
        public decimal AccumulatedBudget { get; }
        public decimal AccumulatedSpent { get; }

        public DashboardMonthTotalsModel
        (
            int month, 
            string name, 
            decimal totalBudget, 
            decimal totalSpent, 
            decimal accumulatedBudget, 
            decimal accumulatedSpent
        )
        {
            Month = month;
            Name = name;
            TotalBudget = totalBudget;
            TotalSpent = totalSpent;
            AccumulatedBudget = accumulatedBudget;
            AccumulatedSpent = accumulatedSpent;
        }
    }
}