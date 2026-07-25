using Contracts.Enums;
using Domain.ValueObjects;

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

        private Transaction() { }

        public Transaction(User user, Category category, EntityInfo info, Money amount, DailyPeriod date)
        {
            ArgumentNullException.ThrowIfNull(user);
            ArgumentNullException.ThrowIfNull(category);
            ArgumentNullException.ThrowIfNull(info);
            ArgumentNullException.ThrowIfNull(amount);
            ArgumentNullException.ThrowIfNull(date);

            // ✅ Validar según la naturaleza de la categoría
            ValidateAmountByNature(category.Nature, amount);

            User = user;
            UserId = user.Id;
            Category = category;
            CategoryId = category.Id;
            Info = info;
            Amount = amount;
            Date = date;
            CreatedAt = DateTime.UtcNow;
        }

        public void Update(EntityInfo info, Money amount, DailyPeriod date)
        {
            ArgumentNullException.ThrowIfNull(info);
            ArgumentNullException.ThrowIfNull(amount);
            ArgumentNullException.ThrowIfNull(date);

            // ✅ Validar según la naturaleza de la categoría
            ValidateAmountByNature(Category.Nature, amount);

            Info = info;
            Amount = amount;
            Date = date;
            UpdatedAt = DateTime.UtcNow;
        }

        public void UpdateAmount(Money amount)
        {
            ArgumentNullException.ThrowIfNull(amount);

            // ✅ Validar según la naturaleza de la categoría
            ValidateAmountByNature(Category.Nature, amount);

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

        public MonthlyPeriod GetMonthlyPeriod() => Date.ToMonthlyPeriod();

        // ✅ VALIDACIÓN POR NATURALEZA
        private void ValidateAmountByNature(CategoryNatureEnum nature, Money amount)
        {
            switch (nature)
            {
                case CategoryNatureEnum.Income:
                case CategoryNatureEnum.Expense:
                    if (amount.Value <= 0)
                        throw new ArgumentException($"Transactions in {nature} categories must have a positive amount");
                    break;

                case CategoryNatureEnum.Mixed:
                    if (amount.Value == 0)
                        throw new ArgumentException("Mixed transactions cannot have a zero amount");
                    break;

                default:
                    throw new InvalidOperationException($"Unknown category nature: {nature}");
            }
        }

        public override string ToString() => $"{(Amount.Value >= 0 ? "+" : "-")}{Math.Abs(Amount.Value):F2} {Amount.Currency} - {Info.Name} ({Date})";
    }
}