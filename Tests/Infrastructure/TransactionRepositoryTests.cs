using Contracts.Enums;
using Domain.Entities;
using Domain.Interfaces;
using Domain.ValueObjects;
using Infrastructure.Data;
using Infrastructure.Repositories;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Tests.Helpers;

namespace Tests.Infrastructure
{
    public class TransactionRepositoryTests : IDisposable
    {
        private readonly SqliteConnection _connection;
        private readonly ApplicationDbContext _dbContext;
        private readonly ITransactionRepository _repository;
        private readonly ICategoryRepository _categoryRepository;

        public TransactionRepositoryTests()
        {
            _connection = new SqliteConnection("DataSource=:memory:");
            _connection.Open();

            DbContextOptions<ApplicationDbContext> options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseSqlite(_connection)
                .Options;

            _dbContext = new ApplicationDbContext(options);
            _dbContext.EnsureDatabaseCreated();

            _repository = new TransactionRepository(_dbContext);
            _categoryRepository = new CategoryRepository(_dbContext);
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
            Assert.Equal(TestDataFactory.DEFAULT_CURRENCY, retrieved.Amount.Currency);
            Assert.Equal(TestDataFactory.DEFAULT_DAILY_DAY, retrieved.Date.Day);
            Assert.Equal(TestDataFactory.DEFAULT_DAILY_MONTH, retrieved.Date.Month);
            Assert.Equal(TestDataFactory.DEFAULT_YEAR, retrieved.Date.Year);
            Assert.Equal(category.Id, retrieved.CategoryId);
            Assert.NotEqual(default, retrieved.CreatedAt);
        }

