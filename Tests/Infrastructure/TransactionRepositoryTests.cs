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
        private readonly ICategoryRepository _categoryRepository;

        public TransactionRepositoryTests()
        {
            string databaseName = Guid.NewGuid().ToString();

            DbContextOptions<ApplicationDbContext> options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: databaseName)
                .Options;

            _dbContext = new ApplicationDbContext(options);
            _repository = new TransactionRepository(_dbContext);
            _categoryRepository = new CategoryRepository(_dbContext);
        }

        // ==================== TEST: ADD ====================

        [Fact]
        public async Task AddAsync_ShouldAddTransactionToDatabase()
        {
            // Arrange
            Category category = await TestDataFactory.SeedCategoryAsync(_categoryRepository);
            Transaction transaction = TestDataFactory.CreateTransaction();

            // Act
            await _repository.AddAsync(transaction);

            // Assert
            Transaction? retrieved = await _dbContext.Transactions.Include(t => t.Category).FirstOrDefaultAsync(t => t.Id == transaction.Id);

            Assert.NotNull(retrieved);
            Assert.Equal("Compra supermercado", retrieved.Info.Name);
            Assert.Equal(45.75m, retrieved.Amount.Value);
            Assert.Equal(TransactionTypeEnum.Expense, retrieved.Type);
            Assert.Equal(15, retrieved.Date.Day);
            Assert.Equal(6, retrieved.Date.Month);
            Assert.Equal(2024, retrieved.Date.Year);
            Assert.Equal(category.Id, retrieved.CategoryId);
            Assert.NotEqual(default, retrieved.CreatedAt);
        }

        // ==================== TEST: GET BY ID ====================

        [Fact]
        public async Task GetByIdAsync_WithExistingId_ReturnsTransaction()
        {
            // Arrange
            int userId = 1;
            Category category = await TestDataFactory.SeedCategoryAsync(_categoryRepository);
            Transaction transaction = await TestDataFactory.SeedTransactionAsync(_repository);

            // Act
            Transaction? retrieved = await _repository.GetByIdAsync(transaction.Id, userId);

            // Assert
            Assert.NotNull(retrieved);
            Assert.Equal(transaction.Id, retrieved.Id);
            Assert.Equal(45.75m, retrieved.Amount.Value);
            Assert.Equal(TransactionTypeEnum.Expense, retrieved.Type);
            Assert.Equal(15, retrieved.Date.Day);
            Assert.Equal(6, retrieved.Date.Month);
            Assert.Equal(2024, retrieved.Date.Year);
            Assert.Equal(category.Id, retrieved.CategoryId);
            Assert.NotNull(retrieved.Category);
        }

        [Fact]
        public async Task GetByIdAsync_WithNonExistingId_ReturnsNull()
        {
            // Act
            int userId = 1;
            Transaction? retrieved = await _repository.GetByIdAsync(999, userId);

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
            Category category = await TestDataFactory.SeedCategoryAsync(_categoryRepository);
            await TestDataFactory.SeedTransactionAsync(_repository);
            await TestDataFactory.SeedTransactionAsync(_repository, category, TestDataFactory.CreateTransaction(2, TestDataFactory.CreateUser(), category, "Compra 2", "", 30.00m));

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
            Category category1 = await TestDataFactory.SeedCategoryAsync(_categoryRepository);
            Category category2 = await TestDataFactory.SeedCategoryAsync(_categoryRepository, TestDataFactory.CreateCategory(2, TestDataFactory.CreateUser(), "Transporte"));

            await TestDataFactory.SeedTransactionAsync(_repository);
            await TestDataFactory.SeedTransactionAsync(_repository, category1, TestDataFactory.CreateTransaction(2, TestDataFactory.CreateUser(), category1, "Compra 2", "", 30.00m));
            await TestDataFactory.SeedTransactionAsync(_repository, category2, TestDataFactory.CreateTransaction(3, TestDataFactory.CreateUser(), category2, "Gasolina", "", 50.00m));

            // Act
            IEnumerable<Transaction> result = await _repository.GetByCategoryIdAsync(category1.Id, userId);

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
            IEnumerable<Transaction> result = await _repository.GetByCategoryIdAsync(999, userId);

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
            Category category = await TestDataFactory.SeedCategoryAsync(_categoryRepository);
            await TestDataFactory.SeedTransactionAsync(_repository);
            await TestDataFactory.SeedTransactionAsync(_repository, category, TestDataFactory.CreateTransaction(2, TestDataFactory.CreateUser(), category, "Compra 2", "", 30.00m));
            await TestDataFactory.SeedTransactionAsync(_repository, category, TestDataFactory.CreateTransaction(3, TestDataFactory.CreateUser(), category, "Compra 3", "", 20.00m, TransactionTypeEnum.Expense, 10, 7, 2024));

            MonthlyPeriod period = new MonthlyPeriod(6, 2024);

            // Act
            IEnumerable<Transaction> result = await _repository.GetByMonthlyPeriodAsync(period, userId);

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
            MonthlyPeriod period = new MonthlyPeriod(12, 2025);

            // Act
            IEnumerable<Transaction> result = await _repository.GetByMonthlyPeriodAsync(period, userId);

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result);
        }

        [Fact]
        public async Task GetByMonthlyPeriodAsync_WithNullPeriod_ThrowsArgumentNullException()
        {
            // Act & Assert
            await Assert.ThrowsAsync<ArgumentNullException>(() => _repository.GetByMonthlyPeriodAsync(null, 1));
        }

        // ==================== TEST: GET BY CATEGORY AND MONTHLY PERIOD ====================

        [Fact]
        public async Task GetByCategoryAndMonthlyPeriodAsync_WithExistingCategoryAndPeriod_ReturnsTransactions()
        {
            // Arrange
            int userId = 1;
            Category category1 = await TestDataFactory.SeedCategoryAsync(_categoryRepository);
            Category category2 = await TestDataFactory.SeedCategoryAsync(_categoryRepository, TestDataFactory.CreateCategory(2, TestDataFactory.CreateUser(), "Transporte"));

            await TestDataFactory.SeedTransactionAsync(_repository);
            await TestDataFactory.SeedTransactionAsync(_repository, category1, TestDataFactory.CreateTransaction(2, TestDataFactory.CreateUser(), category1, "Compra 2", "", 30.00m));
            await TestDataFactory.SeedTransactionAsync(_repository, category2, TestDataFactory.CreateTransaction(3, TestDataFactory.CreateUser(), category2, "Gasolina", "", 50.00m));

            MonthlyPeriod period = new MonthlyPeriod(6, 2024);

            // Act
            IEnumerable<Transaction> result = await _repository.GetByCategoryAndMonthlyPeriodAsync(category1.Id, period, userId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count());
            Assert.All(result, t => Assert.Equal(category1.Id, t.CategoryId));
            Assert.All(result, t => Assert.Equal(6, t.Date.Month));
            Assert.All(result, t => Assert.Equal(2024, t.Date.Year));
        }

        [Fact]
        public async Task GetByCategoryAndMonthlyPeriodAsync_WithNonExistingCategory_ReturnsEmptyList()
        {
            // Arrange
            int userId = 1;
            MonthlyPeriod period = new MonthlyPeriod(6, 2024);

            // Act
            IEnumerable<Transaction> result = await _repository.GetByCategoryAndMonthlyPeriodAsync(999, period, userId);

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
            Category category = await TestDataFactory.SeedCategoryAsync(_categoryRepository);
            await TestDataFactory.SeedTransactionAsync(_repository);
            await TestDataFactory.SeedTransactionAsync(_repository, category, TestDataFactory.CreateTransaction(2, TestDataFactory.CreateUser(), category, "Compra 2", "", 30.00m));
            await TestDataFactory.SeedTransactionAsync(_repository, category, TestDataFactory.CreateTransaction(3, TestDataFactory.CreateUser(), category, "Compra 3", "", 20.00m, TransactionTypeEnum.Expense, 10, 7, 2024));

            DailyPeriod startDate = new DailyPeriod(1, 6, 2024);
            DailyPeriod endDate = new DailyPeriod(30, 6, 2024);

            // Act
            IEnumerable<Transaction> result = await _repository.GetByDateRangeAsync(startDate, endDate, userId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count());
            Assert.All(result, t => Assert.Equal(6, t.Date.Month));
            Assert.All(result, t => Assert.Equal(2024, t.Date.Year));
        }

        [Fact]
        public async Task GetByDateRangeAsync_WithEmptyRange_ReturnsEmptyList()
        {
            // Arrange
            int userId = 1;
            DailyPeriod startDate = new DailyPeriod(1, 6, 2024);
            DailyPeriod endDate = new DailyPeriod(30, 6, 2024);

            // Act
            IEnumerable<Transaction> result = await _repository.GetByDateRangeAsync(startDate, endDate, userId);

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result);
        }

        [Fact]
        public async Task GetByDateRangeAsync_WithNullStartDate_ThrowsArgumentNullException()
        {
            // Arrange
            int userId = 1;
            DailyPeriod endDate = new DailyPeriod(30, 6, 2024);

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentNullException>(() => _repository.GetByDateRangeAsync(null, endDate, userId));
        }

        [Fact]
        public async Task GetByDateRangeAsync_WithNullEndDate_ThrowsArgumentNullException()
        {
            // Arrange
            int userId = 1;
            DailyPeriod startDate = new DailyPeriod(1, 6, 2024);

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentNullException>(() => _repository.GetByDateRangeAsync(startDate, null, userId));
        }

        // ==================== TEST: GET TOTAL ====================

        [Fact]
        public async Task GetTotalByCategoryAndMonthlyPeriodAsync_ReturnsTotal()
        {
            // Arrange
            int userId = 1;
            Category category = await TestDataFactory.SeedCategoryAsync(_categoryRepository);
            await TestDataFactory.SeedTransactionAsync(_repository);
            await TestDataFactory.SeedTransactionAsync(_repository, category, TestDataFactory.CreateTransaction(2, TestDataFactory.CreateUser(), category, "Compra 2", "", 30.00m));
            await TestDataFactory.SeedTransactionAsync(_repository, category, TestDataFactory.CreateTransaction(3, TestDataFactory.CreateUser(), category, "Compra 3", "", 20.00m, TransactionTypeEnum.Expense, 10, 7, 2024));

            MonthlyPeriod period = new MonthlyPeriod(6, 2024);

            // Act
            decimal total = await _repository.GetTotalByCategoryAndMonthlyPeriodAsync(category.Id, period, userId);

            // Assert
            Assert.Equal(75.75m, total);
        }

        [Fact]
        public async Task GetTotalByCategoryAndMonthlyPeriodAsync_WithNoTransactions_ReturnsZero()
        {
            // Arrange
            int userId = 1;
            Category category = await TestDataFactory.SeedCategoryAsync(_categoryRepository);
            MonthlyPeriod period = new MonthlyPeriod(6, 2024);

            // Act
            decimal total = await _repository.GetTotalByCategoryAndMonthlyPeriodAsync(category.Id, period, userId);

            // Assert
            Assert.Equal(0, total);
        }

        // ==================== TEST: EXISTS ====================

        [Fact]
        public async Task ExistsAsync_WithExistingId_ReturnsTrue()
        {
            // Arrange
            int userId = 1;
            Category category = await TestDataFactory.SeedCategoryAsync(_categoryRepository);
            Transaction transaction = await TestDataFactory.SeedTransactionAsync(_repository);

            // Act
            bool exists = await _repository.ExistsAsync(transaction.Id, userId);

            // Assert
            Assert.True(exists);
        }

        [Fact]
        public async Task ExistsAsync_WithNonExistingId_ReturnsFalse()
        {
            // Act
            int userId = 1;
            bool exists = await _repository.ExistsAsync(999,userId);

            // Assert
            Assert.False(exists);
        }

        [Fact]
        public async Task ExistsAsync_WithInvalidId_ReturnsFalse()
        {
            // Act
            int userId = 1;
            bool exists = await _repository.ExistsAsync(0, userId);

            // Assert
            Assert.False(exists);
        }

        // ==================== TEST: UPDATE ====================

        [Fact]
        public async Task UpdateAsync_ShouldUpdateTransaction()
        {
            // Arrange
            int userId = 1;
            Category category = await TestDataFactory.SeedCategoryAsync(_categoryRepository);
            Transaction transaction = await TestDataFactory.SeedTransactionAsync(_repository);

            // Modificar la entidad
            EntityInfo newInfo = new EntityInfo("Compra actualizada", "Nueva descripción");
            Money newAmount = new Money(50.00m);
            DailyPeriod newDate = new DailyPeriod(20, 6, 2024);
            transaction.Update(newInfo, newAmount, TransactionTypeEnum.Income, newDate);

            // Act
            await _repository.UpdateAsync(transaction);

            // Assert
            Transaction? updated = await _repository.GetByIdAsync(transaction.Id, userId);
            Assert.NotNull(updated);
            Assert.Equal("Compra actualizada", updated.Info.Name);
            Assert.Equal("Nueva descripción", updated.Info.Description);
            Assert.Equal(50.00m, updated.Amount.Value);
            Assert.Equal(TransactionTypeEnum.Income, updated.Type);
            Assert.Equal(20, updated.Date.Day);
            Assert.Equal(6, updated.Date.Month);
            Assert.Equal(2024, updated.Date.Year);
            Assert.NotNull(updated.UpdatedAt);
        }

        // ==================== TEST: DELETE ====================

        [Fact]
        public async Task DeleteAsync_ShouldRemoveTransaction()
        {
            // Arrange
            int userId = 1;
            Category category = await TestDataFactory.SeedCategoryAsync(_categoryRepository);
            Transaction transaction = await TestDataFactory.SeedTransactionAsync(_repository);
            int id = transaction.Id;

            // Act
            await _repository.DeleteAsync(id, userId);

            // Assert
            Transaction? deleted = await _repository.GetByIdAsync(id, userId);
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