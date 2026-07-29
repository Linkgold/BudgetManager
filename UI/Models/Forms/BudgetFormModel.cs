using UI.Helpers;

namespace UI.Models.Forms
{
    public class BudgetFormModel : FormModelBase
    {
        public int CategoryId { get; set; }
        public string CategoryName { get; set; } = string.Empty;
        public int Year { get; set; }
        public decimal DefaultAmount { get; set; }
        public Dictionary<int, decimal> MonthlyAmounts { get; set; } = new Dictionary<int, decimal>();
        public Dictionary<int, decimal> OriginalMonthlyAmounts { get; set; } = new Dictionary<int, decimal>();

        public bool HasMonthChanged(int month)
        {
            if (OriginalMonthlyAmounts == null || MonthlyAmounts == null)
            {
                return false;
            }

            if (!OriginalMonthlyAmounts.TryGetValue(month, out decimal originalValue))
            {
                return true;
            }

            if (!MonthlyAmounts.TryGetValue(month, out decimal currentValue))
            {
                return true;
            }

            return currentValue != originalValue;
        }

        public bool HasChanges
        {
            get
            {
                if (OriginalMonthlyAmounts == null || MonthlyAmounts == null)
                {
                    return false;
                }

                foreach (KeyValuePair<int, decimal> kvp in MonthlyAmounts)
                {
                    if (HasMonthChanged(kvp.Key))
                    {
                        return true;
                    }
                }

                return false;
            }
        }

        public decimal TotalAnnualBudget
        {
            get
            {
                decimal total = 0m;

                foreach (KeyValuePair<int, decimal> kvp in MonthlyAmounts)
                {
                    total += kvp.Value;
                }

                return total;
            }
        }

        public void ApplyAmountToAllMonths()
        {
            foreach (MonthModel month in MonthHelper.Months)
            {
                MonthlyAmounts[month.Value] = DefaultAmount;
            }
        }
    }
}