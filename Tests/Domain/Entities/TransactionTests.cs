using Domain.Entities;
using Contracts.Enums;
using Domain.ValueObjects;
using Tests.Helpers;

namespace Tests.Domain.Entities
{
    public class TransactionTests
    {
        // ==================== CONSTRUCTOR ====================

        [Fact]
        public void Constructor_WithValidValues_ShouldCreateTransaction()
        {
            // Arrange
            Category category = TestDataFactory.CreateCategory();

            // Act
            Transaction transaction = TestDataFactory.CreateTransactionWithoutId(category: category);

            // Assert
            Assert.NotNull(transaction);
            Assert.Equal(category.Id, transaction.CategoryId);
            Assert.Equal(TestDataFactory.DEFAULT_ENTITY_INFO_NAME, transaction.Info.Name);
            Assert.Equal(TestDataFactory.DEFAULT_ENTITY_INFO_DESCRIPTION, transaction.Info.Description);
            Assert.Equal(TestDataFactory.DEFAULT_MONEY_AMOUNT, transaction.Amount.Value);
            Assert.Equal(TransactionTypeEnum.Expense, transaction.Type);
            Assert.Equal(TestDataFactory.DEFAULT_DAILY_DAY, transaction.Date.Day);
            Assert.Equal(TestDataFactory.DEFAULT_DAILY_MONTH, transaction.Date.Month);
            Assert.Equal(TestDataFactory.DEFAULT_YEAR, transaction.Date.Year);
            Assert.NotEqual(default, transaction.CreatedAt);
            Assert.Null(transaction.UpdatedAt);
        }

        [Fact]
        public void Constructor_WithNullUser_ShouldThrowArgumentNullException()
        {
            // Arrange
            Category category = TestDataFactory.CreateCategory();
            EntityInfo info = TestDataFactory.CreateEntityInfo();
            Money amount = TestDataFactory.CreateMoney();
            DailyPeriod date = TestDataFactory.CreateDailyPeriod();

            // Act & Assert
            ArgumentNullException exception = Assert.Throws<ArgumentNullException>(() => TestDataFactory.CreateTransactionWithoutAutoCreation(null, category, info, amount, TransactionTypeEnum.Expense, date));

            Assert.Equal("user", exception.ParamName);
        }

        [Fact]
        public void Constructor_WithNullCategory_ShouldThrowArgumentNullException()
        {
            // Arrange
            User user = TestDataFactory.CreateUser();
            EntityInfo info = TestDataFactory.CreateEntityInfo();
            Money amount = TestDataFactory.CreateMoney();
            DailyPeriod date = TestDataFactory.CreateDailyPeriod();

            // Act & Assert
            ArgumentNullException exception = Assert.Throws<ArgumentNullException>(() => TestDataFactory.CreateTransactionWithoutAutoCreation(user, null, info, amount, TransactionTypeEnum.Expense, date));

            Assert.Equal("category", exception.ParamName);
        }

        [Fact]
        public void Constructor_WithNullInfo_ShouldThrowArgumentNullException()
        {
            // Arrange
            User user = TestDataFactory.CreateUser();
            Category category = TestDataFactory.CreateCategory();
            Money amount = TestDataFactory.CreateMoney();
            DailyPeriod date = TestDataFactory.CreateDailyPeriod();

            // Act & Assert
            ArgumentNullException exception = Assert.Throws<ArgumentNullException>(() => TestDataFactory.CreateTransactionWithoutAutoCreation(user, category, null, amount, TransactionTypeEnum.Expense, date));

            Assert.Equal("info", exception.ParamName);
        }

        [Fact]
        public void Constructor_WithNullAmount_ShouldThrowArgumentNullException()
        {
            // Arrange
            User user = TestDataFactory.CreateUser();
            Category category = TestDataFactory.CreateCategory();
            EntityInfo info = TestDataFactory.CreateEntityInfo();
            DailyPeriod date = TestDataFactory.CreateDailyPeriod();

            // Act & Assert
            ArgumentNullException exception = Assert.Throws<ArgumentNullException>(() => TestDataFactory.CreateTransactionWithoutAutoCreation(user, category, info, null, TransactionTypeEnum.Expense, date));

            Assert.Equal("amount", exception.ParamName);
        }

