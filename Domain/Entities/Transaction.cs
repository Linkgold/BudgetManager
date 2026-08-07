using Contracts.Enums;
using Domain.ValueObjects;
using System.Transactions;

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
        public TransactionTypeEnum TransactionType { get; private set; }
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

#pragma warning disable CS8618 // Un campo que no acepta valores NULL debe contener un valor distinto de NULL al salir del constructor. Considere la posibilidad de agregar el modificador "required" o declararlo como un valor que acepta valores NULL.
        private Transaction() { }
#pragma warning restore CS8618 // Un campo que no acepta valores NULL debe contener un valor distinto de NULL al salir del constructor. Considere la posibilidad de agregar el modificador "required" o declararlo como un valor que acepta valores NULL.

        public Transaction
        (
            User user, 
            Category category, 
            EntityInfo info, 
            Money amount,
            TransactionTypeEnum transactionType,
            DailyPeriod date
        )
        {
            ArgumentNullException.ThrowIfNull(user);
            ArgumentNullException.ThrowIfNull(category);
            ArgumentNullException.ThrowIfNull(info);
            ArgumentNullException.ThrowIfNull(amount);
            ArgumentNullException.ThrowIfNull(date);

            // ✅ Validar según la naturaleza de la categoría
            ValidateTransactionByNature(category.Nature, transactionType, amount);

            User = user;
            UserId = user.Id;
            Category = category;
            CategoryId = category.Id;
            Info = info;
            Amount = amount;
            TransactionType = transactionType;
            Date = date;
            CreatedAt = DateTime.UtcNow;
        }

        public void Update(Category category, EntityInfo info, Money amount, TransactionTypeEnum transactionType, DailyPeriod date)
        {
            ArgumentNullException.ThrowIfNull(category);
            ArgumentNullException.ThrowIfNull(info);
            ArgumentNullException.ThrowIfNull(amount);
            ArgumentNullException.ThrowIfNull(date);

            // ✅ Validar según la naturaleza de la categoría
            ValidateTransactionByNature(category.Nature, transactionType, amount);

            Category = category;
            CategoryId = category.Id;
            Info = info;
            Amount = amount;
            TransactionType = transactionType;
            Date = date;
            UpdatedAt = DateTime.UtcNow;
        }

        public MonthlyPeriod GetMonthlyPeriod() => Date.ToMonthlyPeriod();

        // ✅ VALIDACIÓN POR NATURALEZA
        private void ValidateTransactionByNature(CategoryNatureEnum nature, TransactionTypeEnum transactionType, Money amount)
        {
            if (amount.Value <= 0) throw new ArgumentException($"Transactions in {nature} categories must have a positive amount");

            // Validar que el tipo de transacción sea válido para la naturaleza
            if (nature == CategoryNatureEnum.Income && transactionType != TransactionTypeEnum.Income)
            {
                throw new ArgumentException($"Income categories only allow Income transactions. '{transactionType}' is not allowed.");
            }

            if (nature == CategoryNatureEnum.Expense && transactionType != TransactionTypeEnum.Expense)
            {
                throw new ArgumentException($"Expense categories only allow Expense transactions. '{transactionType}' is not allowed.");
            }
        }
    }
}