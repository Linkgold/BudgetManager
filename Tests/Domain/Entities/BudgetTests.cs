using Domain.Entities;
using Domain.Enums;
using Domain.ValueObjects;
using Tests.Helpers;

namespace Tests.Domain.Entities
{
    public class BudgetTests
    {
        [Fact]
        public void Constructor_WithValidValues_ShouldCreateBudget()
        {
            // Arrange
            User user = TestDataFactory.CreateUser();
            Category category = TestDataFactory.CreateCategory();
            Money amount = TestDataFactory.CreateMoney(500.00m);
            MonthlyPeriod period = TestDataFactory.CreateMonthlyPeriod();

            // Act
            Budget budget = TestDataFactory.CreateBudget(user, category, amount, period);

            // Assert
            Assert.NotNull(budget);
            Assert.Equal(category.Id, budget.CategoryId);
            Assert.Equal(500.00m, budget.MonthlyAmount.Value);
            Assert.Equal(TestDataFactory.DEFAULT_MONTHLY_MONTH, budget.Period.Month);
            Assert.Equal(TestDataFactory.DEFAULT_YEAR, budget.Period.Year);
            Assert.NotEqual(default, budget.CreatedAt);
            Assert.Null(budget.UpdatedAt);
        }

        [Fact]
        public void Constructor_WithNullUser_ShouldThrowArgumentNullException()
        {
            // Arrange
            Category category = TestDataFactory.CreateCategory();
            Money amount = TestDataFactory.CreateMoney(500.00m);
            MonthlyPeriod period = TestDataFactory.CreateMonthlyPeriod();

            // Act & Assert
            ArgumentNullException exception = Assert.Throws<ArgumentNullException>(() => TestDataFactory.CreateBudget(null, category, amount, period));

            Assert.Equal("user", exception.ParamName);
        }

        [Fact]
        public void Constructor_WithNullCategory_ShouldThrowArgumentNullException()
        {
            // Arrange
            User user = TestDataFactory.CreateUser();
            Money amount = TestDataFactory.CreateMoney(500.00m);
            MonthlyPeriod period = TestDataFactory.CreateMonthlyPeriod();

            // Act & Assert
            ArgumentNullException exception = Assert.Throws<ArgumentNullException>(() => TestDataFactory.CreateBudget(user, null, amount, period));

            Assert.Equal("category", exception.ParamName);
        }

        [Fact]
        public void Constructor_WithNullAmount_ShouldThrowArgumentNullException()
        {
            // Arrange
            User user = TestDataFactory.CreateUser();
            Category category = TestDataFactory.CreateCategory();
            MonthlyPeriod period = TestDataFactory.CreateMonthlyPeriod();

            // Act & Assert
            ArgumentNullException exception = Assert.Throws<ArgumentNullException>(() => TestDataFactory.CreateBudget(user, category, null, period));

            Assert.Equal("monthlyAmount", exception.ParamName);
        }

        [Fact]
        public void Constructor_WithNullPeriod_ShouldThrowArgumentNullException()
        {
            // Arrange
            User user = TestDataFactory.CreateUser();
            Category category = TestDataFactory.CreateCategory();
            Money amount = TestDataFactory.CreateMoney(500.00m);

            // Act & Assert
            ArgumentNullException exception = Assert.Throws<ArgumentNullException>(() => TestDataFactory.CreateBudget(user, category, amount, null));

            Assert.Equal("period", exception.ParamName);
        }

        [Fact]
        public void UpdateAmount_WithValidValue_ShouldUpdateAmount()
        {
            // Arrange
            User user = TestDataFactory.CreateUser();
            Category category = TestDataFactory.CreateCategory();
            Money amount = TestDataFactory.CreateMoney(500.00m);
            MonthlyPeriod period = TestDataFactory.CreateMonthlyPeriod();
            Budget budget = TestDataFactory.CreateBudget(user, category, amount, period);

            // Act
            Money newAmount = TestDataFactory.CreateMoney(600.00m);
            budget.UpdateAmount(newAmount);

            // Assert
            Assert.Equal(600.00m, budget.MonthlyAmount.Value);
            Assert.NotNull(budget.UpdatedAt);
        }

        [Fact]
        public void UpdateAmount_WithNullAmount_ShouldThrowArgumentNullException()
        {
            // Arrange
            User user = TestDataFactory.CreateUser();
            Category category = TestDataFactory.CreateCategory();
            Money amount = TestDataFactory.CreateMoney(500.00m);
            MonthlyPeriod period = TestDataFactory.CreateMonthlyPeriod();
            Budget budget = TestDataFactory.CreateBudget(user, category, amount, period);

            // Act & Assert
            ArgumentNullException exception = Assert.Throws<ArgumentNullException>(() => budget.UpdateAmount(null));

            Assert.Equal("newAmount", exception.ParamName);
        }

