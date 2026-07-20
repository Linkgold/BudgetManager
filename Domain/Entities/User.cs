using Domain.ValueObjects;

namespace Domain.Entities
{
    /// <summary>
    /// Entidad que representa un usuario de la aplicación
    /// </summary>
    public class User
    {
        public int Id { get; private set; }
        public UserInfo Info { get; private set; }
        public string PasswordHash { get; private set; }
        public DateTime CreatedAt { get; private set; }
        public DateTime? UpdatedAt { get; private set; }

        // Navigation properties (comentadas hasta tener todas las entidades)
        public ICollection<Category> Categories { get; private set; }
        public ICollection<FixedExpense> FixedExpenses { get; private set; }
        public ICollection<Budget> Budgets { get; private set; }
        public ICollection<Transaction> Transactions { get; private set; }

        private User() { }

        public User(UserInfo info, string passwordHash)
        {
            ArgumentNullException.ThrowIfNull(info);
            ArgumentNullException.ThrowIfNull(passwordHash);

            if (string.IsNullOrWhiteSpace(passwordHash)) throw new ArgumentException("Password hash cannot be empty", nameof(passwordHash));

            Info = info;
            PasswordHash = passwordHash;
            CreatedAt = DateTime.UtcNow;

            Categories = new List<Category>();
            FixedExpenses = new List<FixedExpense>();
            Budgets = new List<Budget>();
            Transactions = new List<Transaction>();
        }

        public void Update(UserInfo info)
        {
            ArgumentNullException.ThrowIfNull(info);

            Info = info;
            UpdatedAt = DateTime.UtcNow;
        }

        public void UpdatePassword(string passwordHash)
        {
            ArgumentNullException.ThrowIfNull(passwordHash);
            if (string.IsNullOrWhiteSpace(passwordHash)) throw new ArgumentException("Password hash cannot be empty", nameof(passwordHash));

            PasswordHash = passwordHash;
            UpdatedAt = DateTime.UtcNow;
        }

        public override string ToString() => $"{Info.UserName} <{Info.Email}>";
    }
}