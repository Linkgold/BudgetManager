using Domain.ValueObjects;

namespace Tests.Domain.ValueObjects
{
    public class DailyPeriodTests
    {
        // ==================== CONSTRUCTOR ====================

        [Fact]
        public void Constructor_WithValidValues_ShouldCreateDailyPeriod()
        {
            // Act
            DailyPeriod date = new DailyPeriod(15, 6, 2024);

            // Assert
            Assert.Equal(15, date.Day);
            Assert.Equal(6, date.Month);
            Assert.Equal(2024, date.Year);
        }

        [Fact]
        public void Constructor_WithInvalidYear_ShouldThrowArgumentException()
        {
            // Act & Assert
            ArgumentException exception = Assert.Throws<ArgumentException>(() => new DailyPeriod(15, 6, 1899));

            Assert.Contains("Year must be between 1900 and 2100", exception.Message);
        }

        [Fact]
        public void Constructor_WithInvalidMonth_ShouldThrowArgumentException()
        {
            // Act & Assert
            ArgumentException exception = Assert.Throws<ArgumentException>(() => new DailyPeriod(15, 13, 2024));

            Assert.Contains("Month must be between 1 and 12", exception.Message);
        }

        [Fact]
        public void Constructor_WithInvalidDay_ShouldThrowArgumentException()
        {
            // Act & Assert
            ArgumentException exception = Assert.Throws<ArgumentException>(() => new DailyPeriod(31, 2, 2024));

            Assert.Contains("Day must be between 1 and 29", exception.Message);
        }

        [Fact]
        public void Constructor_WithInvalidDayInApril_ShouldThrowArgumentException()
        {
            // Act & Assert
            ArgumentException exception = Assert.Throws<ArgumentException>(() => new DailyPeriod(31, 4, 2024));

            Assert.Contains("Day must be between 1 and 30", exception.Message);
        }

        [Fact]
        public void Constructor_WithLeapYear_ShouldAllow29February()
        {
            // Act
            DailyPeriod date = new DailyPeriod(29, 2, 2024);

            // Assert
            Assert.Equal(29, date.Day);
            Assert.Equal(2, date.Month);
            Assert.Equal(2024, date.Year);
        }

        // ==================== TO DATE TIME ====================

        [Fact]
        public void ToDateTime_ShouldReturnCorrectDateTime()
        {
            // Arrange
            DailyPeriod date = new DailyPeriod(15, 6, 2024);

            // Act
            DateTime result = date.ToDateTime();

            // Assert
            Assert.Equal(new DateTime(2024, 6, 15), result);
        }

        // ==================== TO MONTHLY PERIOD ====================

        [Fact]
        public void ToMonthlyPeriod_ShouldReturnCorrectMonthlyPeriod()
        {
            // Arrange
            DailyPeriod date = new DailyPeriod(15, 6, 2024);

            // Act
            MonthlyPeriod result = date.ToMonthlyPeriod();

            // Assert
            Assert.Equal(6, result.Month);
            Assert.Equal(2024, result.Year);
        }

        // ==================== IS SAME DAY ====================

