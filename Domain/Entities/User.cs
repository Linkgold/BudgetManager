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
        public bool IsActive { get; private set; }
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
            IsActive = true;
            CreatedAt = DateTime.UtcNow;

            Categories = new List<Category>();
            FixedExpenses = new List<FixedExpense>();
            Budgets = new List<Budget>();
            Transactions = new List<Transaction>();
        }

        public void Update(UserInfo newInfo)
        {
            ArgumentNullException.ThrowIfNull(newInfo);

            Info = newInfo;
            UpdatedAt = DateTime.UtcNow;
        }

        public void UpdatePassword(string newPasswordHash)
        {
            ArgumentNullException.ThrowIfNull(newPasswordHash);
            if (string.IsNullOrWhiteSpace(newPasswordHash)) throw new ArgumentException("Password hash cannot be empty", nameof(newPasswordHash));

            PasswordHash = newPasswordHash;
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
    }
}