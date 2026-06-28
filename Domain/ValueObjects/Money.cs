namespace Domain.ValueObjects
{
    public class Money
    {
        public decimal Value { get; private set; }
        public string Currency { get; private set; }

        public Money(decimal value, string currency = "EUR")
        {
            if (value < 0) throw new ArgumentException("Amount cannot be negative", nameof(value));

            if (string.IsNullOrWhiteSpace(currency)) throw new ArgumentException("Currency cannot be empty", nameof(currency));

            if (currency.Length != 3) throw new ArgumentException("Currency must be a 3-letter ISO code", nameof(currency));

            Value = value;
            Currency = currency;
        }

        public bool Equals(Money other)
        {
            if (other is null) return false;
            return Value == other.Value && Currency == other.Currency;
        }

        public override bool Equals(object obj) => Equals(obj as Money);

        public override int GetHashCode() => HashCode.Combine(Value, Currency);

        public override string ToString() => $"{Value:F2} {Currency}";

        public static Money operator +(Money a, Money b)
        {
            if (a is null) throw new ArgumentNullException(nameof(a));
            if (b is null) throw new ArgumentNullException(nameof(b));

            if (a.Currency != b.Currency) throw new InvalidOperationException("Cannot add different currencies");

            return new Money(a.Value + b.Value, a.Currency);
        }

        public static Money operator -(Money a, Money b)
        {
            if (a is null) throw new ArgumentNullException(nameof(a));
            if (b is null) throw new ArgumentNullException(nameof(b));

            if (a.Currency != b.Currency) throw new InvalidOperationException("Cannot subtract different currencies");

            return new Money(a.Value - b.Value, a.Currency);
        }

        public static bool operator ==(Money a, Money b)
        {
            if (a is null && b is null) return true;
            if (a is null || b is null) return false;
            return a.Equals(b);
        }

        public static bool operator !=(Money a, Money b) => !(a == b);

        public static implicit operator decimal(Money money) => money.Value;
        public static explicit operator Money(decimal value) => new Money(value);
    }
}