        [Fact]
        public void Constructor_WithNullDate_ShouldThrowArgumentNullException()
        {
            // Arrange
            User user = TestDataFactory.CreateUser();
            Category category = TestDataFactory.CreateCategory();
            EntityInfo info = TestDataFactory.CreateEntityInfo();
            Money amount = TestDataFactory.CreateMoney();

            // Act & Assert
            ArgumentNullException exception = Assert.Throws<ArgumentNullException>(() => TestDataFactory.CreateTransactionWithoutAutoCreation(user, category, info, amount, TransactionTypeEnum.Expense, null));

            Assert.Equal("date", exception.ParamName);
        }

        // ==================== UPDATE ====================

        [Fact]
        public void Update_WithValidValues_ShouldUpdateTransaction()
        {
            // Arrange
            string updatedName = "Compra supermercado actualizada";
            string updatedDescription = "Carrefour 20/06/2024";
            decimal updatedAmount = 50.00m;
            int updatedDay = 20;

            Transaction transaction = TestDataFactory.CreateTransactionWithoutId();

            // Act
            transaction.Update
            (
                TestDataFactory.CreateEntityInfo(updatedName, updatedDescription),
                TestDataFactory.CreateMoney(updatedAmount), 
                TransactionTypeEnum.Income,
                TestDataFactory.CreateDailyPeriod(updatedDay)
            );

            // Assert
            Assert.Equal(updatedName, transaction.Info.Name);
            Assert.Equal(updatedDescription, transaction.Info.Description);
            Assert.Equal(updatedAmount, transaction.Amount.Value);
            Assert.Equal(TransactionTypeEnum.Income, transaction.Type);
            Assert.Equal(updatedDay, transaction.Date.Day);
            Assert.Equal(TestDataFactory.DEFAULT_DAILY_MONTH, transaction.Date.Month);
            Assert.Equal(TestDataFactory.DEFAULT_YEAR, transaction.Date.Year);
            Assert.NotNull(transaction.UpdatedAt);
        }

        [Fact]
        public void Update_WithNullInfo_ShouldThrowArgumentNullException()
        {
            // Arrange
            Transaction transaction = TestDataFactory.CreateTransactionWithoutId();

            // Act & Assert
            ArgumentNullException exception = Assert.Throws<ArgumentNullException>(() => transaction.Update(null, TestDataFactory.CreateMoney(), TransactionTypeEnum.Income, TestDataFactory.CreateDailyPeriod()));

            Assert.Equal("info", exception.ParamName);
        }

        [Fact]
        public void Update_WithNullAmount_ShouldThrowArgumentNullException()
        {
            // Arrange
            Transaction transaction = TestDataFactory.CreateTransactionWithoutId();

            // Act & Assert
            ArgumentNullException exception = Assert.Throws<ArgumentNullException>(() => transaction.Update(TestDataFactory.CreateEntityInfo(), null, TransactionTypeEnum.Income, TestDataFactory.CreateDailyPeriod()));

            Assert.Equal("amount", exception.ParamName);
        }

        [Fact]
        public void Update_WithNullDate_ShouldThrowArgumentNullException()
        {
            // Arrange
            Transaction transaction = TestDataFactory.CreateTransactionWithoutId();

            // Act & Assert
            ArgumentNullException exception = Assert.Throws<ArgumentNullException>(() => transaction.Update(TestDataFactory.CreateEntityInfo(), TestDataFactory.CreateMoney(), TransactionTypeEnum.Income, null));

            Assert.Equal("date", exception.ParamName);
        }

        // ==================== UPDATE AMOUNT ====================

        [Fact]
        public void UpdateAmount_WithValidValue_ShouldUpdateAmount()
        {
            // Arrange
            decimal updatedAmount = 50.00m;

            Transaction transaction = TestDataFactory.CreateTransactionWithoutId();

            // Act
            transaction.UpdateAmount(TestDataFactory.CreateMoney(updatedAmount));

            // Assert
            Assert.Equal(updatedAmount, transaction.Amount.Value);
            Assert.NotNull(transaction.UpdatedAt);
        }

        [Fact]
        public void UpdateAmount_WithNullAmount_ShouldThrowArgumentNullException()
        {
            // Arrange
            Transaction transaction = TestDataFactory.CreateTransactionWithoutId();

            // Act & Assert
            ArgumentNullException exception = Assert.Throws<ArgumentNullException>(() => transaction.UpdateAmount(null));

            Assert.Equal("amount", exception.ParamName);
        }

        // ==================== UPDATE INFO ====================