        [Fact]
        public async Task AddAsync_ShouldAddTransactionWithCNYCurrency()
        {
            // Arrange
            string customCurrency = "CNY";

            Transaction transaction = TestDataFactory.CreateTransaction(currency: customCurrency);

            // Act
            await _repository.AddAsync(transaction);

            // Assert
            Transaction? retrieved = await _dbContext.Transactions
                .Include(t => t.Category)
                .FirstOrDefaultAsync(t => t.Id == transaction.Id);

            Assert.NotNull(retrieved);
            Assert.Equal(TestDataFactory.DEFAULT_TRANSACTION_AMOUNT, retrieved.Amount.Value);
            Assert.Equal(customCurrency, retrieved.Amount.Currency);
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
            await TestDataFactory.SeedTransactionAsync(_repository, 1, user, category, year: 2003);
            await TestDataFactory.SeedTransactionAsync(_repository, 2, user, category);

            // Act
            IEnumerable<Transaction> result = await _repository.GetAllByYearAsync(userId, 2003);

            // Assert
            Assert.NotNull(result);
            Assert.Single(result);
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
            User user = TestDataFactory.CreateUser();
            Category category = TestDataFactory.CreateCategory(1, user);
            EntityInfo updatedEntityInfo = TestDataFactory.CreateEntityInfo("Compra actualizada", "Nueva descripción");
            Money updatedAmount = TestDataFactory.CreateMoney(50.00m);
            DailyPeriod updatedDate = TestDataFactory.CreateDailyPeriod(20);

            Category updatedCategory = await TestDataFactory.SeedCategoryAsync(_categoryRepository, 2, user, "Test 2");
            Transaction transaction = await TestDataFactory.SeedTransactionAsync(_repository, 1, user, category);

            // Modificar la entidad
            transaction.Update(updatedCategory, updatedEntityInfo, updatedAmount, TestDataFactory.DEFAULT_TRANSACTION_TYPE, updatedDate);

            // Act
            await _repository.UpdateAsync(transaction);

            // Assert
            Transaction? updated = await _repository.GetByIdAsync(userId, transaction.Id);
            Assert.NotNull(updated);
            Assert.Equal(updatedCategory.Id, updated.CategoryId);
            Assert.Equal(updatedCategory.Info.Name, updated.Category.Info.Name);
            Assert.Equal(updatedEntityInfo.Name, updated.Info.Name);
            Assert.Equal(updatedEntityInfo.Description, updated.Info.Description);
            Assert.Equal(updatedAmount.Value, updated.Amount.Value);
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

        // ==================== GET DISTINCT YEARS AND MONTHS ====================

        [Fact]
        public async Task GetDistinctYearsAndMonthsAsync_WithData_ReturnsDictionary()
        {
            // Arrange
            int userId = 1;
            User user = TestDataFactory.CreateUser(userId);
            Category category = TestDataFactory.CreateCategory(1, user);

            // Crear transacciones en diferentes años y meses
            await TestDataFactory.SeedTransactionAsync(_repository, 1, user, category, "Compra1", amount: 100.00m, day: 15, month: 1, year: 2023);
            await TestDataFactory.SeedTransactionAsync(_repository, 2, user, category, "Compra2", amount: 200.00m, day: 20, month: 2, year: 2023);
            await TestDataFactory.SeedTransactionAsync(_repository, 3, user, category, "Compra3", amount: 150.00m, day: 10, month: 1);
            await TestDataFactory.SeedTransactionAsync(_repository, 4, user, category, "Compra4", amount: 300.00m, month: 3);

            // Expected
            Dictionary<int, List<int>> expected = new Dictionary<int, List<int>>
            {
                { 2023, new List<int> { 1, 2 } },
                { 2024, new List<int> { 1, 3 } }
            };

            // Act
            Dictionary<int, List<int>> result = await _repository.GetDistinctYearsAndMonthsAsync(userId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(expected.Count, result.Count);
            Assert.True(result.ContainsKey(2023));
            Assert.True(result.ContainsKey(2024));
            Assert.Equal(expected[2023], result[2023]);
            Assert.Equal(expected[2024], result[2024]);
        }

        [Fact]
        public async Task GetDistinctYearsAndMonthsAsync_WithNoData_ReturnsEmptyDictionary()
        {
            // Arrange
            int userId = 1;

            // Act
            Dictionary<int, List<int>> result = await _repository.GetDistinctYearsAndMonthsAsync(userId);

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result);
        }

        [Fact]
        public async Task GetDistinctYearsAndMonthsAsync_WithInvalidUserId_ThrowsArgumentException()
        {
            // Arrange
            int invalidUserId = 0;

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() => _repository.GetDistinctYearsAndMonthsAsync(invalidUserId));
        }

        [Fact]
        public async Task GetDistinctYearsAndMonthsAsync_WithMultipleMonthsInSameYear_ReturnsDistinctMonths()
        {
            // Arrange
            int userId = 1;
            User user = TestDataFactory.CreateUser(userId);
            Category category = TestDataFactory.CreateCategory(1, user);

            // Crear múltiples transacciones en el mismo mes (debe devolver solo una vez)
            await TestDataFactory.SeedTransactionAsync(_repository, 1, user, category, "Compra1", amount: 100.00m, month: 1);
            await TestDataFactory.SeedTransactionAsync(_repository, 2, user, category, "Compra2", amount: 200.00m, day: 20, month: 1);
            await TestDataFactory.SeedTransactionAsync(_repository, 3, user, category, "Compra3", amount: 150.00m, day: 10, month: 2);

            // Expected: solo meses 1 y 2 (distintos)
            Dictionary<int, List<int>> expected = new Dictionary<int, List<int>>
            {
                { 2024, new List<int> { 1, 2 } }
            };

            // Act
            Dictionary<int, List<int>> result = await _repository.GetDistinctYearsAndMonthsAsync(userId);

            // Assert
            Assert.NotNull(result);
            Assert.Single(result);
            Assert.True(result.ContainsKey(2024));
            Assert.Equal(expected[2024], result[2024]);
        }

        // ==================== DISPOSE ====================

        public void Dispose()
        {
            _dbContext?.Dispose();
            _connection?.Dispose();
        }
    }
}