        [Fact]
        public void GetStatus_WithZeroTotalSpent_ShouldReturnGreen()
        {
            // Arrange
            User user = TestDataFactory.CreateUser();
            Category category = TestDataFactory.CreateCategory();
            Money amount = TestDataFactory.CreateMoney(500.00m);
            MonthlyPeriod period = TestDataFactory.CreateMonthlyPeriod();
            Budget budget = TestDataFactory.CreateBudget(user, category, amount, period);

            // Act
            BudgetStatusEnum status = budget.GetStatus(0);

            // Assert
            Assert.Equal(BudgetStatusEnum.Green, status);
        }

        [Fact]
        public void GetStatus_WithSpentLessThan80Percent_ShouldReturnGreen()
        {
            // Arrange
            User user = TestDataFactory.CreateUser();
            Category category = TestDataFactory.CreateCategory();
            Money amount = TestDataFactory.CreateMoney(500.00m);
            MonthlyPeriod period = TestDataFactory.CreateMonthlyPeriod();
            Budget budget = TestDataFactory.CreateBudget(user, category, amount, period);

            // Act
            BudgetStatusEnum status = budget.GetStatus(350.00m); // 70%

            // Assert
            Assert.Equal(BudgetStatusEnum.Green, status);
        }

        [Fact]
        public void GetStatus_WithSpentBetween80And100Percent_ShouldReturnYellow()
        {
            // Arrange
            User user = TestDataFactory.CreateUser();
            Category category = TestDataFactory.CreateCategory();
            Money amount = TestDataFactory.CreateMoney(500.00m);
            MonthlyPeriod period = TestDataFactory.CreateMonthlyPeriod();
            Budget budget = TestDataFactory.CreateBudget(user, category, amount, period);

            // Act
            BudgetStatusEnum status = budget.GetStatus(450.00m); // 90%

            // Assert
            Assert.Equal(BudgetStatusEnum.Yellow, status);
        }

        [Fact]
        public void GetStatus_WithSpentGreaterThan100Percent_ShouldReturnRed()
        {
            // Arrange
            User user = TestDataFactory.CreateUser();
            Category category = TestDataFactory.CreateCategory();
            Money amount = TestDataFactory.CreateMoney(500.00m);
            MonthlyPeriod period = TestDataFactory.CreateMonthlyPeriod();
            Budget budget = TestDataFactory.CreateBudget(user, category, amount, period);

            // Act
            BudgetStatusEnum status = budget.GetStatus(550.00m); // 110%

            // Assert
            Assert.Equal(BudgetStatusEnum.Red, status);
        }

        [Fact]
        public void GetStatus_WithZeroBudget_ShouldAlwaysReturnGreen()
        {
            // Arrange
            User user = TestDataFactory.CreateUser();
            Category category = TestDataFactory.CreateCategory();
            Money amount = TestDataFactory.CreateMoney(0);
            MonthlyPeriod period = TestDataFactory.CreateMonthlyPeriod();
            Budget budget = TestDataFactory.CreateBudget(user, category, amount, period);

            // Act
            BudgetStatusEnum status = budget.GetStatus(100.00m);

            // Assert
            Assert.Equal(BudgetStatusEnum.Green, status);
        }

        [Fact]
        public void GetStatus_WithNegativeTotalSpent_ShouldThrowArgumentException()
        {
            // Arrange
            User user = TestDataFactory.CreateUser();
            Category category = TestDataFactory.CreateCategory();
            Money amount = TestDataFactory.CreateMoney(500.00m);
            MonthlyPeriod period = TestDataFactory.CreateMonthlyPeriod();
            Budget budget = TestDataFactory.CreateBudget(user, category, amount, period);

            // Act & Assert
            ArgumentException exception = Assert.Throws<ArgumentException>(() => budget.GetStatus(-100.00m));

            Assert.Contains("Total spent cannot be negative", exception.Message);
        }

        [Fact]
        public void GetPercentageUsed_ShouldReturnCorrectPercentage()
        {
            // Arrange
            User user = TestDataFactory.CreateUser();
            Category category = TestDataFactory.CreateCategory();
            Money amount = TestDataFactory.CreateMoney(500.00m);
            MonthlyPeriod period = TestDataFactory.CreateMonthlyPeriod();
            Budget budget = TestDataFactory.CreateBudget(user, category, amount, period);

            // Act
            decimal percentage = budget.GetPercentageUsed(350.00m);

            // Assert
            Assert.Equal(70m, percentage);
        }

