using Domain.Enums;
using Domain.ValueObjects;

namespace Domain.Entities
{
    public class Budget
    {
        public int Id { get; private set; }
        public Money MonthlyAmount { get; private set; }
        public Period Period { get; private set; }
        public DateTime CreatedAt { get; private set; }
        public DateTime? UpdatedAt { get; private set; }

        // Foreign key
        public int CategoryId { get; private set; }

        // Navigation property
        public Category Category { get; private set; }

        private List<Expense> _expenses = new();
        public IReadOnlyCollection<Expense> Expenses => _expenses.AsReadOnly();

        private Budget() { } // For EF Core

        public Budget(Category category, Money monthlyAmount, Period period)
        {
            Category = category ?? throw new ArgumentNullException(nameof(category));
            CategoryId = category.Id;
            MonthlyAmount = monthlyAmount;
            Period = period;
            CreatedAt = DateTime.UtcNow;
        }

        public void AddExpense(Expense expense)
        {
            if (!Period.IsSameMonth(expense.Period)) throw new InvalidOperationException("Expense does not belong to this budget period");

            _expenses.Add(expense);
            UpdatedAt = DateTime.UtcNow;
        }

        public decimal GetTotalSpent()
        {
            return _expenses.Sum(e => e.Amount.Value);
        }

        public decimal GetRemaining()
        {
            return MonthlyAmount.Value - GetTotalSpent();
        }

        public BudgetStatus GetStatus()
        {
            if (MonthlyAmount.Value == 0) return BudgetStatus.Green;

            decimal percentage = (GetTotalSpent() / MonthlyAmount.Value) * 100;

            if (percentage < 80)
                return BudgetStatus.Green;
            else if (percentage <= 100)
                return BudgetStatus.Yellow;
            else
                return BudgetStatus.Red;
        }

        public void UpdateAmount(Money newAmount)
        {
            MonthlyAmount = newAmount;
            UpdatedAt = DateTime.UtcNow;
        }
    }
}