using System.Globalization;

namespace Domain.ValueObjects
{
    public class Money
    {
        public decimal Value { get; private set; }
        public string Currency { get; private set; }

        // 🔥 Constructor privado SOLO para EF Core
#pragma warning disable CS8618 // Un campo que no acepta valores NULL debe contener un valor distinto de NULL al salir del constructor. Considere la posibilidad de agregar el modificador "required" o declararlo como un valor que acepta valores NULL.
        private Money() { }
#pragma warning restore CS8618 // Un campo que no acepta valores NULL debe contener un valor distinto de NULL al salir del constructor. Considere la posibilidad de agregar el modificador "required" o declararlo como un valor que acepta valores NULL.

        public Money(decimal value, string currency = "EUR", bool allowNegative = false)
        {
            if (!allowNegative && value < 0) throw new ArgumentException("Amount cannot be negative", nameof(value));

            if (string.IsNullOrWhiteSpace(currency)) throw new ArgumentException("Currency cannot be empty", nameof(currency));

            if (currency.Length != 3) throw new ArgumentException("Currency must be a 3-letter ISO code", nameof(currency));

            Value = Math.Truncate(value * 100) / 100;
            Currency = currency.ToUpperInvariant();
        }

        public bool Equals(Money other)
        {
            if (other is null) return false;
            return Value == other.Value && Currency == other.Currency;
        }

        public override bool Equals(object? obj) => obj is Money other && Equals(other);

        public override int GetHashCode() => HashCode.Combine(Value, Currency);

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