using Contracts.Enums;

namespace UI.Models.Forms
{
    public class TransactionFormModel : FormModelBase
    {
        public int CategoryId { get; set; }
        public string CategoryName { get; set; } = string.Empty;
        public CategoryNatureEnum CategoryNature { get; set; }
        public TransactionTypeEnum TransactionType { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public DateTime Date { get; set; } = DateTime.Now;
        public DateTime? MudDate { get => Date; set => Date = value ?? DateTime.Now; }

        public decimal DisplayAmount
        {
            get
            {
                return TransactionType switch
                {
                    TransactionTypeEnum.Income => Math.Abs(Amount),
                    TransactionTypeEnum.Expense => -Math.Abs(Amount),
                    _ => Math.Abs(Amount),
                };
            }
        }
    }
}