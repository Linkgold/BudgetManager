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
            User user = TestDataFactory.CreateUser();
            Category category = TestDataFactory.CreateCategory();
            EntityInfo info = TestDataFactory.CreateEntityInfo();
            Money amount = new Money(100.00m, "EUR");
            DailyPeriod date = TestDataFactory.CreateDailyPeriod();

            // Act
            Transaction transaction = TestDataFactory.CreateTransactionWithoutId(category: category);

            // Assert
            Assert.NotNull(transaction);
            Assert.Equal(category.Id, transaction.CategoryId);
            Assert.Equal(TestDataFactory.DEFAULT_ENTITY_INFO_NAME, transaction.Info.Name);
            Assert.Equal(TestDataFactory.DEFAULT_ENTITY_INFO_DESCRIPTION, transaction.Info.Description);
            Assert.Equal(TestDataFactory.DEFAULT_MONEY_AMOUNT, transaction.Amount.Value);
            Assert.Equal(TestDataFactory.DEFAULT_CURRENCY, transaction.Amount.Currency);
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
            ArgumentNullException exception = Assert.Throws<ArgumentNullException>(() => TestDataFactory.CreateTransactionWithoutAutoCreation(null!, category, info, amount, TestDataFactory.DEFAULT_TRANSACTION_TYPE, date));

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
            ArgumentNullException exception = Assert.Throws<ArgumentNullException>(() => TestDataFactory.CreateTransactionWithoutAutoCreation(user, null!, info, amount, TestDataFactory.DEFAULT_TRANSACTION_TYPE, date));

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
            ArgumentNullException exception = Assert.Throws<ArgumentNullException>(() => TestDataFactory.CreateTransactionWithoutAutoCreation(user, category, null!, amount, TestDataFactory.DEFAULT_TRANSACTION_TYPE, date));

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
            ArgumentNullException exception = Assert.Throws<ArgumentNullException>(() => TestDataFactory.CreateTransactionWithoutAutoCreation(user, category, info, null!, TestDataFactory.DEFAULT_TRANSACTION_TYPE, date));

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
            ArgumentNullException exception = Assert.Throws<ArgumentNullException>(() => TestDataFactory.CreateTransactionWithoutAutoCreation(user, category, info, amount, TestDataFactory.DEFAULT_TRANSACTION_TYPE, null!));

            Assert.Equal("date", exception.ParamName);
        }

        [Fact]
        public void Constructor_WithMixedCategoryAndZeroAmount_ShouldThrowArgumentException()
        {
            // Arrange
            User user = TestDataFactory.CreateUser();
            Category category = TestDataFactory.CreateCategory(nature: CategoryNatureEnum.Mixed);
            EntityInfo info = TestDataFactory.CreateEntityInfo();
            Money amount = new Money(0m, "EUR");
            DailyPeriod date = TestDataFactory.CreateDailyPeriod();

            // Act & Assert
            ArgumentException exception = Assert.Throws<ArgumentException>(() => TestDataFactory.CreateTransactionWithoutAutoCreation(user, category, info, amount, TestDataFactory.DEFAULT_TRANSACTION_TYPE, date));

            Assert.Contains("must have a positive amount", exception.Message);
        }

        [Fact]
        public void Constructor_WithIncomeCategoryAndIncomeTransactionType_ShouldSucceed()
        {
            // Arrange
            User user = TestDataFactory.CreateUser();
            Category category = TestDataFactory.CreateCategory(nature: CategoryNatureEnum.Income);
            EntityInfo info = TestDataFactory.CreateEntityInfo();
            Money amount = new Money(100.00m, "EUR");
            TransactionTypeEnum transactionType = TransactionTypeEnum.Income;
            DailyPeriod date = TestDataFactory.CreateDailyPeriod();

            // Act
            Transaction transaction = TestDataFactory.CreateTransactionWithoutAutoCreation(user, category, info, amount, transactionType, date);

            // Assert
            Assert.NotNull(transaction);
            Assert.Equal(100.00m, transaction.Amount.Value);
        }

        [Fact]
        public void Constructor_WithExpenseCategoryAndExpenseTransactionType_ShouldSucceed()
        {
            // Arrange
            User user = TestDataFactory.CreateUser();
            Category category = TestDataFactory.CreateCategory(nature: CategoryNatureEnum.Expense);
            EntityInfo info = TestDataFactory.CreateEntityInfo();
            Money amount = new Money(100.00m, "EUR");
            TransactionTypeEnum transactionType = TransactionTypeEnum.Expense;
            DailyPeriod date = TestDataFactory.CreateDailyPeriod();

            // Act
            Transaction transaction = TestDataFactory.CreateTransactionWithoutAutoCreation(user, category, info, amount, transactionType, date);

            // Assert
            Assert.NotNull(transaction);
            Assert.Equal(100.00m, transaction.Amount.Value);
        }

        [Fact]
        public void Constructor_WithExpenseCategoryAndIncomeTransactionType_ShouldThrowArgumentNullException()
        {
            // Arrange
            User user = TestDataFactory.CreateUser();
            Category category = TestDataFactory.CreateCategory(nature: CategoryNatureEnum.Income);
            EntityInfo info = TestDataFactory.CreateEntityInfo();
            Money amount = new Money(100.00m, "EUR");
            TransactionTypeEnum transactionType = TransactionTypeEnum.Expense;
            DailyPeriod date = TestDataFactory.CreateDailyPeriod();

            // Act & Assert
            ArgumentException exception = Assert.Throws<ArgumentException>(() => TestDataFactory.CreateTransactionWithoutAutoCreation(user, category, info, amount, transactionType, date));

            Assert.Contains("Expense", exception.Message);
        }

        [Fact]
        public void Constructor_WithIncomeCategoryAndExpenseTransactionType_ShouldThrowArgumentNullException()
        {
            // Arrange
            User user = TestDataFactory.CreateUser();
            Category category = TestDataFactory.CreateCategory(nature: CategoryNatureEnum.Income);
            EntityInfo info = TestDataFactory.CreateEntityInfo();
            Money amount = new Money(100.00m, "EUR");
            TransactionTypeEnum transactionType = TransactionTypeEnum.Expense;
            DailyPeriod date = TestDataFactory.CreateDailyPeriod();

            // Act & Assert
            ArgumentException exception = Assert.Throws<ArgumentException>(() => TestDataFactory.CreateTransactionWithoutAutoCreation(user, category, info, amount, transactionType, date));

            Assert.Contains("Income", exception.Message);
        }

        // ==================== UPDATE ====================

        [Fact]
        public void Update_WithValidValues_ShouldUpdateTransaction()
        {
            // Arrange
            User user = TestDataFactory.CreateUser();
            Category updatedCategory = TestDataFactory.CreateCategory(nature: CategoryNatureEnum.Income);
            string updatedName = "Compra supermercado actualizada";
            string updatedDescription = "Carrefour 20/06/2024";
            decimal updatedAmount = 50.00m;
            string updatedCurrency = "CNY";
            int updatedDay = 20;

            Transaction transaction = TestDataFactory.CreateTransactionWithoutId();

            // Act
            transaction.Update
            (
                updatedCategory,
                TestDataFactory.CreateEntityInfo(updatedName, updatedDescription),
                TestDataFactory.CreateMoney(updatedAmount, updatedCurrency),
                TransactionTypeEnum.Income,
                TestDataFactory.CreateDailyPeriod(updatedDay)
            );

            // Assert
            Assert.Equal(updatedCategory.Id, transaction.CategoryId);
            Assert.Equal(updatedName, transaction.Info.Name);
            Assert.Equal(updatedDescription, transaction.Info.Description);
            Assert.Equal(updatedAmount, transaction.Amount.Value);
            Assert.Equal(updatedCurrency, transaction.Amount.Currency);
            Assert.Equal(updatedDay, transaction.Date.Day);
            Assert.Equal(TestDataFactory.DEFAULT_DAILY_MONTH, transaction.Date.Month);
            Assert.Equal(TestDataFactory.DEFAULT_YEAR, transaction.Date.Year);
            Assert.NotNull(transaction.UpdatedAt);
        }

        [Fact]
        public void Update_WithNullCategory_ShouldThrowArgumentNullException()
        {
            // Arrange
            Transaction transaction = TestDataFactory.CreateTransactionWithoutId();

            // Act & Assert
            ArgumentNullException exception = Assert.Throws<ArgumentNullException>(() => transaction.Update(null!, TestDataFactory.CreateEntityInfo(), TestDataFactory.CreateMoney(), TestDataFactory.DEFAULT_TRANSACTION_TYPE, TestDataFactory.CreateDailyPeriod()));

            Assert.Equal("category", exception.ParamName);
        }

        [Fact]
        public void Update_WithNullInfo_ShouldThrowArgumentNullException()
        {
            // Arrange
            Transaction transaction = TestDataFactory.CreateTransactionWithoutId();

            // Act & Assert
            ArgumentNullException exception = Assert.Throws<ArgumentNullException>(() => transaction.Update(TestDataFactory.CreateCategory(), null!, TestDataFactory.CreateMoney(), TestDataFactory.DEFAULT_TRANSACTION_TYPE, TestDataFactory.CreateDailyPeriod()));

            Assert.Equal("info", exception.ParamName);
        }

        [Fact]
        public void Update_WithNullAmount_ShouldThrowArgumentNullException()
        {
            // Arrange
            Transaction transaction = TestDataFactory.CreateTransactionWithoutId();

            // Act & Assert
            ArgumentNullException exception = Assert.Throws<ArgumentNullException>(() => transaction.Update(TestDataFactory.CreateCategory(), TestDataFactory.CreateEntityInfo(), null!, TestDataFactory.DEFAULT_TRANSACTION_TYPE, TestDataFactory.CreateDailyPeriod()));

            Assert.Equal("amount", exception.ParamName);
        }

        [Fact]
        public void Update_WithNullDate_ShouldThrowArgumentNullException()
        {
            // Arrange
            Transaction transaction = TestDataFactory.CreateTransactionWithoutId();

            // Act & Assert
            ArgumentNullException exception = Assert.Throws<ArgumentNullException>(() => transaction.Update(TestDataFactory.CreateCategory(), TestDataFactory.CreateEntityInfo(), TestDataFactory.CreateMoney(), TestDataFactory.DEFAULT_TRANSACTION_TYPE, null!));

            Assert.Equal("date", exception.ParamName);
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
    }
}