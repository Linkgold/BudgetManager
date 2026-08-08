namespace Shared.DTOs.Response.HasData
{
    public class HasDataResponseDTO
    {
        public HasDataTransactionsDTO Transactions { get; set; } = new();
        public List<int> BudgetYears { get; set; } = [];
        public List<int> FixedExpenseYears { get; set; } = [];
    }
}