using Domain.Entities;
using Contracts.Enums;
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
            Category category = TestDataFactory.CreateCategory();

            // Act
            Budget budget = TestDataFactory.CreateBudgetWithoutId(category: category);

            // Assert
            Assert.NotNull(budget);
            Assert.Equal(category.Id, budget.CategoryId);
            Assert.Equal(TestDataFactory.DEFAULT_MONEY_AMOUNT, budget.MonthlyAmount.Value);
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
            Money amount = TestDataFactory.CreateMoney();
            MonthlyPeriod period = TestDataFactory.CreateMonthlyPeriod();

            // Act & Assert
            ArgumentNullException exception = Assert.Throws<ArgumentNullException>(() => TestDataFactory.CreateBudgetWithoutAutoCreation(null, category, amount, period));

            Assert.Equal("user", exception.ParamName);
        }

        [Fact]
        public void Constructor_WithNullCategory_ShouldThrowArgumentNullException()
        {
            // Arrange
            User user = TestDataFactory.CreateUser();
            Money amount = TestDataFactory.CreateMoney();
            MonthlyPeriod period = TestDataFactory.CreateMonthlyPeriod();

            // Act & Assert
            ArgumentNullException exception = Assert.Throws<ArgumentNullException>(() => TestDataFactory.CreateBudgetWithoutAutoCreation(user, null, amount, period));

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
            ArgumentNullException exception = Assert.Throws<ArgumentNullException>(() => TestDataFactory.CreateBudgetWithoutAutoCreation(user, category, null, period));

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
            ArgumentNullException exception = Assert.Throws<ArgumentNullException>(() => TestDataFactory.CreateBudgetWithoutAutoCreation(user, category, amount, null));

            Assert.Equal("period", exception.ParamName);
        }

        [Fact]
        public void UpdateAmount_WithValidValue_ShouldUpdateAmount()
        {
            // Arrange
            decimal updatedAmount = 600.00m;
            Budget budget = TestDataFactory.CreateBudgetWithoutId();

            // Act
            budget.UpdateAmount(TestDataFactory.CreateMoney(updatedAmount));

            // Assert
            Assert.Equal(updatedAmount, budget.MonthlyAmount.Value);
            Assert.NotNull(budget.UpdatedAt);
        }

        [Fact]
        public void UpdateAmount_WithNullAmount_ShouldThrowArgumentNullException()
        {
            // Arrange
            Budget budget = TestDataFactory.CreateBudgetWithoutId();

            // Act & Assert
            ArgumentNullException exception = Assert.Throws<ArgumentNullException>(() => budget.UpdateAmount(null));

            Assert.Equal("amount", exception.ParamName);
        }

        [Fact]
        public void GetStatus_WithZeroTotalSpent_ShouldReturnGreen()
        {
            // Arrange
            Budget budget = TestDataFactory.CreateBudgetWithoutId();

            // Act
            BudgetStatusEnum status = budget.GetStatus(0);

            // Assert
            Assert.Equal(BudgetStatusEnum.Green, status);
        }

        [Fact]
        public void GetStatus_WithSpentLessThan80Percent_ShouldReturnGreen()
        {
            // Arrange
            Budget budget = TestDataFactory.CreateBudgetWithoutId();

            // Act
            BudgetStatusEnum status = budget.GetStatus(70.00m); // 70%

            // Assert
            Assert.Equal(BudgetStatusEnum.Green, status);
        }

        [Fact]
        public void GetStatus_WithSpentBetween80And100Percent_ShouldReturnYellow()
        {
            // Arrange
            Budget budget = TestDataFactory.CreateBudgetWithoutId();

            // Act
            BudgetStatusEnum status = budget.GetStatus(90.00m); // 90%

            // Assert
            Assert.Equal(BudgetStatusEnum.Yellow, status);
        }

        [Fact]
        public void GetStatus_WithSpentGreaterThan100Percent_ShouldReturnRed()
        {
            // Arrange
            Budget budget = TestDataFactory.CreateBudgetWithoutId();

            // Act
            BudgetStatusEnum status = budget.GetStatus(110.00m); // 110%

            // Assert
            Assert.Equal(BudgetStatusEnum.Red, status);
        }

        [Fact]
        public void GetStatus_WithZeroBudget_ShouldAlwaysReturnGreen()
        {
            // Arrange
            Budget budget = TestDataFactory.CreateBudgetWithoutId(money: TestDataFactory.CreateMoney(0));

            // Act
            BudgetStatusEnum status = budget.GetStatus(100.00m);

            // Assert
            Assert.Equal(BudgetStatusEnum.Green, status);
        }

        [Fact]
        public void GetStatus_WithNegativeTotalSpent_ShouldThrowArgumentException()
        {
            // Arrange
            Budget budget = TestDataFactory.CreateBudgetWithoutId();

            // Act & Assert
            ArgumentException exception = Assert.Throws<ArgumentException>(() => budget.GetStatus(-100.00m));
            Assert.Contains("Total spent cannot be negative", exception.Message);
        }

        [Fact]
        public void GetPercentageUsed_ShouldReturnCorrectPercentage()
        {
            // Arrange
            Budget budget = TestDataFactory.CreateBudgetWithoutId();

            // Act
            decimal percentage = budget.GetPercentageUsed(70.00m);

            // Assert
            Assert.Equal(70m, percentage);
        }

        [Fact]
        public void GetPercentageUsed_ShouldNotExceed100Percent()
        {
            // Arrange
            Budget budget = TestDataFactory.CreateBudgetWithoutId();

            // Act
            decimal percentage = budget.GetPercentageUsed(200m);

            // Assert
            Assert.Equal(100m, percentage);
        }

        [Fact]
        public void GetPercentageUsed_WithZeroBudget_ShouldReturnZero()
        {
            // Arrange
            Budget budget = TestDataFactory.CreateBudgetWithoutId(money: TestDataFactory.CreateMoney(0));

            // Act
            decimal percentage = budget.GetPercentageUsed(100.00m);

            // Assert
            Assert.Equal(0m, percentage);
        }

        [Fact]
        public void GetRemaining_ShouldReturnCorrectRemaining()
        {
            // Arrange
            Budget budget = TestDataFactory.CreateBudgetWithoutId();

            // Act
            Money remaining = budget.GetRemaining(70.00m);

            // Assert
            Assert.Equal(30.00m, remaining.Value);
            Assert.Equal(TestDataFactory.DEFAULT_MONEY_CURRENCY, remaining.Currency);
        }

        [Fact]
        public void GetRemaining_ShouldNotReturnNegative()
        {
            // Arrange
            Budget budget = TestDataFactory.CreateBudgetWithoutId();

            // Act
            Money remaining = budget.GetRemaining(200);

            // Assert
            Assert.Equal(0, remaining.Value);
        }

        [Fact]
        public void IsOverBudget_WithSpentGreaterThanBudget_ShouldReturnTrue()
        {
            // Arrange
            Budget budget = TestDataFactory.CreateBudgetWithoutId();

            // Act
            bool isOver = budget.IsOverBudget(150.00m);

            // Assert
            Assert.True(isOver);
        }

        [Fact]
        public void IsOverBudget_WithSpentLessThanBudget_ShouldReturnFalse()
        {
            // Arrange
            Budget budget = TestDataFactory.CreateBudgetWithoutId();

            // Act
            bool isOver = budget.IsOverBudget(70m);

            // Assert
            Assert.False(isOver);
        }

        [Fact]
        public void IsOverBudget_WithSpentEqualToBudget_ShouldReturnFalse()
        {
            // Arrange
            Budget budget = TestDataFactory.CreateBudgetWithoutId();

            // Act
            bool isOver = budget.IsOverBudget(100.00m);

            // Assert
            Assert.False(isOver);
        }

        [Fact]
        public void ToString_ShouldReturnFormattedString()
        {
            // Arrange
            Budget budget = TestDataFactory.CreateBudgetWithoutId();

            // Act
            string result = budget.ToString();

            // Assert
            Assert.Contains(TestDataFactory.DEFAULT_CATEGORY_NAME, result);
            Assert.Contains($"{TestDataFactory.DEFAULT_MONEY_AMOUNT} {TestDataFactory.DEFAULT_MONEY_CURRENCY}", result);
            Assert.Contains($"{TestDataFactory.DEFAULT_MONTHLY_MONTH}-{TestDataFactory.DEFAULT_YEAR}", result);
        }
    }
}