using Contracts.Enums;

namespace UI.Models.MonthDetail
{
    public class MonthDetailTransactionModel
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public DateTime Date { get; set; }
        public TransactionTypeEnum TransactionType { get; set; }

        // Propiedades calculadas
        public decimal DisplayAmount => TransactionType == TransactionTypeEnum.Income ? Amount : -Amount;
        public bool IsIncome => TransactionType == TransactionTypeEnum.Income;
        public bool IsExpense => TransactionType == TransactionTypeEnum.Expense;
    }
}