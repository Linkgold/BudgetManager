namespace Domain.ValueObjects
{
    public class Money
    {
        public decimal Value { get; private set; }
        public string Currency { get; private set; }

        public Money(decimal value, string currency = "EUR")
        {
            if (value < 0)
                throw new ArgumentException("Amount cannot be negative", nameof(value));

            Value = value;
            Currency = currency;
        }

        public static Money operator +(Money a, Money b)
        {
            if (a.Currency != b.Currency)
                throw new InvalidOperationException("Cannot add different currencies");

            return new Money(a.Value + b.Value, a.Currency);
        }

        public static Money operator -(Money a, Money b)
        {
            if (a.Currency != b.Currency)
                throw new InvalidOperationException("Cannot subtract different currencies");

            return new Money(a.Value - b.Value, a.Currency);
        }

        public override string ToString() => $"{Value:F2} {Currency}";
    }
}