using Domain.Entities;
using Domain.Enums;
using Domain.ValueObjects;

namespace Tests.Domain.Entities
{
    public class BudgetTests
    {
        private Category CreateCategory()
        {
            EntityInfo info = new EntityInfo("Alimentación", "Gastos de comida");
            return new Category(info);
        }

        [Fact]
        public void Constructor_WithValidValues_ShouldCreateBudget()
        {
            // Arrange
            Category category = CreateCategory();
            Money amount = new Money(500.00m, "EUR");
            MonthlyPeriod period = new MonthlyPeriod(1, 2024);

            // Act
            Budget budget = new Budget(category, amount, period);

            // Assert
            Assert.NotNull(budget);
            Assert.Equal(category.Id, budget.CategoryId);
            Assert.Equal(500.00m, budget.MonthlyAmount.Value);
            Assert.Equal(1, budget.Period.Month);
            Assert.Equal(2024, budget.Period.Year);
            Assert.NotEqual(default(DateTime), budget.CreatedAt);
            Assert.Null(budget.UpdatedAt);
        }

        [Fact]
        public void Constructor_WithNullCategory_ShouldThrowArgumentNullException()
        {
            // Arrange
            Money amount = new Money(500.00m, "EUR");
            MonthlyPeriod period = new MonthlyPeriod(1, 2024);

            // Act & Assert
            ArgumentNullException exception = Assert.Throws<ArgumentNullException>(() => new Budget(null, amount, period));

            Assert.Equal("category", exception.ParamName);
        }

        [Fact]
        public void Constructor_WithNullAmount_ShouldThrowArgumentNullException()
        {
            // Arrange
            Category category = CreateCategory();
            MonthlyPeriod period = new MonthlyPeriod(1, 2024);

            // Act & Assert
            ArgumentNullException exception = Assert.Throws<ArgumentNullException>(() => new Budget(category, null, period));

            Assert.Equal("monthlyAmount", exception.ParamName);
        }

        [Fact]
        public void Constructor_WithNullPeriod_ShouldThrowArgumentNullException()
        {
            // Arrange
            Category category = CreateCategory();
            Money amount = new Money(500.00m, "EUR");

            // Act & Assert
            ArgumentNullException exception = Assert.Throws<ArgumentNullException>(() => new Budget(category, amount, null));

            Assert.Equal("period", exception.ParamName);
        }

        [Fact]
        public void UpdateAmount_WithValidValue_ShouldUpdateAmount()
        {
            // Arrange
            Category category = CreateCategory();
            Money amount = new Money(500.00m, "EUR");
            MonthlyPeriod period = new MonthlyPeriod(1, 2024);
            Budget budget = new Budget(category, amount, period);

            // Act
            Money newAmount = new Money(600.00m, "EUR");
            budget.UpdateAmount(newAmount);

            // Assert
            Assert.Equal(600.00m, budget.MonthlyAmount.Value);
            Assert.NotNull(budget.UpdatedAt);
        }

        [Fact]
        public void UpdateAmount_WithNullAmount_ShouldThrowArgumentNullException()
        {
            // Arrange
            Category category = CreateCategory();
            Money amount = new Money(500.00m, "EUR");
            MonthlyPeriod period = new MonthlyPeriod(1, 2024);
            Budget budget = new Budget(category, amount, period);

            // Act & Assert
            ArgumentNullException exception = Assert.Throws<ArgumentNullException>(() => budget.UpdateAmount(null));

            Assert.Equal("newAmount", exception.ParamName);
        }

        [Fact]
        public void GetStatus_WithZeroTotalSpent_ShouldReturnGreen()
        {
            // Arrange
            Category category = CreateCategory();
            Money amount = new Money(500.00m, "EUR");
            MonthlyPeriod period = new MonthlyPeriod(1, 2024);
            Budget budget = new Budget(category, amount, period);

            // Act
            BudgetStatusEnum status = budget.GetStatus(0);

            // Assert
            Assert.Equal(BudgetStatusEnum.Green, status);
        }

        [Fact]
        public void GetStatus_WithSpentLessThan80Percent_ShouldReturnGreen()
        {
            // Arrange
            Category category = CreateCategory();
            Money amount = new Money(500.00m, "EUR");
            MonthlyPeriod period = new MonthlyPeriod(1, 2024);
            Budget budget = new Budget(category, amount, period);

            // Act
            BudgetStatusEnum status = budget.GetStatus(350.00m); // 70%

            // Assert
            Assert.Equal(BudgetStatusEnum.Green, status);
        }

        [Fact]
        public void GetStatus_WithSpentBetween80And100Percent_ShouldReturnYellow()
        {
            // Arrange
            Category category = CreateCategory();
            Money amount = new Money(500.00m, "EUR");
            MonthlyPeriod period = new MonthlyPeriod(1, 2024);
            Budget budget = new Budget(category, amount, period);

            // Act
            BudgetStatusEnum status = budget.GetStatus(450.00m); // 90%

            // Assert
            Assert.Equal(BudgetStatusEnum.Yellow, status);
        }

        [Fact]
        public void GetStatus_WithSpentGreaterThan100Percent_ShouldReturnRed()
        {
            // Arrange
            Category category = CreateCategory();
            Money amount = new Money(500.00m, "EUR");
            MonthlyPeriod period = new MonthlyPeriod(1, 2024);
            Budget budget = new Budget(category, amount, period);

            // Act
            BudgetStatusEnum status = budget.GetStatus(550.00m); // 110%

            // Assert
            Assert.Equal(BudgetStatusEnum.Red, status);
        }

