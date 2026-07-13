using Domain.Entities;
using Domain.Enums;
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
            EntityInfo info = TestDataFactory.CreateEntityInfo("Compra supermercado", "Carrefour 15/06/2024");
            Money amount = TestDataFactory.CreateMoney(45.75m);
            DailyPeriod date = TestDataFactory.CreateDailyPeriod();

            // Act
            Transaction transaction = TestDataFactory.CreateTransaction(user, category, info, amount, TransactionTypeEnum.Expense, date);

            // Assert
            Assert.NotNull(transaction);
            Assert.Equal(category.Id, transaction.CategoryId);
            Assert.Equal("Compra supermercado", transaction.Info.Name);
            Assert.Equal("Carrefour 15/06/2024", transaction.Info.Description);
            Assert.Equal(45.75m, transaction.Amount.Value);
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
            EntityInfo info = TestDataFactory.CreateEntityInfo("Compra supermercado", null);
            Money amount = TestDataFactory.CreateMoney(45.75m);
            DailyPeriod date = TestDataFactory.CreateDailyPeriod();

            // Act & Assert
            ArgumentNullException exception = Assert.Throws<ArgumentNullException>(() => TestDataFactory.CreateTransaction(null, category, info, amount, TransactionTypeEnum.Expense, date));

            Assert.Equal("user", exception.ParamName);
        }

        [Fact]
        public void Constructor_WithNullCategory_ShouldThrowArgumentNullException()
        {
            // Arrange
            User user = TestDataFactory.CreateUser();
            EntityInfo info = TestDataFactory.CreateEntityInfo("Compra supermercado", null);
            Money amount = TestDataFactory.CreateMoney(45.75m);
            DailyPeriod date = TestDataFactory.CreateDailyPeriod();

            // Act & Assert
            ArgumentNullException exception = Assert.Throws<ArgumentNullException>(() => TestDataFactory.CreateTransaction(user, null, info, amount, TransactionTypeEnum.Expense, date));

            Assert.Equal("category", exception.ParamName);
        }

        [Fact]
        public void Constructor_WithNullInfo_ShouldThrowArgumentNullException()
        {
            // Arrange
            User user = TestDataFactory.CreateUser();
            Category category = TestDataFactory.CreateCategory();
            Money amount = TestDataFactory.CreateMoney(45.75m);
            DailyPeriod date = TestDataFactory.CreateDailyPeriod();

            // Act & Assert
            ArgumentNullException exception = Assert.Throws<ArgumentNullException>(() => TestDataFactory.CreateTransaction(user, category, null, amount, TransactionTypeEnum.Expense, date));

            Assert.Equal("info", exception.ParamName);
        }

        [Fact]
        public void Constructor_WithNullAmount_ShouldThrowArgumentNullException()
        {
            // Arrange
            User user = TestDataFactory.CreateUser();
            Category category = TestDataFactory.CreateCategory();
            EntityInfo info = TestDataFactory.CreateEntityInfo("Compra supermercado", null);
            DailyPeriod date = TestDataFactory.CreateDailyPeriod();

            // Act & Assert
            ArgumentNullException exception = Assert.Throws<ArgumentNullException>(() => TestDataFactory.CreateTransaction(user, category, info, null, TransactionTypeEnum.Expense, date));

            Assert.Equal("amount", exception.ParamName);
        }

        [Fact]
        public void Constructor_WithNullDate_ShouldThrowArgumentNullException()
        {
            // Arrange
            User user = TestDataFactory.CreateUser();
            Category category = TestDataFactory.CreateCategory();
            EntityInfo info = TestDataFactory.CreateEntityInfo("Compra supermercado", null);
            Money amount = TestDataFactory.CreateMoney(45.75m);

            // Act & Assert
            ArgumentNullException exception = Assert.Throws<ArgumentNullException>(() => TestDataFactory.CreateTransaction(user, category, info, amount, TransactionTypeEnum.Expense, null));

            Assert.Equal("date", exception.ParamName);
        }

        // ==================== UPDATE ====================

        [Fact]
        public void Update_WithValidValues_ShouldUpdateTransaction()
        {
            // Arrange
            User user = TestDataFactory.CreateUser();
            Category category = TestDataFactory.CreateCategory();
            EntityInfo info = TestDataFactory.CreateEntityInfo("Compra supermercado", "Carrefour 15/06/2024");
            Money amount = TestDataFactory.CreateMoney(45.75m);
            DailyPeriod date = TestDataFactory.CreateDailyPeriod();
            Transaction transaction = TestDataFactory.CreateTransaction(user, category, info, amount, TransactionTypeEnum.Expense, date);

            // Act
            EntityInfo newInfo = TestDataFactory.CreateEntityInfo("Compra supermercado actualizada", "Carrefour 20/06/2024");
            Money newAmount = TestDataFactory.CreateMoney(50.00m);
            DailyPeriod newDate = TestDataFactory.CreateDailyPeriod(20);
            transaction.Update(newInfo, newAmount, TransactionTypeEnum.Income, newDate);

            // Assert
            Assert.Equal("Compra supermercado actualizada", transaction.Info.Name);
            Assert.Equal("Carrefour 20/06/2024", transaction.Info.Description);
            Assert.Equal(50.00m, transaction.Amount.Value);
            Assert.Equal(TransactionTypeEnum.Income, transaction.Type);
            Assert.Equal(20, transaction.Date.Day);
            Assert.Equal(TestDataFactory.DEFAULT_DAILY_MONTH, transaction.Date.Month);
            Assert.Equal(TestDataFactory.DEFAULT_YEAR, transaction.Date.Year);
            Assert.NotNull(transaction.UpdatedAt);
        }

        [Fact]
        public void Update_WithNullInfo_ShouldThrowArgumentNullException()
        {
            // Arrange
            User user = TestDataFactory.CreateUser();
            Category category = TestDataFactory.CreateCategory();
            EntityInfo info = TestDataFactory.CreateEntityInfo("Compra supermercado", null);
            Money amount = TestDataFactory.CreateMoney(45.75m);
            DailyPeriod date = TestDataFactory.CreateDailyPeriod();
            Transaction transaction = TestDataFactory.CreateTransaction(user, category, info, amount, TransactionTypeEnum.Expense, date);

            // Act & Assert
            ArgumentNullException exception = Assert.Throws<ArgumentNullException>(() => transaction.Update(null, TestDataFactory.CreateMoney(50.00m), TransactionTypeEnum.Income, TestDataFactory.CreateDailyPeriod(20)));

            Assert.Equal("info", exception.ParamName);
        }

        [Fact]
        public void Update_WithNullAmount_ShouldThrowArgumentNullException()
        {
            // Arrange
            User user = TestDataFactory.CreateUser();
            Category category = TestDataFactory.CreateCategory();
            EntityInfo info = TestDataFactory.CreateEntityInfo("Compra supermercado", null);
            Money amount = TestDataFactory.CreateMoney(45.75m);
            DailyPeriod date = TestDataFactory.CreateDailyPeriod();
            Transaction transaction = TestDataFactory.CreateTransaction(user, category, info, amount, TransactionTypeEnum.Expense, date);

            // Act & Assert
            ArgumentNullException exception = Assert.Throws<ArgumentNullException>(() => transaction.Update(TestDataFactory.CreateEntityInfo("Nuevo nombre", null), null, TransactionTypeEnum.Income, TestDataFactory.CreateDailyPeriod(20)));

            Assert.Equal("amount", exception.ParamName);
        }

        [Fact]
        public void Update_WithNullDate_ShouldThrowArgumentNullException()
        {
            // Arrange
            User user = TestDataFactory.CreateUser();
            Category category = TestDataFactory.CreateCategory();
            EntityInfo info = TestDataFactory.CreateEntityInfo("Compra supermercado", null);
            Money amount = TestDataFactory.CreateMoney(45.75m);
            DailyPeriod date = TestDataFactory.CreateDailyPeriod();
            Transaction transaction = TestDataFactory.CreateTransaction(user, category, info, amount, TransactionTypeEnum.Expense, date);

            // Act & Assert
            ArgumentNullException exception = Assert.Throws<ArgumentNullException>(() => transaction.Update(TestDataFactory.CreateEntityInfo("Nuevo nombre", null), TestDataFactory.CreateMoney(50.00m), TransactionTypeEnum.Income, null));

            Assert.Equal("date", exception.ParamName);
        }

        // ==================== UPDATE AMOUNT ====================

        [Fact]
        public void UpdateAmount_WithValidValue_ShouldUpdateAmount()
        {
            // Arrange
            User user = TestDataFactory.CreateUser();
            Category category = TestDataFactory.CreateCategory();
            EntityInfo info = TestDataFactory.CreateEntityInfo("Compra supermercado", null);
            Money amount = TestDataFactory.CreateMoney(45.75m);
            DailyPeriod date = TestDataFactory.CreateDailyPeriod();
            Transaction transaction = TestDataFactory.CreateTransaction(user, category, info, amount, TransactionTypeEnum.Expense, date);

            // Act
            Money newAmount = TestDataFactory.CreateMoney(50.00m);
            transaction.UpdateAmount(newAmount);

            // Assert
            Assert.Equal(50.00m, transaction.Amount.Value);
            Assert.NotNull(transaction.UpdatedAt);
        }

        [Fact]
        public void UpdateAmount_WithNullAmount_ShouldThrowArgumentNullException()
        {
            // Arrange
            User user = TestDataFactory.CreateUser();
            Category category = TestDataFactory.CreateCategory();
            EntityInfo info = TestDataFactory.CreateEntityInfo("Compra supermercado", null);
            Money amount = TestDataFactory.CreateMoney(45.75m);
            DailyPeriod date = TestDataFactory.CreateDailyPeriod();
            Transaction transaction = TestDataFactory.CreateTransaction(user, category, info, amount, TransactionTypeEnum.Expense, date);

            // Act & Assert
            ArgumentNullException exception = Assert.Throws<ArgumentNullException>(() => transaction.UpdateAmount(null));

            Assert.Equal("newAmount", exception.ParamName);
        }

        // ==================== UPDATE INFO ====================

        [Fact]
        public void UpdateInfo_WithValidValue_ShouldUpdateInfo()
        {
            // Arrange
            User user = TestDataFactory.CreateUser();
            Category category = TestDataFactory.CreateCategory();
            EntityInfo info = TestDataFactory.CreateEntityInfo("Compra supermercado", null);
            Money amount = TestDataFactory.CreateMoney(45.75m);
            DailyPeriod date = TestDataFactory.CreateDailyPeriod();
            Transaction transaction = TestDataFactory.CreateTransaction(user, category, info, amount, TransactionTypeEnum.Expense, date);

            // Act
            EntityInfo newInfo = TestDataFactory.CreateEntityInfo("Nuevo nombre", "Nueva descripción");
            transaction.UpdateInfo(newInfo);

            // Assert
            Assert.Equal("Nuevo nombre", transaction.Info.Name);
            Assert.Equal("Nueva descripción", transaction.Info.Description);
            Assert.NotNull(transaction.UpdatedAt);
        }

        [Fact]
        public void UpdateInfo_WithNullInfo_ShouldThrowArgumentNullException()
        {
            // Arrange
            User user = TestDataFactory.CreateUser();
            Category category = TestDataFactory.CreateCategory();
            EntityInfo info = TestDataFactory.CreateEntityInfo("Compra supermercado", null);
            Money amount = TestDataFactory.CreateMoney(45.75m);
            DailyPeriod date = TestDataFactory.CreateDailyPeriod();
            Transaction transaction = TestDataFactory.CreateTransaction(user, category, info, amount, TransactionTypeEnum.Expense, date);

            // Act & Assert
            ArgumentNullException exception = Assert.Throws<ArgumentNullException>(() => transaction.UpdateInfo(null));

            Assert.Equal("newInfo", exception.ParamName);
        }

        // ==================== UPDATE DATE ====================

        [Fact]
        public void UpdateDate_WithValidValue_ShouldUpdateDate()
        {
            // Arrange
            User user = TestDataFactory.CreateUser();
            Category category = TestDataFactory.CreateCategory();
            EntityInfo info = TestDataFactory.CreateEntityInfo("Compra supermercado", null);
            Money amount = TestDataFactory.CreateMoney(45.75m);
            DailyPeriod date = TestDataFactory.CreateDailyPeriod();
            Transaction transaction = TestDataFactory.CreateTransaction(user, category, info, amount, TransactionTypeEnum.Expense, date);

            // Act
            DailyPeriod newDate = TestDataFactory.CreateDailyPeriod(20);
            transaction.UpdateDate(newDate);

            // Assert
            Assert.Equal(20, transaction.Date.Day);
            Assert.Equal(TestDataFactory.DEFAULT_DAILY_MONTH, transaction.Date.Month);
            Assert.Equal(TestDataFactory.DEFAULT_YEAR, transaction.Date.Year);
            Assert.NotNull(transaction.UpdatedAt);
        }

        [Fact]
        public void UpdateDate_WithNullDate_ShouldThrowArgumentNullException()
        {
            // Arrange
            User user = TestDataFactory.CreateUser();
            Category category = TestDataFactory.CreateCategory();
            EntityInfo info = TestDataFactory.CreateEntityInfo("Compra supermercado", null);
            Money amount = TestDataFactory.CreateMoney(45.75m);
            DailyPeriod date = TestDataFactory.CreateDailyPeriod();
            Transaction transaction = TestDataFactory.CreateTransaction(user, category, info, amount, TransactionTypeEnum.Expense, date);

            // Act & Assert
            ArgumentNullException exception = Assert.Throws<ArgumentNullException>(() => transaction.UpdateDate(null));

            Assert.Equal("newDate", exception.ParamName);
        }

        // ==================== IS INCOME / IS EXPENSE ====================

        [Fact]
        public void IsIncome_ForIncomeTransaction_ShouldReturnTrue()
        {
            // Arrange
            User user = TestDataFactory.CreateUser();
            Category category = TestDataFactory.CreateCategory();
            EntityInfo info = TestDataFactory.CreateEntityInfo("Salario", null);
            Money amount = TestDataFactory.CreateMoney(1500.00m);
            DailyPeriod date = TestDataFactory.CreateDailyPeriod(1);
            Transaction transaction = TestDataFactory.CreateTransaction(user, category, info, amount, TransactionTypeEnum.Income, date);

            // Act & Assert
            Assert.True(transaction.IsIncome);
            Assert.False(transaction.IsExpense);
        }

        [Fact]
        public void IsExpense_ForExpenseTransaction_ShouldReturnTrue()
        {
            // Arrange
            User user = TestDataFactory.CreateUser();
            Category category = TestDataFactory.CreateCategory();
            EntityInfo info = TestDataFactory.CreateEntityInfo("Compra supermercado", null);
            Money amount = TestDataFactory.CreateMoney(45.75m);
            DailyPeriod date = TestDataFactory.CreateDailyPeriod();
            Transaction transaction = TestDataFactory.CreateTransaction(user, category, info, amount, TransactionTypeEnum.Expense, date);

            // Act & Assert
            Assert.True(transaction.IsExpense);
            Assert.False(transaction.IsIncome);
        }

        // ==================== GET MONTHLY PERIOD ====================

        [Fact]
        public void GetMonthlyPeriod_ShouldReturnCorrectMonthlyPeriod()
        {
            // Arrange
            User user = TestDataFactory.CreateUser();
            Category category = TestDataFactory.CreateCategory();
            EntityInfo info = TestDataFactory.CreateEntityInfo("Compra supermercado", null);
            Money amount = TestDataFactory.CreateMoney(45.75m);
            DailyPeriod date = TestDataFactory.CreateDailyPeriod();
            Transaction transaction = TestDataFactory.CreateTransaction(user, category, info, amount, TransactionTypeEnum.Expense, date);

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
            User user = TestDataFactory.CreateUser();
            Category category = TestDataFactory.CreateCategory();
            EntityInfo info = TestDataFactory.CreateEntityInfo("Compra supermercado", null);
            Money amount = TestDataFactory.CreateMoney(45.75m);
            DailyPeriod date = TestDataFactory.CreateDailyPeriod();
            Transaction transaction = TestDataFactory.CreateTransaction(user, category, info, amount, TransactionTypeEnum.Expense, date);

            // Act
            string result = transaction.ToString();

            // Assert
            Assert.Contains("Expense", result);
            Assert.Contains("Compra supermercado", result);
            Assert.Contains("45,75 EUR", result);
            Assert.Contains("15/06/2024", result);
        }
    }
}