        [Fact]
        public void GetPercentageUsed_ShouldNotExceed100Percent()
        {
            // Arrange
            User user = TestDataFactory.CreateUser();
            Category category =     TestDataFactory.CreateCategory();
            Money amount = TestDataFactory.CreateMoney(500.00m);
            MonthlyPeriod period = TestDataFactory.CreateMonthlyPeriod();
            Budget budget = TestDataFactory.CreateBudget(user, category, amount, period);

            // Act
            decimal percentage = budget.GetPercentageUsed(600.00m);

            // Assert
            Assert.Equal(100m, percentage);
        }

        [Fact]
        public void GetPercentageUsed_WithZeroBudget_ShouldReturnZero()
        {
            // Arrange
            User user = TestDataFactory.CreateUser();
            Category category = TestDataFactory.CreateCategory();
            Money amount = TestDataFactory.CreateMoney(0);
            MonthlyPeriod period = TestDataFactory.CreateMonthlyPeriod();
            Budget budget = TestDataFactory.CreateBudget(user, category, amount, period);

            // Act
            decimal percentage = budget.GetPercentageUsed(100.00m);

            // Assert
            Assert.Equal(0m, percentage);
        }

        [Fact]
        public void GetRemaining_ShouldReturnCorrectRemaining()
        {
            // Arrange
            User user = TestDataFactory.CreateUser();
            Category category = TestDataFactory.CreateCategory();
            Money amount = TestDataFactory.CreateMoney(500.00m);
            MonthlyPeriod period = TestDataFactory.CreateMonthlyPeriod();
            Budget budget = TestDataFactory.CreateBudget(user, category, amount, period);

            // Act
            Money remaining = budget.GetRemaining(350.00m);

            // Assert
            Assert.Equal(150.00m, remaining.Value);
            Assert.Equal(TestDataFactory.DEFAULT_CURRENCY, remaining.Currency);
        }

        [Fact]
        public void GetRemaining_ShouldNotReturnNegative()
        {
            // Arrange
            User user = TestDataFactory.CreateUser();
            Category category = TestDataFactory.CreateCategory();
            Money amount = TestDataFactory.CreateMoney(500.00m);
            MonthlyPeriod period = TestDataFactory.CreateMonthlyPeriod();
            Budget budget = TestDataFactory.CreateBudget(user, category, amount, period);

            // Act
            Money remaining = budget.GetRemaining(600.00m);

            // Assert
            Assert.Equal(0, remaining.Value);
        }

        [Fact]
        public void IsOverBudget_WithSpentGreaterThanBudget_ShouldReturnTrue()
        {
            // Arrange
            User user = TestDataFactory.CreateUser();
            Category category = TestDataFactory.CreateCategory();
            Money amount = TestDataFactory.CreateMoney(500.00m);
            MonthlyPeriod period = TestDataFactory.CreateMonthlyPeriod();
            Budget budget = TestDataFactory.CreateBudget(user, category, amount, period);

            // Act
            bool isOver = budget.IsOverBudget(550.00m);

            // Assert
            Assert.True(isOver);
        }

        [Fact]
        public void IsOverBudget_WithSpentLessThanBudget_ShouldReturnFalse()
        {
            // Arrange
            User user = TestDataFactory.CreateUser();
            Category category = TestDataFactory.CreateCategory();
            Money amount = TestDataFactory.CreateMoney(500.00m);
            MonthlyPeriod period = TestDataFactory.CreateMonthlyPeriod();
            Budget budget = TestDataFactory.CreateBudget(user, category, amount, period);

            // Act
            bool isOver = budget.IsOverBudget(350.00m);

            // Assert
            Assert.False(isOver);
        }

        [Fact]
        public void IsOverBudget_WithSpentEqualToBudget_ShouldReturnFalse()
        {
            // Arrange
            User user = TestDataFactory.CreateUser();
            Category category = TestDataFactory.CreateCategory();
            Money amount = TestDataFactory.CreateMoney(500.00m);
            MonthlyPeriod period = TestDataFactory.CreateMonthlyPeriod();
            Budget budget = TestDataFactory.CreateBudget(user, category, amount, period);

            // Act
            bool isOver = budget.IsOverBudget(500.00m);

            // Assert
            Assert.False(isOver);
        }

        [Fact]
        public void ToString_ShouldReturnFormattedString()
        {
            // Arrange
            User user = TestDataFactory.CreateUser();
            Category category = TestDataFactory.CreateCategory();
            Money amount = TestDataFactory.CreateMoney(500.00m);
            MonthlyPeriod period = TestDataFactory.CreateMonthlyPeriod();
            Budget budget = TestDataFactory.CreateBudget(user, category, amount, period);

            // Act
            string result = budget.ToString();

            // Assert
            Assert.Contains(TestDataFactory.DEFAULT_CATEGORY_NAME, result);
            Assert.Contains("500,00 EUR", result);
            Assert.Contains("01-2024", result);
        }
    }
}