using Domain.ValueObjects;

namespace Domain.Entities
{
    public class Category
    {
        public int Id { get; private set; }
        public EntityInfo Info { get; private set; }
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
            CreatedAt = DateTime.UtcNow;

            // Inicialización de colecciones (cuando se descomenten)
            // Expenses = new List<Expense>();
            // FixedExpenses = new List<FixedExpense>();
            // Budgets = new List<Budget>();
        }

        public void Update(EntityInfo info)
        {
            ArgumentNullException.ThrowIfNull(info);

            Info = info;
            UpdatedAt = DateTime.UtcNow;
        }

        public override string ToString() => $"Category: {Info.Name} (Description: {Info.Description})";
    }
}