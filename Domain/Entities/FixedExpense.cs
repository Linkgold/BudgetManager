using Domain.ValueObjects;

namespace Domain.Entities
{
    /// <summary>
    /// Entidad que representa un gasto fijo recurrente (mensual)
    /// </summary>
    public class FixedExpense
    {
        // Propiedades
        public int Id { get; private set; }
        public EntityInfo Info { get; private set; }
        public Money Amount { get; private set; }
        public MonthlyPeriod ChargePeriod { get; private set; }
        public DateTime CreatedAt { get; private set; }
        public DateTime? UpdatedAt { get; private set; }

        // Foreign key
        public int CategoryId { get; private set; }

        // Navigation property
        public Category Category { get; private set; }

        // 🔥 Foreign key a User
        public int UserId { get; private set; }

        // 🔥 Navigation property
        public User User { get; private set; }

        // Constructor privado para EF Core
#pragma warning disable CS8618 // Un campo que no acepta valores NULL debe contener un valor distinto de NULL al salir del constructor. Considere la posibilidad de agregar el modificador "required" o declararlo como un valor que acepta valores NULL.
        private FixedExpense() { }
#pragma warning restore CS8618 // Un campo que no acepta valores NULL debe contener un valor distinto de NULL al salir del constructor. Considere la posibilidad de agregar el modificador "required" o declararlo como un valor que acepta valores NULL.

        // Constructor de dominio
        public FixedExpense(User user, Category category, EntityInfo info, Money amount, MonthlyPeriod chargePeriod)
        {
            ArgumentNullException.ThrowIfNull(user);
            ArgumentNullException.ThrowIfNull(category);
            ArgumentNullException.ThrowIfNull(info);
            ArgumentNullException.ThrowIfNull(amount);
            ArgumentNullException.ThrowIfNull(chargePeriod);

            User = user;
            UserId = user.Id;
            Category = category;
            CategoryId = category.Id;
            Info = info;
            Amount = amount;
            ChargePeriod = chargePeriod;
            CreatedAt = DateTime.UtcNow;
        }

        // ==================== MÉTODOS DE COMPORTAMIENTO ====================

        public void Update(Category category, EntityInfo info, Money amount, MonthlyPeriod chargePeriod)
        {
            ArgumentNullException.ThrowIfNull(category);
            ArgumentNullException.ThrowIfNull(info);
            ArgumentNullException.ThrowIfNull(amount);
            ArgumentNullException.ThrowIfNull(chargePeriod);

            Category = category;
            CategoryId = category.Id;
            Info = info;
            Amount = amount;
            ChargePeriod = chargePeriod;
            UpdatedAt = DateTime.UtcNow;
        }

        public override string ToString() => $"FixedExpense: {Info.Name} - {Amount:F2} {Amount.Currency} (Desde {ChargePeriod})";
    }
}