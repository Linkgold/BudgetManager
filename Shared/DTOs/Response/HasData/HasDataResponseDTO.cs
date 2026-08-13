namespace Shared.DTOs.Response.HasData
{
    public class HasDataResponseDTO
    {
        public Dictionary<int, List<int>> TransactionMonthsByYear { get; set; } = new();
        public List<int> BudgetYears { get; set; } = [];
        public List<int> FixedExpenseYears { get; set; } = [];
    }
}