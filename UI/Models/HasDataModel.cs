using Shared.DTOs.Response.HasData;

namespace UI.Models
{
    public class HasDataModel
    {
        public Dictionary<int, List<int>> TransactionMonthsByYear { get; set; } = new();
        public List<int> BudgetYears { get; set; } = new();
        public List<int> FixedExpenseYears { get; set; } = new();

        // Propiedades calculadas para facilitar el uso en la UI
        public List<int> TransactionYears => TransactionMonthsByYear.Keys.Order().ToList();

        public bool HasTransactionData(int year) => TransactionMonthsByYear.ContainsKey(year);

        public bool HasTransactionMonthData(int year, int month) => TransactionMonthsByYear.TryGetValue(year, out List<int>? months) && months.Contains(month);

        public bool HasBudgetData(int year) => BudgetYears.Contains(year);

        public bool HasFixedExpenseData(int year) => FixedExpenseYears.Contains(year);

        public static HasDataModel FromDTO(HasDataResponseDTO dto)
        {
            return new HasDataModel
            {
                TransactionMonthsByYear = dto.TransactionMonthsByYear,
                BudgetYears = dto.BudgetYears,
                FixedExpenseYears = dto.FixedExpenseYears
            };
        }
    }
}