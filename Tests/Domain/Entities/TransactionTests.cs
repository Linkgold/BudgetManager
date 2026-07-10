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
            EntityInfo info = new EntityInfo("Compra supermercado", "Carrefour 15/06/2024");
            Money amount = new Money(45.75m, "EUR");
            DailyPeriod date = new DailyPeriod(15, 6, 2024);

            // Act
            Transaction transaction = new Transaction(user, category, info, amount, TransactionTypeEnum.Expense, date);

            // Assert
            Assert.NotNull(transaction);
            Assert.Equal(category.Id, transaction.CategoryId);
            Assert.Equal("Compra supermercado", transaction.Info.Name);
            Assert.Equal("Carrefour 15/06/2024", transaction.Info.Description);
            Assert.Equal(45.75m, transaction.Amount.Value);
            Assert.Equal(TransactionTypeEnum.Expense, transaction.Type);
            Assert.Equal(15, transaction.Date.Day);
            Assert.Equal(6, transaction.Date.Month);
            Assert.Equal(2024, transaction.Date.Year);
            Assert.NotEqual(default(DateTime), transaction.CreatedAt);
            Assert.Null(transaction.UpdatedAt);
        }

        [Fact]
        public void Constructor_WithNullUser_ShouldThrowArgumentNullException()
        {
            // Arrange
            Category category = TestDataFactory.CreateCategory();
            EntityInfo info = new EntityInfo("Compra supermercado", null);
            Money amount = new Money(45.75m, "EUR");
            DailyPeriod date = new DailyPeriod(15, 6, 2024);

            // Act & Assert
            ArgumentNullException exception = Assert.Throws<ArgumentNullException>(() => new Transaction(null, category, info, amount, TransactionTypeEnum.Expense, date));

            Assert.Equal("info", exception.ParamName);
        }

        [Fact]
        public void Constructor_WithNullCategory_ShouldThrowArgumentNullException()
        {
            // Arrange
            User user = TestDataFactory.CreateUser();
            EntityInfo info = new EntityInfo("Compra supermercado", null);
            Money amount = new Money(45.75m, "EUR");
            DailyPeriod date = new DailyPeriod(15, 6, 2024);

            // Act & Assert
            ArgumentNullException exception = Assert.Throws<ArgumentNullException>(() => new Transaction(user, null, info, amount, TransactionTypeEnum.Expense, date));

            Assert.Equal("category", exception.ParamName);
        }

        [Fact]
        public void Constructor_WithNullInfo_ShouldThrowArgumentNullException()
        {
            // Arrange
            User user = TestDataFactory.CreateUser();
            Category category = TestDataFactory.CreateCategory();
            Money amount = new Money(45.75m, "EUR");
            DailyPeriod date = new DailyPeriod(15, 6, 2024);

            // Act & Assert
            ArgumentNullException exception = Assert.Throws<ArgumentNullException>(() => new Transaction(user, category, null, amount, TransactionTypeEnum.Expense, date));

            Assert.Equal("info", exception.ParamName);
        }

        [Fact]
        public void Constructor_WithNullAmount_ShouldThrowArgumentNullException()
        {
            // Arrange
            User user = TestDataFactory.CreateUser();
            Category category = TestDataFactory.CreateCategory();
            EntityInfo info = new EntityInfo("Compra supermercado", null);
            DailyPeriod date = new DailyPeriod(15, 6, 2024);

            // Act & Assert
            ArgumentNullException exception = Assert.Throws<ArgumentNullException>(() => new Transaction(user, category, info, null, TransactionTypeEnum.Expense, date));

            Assert.Equal("amount", exception.ParamName);
        }

        [Fact]
        public void Constructor_WithNullDate_ShouldThrowArgumentNullException()
        {
            // Arrange
            User user = TestDataFactory.CreateUser();
            Category category = TestDataFactory.CreateCategory();
            EntityInfo info = new EntityInfo("Compra supermercado", null);
            Money amount = new Money(45.75m, "EUR");

            // Act & Assert
            ArgumentNullException exception = Assert.Throws<ArgumentNullException>(() => new Transaction(user, category, info, amount, TransactionTypeEnum.Expense, null));

            Assert.Equal("date", exception.ParamName);
        }

        // ==================== UPDATE ====================

        [Fact]
        public void Update_WithValidValues_ShouldUpdateTransaction()
        {
            // Arrange
            User user = TestDataFactory.CreateUser();
            Category category = TestDataFactory.CreateCategory();
            EntityInfo info = new EntityInfo("Compra supermercado", "Carrefour 15/06/2024");
            Money amount = new Money(45.75m, "EUR");
            DailyPeriod date = new DailyPeriod(15, 6, 2024);
            Transaction transaction = new Transaction(user, category, info, amount, TransactionTypeEnum.Expense, date);

            // Act
            EntityInfo newInfo = new EntityInfo("Compra supermercado actualizada", "Carrefour 20/06/2024");
            Money newAmount = new Money(50.00m, "EUR");
            DailyPeriod newDate = new DailyPeriod(20, 6, 2024);
            transaction.Update(newInfo, newAmount, TransactionTypeEnum.Income, newDate);

            // Assert
            Assert.Equal("Compra supermercado actualizada", transaction.Info.Name);
            Assert.Equal("Carrefour 20/06/2024", transaction.Info.Description);
            Assert.Equal(50.00m, transaction.Amount.Value);
            Assert.Equal(TransactionTypeEnum.Income, transaction.Type);
            Assert.Equal(20, transaction.Date.Day);
            Assert.Equal(6, transaction.Date.Month);
            Assert.Equal(2024, transaction.Date.Year);
            Assert.NotNull(transaction.UpdatedAt);
        }

        [Fact]
        public void Update_WithNullInfo_ShouldThrowArgumentNullException()
        {
            // Arrange
            User user = TestDataFactory.CreateUser();
            Category category = TestDataFactory.CreateCategory();
            EntityInfo info = new EntityInfo("Compra supermercado", null);
            Money amount = new Money(45.75m, "EUR");
            DailyPeriod date = new DailyPeriod(15, 6, 2024);
            Transaction transaction = new Transaction(user, category, info, amount, TransactionTypeEnum.Expense, date);

            // Act & Assert
            ArgumentNullException exception = Assert.Throws<ArgumentNullException>(() => transaction.Update(null, new Money(50.00m, "EUR"), TransactionTypeEnum.Income, new DailyPeriod(20, 6, 2024)));

            Assert.Equal("info", exception.ParamName);
        }

        [Fact]
        public void Update_WithNullAmount_ShouldThrowArgumentNullException()
        {
            // Arrange
            User user = TestDataFactory.CreateUser();
            Category category = TestDataFactory.CreateCategory();
            EntityInfo info = new EntityInfo("Compra supermercado", null);
            Money amount = new Money(45.75m, "EUR");
            DailyPeriod date = new DailyPeriod(15, 6, 2024);
            Transaction transaction = new Transaction(user, category, info, amount, TransactionTypeEnum.Expense, date);

            // Act & Assert
            ArgumentNullException exception = Assert.Throws<ArgumentNullException>(() => transaction.Update(new EntityInfo("Nuevo nombre", null), null, TransactionTypeEnum.Income, new DailyPeriod(20, 6, 2024)));

            Assert.Equal("amount", exception.ParamName);
        }

        [Fact]
        public void Update_WithNullDate_ShouldThrowArgumentNullException()
        {
            // Arrange
            User user = TestDataFactory.CreateUser();
            Category category = TestDataFactory.CreateCategory();
            EntityInfo info = new EntityInfo("Compra supermercado", null);
            Money amount = new Money(45.75m, "EUR");
            DailyPeriod date = new DailyPeriod(15, 6, 2024);
            Transaction transaction = new Transaction(user, category, info, amount, TransactionTypeEnum.Expense, date);

            // Act & Assert
            ArgumentNullException exception = Assert.Throws<ArgumentNullException>(() => transaction.Update(new EntityInfo("Nuevo nombre", null), new Money(50.00m, "EUR"), TransactionTypeEnum.Income, null));

            Assert.Equal("date", exception.ParamName);
        }

        // ==================== UPDATE AMOUNT ====================

        [Fact]
        public void UpdateAmount_WithValidValue_ShouldUpdateAmount()
        {
            // Arrange
            User user = TestDataFactory.CreateUser();
            Category category = TestDataFactory.CreateCategory();
            EntityInfo info = new EntityInfo("Compra supermercado", null);
            Money amount = new Money(45.75m, "EUR");
            DailyPeriod date = new DailyPeriod(15, 6, 2024);
            Transaction transaction = new Transaction(user, category, info, amount, TransactionTypeEnum.Expense, date);

            // Act
            Money newAmount = new Money(50.00m, "EUR");
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
            EntityInfo info = new EntityInfo("Compra supermercado", null);
            Money amount = new Money(45.75m, "EUR");
            DailyPeriod date = new DailyPeriod(15, 6, 2024);
            Transaction transaction = new Transaction(user, category, info, amount, TransactionTypeEnum.Expense, date);

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
            EntityInfo info = new EntityInfo("Compra supermercado", null);
            Money amount = new Money(45.75m, "EUR");
            DailyPeriod date = new DailyPeriod(15, 6, 2024);
            Transaction transaction = new Transaction(user, category, info, amount, TransactionTypeEnum.Expense, date);

            // Act
            EntityInfo newInfo = new EntityInfo("Nuevo nombre", "Nueva descripción");
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
            EntityInfo info = new EntityInfo("Compra supermercado", null);
            Money amount = new Money(45.75m, "EUR");
            DailyPeriod date = new DailyPeriod(15, 6, 2024);
            Transaction transaction = new Transaction(user, category, info, amount, TransactionTypeEnum.Expense, date);

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
            EntityInfo info = new EntityInfo("Compra supermercado", null);
            Money amount = new Money(45.75m, "EUR");
            DailyPeriod date = new DailyPeriod(15, 6, 2024);
            Transaction transaction = new Transaction(user, category, info, amount, TransactionTypeEnum.Expense, date);

            // Act
            DailyPeriod newDate = new DailyPeriod(20, 6, 2024);
            transaction.UpdateDate(newDate);

            // Assert
            Assert.Equal(20, transaction.Date.Day);
            Assert.Equal(6, transaction.Date.Month);
            Assert.Equal(2024, transaction.Date.Year);
            Assert.NotNull(transaction.UpdatedAt);
        }

        [Fact]
        public void UpdateDate_WithNullDate_ShouldThrowArgumentNullException()
        {
            // Arrange
            User user = TestDataFactory.CreateUser();
            Category category = TestDataFactory.CreateCategory();
            EntityInfo info = new EntityInfo("Compra supermercado", null);
            Money amount = new Money(45.75m, "EUR");
            DailyPeriod date = new DailyPeriod(15, 6, 2024);
            Transaction transaction = new Transaction(user, category, info, amount, TransactionTypeEnum.Expense, date);

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
            EntityInfo info = new EntityInfo("Salario", null);
            Money amount = new Money(1500.00m, "EUR");
            DailyPeriod date = new DailyPeriod(1, 6, 2024);
            Transaction transaction = new Transaction(user, category, info, amount, TransactionTypeEnum.Income, date);

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
            EntityInfo info = new EntityInfo("Compra supermercado", null);
            Money amount = new Money(45.75m, "EUR");
            DailyPeriod date = new DailyPeriod(15, 6, 2024);
            Transaction transaction = new Transaction(user, category, info, amount, TransactionTypeEnum.Expense, date);

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
            EntityInfo info = new EntityInfo("Compra supermercado", null);
            Money amount = new Money(45.75m, "EUR");
            DailyPeriod date = new DailyPeriod(15, 6, 2024);
            Transaction transaction = new Transaction(user, category, info, amount, TransactionTypeEnum.Expense, date);

            // Act
            MonthlyPeriod result = transaction.GetMonthlyPeriod();

            // Assert
            Assert.Equal(6, result.Month);
            Assert.Equal(2024, result.Year);
        }

        // ==================== TO STRING ====================

        [Fact]
        public void ToString_ShouldReturnFormattedString()
        {
            // Arrange
            User user = TestDataFactory.CreateUser();
            Category category = TestDataFactory.CreateCategory();
            EntityInfo info = new EntityInfo("Compra supermercado", null);
            Money amount = new Money(45.75m, "EUR");
            DailyPeriod date = new DailyPeriod(15, 6, 2024);
            Transaction transaction = new Transaction(user, category, info, amount, TransactionTypeEnum.Expense, date);

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