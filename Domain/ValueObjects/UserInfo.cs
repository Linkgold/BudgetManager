using System.Net.Mail;

namespace Domain.ValueObjects
{
    public class UserInfo : IEquatable<UserInfo>
    {
        public string UserName { get; }
        public string Email { get; }

        public UserInfo(string userName, string email)
        {
            // Validar UserName
            if (string.IsNullOrWhiteSpace(userName)) throw new ArgumentException("User name cannot be empty", nameof(userName));
            if (userName.Length < 2 || userName.Length > 50) throw new ArgumentException("User name must be between 2 and 50 characters", nameof(userName));

            // Validar Email
            if (string.IsNullOrWhiteSpace(email)) throw new ArgumentException("Email cannot be empty", nameof(email));
            if (email.Length > 100) throw new ArgumentException("Email cannot exceed 100 characters", nameof(email));
            if (!IsValidEmail(email)) throw new ArgumentException("Invalid email format", nameof(email));

            UserName = userName.Trim();
            Email = email.Trim().ToLowerInvariant();
        }

        private bool IsValidEmail(string email)
        {
            try
            {
                MailAddress addr = new MailAddress(email);
                return addr.Address == email.Trim();
            }
            catch
            {
                return false;
            }
        }

        public bool Equals(UserInfo other)
        {
            if (other is null) return false;

            return UserName == other.UserName && Email == other.Email;
        }

        public override bool Equals(object obj) => Equals(obj as UserInfo);
        public override int GetHashCode() => HashCode.Combine(UserName, Email);
        public override string ToString() => $"{UserName} <{Email}>";

        public static bool operator ==(UserInfo a, UserInfo b)
        {
            if (a is null && b is null) return true;

            if (a is null || b is null) return false;

            return a.Equals(b);
        }

        public static bool operator !=(UserInfo a, UserInfo b) => !(a == b);
    }
}