        [Fact]
        public void IsSameDay_WithSameDate_ShouldReturnTrue()
        {
            // Arrange
            DailyPeriod date1 = new DailyPeriod(15, 6, 2024);
            DailyPeriod date2 = new DailyPeriod(15, 6, 2024);

            // Act
            bool result = date1.IsSameDay(date2);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void IsSameDay_WithDifferentDay_ShouldReturnFalse()
        {
            // Arrange
            DailyPeriod date1 = new DailyPeriod(15, 6, 2024);
            DailyPeriod date2 = new DailyPeriod(16, 6, 2024);

            // Act
            bool result = date1.IsSameDay(date2);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void IsSameDay_WithNull_ShouldReturnFalse()
        {
            // Arrange
            DailyPeriod date = new DailyPeriod(15, 6, 2024);

            // Act
            bool result = date.IsSameDay(null);

            // Assert
            Assert.False(result);
        }

        // ==================== IS SAME MONTH ====================

        [Fact]
        public void IsSameMonth_WithSameMonth_ShouldReturnTrue()
        {
            // Arrange
            DailyPeriod date1 = new DailyPeriod(15, 6, 2024);
            DailyPeriod date2 = new DailyPeriod(20, 6, 2024);

            // Act
            bool result = date1.IsSameMonth(date2);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void IsSameMonth_WithDifferentMonth_ShouldReturnFalse()
        {
            // Arrange
            DailyPeriod date1 = new DailyPeriod(15, 6, 2024);
            DailyPeriod date2 = new DailyPeriod(15, 7, 2024);

            // Act
            bool result = date1.IsSameMonth(date2);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void IsSameMonth_WithMonthlyPeriod_ShouldReturnTrue()
        {
            // Arrange
            DailyPeriod date = new DailyPeriod(15, 6, 2024);
            MonthlyPeriod period = new MonthlyPeriod(6, 2024);

            // Act
            bool result = date.IsSameMonth(period);

            // Assert
            Assert.True(result);
        }

        // ==================== NEXT DAY ====================

        [Fact]
        public void NextDay_ShouldReturnNextDay()
        {
            // Arrange
            DailyPeriod date = new DailyPeriod(15, 6, 2024);

            // Act
            DailyPeriod result = date.NextDay();

            // Assert
            Assert.Equal(16, result.Day);
            Assert.Equal(6, result.Month);
            Assert.Equal(2024, result.Year);
        }

        [Fact]
        public void NextDay_AtEndOfMonth_ShouldReturnFirstDayOfNextMonth()
        {
            // Arrange
            DailyPeriod date = new DailyPeriod(30, 6, 2024);

            // Act
            DailyPeriod result = date.NextDay();

            // Assert
            Assert.Equal(1, result.Day);
            Assert.Equal(7, result.Month);
            Assert.Equal(2024, result.Year);
        }

        [Fact]
        public void NextDay_AtEndOfYear_ShouldReturnFirstDayOfNextYear()
        {
            // Arrange
            DailyPeriod date = new DailyPeriod(31, 12, 2024);

            // Act
            DailyPeriod result = date.NextDay();

            // Assert
            Assert.Equal(1, result.Day);
            Assert.Equal(1, result.Month);
            Assert.Equal(2025, result.Year);
        }

        // ==================== PREVIOUS DAY ====================

        [Fact]
        public void PreviousDay_ShouldReturnPreviousDay()
        {
            // Arrange
            DailyPeriod date = new DailyPeriod(15, 6, 2024);

            // Act
            DailyPeriod result = date.PreviousDay();

            // Assert
            Assert.Equal(14, result.Day);
            Assert.Equal(6, result.Month);
            Assert.Equal(2024, result.Year);
        }

        [Fact]
        public void PreviousDay_AtStartOfMonth_ShouldReturnLastDayOfPreviousMonth()
        {
            // Arrange
            DailyPeriod date = new DailyPeriod(1, 7, 2024);

            // Act
            DailyPeriod result = date.PreviousDay();

            // Assert
            Assert.Equal(30, result.Day);
            Assert.Equal(6, result.Month);
            Assert.Equal(2024, result.Year);
        }

        [Fact]
        public void PreviousDay_AtStartOfYear_ShouldReturnLastDayOfPreviousYear()
        {
            // Arrange
            DailyPeriod date = new DailyPeriod(1, 1, 2024);

            // Act
            DailyPeriod result = date.PreviousDay();

            // Assert
            Assert.Equal(31, result.Day);
            Assert.Equal(12, result.Month);
            Assert.Equal(2023, result.Year);
        }

        // ==================== NEXT MONTH ====================

        [Fact]
        public void NextMonth_ShouldReturnSameDayNextMonth()
        {
            // Arrange
            DailyPeriod date = new DailyPeriod(15, 6, 2024);

            // Act
            DailyPeriod result = date.NextMonth();

            // Assert
            Assert.Equal(15, result.Day);
            Assert.Equal(7, result.Month);
            Assert.Equal(2024, result.Year);
        }

        [Fact]
        public void NextMonth_WithInvalidDay_ShouldAdjustDay()
        {
            // Arrange
            DailyPeriod date = new DailyPeriod(31, 1, 2024);

            // Act
            DailyPeriod result = date.NextMonth();

            // Assert
            Assert.Equal(29, result.Day); // Febrero 2024 tiene 29 días (bisiesto)
            Assert.Equal(2, result.Month);
            Assert.Equal(2024, result.Year);
        }

        [Fact]
        public void NextMonth_FromDecember_ShouldReturnJanuaryNextYear()
        {
            // Arrange
            DailyPeriod date = new DailyPeriod(15, 12, 2024);

            // Act
            DailyPeriod result = date.NextMonth();

            // Assert
            Assert.Equal(15, result.Day);
            Assert.Equal(1, result.Month);
            Assert.Equal(2025, result.Year);
        }

        // ==================== PREVIOUS MONTH ====================

        [Fact]
        public void PreviousMonth_ShouldReturnSameDayPreviousMonth()
        {
            // Arrange
            DailyPeriod date = new DailyPeriod(15, 6, 2024);

            // Act
            DailyPeriod result = date.PreviousMonth();

            // Assert
            Assert.Equal(15, result.Day);
            Assert.Equal(5, result.Month);
            Assert.Equal(2024, result.Year);
        }

        [Fact]
        public void PreviousMonth_WithInvalidDay_ShouldAdjustDay()
        {
            // Arrange
            DailyPeriod date = new DailyPeriod(31, 3, 2024);

            // Act
            DailyPeriod result = date.PreviousMonth();

            // Assert
            Assert.Equal(29, result.Day); // Febrero 2024 tiene 29 días (bisiesto)
            Assert.Equal(2, result.Month);
            Assert.Equal(2024, result.Year);
        }

        [Fact]
        public void PreviousMonth_FromJanuary_ShouldReturnDecemberPreviousYear()
        {
            // Arrange
            DailyPeriod date = new DailyPeriod(15, 1, 2024);

            // Act
            DailyPeriod result = date.PreviousMonth();

            // Assert
            Assert.Equal(15, result.Day);
            Assert.Equal(12, result.Month);
            Assert.Equal(2023, result.Year);
        }

        // ==================== IS WEEKEND ====================

        [Fact]
        public void IsWeekend_OnSaturday_ShouldReturnTrue()
        {
            // Arrange
            DailyPeriod date = new DailyPeriod(15, 6, 2024); // 15/06/2024 es sábado

            // Act
            bool result = date.IsWeekend();

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void IsWeekend_OnSunday_ShouldReturnTrue()
        {
            // Arrange
            DailyPeriod date = new DailyPeriod(16, 6, 2024); // 16/06/2024 es domingo

            // Act
            bool result = date.IsWeekend();

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void IsWeekend_OnMonday_ShouldReturnFalse()
        {
            // Arrange
            DailyPeriod date = new DailyPeriod(17, 6, 2024); // 17/06/2024 es lunes

            // Act
            bool result = date.IsWeekend();

            // Assert
            Assert.False(result);
        }

        // ==================== EQUALITY ====================

        [Fact]
        public void Equals_WithSameValues_ShouldReturnTrue()
        {
            // Arrange
            DailyPeriod date1 = new DailyPeriod(15, 6, 2024);
            DailyPeriod date2 = new DailyPeriod(15, 6, 2024);

            // Act & Assert
            Assert.True(date1.Equals(date2));
            Assert.True(date1 == date2);
        }

        [Fact]
        public void Equals_WithDifferentValues_ShouldReturnFalse()
        {
            // Arrange
            DailyPeriod date1 = new DailyPeriod(15, 6, 2024);
            DailyPeriod date2 = new DailyPeriod(16, 6, 2024);

            // Act & Assert
            Assert.False(date1.Equals(date2));
            Assert.False(date1 == date2);
        }

        [Fact]
        public void GetHashCode_ShouldBeConsistent()
        {
            // Arrange
            DailyPeriod date1 = new DailyPeriod(15, 6, 2024);
            DailyPeriod date2 = new DailyPeriod(15, 6, 2024);

            // Act & Assert
            Assert.Equal(date1.GetHashCode(), date2.GetHashCode());
        }

        // ==================== TO STRING ====================

        [Fact]
        public void ToString_ShouldReturnFormattedString()
        {
            // Arrange
            DailyPeriod date = new DailyPeriod(15, 6, 2024);

            // Act
            string result = date.ToString();

            // Assert
            Assert.Equal("15/06/2024", result);
        }
    }
}