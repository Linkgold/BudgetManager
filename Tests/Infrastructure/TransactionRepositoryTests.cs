using Domain.Entities;
using Domain.Enums;
using Domain.Interfaces;
using Domain.ValueObjects;
using Infrastructure.Data;
using Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;

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

        // ==================== HELPERS ====================

        private Category CreateCategory(string name)
        {
            EntityInfo info = new EntityInfo(name, null);
            Category category = new Category(info);

            return category;
        }

        private Transaction CreateTransaction(Category category, string name, decimal amount, TransactionTypeEnum type, int day, int month, int year)
        {
            EntityInfo info = new EntityInfo(name, null);
            Money money = new Money(amount);
            DailyPeriod date = new DailyPeriod(day, month, year);
            Transaction transaction = new Transaction(category, info, money, type, date);

            return transaction;
        }

        private async Task<Category> SeedCategoryAsync(string name)
        {
            Category category = CreateCategory(name);
            await _categoryRepository.AddAsync(category);

            return category;
        }

        private async Task<Transaction> SeedTransactionAsync(Category category, string name, decimal amount, TransactionTypeEnum type, int day, int month, int year)
        {
            Transaction transaction = CreateTransaction(category, name, amount, type, day, month, year);
            await _repository.AddAsync(transaction);

            return transaction;
        }

        // ==================== TEST: ADD ====================

        [Fact]
        public async Task AddAsync_ShouldAddTransactionToDatabase()
        {
            // Arrange
            Category category = await SeedCategoryAsync("Alimentación");
            Transaction transaction = CreateTransaction(category, "Compra supermercado", 45.75m, TransactionTypeEnum.Expense, 15, 6, 2024);

            // Act
            await _repository.AddAsync(transaction);

            // Assert
            Transaction retrieved = await _dbContext.Transactions
                .Include(t => t.Category)
                .FirstOrDefaultAsync(t => t.Id == transaction.Id);

            Assert.NotNull(retrieved);
            Assert.Equal("Compra supermercado", retrieved.Info.Name);
            Assert.Equal(45.75m, retrieved.Amount.Value);
            Assert.Equal(TransactionTypeEnum.Expense, retrieved.Type);
            Assert.Equal(15, retrieved.Date.Day);
            Assert.Equal(6, retrieved.Date.Month);
            Assert.Equal(2024, retrieved.Date.Year);
            Assert.Equal(category.Id, retrieved.CategoryId);
            Assert.NotEqual(default(DateTime), retrieved.CreatedAt);
        }

        // ==================== TEST: GET BY ID ====================

        [Fact]
        public async Task GetByIdAsync_WithExistingId_ReturnsTransaction()
        {
            // Arrange
            Category category = await SeedCategoryAsync("Alimentación");
            Transaction transaction = await SeedTransactionAsync(category, "Compra supermercado", 45.75m, TransactionTypeEnum.Expense, 15, 6, 2024);

            // Act
            Transaction retrieved = await _repository.GetByIdAsync(transaction.Id);

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
            Transaction retrieved = await _repository.GetByIdAsync(999);

            // Assert
            Assert.Null(retrieved);
        }

        [Fact]
        public async Task GetByIdAsync_WithInvalidId_ThrowsArgumentException()
        {
            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() => _repository.GetByIdAsync(0));
        }

        // ==================== TEST: GET ALL ====================

        [Fact]
        public async Task GetAllAsync_ReturnsAllTransactions()
        {
            // Arrange
            Category category = await SeedCategoryAsync("Alimentación");
            await SeedTransactionAsync(category, "Compra 1", 45.75m, TransactionTypeEnum.Expense, 15, 6, 2024);
            await SeedTransactionAsync(category, "Compra 2", 30.00m, TransactionTypeEnum.Expense, 20, 6, 2024);

            // Act
            IEnumerable<Transaction> result = await _repository.GetAllAsync();

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count());
        }

        // ==================== TEST: GET BY CATEGORY ====================

        [Fact]
        public async Task GetByCategoryIdAsync_WithExistingCategory_ReturnsTransactions()
        {
            // Arrange
            Category category1 = await SeedCategoryAsync("Alimentación");
            Category category2 = await SeedCategoryAsync("Transporte");

            await SeedTransactionAsync(category1, "Compra 1", 45.75m, TransactionTypeEnum.Expense, 15, 6, 2024);
            await SeedTransactionAsync(category1, "Compra 2", 30.00m, TransactionTypeEnum.Expense, 20, 6, 2024);
            await SeedTransactionAsync(category2, "Gasolina", 50.00m, TransactionTypeEnum.Expense, 10, 6, 2024);

            // Act
            IEnumerable<Transaction> result = await _repository.GetByCategoryIdAsync(category1.Id);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count());
            Assert.All(result, t => Assert.Equal(category1.Id, t.CategoryId));
        }

        [Fact]
        public async Task GetByCategoryIdAsync_WithNonExistingCategory_ReturnsEmptyList()
        {
            // Act
            IEnumerable<Transaction> result = await _repository.GetByCategoryIdAsync(999);

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result);
        }

        // ==================== TEST: GET BY MONTHLY PERIOD ====================

        [Fact]
        public async Task GetByMonthlyPeriodAsync_WithExistingPeriod_ReturnsTransactions()
        {
            // Arrange
            Category category = await SeedCategoryAsync("Alimentación");
            await SeedTransactionAsync(category, "Compra 1", 45.75m, TransactionTypeEnum.Expense, 15, 6, 2024);
            await SeedTransactionAsync(category, "Compra 2", 30.00m, TransactionTypeEnum.Expense, 20, 6, 2024);
            await SeedTransactionAsync(category, "Compra 3", 20.00m, TransactionTypeEnum.Expense, 10, 7, 2024);

            MonthlyPeriod period = new MonthlyPeriod(6, 2024);

            // Act
            IEnumerable<Transaction> result = await _repository.GetByMonthlyPeriodAsync(period);

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
            MonthlyPeriod period = new MonthlyPeriod(12, 2025);

            // Act
            IEnumerable<Transaction> result = await _repository.GetByMonthlyPeriodAsync(period);

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result);
        }

        [Fact]
        public async Task GetByMonthlyPeriodAsync_WithNullPeriod_ThrowsArgumentNullException()
        {
            // Act & Assert
            await Assert.ThrowsAsync<ArgumentNullException>(() => _repository.GetByMonthlyPeriodAsync(null));
        }

        // ==================== TEST: GET BY CATEGORY AND MONTHLY PERIOD ====================

        [Fact]
        public async Task GetByCategoryAndMonthlyPeriodAsync_WithExistingCategoryAndPeriod_ReturnsTransactions()
        {
            // Arrange
            Category category1 = await SeedCategoryAsync("Alimentación");
            Category category2 = await SeedCategoryAsync("Transporte");

            await SeedTransactionAsync(category1, "Compra 1", 45.75m, TransactionTypeEnum.Expense, 15, 6, 2024);
            await SeedTransactionAsync(category1, "Compra 2", 30.00m, TransactionTypeEnum.Expense, 20, 6, 2024);
            await SeedTransactionAsync(category2, "Gasolina", 50.00m, TransactionTypeEnum.Expense, 10, 6, 2024);

            MonthlyPeriod period = new MonthlyPeriod(6, 2024);

            // Act
            IEnumerable<Transaction> result = await _repository.GetByCategoryAndMonthlyPeriodAsync(category1.Id, period);

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
            MonthlyPeriod period = new MonthlyPeriod(6, 2024);

            // Act
            IEnumerable<Transaction> result = await _repository.GetByCategoryAndMonthlyPeriodAsync(999, period);

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result);
        }

        // ==================== TEST: GET BY DATE RANGE ====================

        [Fact]
        public async Task GetByDateRangeAsync_WithExistingRange_ReturnsTransactions()
        {
            // Arrange
            Category category = await SeedCategoryAsync("Alimentación");
            await SeedTransactionAsync(category, "Compra 1", 45.75m, TransactionTypeEnum.Expense, 15, 6, 2024);
            await SeedTransactionAsync(category, "Compra 2", 30.00m, TransactionTypeEnum.Expense, 20, 6, 2024);
            await SeedTransactionAsync(category, "Compra 3", 20.00m, TransactionTypeEnum.Expense, 10, 7, 2024);

            DailyPeriod startDate = new DailyPeriod(1, 6, 2024);
            DailyPeriod endDate = new DailyPeriod(30, 6, 2024);

            // Act
            IEnumerable<Transaction> result = await _repository.GetByDateRangeAsync(startDate, endDate);

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
            DailyPeriod startDate = new DailyPeriod(1, 6, 2024);
            DailyPeriod endDate = new DailyPeriod(30, 6, 2024);

            // Act
            IEnumerable<Transaction> result = await _repository.GetByDateRangeAsync(startDate, endDate);

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result);
        }

        [Fact]
        public async Task GetByDateRangeAsync_WithNullStartDate_ThrowsArgumentNullException()
        {
            // Arrange
            DailyPeriod endDate = new DailyPeriod(30, 6, 2024);

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentNullException>(() => _repository.GetByDateRangeAsync(null, endDate));
        }

        [Fact]
        public async Task GetByDateRangeAsync_WithNullEndDate_ThrowsArgumentNullException()
        {
            // Arrange
            DailyPeriod startDate = new DailyPeriod(1, 6, 2024);

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentNullException>(() => _repository.GetByDateRangeAsync(startDate, null));
        }

        // ==================== TEST: GET TOTAL ====================

        [Fact]
        public async Task GetTotalByCategoryAndMonthlyPeriodAsync_ReturnsTotal()
        {
            // Arrange
            Category category = await SeedCategoryAsync("Alimentación");
            await SeedTransactionAsync(category, "Compra 1", 45.75m, TransactionTypeEnum.Expense, 15, 6, 2024);
            await SeedTransactionAsync(category, "Compra 2", 30.00m, TransactionTypeEnum.Expense, 20, 6, 2024);
            await SeedTransactionAsync(category, "Compra 3", 20.00m, TransactionTypeEnum.Expense, 10, 7, 2024);

            MonthlyPeriod period = new MonthlyPeriod(6, 2024);

            // Act
            decimal total = await _repository.GetTotalByCategoryAndMonthlyPeriodAsync(category.Id, period);

            // Assert
            Assert.Equal(75.75m, total);
        }

        [Fact]
        public async Task GetTotalByCategoryAndMonthlyPeriodAsync_WithNoTransactions_ReturnsZero()
        {
            // Arrange
            Category category = await SeedCategoryAsync("Alimentación");
            MonthlyPeriod period = new MonthlyPeriod(6, 2024);

            // Act
            decimal total = await _repository.GetTotalByCategoryAndMonthlyPeriodAsync(category.Id, period);

            // Assert
            Assert.Equal(0, total);
        }

        // ==================== TEST: EXISTS ====================

        [Fact]
        public async Task ExistsAsync_WithExistingId_ReturnsTrue()
        {
            // Arrange
            Category category = await SeedCategoryAsync("Alimentación");
            Transaction transaction = await SeedTransactionAsync(category, "Compra 1", 45.75m, TransactionTypeEnum.Expense, 15, 6, 2024);

            // Act
            bool exists = await _repository.ExistsAsync(transaction.Id);

            // Assert
            Assert.True(exists);
        }

        [Fact]
        public async Task ExistsAsync_WithNonExistingId_ReturnsFalse()
        {
            // Act
            bool exists = await _repository.ExistsAsync(999);

            // Assert
            Assert.False(exists);
        }

        [Fact]
        public async Task ExistsAsync_WithInvalidId_ReturnsFalse()
        {
            // Act
            bool exists = await _repository.ExistsAsync(0);

            // Assert
            Assert.False(exists);
        }

        // ==================== TEST: UPDATE ====================

        [Fact]
        public async Task UpdateAsync_ShouldUpdateTransaction()
        {
            // Arrange
            Category category = await SeedCategoryAsync("Alimentación");
            Transaction transaction = await SeedTransactionAsync(category, "Compra 1", 45.75m, TransactionTypeEnum.Expense, 15, 6, 2024);

            // Modificar la entidad
            EntityInfo newInfo = new EntityInfo("Compra actualizada", "Nueva descripción");
            Money newAmount = new Money(50.00m);
            DailyPeriod newDate = new DailyPeriod(20, 6, 2024);
            transaction.Update(newInfo, newAmount, TransactionTypeEnum.Income, newDate);

            // Act
            await _repository.UpdateAsync(transaction);

            // Assert
            Transaction updated = await _repository.GetByIdAsync(transaction.Id);
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
            Category category = await SeedCategoryAsync("Alimentación");
            Transaction transaction = await SeedTransactionAsync(category, "Compra 1", 45.75m, TransactionTypeEnum.Expense, 15, 6, 2024);
            int id = transaction.Id;

            // Act
            await _repository.DeleteAsync(id);

            // Assert
            Transaction deleted = await _repository.GetByIdAsync(id);
            Assert.Null(deleted);
        }

        [Fact]
        public async Task DeleteAsync_WithNonExistingId_ThrowsKeyNotFoundException()
        {
            // Act & Assert
            await Assert.ThrowsAsync<KeyNotFoundException>(() => _repository.DeleteAsync(999));
        }

        [Fact]
        public async Task DeleteAsync_WithInvalidId_ThrowsArgumentException()
        {
            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() => _repository.DeleteAsync(0));
        }

        // ==================== DISPOSE ====================

        public void Dispose()
        {
            _dbContext?.Dispose();
        }
    }
}