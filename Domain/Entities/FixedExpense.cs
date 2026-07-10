using Domain.Enums;
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
        public bool IsActive { get; private set; }
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
        private FixedExpense() { }

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
            IsActive = true;
            CreatedAt = DateTime.UtcNow;
        }

        // ==================== MÉTODOS DE COMPORTAMIENTO ====================

        public void Update(EntityInfo info, Money amount, MonthlyPeriod chargePeriod)
        {
            ArgumentNullException.ThrowIfNull(info);
            ArgumentNullException.ThrowIfNull(amount);
            ArgumentNullException.ThrowIfNull(chargePeriod);

            Info = info;
            Amount = amount;
            ChargePeriod = chargePeriod;
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

        public void UpdateAmount(Money newAmount)
        {
            ArgumentNullException.ThrowIfNull(newAmount);

            Amount = newAmount;
            UpdatedAt = DateTime.UtcNow;
        }

        /// <summary>
        /// Verifica si el gasto fijo está activo para un período específico
        /// </summary>
        public bool IsActiveForPeriod(MonthlyPeriod period)
        {
            ArgumentNullException.ThrowIfNull(period);

            if (!IsActive) return false;

            // El gasto fijo aplica si el período es igual o posterior al de inicio
            return period.Year > ChargePeriod.Year || (period.Year == ChargePeriod.Year && period.Month >= ChargePeriod.Month);
        }

        public override string ToString() => $"FixedExpense: {Info.Name} - {Amount:F2} {Amount.Currency} (Desde {ChargePeriod})";
    }
}