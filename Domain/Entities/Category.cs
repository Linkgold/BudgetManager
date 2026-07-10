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

        // 🔥 Foreign key a User
        public int UserId { get; private set; }

        // 🔥 Navigation property
        public User User { get; private set; }

        private Category() { } // For EF Core

        public Category(User user, EntityInfo info)
        {
            ArgumentNullException.ThrowIfNull(user);
            ArgumentNullException.ThrowIfNull(info);

            User = user;
            UserId = user.Id;
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
            ArgumentNullException.ThrowIfNull(newInfo);

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

        public override string ToString() => $"Category: {Info.Name} (ID: {Id})";
    }
}