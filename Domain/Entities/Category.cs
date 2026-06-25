using Domain.ValueObjects;

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

        private Category() { } // For EF Core

        public Category(string name, string? description = null)
        {
            Name = new CategoryName(name);
            Description = description;
            IsActive = true;
            CreatedAt = DateTime.UtcNow;
        }

        public void Update(string name, string? description = null)
        {
            Name = new CategoryName(name);
            Description = description;
            UpdatedAt = DateTime.UtcNow;
        }

        public void Activate()
        {
            IsActive = true;
            UpdatedAt = DateTime.UtcNow;
        }

        public void Deactivate()
        {
            IsActive = false;
            UpdatedAt = DateTime.UtcNow;
        }

        public void UpdateName(string newName)
        {
            Name = new CategoryName(newName);
            UpdatedAt = DateTime.UtcNow;
        }

        public void UpdateDescription(string newDescription)
        {
            Description = newDescription;
            UpdatedAt = DateTime.UtcNow;
        }

        public bool CanBeDeleted()
        {
            // Por ahora siempre true, luego se validará con repositorios
            return true;
            //return !_budgets.Any() && !_fixedExpenses.Any();
        }

        public override string ToString()
        {
            return $"Category: {Name} (ID: {Id})";
        }
    }
}