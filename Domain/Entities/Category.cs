using Contracts.Enums;
using Domain.ValueObjects;

namespace Domain.Entities
{
    public class Category
    {
        public int Id { get; private set; }
        public EntityInfo Info { get; private set; }
        public CategoryNatureEnum Nature { get; private set; }
        public DateTime CreatedAt { get; private set; }
        public DateTime? UpdatedAt { get; private set; }

        // 🔥 Foreign key a User
        public int UserId { get; private set; }

        // 🔥 Navigation property
        public User User { get; private set; }

#pragma warning disable CS8618 // Un campo que no acepta valores NULL debe contener un valor distinto de NULL al salir del constructor. Considere la posibilidad de agregar el modificador "required" o declararlo como un valor que acepta valores NULL.
        private Category() { } // For EF Core
#pragma warning restore CS8618 // Un campo que no acepta valores NULL debe contener un valor distinto de NULL al salir del constructor. Considere la posibilidad de agregar el modificador "required" o declararlo como un valor que acepta valores NULL.

        public Category(User user, EntityInfo info, CategoryNatureEnum nature)
        {
            ArgumentNullException.ThrowIfNull(user);
            ArgumentNullException.ThrowIfNull(info);

            User = user;
            UserId = user.Id;
            Info = info;
            Nature = nature;
            CreatedAt = DateTime.UtcNow;

            // Inicialización de colecciones (cuando se descomenten)
            // Expenses = new List<Expense>();
            // FixedExpenses = new List<FixedExpense>();
            // Budgets = new List<Budget>();
        }

        public void Update(EntityInfo info, CategoryNatureEnum? nature = null)
        {
            ArgumentNullException.ThrowIfNull(info);

            Info = info;

            if (nature.HasValue)
            {
                Nature = nature.Value;
            }

            UpdatedAt = DateTime.UtcNow;
        }
    }
}