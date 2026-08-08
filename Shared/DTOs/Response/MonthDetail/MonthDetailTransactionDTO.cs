using Contracts.Enums;

namespace Shared.DTOs.Response.MonthDetail
{
    public class MonthDetailTransactionDTO
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public DateTime Date { get; set; }
        public TransactionTypeEnum TransactionType { get; set; }
        public decimal DisplayAmount { get; set; }
    }
}