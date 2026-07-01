using Domain.ValueObjects;

namespace Tests.Domain.ValueObjects
{
    public class MoneyTests
    {
        [Fact]
        public void Constructor_WithValidValues_ShouldCreateMoney()
        {
            // Act
            Money money = new Money(100.50m, "EUR");

            // Assert
            Assert.Equal(100.50m, money.Value);
            Assert.Equal("EUR", money.Currency);
        }

        [Fact]
        public void Constructor_WithNegativeValue_ShouldThrowArgumentException()
        {
            // Act & Assert
            ArgumentException exception = Assert.Throws<ArgumentException>(() => new Money(-10.00m, "EUR"));

            Assert.Contains("Amount cannot be negative", exception.Message);
        }

        [Fact]
        public void Constructor_WithInvalidCurrency_ShouldThrowArgumentException()
        {
            // Act & Assert
            ArgumentException exception = Assert.Throws<ArgumentException>(() => new Money(100.00m, "EURO"));

            Assert.Contains("Currency must be a 3-letter ISO code", exception.Message);
        }

        [Fact]
        public void Constructor_WithNullCurrency_ShouldThrowArgumentException()
        {
            // Act & Assert
            ArgumentException exception = Assert.Throws<ArgumentException>(() => new Money(100.00m, null));

            Assert.Contains("Currency cannot be empty", exception.Message);
        }

        [Fact]
        public void Constructor_WithEmptyCurrency_ShouldThrowArgumentException()
        {
            // Act & Assert
            ArgumentException exception = Assert.Throws<ArgumentException>(() => new Money(100.00m, ""));

            Assert.Contains("Currency cannot be empty", exception.Message);
        }

        [Fact]
        public void Constructor_ShouldTruncateToTwoDecimals()
        {
            // Act
            Money money = new Money(100.567m, "EUR");

            // Assert
            Assert.Equal(100.56m, money.Value); // Trunca, no redondea
        }

        [Fact]
        public void Constructor_WithTwoDecimals_ShouldNotChange()
        {
            // Act
            Money money = new Money(100.56m, "EUR");

            // Assert
            Assert.Equal(100.56m, money.Value);
        }

        [Fact]
        public void Constructor_WithMidpointValue_ShouldTruncateDown()
        {
            // Act
            Money money = new Money(2.345m, "EUR");

            // Assert
            Assert.Equal(2.34m, money.Value); // Trunca, no redondea
        }

        [Fact]
        public void AddOperator_WithSameCurrency_ShouldReturnSum()
        {
            // Arrange
            Money money1 = new Money(100.00m, "EUR");
            Money money2 = new Money(50.50m, "EUR");

            // Act
            Money result = money1 + money2;

            // Assert
            Assert.Equal(150.50m, result.Value);
            Assert.Equal("EUR", result.Currency);
        }

        [Fact]
        public void AddOperator_WithDifferentCurrencies_ShouldThrowInvalidOperationException()
        {
            // Arrange
            Money money1 = new Money(100.00m, "EUR");
            Money money2 = new Money(50.50m, "USD");

            // Act & Assert
            InvalidOperationException exception = Assert.Throws<InvalidOperationException>(() => money1 + money2);

            Assert.Contains("Cannot add different currencies", exception.Message);
        }

        [Fact]
        public void Equals_WithSameValueAndCurrency_ShouldReturnTrue()
        {
            // Arrange
            Money money1 = new Money(100.00m, "EUR");
            Money money2 = new Money(100.00m, "EUR");

            // Act & Assert
            Assert.True(money1.Equals(money2));
            Assert.True(money1 == money2);
        }

        [Fact]
        public void Equals_WithDifferentValue_ShouldReturnFalse()
        {
            // Arrange
            Money money1 = new Money(100.00m, "EUR");
            Money money2 = new Money(200.00m, "EUR");

            // Act & Assert
            Assert.False(money1.Equals(money2));
        }

        [Fact]
        public void Equals_WithDifferentCurrency_ShouldReturnFalse()
        {
            // Arrange
            Money money1 = new Money(100.00m, "EUR");
            Money money2 = new Money(100.00m, "USD");

            // Act & Assert
            Assert.False(money1.Equals(money2));
        }

        [Fact]
        public void ToString_ShouldReturnFormattedString()
        {
            // Arrange
            Money money = new Money(100.50m, "EUR");

            // Act
            string result = money.ToString();

            // Assert
            Assert.Equal("100,50 EUR", result);
        }
    }
}