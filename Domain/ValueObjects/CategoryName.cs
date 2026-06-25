namespace Domain.ValueObjects
{
    public class CategoryName : IEquatable<CategoryName>
    {
        public string Value { get; }

        public CategoryName(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new ArgumentException("Category name cannot be empty", nameof(value));

            if (value.Length < 2)
                throw new ArgumentException("Category name must have at least 2 characters", nameof(value));

            if (value.Length > 50)
                throw new ArgumentException("Category name cannot exceed 50 characters", nameof(value));

            // Normalizar: trim
            Value = value.Trim();
        }

        public bool Equals(CategoryName other)
        {
            if (other is null) return false;
            return Value.Equals(other.Value, StringComparison.OrdinalIgnoreCase);
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as CategoryName);
        }

        public override int GetHashCode()
        {
            return Value.ToLowerInvariant().GetHashCode();
        }

        public override string ToString()
        {
            return Value;
        }

        public static bool operator ==(CategoryName left, CategoryName right)
        {
            if (left is null && right is null) return true;
            if (left is null || right is null) return false;
            return left.Equals(right);
        }

        public static bool operator !=(CategoryName left, CategoryName right)
        {
            return !(left == right);
        }

        public static implicit operator string(CategoryName name) => name.Value;
        public static explicit operator CategoryName(string value) => new CategoryName(value);
    }
}
