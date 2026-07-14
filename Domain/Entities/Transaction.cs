using Domain.Enums;
using Domain.ValueObjects;
using Microsoft.VisualBasic;

namespace Domain.Entities
{
    /// <summary>
    /// Entidad que representa un gasto real (transaccional)
    /// </summary>
    public class Transaction
    {
        public int Id { get; private set; }
        public EntityInfo Info { get; private set; }
        public Money Amount { get; private set; }
        public TransactionTypeEnum Type { get; private set; }
        public DailyPeriod Date { get; private set; }
        public DateTime CreatedAt { get; private set; }
        public DateTime? UpdatedAt { get; private set; }

        // 🔥 Foreign key a User
        public int UserId { get; private set; }

        // 🔥 Navigation property
        public User User { get; private set; }

        // Foreign key
        public int CategoryId { get; private set; }

        // Navigation property
        public Category Category { get; private set; }

        // 🔥 Propiedades de solo lectura para consultas rápidas
        public bool IsIncome => Type == TransactionTypeEnum.Income;
        public bool IsExpense => Type == TransactionTypeEnum.Expense;

        private Transaction() { }

        public Transaction(User user, Category category, EntityInfo info, Money amount, TransactionTypeEnum type, DailyPeriod date)
        {
            ArgumentNullException.ThrowIfNull(user);
            ArgumentNullException.ThrowIfNull(category);
            ArgumentNullException.ThrowIfNull(info);
            ArgumentNullException.ThrowIfNull(amount);
            ArgumentNullException.ThrowIfNull(date);

            User = user;
            UserId = user.Id;
            Category = category;
            CategoryId = category.Id;
            Info = info;
            Amount = amount;
            Type = type;
            Date = date;
            CreatedAt = DateTime.UtcNow;
        }

        public void Update(EntityInfo info, Money amount, TransactionTypeEnum type, DailyPeriod date)
        {
            ArgumentNullException.ThrowIfNull(info);
            ArgumentNullException.ThrowIfNull(amount);
            ArgumentNullException.ThrowIfNull(date);

            Info = info;
            Amount = amount;
            Type = type;
            Date = date;
            UpdatedAt = DateTime.UtcNow;
        }

        public void UpdateAmount(Money amount)
        {
            ArgumentNullException.ThrowIfNull(amount);

            Amount = amount;
            UpdatedAt = DateTime.UtcNow;
        }

        public void UpdateInfo(EntityInfo info)
        {
            ArgumentNullException.ThrowIfNull(info);

            Info = info;
            UpdatedAt = DateTime.UtcNow;
        }

        public void UpdateDate(DailyPeriod date)
        {
            ArgumentNullException.ThrowIfNull(date);

            Date = date;
            UpdatedAt = DateTime.UtcNow;
        }

        public MonthlyPeriod GetMonthlyPeriod()
        {
            return Date.ToMonthlyPeriod();
        }

        public override string ToString()
        {
            return $"Type [{Type}] - {Info.Name}: {Amount.Value:F2} {Amount.Currency} ({Date})";
        }
    }
}