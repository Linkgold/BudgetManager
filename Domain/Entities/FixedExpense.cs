using Domain.Enums;
using Domain.ValueObjects;

namespace Domain.Entities
{
    public class FixedExpense
    {
        public int Id { get; private set; }
        public string Name { get; private set; }
        public Money Amount { get; private set; }
        public int ChargeMonth { get; private set; }
        public int Year { get; private set; }
        public int? ChargeDay { get; private set; }
        public string? Description { get; private set; }
        public bool IsActive { get; private set; }
        public DateTime CreatedAt { get; private set; }
        public DateTime? UpdatedAt { get; private set; }

        public int? BudgetId { get; private set; }
        public int? CategoryId { get; private set; }

        public Budget Budget { get; private set; }

        public Category Category { get; private set; }

        private FixedExpense() { } // For EF Core

        public FixedExpense(string name, Money amount, int chargeMonth, int year, int? chargeDay = null, string? description = null, Category? category = null)
        {
            if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("Name is required", nameof(name));

            if (chargeMonth < 1 || chargeMonth > 12) throw new ArgumentException("Charge month must be between 1 and 12", nameof(chargeMonth));

            Name = name;
            Amount = amount;
            ChargeMonth = chargeMonth;
            Year = year;
            ChargeDay = chargeDay;
            Description = description;
            Category = category;
            IsActive = true;
            CreatedAt = DateTime.UtcNow;
        }

        public void AssignBudget(Budget budget)
        {
            if (!budget.Period.IsSameMonth(new Period(ChargeMonth, Year)))
                throw new InvalidOperationException("Fixed expense does not match budget period");

            Budget = budget;
        }

        public void AssignCategory(Category category)
        {
            Category = category;
            UpdatedAt = DateTime.UtcNow;
        }

        public void Deactivate()
        {
            IsActive = false;
            UpdatedAt = DateTime.UtcNow;
        }

        public void Activate()
        {
            IsActive = true;
            UpdatedAt = DateTime.UtcNow;
        }

        public void UpdateAmount(Money newAmount)
        {
            Amount = newAmount;
            UpdatedAt = DateTime.UtcNow;
        }
    }
}