using Domain.Entities;
using Domain.Enums;
using Domain.Interfaces;
using Domain.ValueObjects;
using Infrastructure.Data;
using Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Tests.Helpers;

namespace Tests.Infrastructure
{
    public class TransactionRepositoryTests : IDisposable
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly ITransactionRepository _repository;

        public TransactionRepositoryTests()
        {
            string databaseName = Guid.NewGuid().ToString();

            DbContextOptions<ApplicationDbContext> options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: databaseName)
                .Options;

            _dbContext = new ApplicationDbContext(options);
            _repository = new TransactionRepository(_dbContext);
        }

        // ==================== TEST: ADD ====================

        [Fact]
        public async Task AddAsync_ShouldAddTransactionToDatabase()
        {
            // Arrange
            Category category = TestDataFactory.CreateCategory();
            Transaction transaction = TestDataFactory.CreateTransaction();

            // Act
            await _repository.AddAsync(transaction);

            // Assert
            Transaction? retrieved = await _dbContext.Transactions.Include(t => t.Category).FirstOrDefaultAsync(t => t.Id == transaction.Id);

            Assert.NotNull(retrieved);
            Assert.Equal(TestDataFactory.DEFAULT_TRANSACTION_NAME, retrieved.Info.Name);
            Assert.Equal(TestDataFactory.DEFAULT_TRANSACTION_AMOUNT, retrieved.Amount.Value);
            Assert.Equal(TransactionTypeEnum.Expense, retrieved.Type);
            Assert.Equal(TestDataFactory.DEFAULT_DAILY_DAY, retrieved.Date.Day);
            Assert.Equal(TestDataFactory.DEFAULT_DAILY_MONTH, retrieved.Date.Month);
            Assert.Equal(TestDataFactory.DEFAULT_YEAR, retrieved.Date.Year);
            Assert.Equal(category.Id, retrieved.CategoryId);
            Assert.NotEqual(default, retrieved.CreatedAt);
        }

        // ==================== TEST: GET BY ID ====================

        [Fact]
        public async Task GetByIdAsync_WithExistingId_ReturnsTransaction()
        {
            // Arrange
            int userId = 1;
            Category category = TestDataFactory.CreateCategory();
            Transaction transaction = await TestDataFactory.SeedTransactionAsync(_repository, 1, TestDataFactory.CreateUser(), category);

            // Act
            Transaction? retrieved = await _repository.GetByIdAsync(userId, transaction.Id);

            // Assert
            Assert.NotNull(retrieved);
            Assert.Equal(transaction.Id, retrieved.Id);
            Assert.Equal(TestDataFactory.DEFAULT_TRANSACTION_AMOUNT, retrieved.Amount.Value);
            Assert.Equal(TransactionTypeEnum.Expense, retrieved.Type);
            Assert.Equal(TestDataFactory.DEFAULT_DAILY_DAY, retrieved.Date.Day);
            Assert.Equal(TestDataFactory.DEFAULT_DAILY_MONTH, retrieved.Date.Month);
            Assert.Equal(TestDataFactory.DEFAULT_YEAR, retrieved.Date.Year);
            Assert.Equal(category.Id, retrieved.CategoryId);
            Assert.NotNull(retrieved.Category);
        }

        [Fact]
        public async Task GetByIdAsync_WithNonExistingId_ReturnsNull()
        {
            // Act
            int userId = 1;
            Transaction? retrieved = await _repository.GetByIdAsync(userId, 999);

            // Assert
            Assert.Null(retrieved);
        }

        [Fact]
        public async Task GetByIdAsync_WithInvalidId_ThrowsArgumentException()
        {
            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() => _repository.GetByIdAsync(0, 1));
        }

        // ==================== TEST: GET ALL ====================

        [Fact]
        public async Task GetAllAsync_ReturnsAllTransactions()
        {
            // Arrange
            int userId = 1;
            User user = TestDataFactory.CreateUser();
            Category category = TestDataFactory.CreateCategory(user: user);
            await TestDataFactory.SeedTransactionAsync(_repository, 1, user, category);
            await TestDataFactory.SeedTransactionAsync(_repository, 2, user, category);

            // Act
            IEnumerable<Transaction> result = await _repository.GetAllAsync(userId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count());
        }

        // ==================== TEST: GET BY CATEGORY ====================

        [Fact]
        public async Task GetByCategoryIdAsync_WithExistingCategory_ReturnsTransactions()
        {
            // Arrange
            int userId = 1;
            User user = TestDataFactory.CreateUser();
            Category category1 = TestDataFactory.CreateCategory(1, user);
            Category category2 = TestDataFactory.CreateCategory(2, user, "Transporte");

            await TestDataFactory.SeedTransactionAsync(_repository, 1, user, category1);
            await TestDataFactory.SeedTransactionAsync(_repository, 2, user, category1);
            await TestDataFactory.SeedTransactionAsync(_repository, 3, user, category2);

            // Act
            IEnumerable<Transaction> result = await _repository.GetByCategoryIdAsync(userId, category1.Id);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count());
            Assert.All(result, t => Assert.Equal(category1.Id, t.CategoryId));
        }

        [Fact]
        public async Task GetByCategoryIdAsync_WithNonExistingCategory_ReturnsEmptyList()
        {
            // Act
            int userId = 1;
            IEnumerable<Transaction> result = await _repository.GetByCategoryIdAsync(userId, 999);

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result);
        }

        // ==================== TEST: GET BY MONTHLY PERIOD ====================

        [Fact]
        public async Task GetByMonthlyPeriodAsync_WithExistingPeriod_ReturnsTransactions()
        {
            // Arrange
            int userId = 1;
            User user = TestDataFactory.CreateUser();
            Category category = TestDataFactory.CreateCategory(user: user);
            await TestDataFactory.SeedTransactionAsync(_repository, 1, user, category);
            await TestDataFactory.SeedTransactionAsync(_repository, 2, user, category);
            await TestDataFactory.SeedTransactionAsync(_repository, 3, user, category, month: 7);

            MonthlyPeriod period = TestDataFactory.CreateMonthlyPeriod(TestDataFactory.DEFAULT_DAILY_MONTH);

            // Act
            IEnumerable<Transaction> result = await _repository.GetByMonthlyPeriodAsync(userId, period);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count());
            Assert.All(result, t => Assert.Equal(6, t.Date.Month));
            Assert.All(result, t => Assert.Equal(2024, t.Date.Year));
        }

        [Fact]
        public async Task GetByMonthlyPeriodAsync_WithNonExistingPeriod_ReturnsEmptyList()
        {
            // Arrange
            int userId = 1;
            MonthlyPeriod period = TestDataFactory.CreateMonthlyPeriod(12, 2025);

            // Act
            IEnumerable<Transaction> result = await _repository.GetByMonthlyPeriodAsync(userId, period);

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result);
        }

        [Fact]
        public async Task GetByMonthlyPeriodAsync_WithNullPeriod_ThrowsArgumentNullException()
        {
            // Act & Assert
            await Assert.ThrowsAsync<ArgumentNullException>(() => _repository.GetByMonthlyPeriodAsync(1, null));
        }

        // ==================== TEST: GET BY CATEGORY AND MONTHLY PERIOD ====================

        [Fact]
        public async Task GetByCategoryAndMonthlyPeriodAsync_WithExistingCategoryAndPeriod_ReturnsTransactions()
        {
            // Arrange
            int userId = 1;
            User user = TestDataFactory.CreateUser();
            Category category1 = TestDataFactory.CreateCategory(1, user);
            Category category2 = TestDataFactory.CreateCategory(2, user, "Transporte");

            await TestDataFactory.SeedTransactionAsync(_repository, 1, user, category1);
            await TestDataFactory.SeedTransactionAsync(_repository, 2, user, category1);
            await TestDataFactory.SeedTransactionAsync(_repository, 3, user, category2);

            MonthlyPeriod period = TestDataFactory.CreateMonthlyPeriod(TestDataFactory.DEFAULT_DAILY_MONTH);

            // Act
            IEnumerable<Transaction> result = await _repository.GetByCategoryAndMonthlyPeriodAsync(userId, category1.Id, period);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count());
            Assert.All(result, t => Assert.Equal(category1.Id, t.CategoryId));
            Assert.All(result, t => Assert.Equal(TestDataFactory.DEFAULT_DAILY_MONTH, t.Date.Month));
            Assert.All(result, t => Assert.Equal(TestDataFactory.DEFAULT_YEAR, t.Date.Year));
        }

        [Fact]
        public async Task GetByCategoryAndMonthlyPeriodAsync_WithNonExistingCategory_ReturnsEmptyList()
        {
            // Arrange
            int userId = 1;
            MonthlyPeriod period = TestDataFactory.CreateMonthlyPeriod(TestDataFactory.DEFAULT_DAILY_MONTH);

            // Act
            IEnumerable<Transaction> result = await _repository.GetByCategoryAndMonthlyPeriodAsync(userId, 999, period);

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result);
        }

        // ==================== TEST: GET BY DATE RANGE ====================

        [Fact]
        public async Task GetByDateRangeAsync_WithExistingRange_ReturnsTransactions()
        {
            // Arrange
            int userId = 1;
            User user = TestDataFactory.CreateUser();
            Category category = TestDataFactory.CreateCategory(user: user);
            await TestDataFactory.SeedTransactionAsync(_repository, 1, user, category);
            await TestDataFactory.SeedTransactionAsync(_repository, 2, user, category);
            await TestDataFactory.SeedTransactionAsync(_repository, 3, user, category, month: 10);

            DailyPeriod startDate = TestDataFactory.CreateDailyPeriod(1);
            DailyPeriod endDate = TestDataFactory.CreateDailyPeriod(30);

            // Act
            IEnumerable<Transaction> result = await _repository.GetByDateRangeAsync(userId, startDate, endDate);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count());
            Assert.All(result, t => Assert.Equal(TestDataFactory.DEFAULT_DAILY_MONTH, t.Date.Month));
            Assert.All(result, t => Assert.Equal(TestDataFactory.DEFAULT_YEAR, t.Date.Year));
        }

        [Fact]
        public async Task GetByDateRangeAsync_WithEmptyRange_ReturnsEmptyList()
        {
            // Arrange
            int userId = 1;
            DailyPeriod startDate = TestDataFactory.CreateDailyPeriod(1);
            DailyPeriod endDate = TestDataFactory.CreateDailyPeriod(30);

            // Act
            IEnumerable<Transaction> result = await _repository.GetByDateRangeAsync(userId, startDate, endDate);

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result);
        }

        [Fact]
        public async Task GetByDateRangeAsync_WithNullStartDate_ThrowsArgumentNullException()
        {
            // Arrange
            int userId = 1;
            DailyPeriod endDate = TestDataFactory.CreateDailyPeriod(30);

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentNullException>(() => _repository.GetByDateRangeAsync(userId, null, endDate));
        }

        [Fact]
        public async Task GetByDateRangeAsync_WithNullEndDate_ThrowsArgumentNullException()
        {
            // Arrange
            int userId = 1;
            DailyPeriod startDate = TestDataFactory.CreateDailyPeriod(1);

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentNullException>(() => _repository.GetByDateRangeAsync(userId, startDate, null));
        }

        // ==================== TEST: GET TOTAL ====================

        [Fact]
        public async Task GetTotalByCategoryAndMonthlyPeriodAsync_ReturnsTotal()
        {
            // Arrange
            int userId = 1;
            decimal otherAmount = 30.00m;
            User user = TestDataFactory.CreateUser();
            Category category = TestDataFactory.CreateCategory(user: user);
            await TestDataFactory.SeedTransactionAsync(_repository, 1, user, category);
            await TestDataFactory.SeedTransactionAsync(_repository, 2, user, category, amount: otherAmount);
            await TestDataFactory.SeedTransactionAsync(_repository, 3, user, category, amount: 20.00m, month: 7);

            MonthlyPeriod period = TestDataFactory.CreateMonthlyPeriod(6);

            // Act
            decimal total = await _repository.GetTotalByCategoryAndMonthlyPeriodAsync(userId, category.Id, period);

            // Assert
            Assert.Equal(otherAmount + TestDataFactory.DEFAULT_TRANSACTION_AMOUNT, total);
        }

        [Fact]
        public async Task GetTotalByCategoryAndMonthlyPeriodAsync_WithNoTransactions_ReturnsZero()
        {
            // Arrange
            int userId = 1;
            int categoryId = 1;
            MonthlyPeriod period = TestDataFactory.CreateMonthlyPeriod(6);

            // Act
            decimal total = await _repository.GetTotalByCategoryAndMonthlyPeriodAsync(userId, categoryId, period);

            // Assert
            Assert.Equal(0, total);
        }

        // ==================== TEST: EXISTS ====================

        [Fact]
        public async Task ExistsAsync_WithExistingId_ReturnsTrue()
        {
            // Arrange
            int userId = 1;
            Transaction transaction = await TestDataFactory.SeedTransactionAsync(_repository, 1, TestDataFactory.CreateUser(), TestDataFactory.CreateCategory());

            // Act
            bool exists = await _repository.ExistsAsync(userId, transaction.Id);

            // Assert
            Assert.True(exists);
        }

        [Fact]
        public async Task ExistsAsync_WithNonExistingId_ReturnsFalse()
        {
            // Act
            int userId = 1;
            bool exists = await _repository.ExistsAsync(userId, 999);

            // Assert
            Assert.False(exists);
        }

        [Fact]
        public async Task ExistsAsync_WithInvalidId_ReturnsFalse()
        {
            // Act
            int userId = 1;
            bool exists = await _repository.ExistsAsync(userId, 0);

            // Assert
            Assert.False(exists);
        }

        // ==================== TEST: UPDATE ====================

        [Fact]
        public async Task UpdateAsync_ShouldUpdateTransaction()
        {
            // Arrange
            int userId = 1;
            EntityInfo updatedEntityInfo = TestDataFactory.CreateEntityInfo("Compra actualizada", "Nueva descripción");
            Money updatedAmount = TestDataFactory.CreateMoney(50.00m);
            TransactionTypeEnum updatedTransaction = TransactionTypeEnum.Income;
            DailyPeriod updatedDate = TestDataFactory.CreateDailyPeriod(20);

            Transaction transaction = await TestDataFactory.SeedTransactionAsync(_repository, 1, TestDataFactory.CreateUser(), TestDataFactory.CreateCategory());

            // Modificar la entidad
            transaction.Update
            (
                updatedEntityInfo,
                updatedAmount,
                updatedTransaction,
                updatedDate
            );

            // Act
            await _repository.UpdateAsync(transaction);

            // Assert
            Transaction? updated = await _repository.GetByIdAsync(userId, transaction.Id);
            Assert.NotNull(updated);
            Assert.Equal(updatedEntityInfo.Name, updated.Info.Name);
            Assert.Equal(updatedEntityInfo.Description, updated.Info.Description);
            Assert.Equal(updatedAmount.Value, updated.Amount.Value);
            Assert.Equal(updatedTransaction, updated.Type);
            Assert.Equal(updatedDate.Day, updated.Date.Day);
            Assert.Equal(updatedDate.Month, updated.Date.Month);
            Assert.Equal(updatedDate.Year, updated.Date.Year);
            Assert.NotNull(updated.UpdatedAt);
        }

        // ==================== TEST: DELETE ====================

        [Fact]
        public async Task DeleteAsync_ShouldRemoveTransaction()
        {
            // Arrange
            int userId = 1;
            Transaction transaction = await TestDataFactory.SeedTransactionAsync(_repository, 1, TestDataFactory.CreateUser(), TestDataFactory.CreateCategory());
            int id = transaction.Id;

            // Act
            await _repository.DeleteAsync(userId, id);

            // Assert
            Transaction? deleted = await _repository.GetByIdAsync(userId, id);
            Assert.Null(deleted);
        }

        [Fact]
        public async Task DeleteAsync_WithNonExistingId_ThrowsKeyNotFoundException()
        {
            // Act & Assert
            await Assert.ThrowsAsync<KeyNotFoundException>(() => _repository.DeleteAsync(999, 1));
        }

        [Fact]
        public async Task DeleteAsync_WithInvalidId_ThrowsArgumentException()
        {
            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() => _repository.DeleteAsync(0, 1));
        }

        // ==================== DISPOSE ====================

        public void Dispose()
        {
            _dbContext?.Dispose();
        }
    }
}