        [Fact]
        public void GetStatus_WithZeroBudget_ShouldAlwaysReturnGreen()
        {
            // Arrange
            Category category = CreateCategory();
            Money amount = new Money(0, "EUR");
            MonthlyPeriod period = new MonthlyPeriod(1, 2024);
            Budget budget = new Budget(category, amount, period);

            // Act
            BudgetStatusEnum status = budget.GetStatus(100.00m);

            // Assert
            Assert.Equal(BudgetStatusEnum.Green, status);
        }

        [Fact]
        public void GetStatus_WithNegativeTotalSpent_ShouldThrowArgumentException()
        {
            // Arrange
            Category category = CreateCategory();
            Money amount = new Money(500.00m, "EUR");
            MonthlyPeriod period = new MonthlyPeriod(1, 2024);
            Budget budget = new Budget(category, amount, period);

            // Act & Assert
            ArgumentException exception = Assert.Throws<ArgumentException>(() => budget.GetStatus(-100.00m));

            Assert.Contains("Total spent cannot be negative", exception.Message);
        }

        [Fact]
        public void GetPercentageUsed_ShouldReturnCorrectPercentage()
        {
            // Arrange
            Category category = CreateCategory();
            Money amount = new Money(500.00m, "EUR");
            MonthlyPeriod period = new MonthlyPeriod(1, 2024);
            Budget budget = new Budget(category, amount, period);

            // Act
            decimal percentage = budget.GetPercentageUsed(350.00m);

            // Assert
            Assert.Equal(70m, percentage);
        }

        [Fact]
        public void GetPercentageUsed_ShouldNotExceed100Percent()
        {
            // Arrange
            Category category = CreateCategory();
            Money amount = new Money(500.00m, "EUR");
            MonthlyPeriod period = new MonthlyPeriod(1, 2024);
            Budget budget = new Budget(category, amount, period);

            // Act
            decimal percentage = budget.GetPercentageUsed(600.00m);

            // Assert
            Assert.Equal(100m, percentage);
        }

        [Fact]
        public void GetPercentageUsed_WithZeroBudget_ShouldReturnZero()
        {
            // Arrange
            Category category = CreateCategory();
            Money amount = new Money(0, "EUR");
            MonthlyPeriod period = new MonthlyPeriod(1, 2024);
            Budget budget = new Budget(category, amount, period);

            // Act
            decimal percentage = budget.GetPercentageUsed(100.00m);

            // Assert
            Assert.Equal(0m, percentage);
        }

        [Fact]
        public void GetRemaining_ShouldReturnCorrectRemaining()
        {
            // Arrange
            Category category = CreateCategory();
            Money amount = new Money(500.00m, "EUR");
            MonthlyPeriod period = new MonthlyPeriod(1, 2024);
            Budget budget = new Budget(category, amount, period);

            // Act
            Money remaining = budget.GetRemaining(350.00m);

            // Assert
            Assert.Equal(150.00m, remaining.Value);
            Assert.Equal("EUR", remaining.Currency);
        }

        [Fact]
        public void GetRemaining_ShouldNotReturnNegative()
        {
            // Arrange
            Category category = CreateCategory();
            Money amount = new Money(500.00m, "EUR");
            MonthlyPeriod period = new MonthlyPeriod(1, 2024);
            Budget budget = new Budget(category, amount, period);

            // Act
            Money remaining = budget.GetRemaining(600.00m);

            // Assert
            Assert.Equal(0, remaining.Value);
        }

        [Fact]
        public void IsOverBudget_WithSpentGreaterThanBudget_ShouldReturnTrue()
        {
            // Arrange
            Category category = CreateCategory();
            Money amount = new Money(500.00m, "EUR");
            MonthlyPeriod period = new MonthlyPeriod(1, 2024);
            Budget budget = new Budget(category, amount, period);

            // Act
            bool isOver = budget.IsOverBudget(550.00m);

            // Assert
            Assert.True(isOver);
        }

        [Fact]
        public void IsOverBudget_WithSpentLessThanBudget_ShouldReturnFalse()
        {
            // Arrange
            Category category = CreateCategory();
            Money amount = new Money(500.00m, "EUR");
            MonthlyPeriod period = new MonthlyPeriod(1, 2024);
            Budget budget = new Budget(category, amount, period);

            // Act
            bool isOver = budget.IsOverBudget(350.00m);

            // Assert
            Assert.False(isOver);
        }

        [Fact]
        public void IsOverBudget_WithSpentEqualToBudget_ShouldReturnFalse()
        {
            // Arrange
            Category category = CreateCategory();
            Money amount = new Money(500.00m, "EUR");
            MonthlyPeriod period = new MonthlyPeriod(1, 2024);
            Budget budget = new Budget(category, amount, period);

            // Act
            bool isOver = budget.IsOverBudget(500.00m);

            // Assert
            Assert.False(isOver);
        }

        [Fact]
        public void ToString_ShouldReturnFormattedString()
        {
            // Arrange
            Category category = CreateCategory();
            Money amount = new Money(500.00m, "EUR");
            MonthlyPeriod period = new MonthlyPeriod(1, 2024);
            Budget budget = new Budget(category, amount, period);

            // Act
            string result = budget.ToString();

            // Assert
            Assert.Contains("Alimentación", result);
            Assert.Contains("500,00 EUR", result);
            Assert.Contains("01-2024", result);
        }
    }
}