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
        public decimal DisplaySpent => Math.Abs(Spent);
        public decimal Difference
        {
            get
            {
                if (Nature == CategoryNatureEnum.Income)
                {
                    return Spent - Budget;  // Ingreso: real vs presupuesto
                }

                return Math.Abs(Budget) - Math.Abs(Spent);  // Gasto: presupuesto vs real
            }
        }
    }
}