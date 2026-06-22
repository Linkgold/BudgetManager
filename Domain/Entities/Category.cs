namespace Domain.Entities
{
    public class Category
    {
        public int Id { get; private set; }
        public string Name { get; private set; }
        public string? Description { get; private set; }
        public bool IsActive { get; private set; }
        public DateTime CreatedAt { get; private set; }
        public DateTime? UpdatedAt { get; private set; }

        // Navigation properties
        private List<Budget> _budgets = new();
        public IReadOnlyCollection<Budget> Budgets => _budgets.AsReadOnly();

        private List<FixedExpense> _fixedExpenses = new();
        public IReadOnlyCollection<FixedExpense> FixedExpenses => _fixedExpenses.AsReadOnly();

        private Category() { } // For EF Core

        public Category(string name, string? description = null)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Name is required", nameof(name));

            Name = name;
            Description = description;
            IsActive = true;
            CreatedAt = DateTime.UtcNow;
        }

        public void Update(string name, string? description = null)
        {
            Name = name;
            Description = description;
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

        public bool CanBeDeleted()
        {
            return !_budgets.Any() && !_fixedExpenses.Any();
        }
    }
}