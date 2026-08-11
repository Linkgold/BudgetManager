using Contracts.Enums;

namespace UI.Models.Dashboard
{
    public class DashboardCategoryMonthModel
    {
        public int Month { get; }
        public decimal Budget { get; }
        public decimal Spent { get; }
        public decimal DisplayBudget { get; }
        public decimal DisplaySpent { get; }

        public DashboardCategoryMonthModel(int month, decimal budget, decimal spent, CategoryNatureEnum nature)
        {
            Month = month;
            Budget = budget;
            Spent = spent;

            DisplayBudget = Math.Abs(budget);

            if (nature == CategoryNatureEnum.Mixed && spent > 0)
            {
                DisplaySpent = -spent;
            }
            else
            {
                DisplaySpent = Math.Abs(spent);
            }
        }
    }
}