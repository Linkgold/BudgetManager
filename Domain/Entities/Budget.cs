using Domain.Enums;
using Domain.ValueObjects;

namespace Domain.Entities
{
    /// <summary>
    /// Entidad que representa un presupuesto mensual para una categoría
    /// </summary>
    public class Budget
    {
        // ==================== PROPIEDADES ====================

        public int Id { get; private set; }
        public Money MonthlyAmount { get; private set; }
        public MonthlyPeriod Period { get; private set; }
        public DateTime CreatedAt { get; private set; }
        public DateTime? UpdatedAt { get; private set; }

        // Foreign key
        public int CategoryId { get; private set; }

        // Navigation property
        public Category Category { get; private set; }

        // ==================== CONSTRUCTORES ====================

        // Constructor privado para EF Core
        private Budget() { } // For EF Core

        // Constructor de dominio
        public Budget(Category category, Money monthlyAmount, MonthlyPeriod period)
        {
            ArgumentNullException.ThrowIfNull(category);
            ArgumentNullException.ThrowIfNull(monthlyAmount);
            ArgumentNullException.ThrowIfNull(period);

            Category = category;
            CategoryId = category.Id;
            MonthlyAmount = monthlyAmount;
            Period = period;
            CreatedAt = DateTime.UtcNow;
        }

        // ==================== MÉTODOS DE COMPORTAMIENTO ====================

        /// <summary>
        /// Actualiza el importe mensual del presupuesto
        /// </summary>
        public void UpdateAmount(Money newAmount)
        {
            ArgumentNullException.ThrowIfNull(newAmount);

            MonthlyAmount = newAmount;
            UpdatedAt = DateTime.UtcNow;
        }
        /// <summary>
        /// Calcula el estado del presupuesto basado en el gasto total
        /// </summary>
        /// <param name="totalSpent">Total gastado en el período</param>
        /// <returns>Estado del presupuesto (Green, Yellow, Red)</returns>
        public BudgetStatusEnum GetStatus(decimal totalSpent)
        {
            if (totalSpent < 0) throw new ArgumentException("Total spent cannot be negative", nameof(totalSpent));

            if (MonthlyAmount.Value == 0) return BudgetStatusEnum.Green;

            decimal percentage = (totalSpent / MonthlyAmount.Value) * 100;

            if (percentage < 80)
            {
                return BudgetStatusEnum.Green;
            }
            else if (percentage <= 100)
            {
                return BudgetStatusEnum.Yellow;
            }
            else
            {
                return BudgetStatusEnum.Red;
            }
        }

        /// <summary>
        /// Calcula el porcentaje utilizado del presupuesto
        /// </summary>
        public decimal GetPercentageUsed(decimal totalSpent)
        {
            if (totalSpent < 0) throw new ArgumentException("Total spent cannot be negative", nameof(totalSpent));

            if (MonthlyAmount.Value == 0) return 0;

            decimal percentage = (totalSpent / MonthlyAmount.Value) * 100;

            return Math.Min(percentage, 100); // No superar el 100%
        }

        /// <summary>
        /// Calcula el importe restante del presupuesto
        /// </summary>
        public Money GetRemaining(decimal totalSpent)
        {
            if (totalSpent < 0) throw new ArgumentException("Total spent cannot be negative", nameof(totalSpent));

            decimal remaining = MonthlyAmount.Value - totalSpent;

            return new Money(Math.Max(remaining, 0), MonthlyAmount.Currency);
        }

        /// <summary>
        /// Verifica si el presupuesto está excedido
        /// </summary>
        public bool IsOverBudget(decimal totalSpent)
        {
            if (totalSpent < 0) throw new ArgumentException("Total spent cannot be negative", nameof(totalSpent));

            return totalSpent > MonthlyAmount.Value;
        }

        public override string ToString()
        {
            return $"Budget: {Category?.Info?.Name ?? "Unknown"} - {MonthlyAmount:F2} {MonthlyAmount.Currency} ({Period})";
        }
    }
}