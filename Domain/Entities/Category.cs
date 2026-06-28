using Domain.ValueObjects;

namespace Domain.Entities
{
    public class Category
    {
        public int Id { get; private set; }
        public EntityInfo Info { get; private set; }
        public bool IsActive { get; private set; }
        public DateTime CreatedAt { get; private set; }
        public DateTime? UpdatedAt { get; private set; }

        // ========================================================
        // NAVIGATION PROPERTIES (comentadas hasta tener todas las entidades)
        // ========================================================
        // public virtual ICollection<Expense> Expenses { get; private set; }
        // public virtual ICollection<FixedExpense> FixedExpenses { get; private set; }
        // public virtual ICollection<Budget> Budgets { get; private set; }
        // ========================================================

        private Category() { } // For EF Core

        public Category(EntityInfo info)
        {
            if (info == null) throw new ArgumentNullException(nameof(info));

            Info = info;
            IsActive = true;
            CreatedAt = DateTime.UtcNow;

            // Inicialización de colecciones (cuando se descomenten)
            // Expenses = new List<Expense>();
            // FixedExpenses = new List<FixedExpense>();
            // Budgets = new List<Budget>();
        }

        public void Update(EntityInfo newInfo)
        {
            if (newInfo == null) throw new ArgumentNullException(nameof(newInfo));

            Info = newInfo;
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

        public bool CanBeDeleted()
        {
            // Por ahora siempre true, luego se validará con repositorios
            return true;
            //return !_budgets.Any() && !_fixedExpenses.Any();
        }

        public override string ToString() => $"Category: {Info.Name} (ID: {Id})";
    }
}