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

        private Budget() { } // For EF Core

        public Budget(Category category, Money monthlyAmount, Period period)
        {
            Category = category ?? throw new ArgumentNullException(nameof(category));
            CategoryId = category.Id;
            MonthlyAmount = monthlyAmount;
            Period = period;
            CreatedAt = DateTime.UtcNow;
        }

        public void UpdateAmount(Money newAmount)
        {
            MonthlyAmount = newAmount;
            UpdatedAt = DateTime.UtcNow;
        }
    }
}