        [Fact]
        public void UpdateInfo_WithValidValue_ShouldUpdateInfo()
        {
            // Arrange
            string updatedName = "Nuevo nombre";
            string udatedDescription = "Nueva descripción";

            Transaction transaction = TestDataFactory.CreateTransactionWithoutId();

            // Act
            EntityInfo newInfo = TestDataFactory.CreateEntityInfo(updatedName, udatedDescription);
            transaction.UpdateInfo(newInfo);

            // Assert
            Assert.Equal(updatedName, transaction.Info.Name);
            Assert.Equal(udatedDescription, transaction.Info.Description);
            Assert.NotNull(transaction.UpdatedAt);
        }

        [Fact]
        public void UpdateInfo_WithNullInfo_ShouldThrowArgumentNullException()
        {
            // Arrange
            Transaction transaction = TestDataFactory.CreateTransactionWithoutId();

            // Act & Assert
            ArgumentNullException exception = Assert.Throws<ArgumentNullException>(() => transaction.UpdateInfo(null));

            Assert.Equal("info", exception.ParamName);
        }

        // ==================== UPDATE DATE ====================

        [Fact]
        public void UpdateDate_WithValidValue_ShouldUpdateDate()
        {
            int updatedDay = 20;

            // Arrange
            Transaction transaction = TestDataFactory.CreateTransactionWithoutId();

            // Act
            transaction.UpdateDate(TestDataFactory.CreateDailyPeriod(updatedDay));

            // Assert
            Assert.Equal(updatedDay, transaction.Date.Day);
            Assert.Equal(TestDataFactory.DEFAULT_DAILY_MONTH, transaction.Date.Month);
            Assert.Equal(TestDataFactory.DEFAULT_YEAR, transaction.Date.Year);
            Assert.NotNull(transaction.UpdatedAt);
        }

        [Fact]
        public void UpdateDate_WithNullDate_ShouldThrowArgumentNullException()
        {
            // Arrange
            Transaction transaction = TestDataFactory.CreateTransactionWithoutId();

            // Act & Assert
            ArgumentNullException exception = Assert.Throws<ArgumentNullException>(() => transaction.UpdateDate(null));

            Assert.Equal("date", exception.ParamName);
        }

        // ==================== IS INCOME / IS EXPENSE ====================

        [Fact]
        public void IsIncome_ForIncomeTransaction_ShouldReturnTrue()
        {
            // Arrange
            Transaction transaction = TestDataFactory.CreateTransactionWithoutId(type: TransactionTypeEnum.Income);

            // Act & Assert
            Assert.True(transaction.IsIncome);
            Assert.False(transaction.IsExpense);
        }

        [Fact]
        public void IsExpense_ForExpenseTransaction_ShouldReturnTrue()
        {
            // Arrange
            Transaction transaction = TestDataFactory.CreateTransactionWithoutId(type: TransactionTypeEnum.Expense);

            // Act & Assert
            Assert.True(transaction.IsExpense);
            Assert.False(transaction.IsIncome);
        }

        // ==================== GET MONTHLY PERIOD ====================

        [Fact]
        public void GetMonthlyPeriod_ShouldReturnCorrectMonthlyPeriod()
        {
            // Arrange
            Transaction transaction = TestDataFactory.CreateTransactionWithoutId();

            // Act
            MonthlyPeriod result = transaction.GetMonthlyPeriod();

            // Assert
            Assert.Equal(TestDataFactory.DEFAULT_DAILY_MONTH, result.Month);
            Assert.Equal(TestDataFactory.DEFAULT_YEAR, result.Year);
        }

        // ==================== TO STRING ====================

        [Fact]
        public void ToString_ShouldReturnFormattedString()
        {
            // Arrange
            Transaction transaction = TestDataFactory.CreateTransactionWithoutId();

            // Act
            string result = transaction.ToString();

            // Assert
            Assert.Contains($"{TransactionTypeEnum.Expense}", result);
            Assert.Contains(TestDataFactory.DEFAULT_ENTITY_INFO_NAME, result);
            Assert.Contains($"{TestDataFactory.DEFAULT_MONEY_AMOUNT}", result);
            Assert.Contains($"{TestDataFactory.DEFAULT_DAILY_DAY:00}/{TestDataFactory.DEFAULT_DAILY_MONTH:00}/{TestDataFactory.DEFAULT_YEAR:0000}", result);
        }
    }
}