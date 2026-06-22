using Domain.ValueObjects;

namespace Domain.Entities
{
    public class Expense
    {
        public int Id { get; private set; }
        public string Description { get; private set; }
        public Money Amount { get; private set; }
        public DateTime DateTime { get; private set; }
        public string? Category { get; private set; }
        public string? Notes { get; private set; }
        public DateTime CreatedAt { get; private set; }

        // Relationship
        public int BudgetId { get; private set; }
        public Budget? Budget { get; private set; }

        // Derived value object
        public Period Period => new Period(DateTime.Month, DateTime.Year);

        private Expense() { } // For EF Core

        public Expense(string description, Money amount, DateTime dateTime,
                       string? category = null, string? notes = null)
        {
            if (string.IsNullOrWhiteSpace(description))
                throw new ArgumentException("Description is required", nameof(description));

            Description = description;
            Amount = amount;
            DateTime = dateTime;
            Category = category;
            Notes = notes;
            CreatedAt = DateTime.UtcNow;
        }

        public void AssignBudget(Budget budget)
        {
            if (!Period.IsSameMonth(budget.Period))
                throw new InvalidOperationException("Expense does not match budget period");

            Budget = budget;
            BudgetId = budget.Id;
        }
    }
}