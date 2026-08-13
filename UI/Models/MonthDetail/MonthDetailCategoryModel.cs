using Contracts.Enums;

namespace UI.Models.MonthDetail
{
    public class MonthDetailCategoryModel
    {
        public int CategoryId { get; set; }
        public string CategoryName { get; set; } = string.Empty;
        public CategoryNatureEnum Nature { get; set; }
        public decimal Budget { get; set; }
        public decimal Spent { get; set; }
        public List<MonthDetailTransactionModel> Transactions { get; set; } = new();

        // Propiedades calculadas
        public decimal DisplayBudget => Math.Abs(Budget);
        public decimal DisplaySpent => Nature == CategoryNatureEnum.Mixed && Spent > 0 ? -Spent : Math.Abs(Spent);
        public decimal Difference
        {
            get
            {
                if (Nature == CategoryNatureEnum.Income)
                {
                    return Spent - Budget;
                }

                if (Nature == CategoryNatureEnum.Mixed && Spent > 0)
                {
                    return Math.Abs(Budget) + Spent;
                }

                return Math.Abs(Budget) - Math.Abs(Spent);
            }
        }
    }
}