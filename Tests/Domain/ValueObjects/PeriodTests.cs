using Domain.ValueObjects;

namespace Tests.Domain.ValueObjects
{
    public class PeriodTests
    {
        [Fact]
        public void Constructor_WithValidValues_ShouldCreatePeriod()
        {
            // Act
            Period period = new Period(6, 2024);

            // Assert
            Assert.Equal(6, period.Month);
            Assert.Equal(2024, period.Year);
        }

        [Fact]
        public void Constructor_WithInvalidYear_ShouldThrowArgumentException()
        {
            // Act & Assert
            ArgumentException exception = Assert.Throws<ArgumentException>(() => new Period(1, 1899));

            Assert.Contains("Year must be between 1900 and 2100", exception.Message);
        }

        [Fact]
        public void Constructor_WithInvalidMonth_ShouldThrowArgumentException()
        {
            // Act & Assert
            ArgumentException exception = Assert.Throws<ArgumentException>(() => new Period(13, 2024));

            Assert.Contains("Month must be between 1 and 12", exception.Message);
        }

        [Fact]
        public void IsSameMonth_WithSamePeriod_ShouldReturnTrue()
        {
            // Arrange
            Period period1 = new Period(6, 2024);
            Period period2 = new Period(6, 2024);

            // Act & Assert
            Assert.True(period1.IsSameMonth(period2));
        }

        [Fact]
        public void IsSameMonth_WithDifferentPeriod_ShouldReturnFalse()
        {
            // Arrange
            Period period1 = new Period(6, 2024);
            Period period2 = new Period(7, 2024);

            // Act & Assert
            Assert.False(period1.IsSameMonth(period2));
        }

        [Fact]
        public void NextMonth_ShouldReturnNextPeriod()
        {
            // Arrange
            Period period = new Period(6, 2024);

            // Act
            Period next = period.NextMonth();

            // Assert
            Assert.Equal(7, next.Month);
            Assert.Equal(2024, next.Year);
        }

        [Fact]
        public void NextMonth_ForDecember_ShouldReturnJanuaryNextYear()
        {
            // Arrange
            Period period = new Period(12, 2024);

            // Act
            Period next = period.NextMonth();

            // Assert
            Assert.Equal(1, next.Month);
            Assert.Equal(2025, next.Year);
        }

        [Fact]
        public void PreviousMonth_ShouldReturnPreviousPeriod()
        {
            // Arrange
            Period period = new Period(6, 2024);

            // Act
            Period previous = period.PreviousMonth();

            // Assert
            Assert.Equal(5, previous.Month);
            Assert.Equal(2024, previous.Year);
        }

        [Fact]
        public void PreviousMonth_ForJanuary_ShouldReturnDecemberLastYear()
        {
            // Arrange
            Period period = new Period(1, 2024);

            // Act
            Period previous = period.PreviousMonth();

            // Assert
            Assert.Equal(12, previous.Month);
            Assert.Equal(2023, previous.Year);
        }

        [Fact]
        public void Equals_WithSameValues_ShouldReturnTrue()
        {
            // Arrange
            Period period1 = new Period(6, 2024);
            Period period2 = new Period(6, 2024);

            // Act & Assert
            Assert.True(period1.Equals(period2));
            Assert.True(period1 == period2);
        }

        [Fact]
        public void ToString_ShouldReturnFormattedString()
        {
            // Arrange
            Period period = new Period(6, 2024);

            // Act
            string result = period.ToString();

            // Assert
            Assert.Equal("06-2024", result);
        }
    }
}