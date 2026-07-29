using Contracts.Enums;

namespace UI.Models.Forms
{
    public class TransactionFormModel : FormModelBase
    {
        public int CategoryId { get; set; }
        public string CategoryName { get; set; } = string.Empty;
        public CategoryNatureEnum CategoryNature { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public DateTime Date { get; set; } = DateTime.Now;
        public DateTime? MudDate { get => Date; set => Date = value ?? DateTime.Now; }
        public string TransactionType { get; set; } = "Expense";
        public bool IsIncome => TransactionType == "Income";
        public bool IsExpense => TransactionType == "Expense";

        public decimal FinalAmount
        {
            get
            {
                return CategoryNature switch
                {
                    CategoryNatureEnum.Income => Math.Abs(Amount),
                    CategoryNatureEnum.Expense => Math.Abs(Amount),
                    CategoryNatureEnum.Mixed => IsIncome ? Math.Abs(Amount) : -Math.Abs(Amount),
                    _ => Math.Abs(Amount)
                };
            }
        }

        public decimal DisplayAmount
        {
            get
            {
                return CategoryNature switch
                {
                    CategoryNatureEnum.Income => Math.Abs(Amount),
                    CategoryNatureEnum.Expense => -Math.Abs(Amount),// ✅ Negativo para mostrar en UI
                    CategoryNatureEnum.Mixed => IsIncome ? Math.Abs(Amount) : -Math.Abs(Amount),
                    _ => Math.Abs(Amount),
                };
            }
        }
    }
}