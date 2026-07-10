using Domain.Entities;
using Domain.Interfaces;
using Domain.ValueObjects;
using Infrastructure.Data;
using Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Tests.Helpers;

namespace Tests.Infrastructure
{
    /// <summary>
    /// Pruebas unitarias para BudgetRepository usando InMemory Database
    /// </summary>
    public class BudgetRepositoryTests : IDisposable
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly IBudgetRepository _repository;
        private readonly ICategoryRepository _categoryRepository;

        public BudgetRepositoryTests()
        {
            // Crear un nombre de base de datos único para cada prueba
            string databaseName = Guid.NewGuid().ToString();

            DbContextOptions<ApplicationDbContext> options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: databaseName)
                .Options;

            _dbContext = new ApplicationDbContext(options);
            _repository = new BudgetRepository(_dbContext);
            _categoryRepository = new CategoryRepository(_dbContext);
        }

        // ==================== TEST: ADD ====================

        [Fact]
        public async Task AddAsync_ShouldAddBudgetToDatabase()
        {
            // Arrange
            Category category = await TestDataFactory.SeedCategoryAsync(_categoryRepository);
            Budget budget = TestDataFactory.CreateBudget();

            // Act
            await _repository.AddAsync(budget);

            // Assert
            Budget? retrieved = await _dbContext.Budgets
                .Include(b => b.Category)
                .FirstOrDefaultAsync(b => b.Id == budget.Id);

            Assert.NotNull(retrieved);
            Assert.Equal(500.00m, retrieved.MonthlyAmount.Value);
            Assert.Equal(2024, retrieved.Period.Year);
            Assert.Equal(1, retrieved.Period.Month);
            Assert.Equal(category.Id, retrieved.CategoryId);
            Assert.NotEqual(default, retrieved.CreatedAt);
        }

        // ==================== TEST: GET BY ID ====================

        [Fact]
        public async Task GetByIdAsync_WithExistingId_ReturnsBudget()
        {
            // Arrange
            int userId = 1;
            Category category = await TestDataFactory.SeedCategoryAsync(_categoryRepository);
            Budget budget = await TestDataFactory.SeedBudgetAsync(_repository);

            // Act
            Budget? retrieved = await _repository.GetByIdAsync(budget.Id, userId);

            // Assert
            Assert.NotNull(retrieved);
            Assert.Equal(budget.Id, retrieved.Id);
            Assert.Equal(500.00m, retrieved.MonthlyAmount.Value);
            Assert.Equal(2024, retrieved.Period.Year);
            Assert.Equal(1, retrieved.Period.Month);
            Assert.Equal(category.Id, retrieved.CategoryId);
            Assert.NotNull(retrieved.Category);
        }

        [Fact]
        public async Task GetByIdAsync_WithNonExistingId_ReturnsNull()
        {
            // Act
            int userId = 1;
            Budget? retrieved = await _repository.GetByIdAsync(999, userId);

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
        public async Task GetAllAsync_ReturnsAllBudgets()
        {
            // Arrange
            int userId = 1;
            Category category = await TestDataFactory.SeedCategoryAsync(_categoryRepository);
            await TestDataFactory.SeedBudgetAsync(_repository);
            await TestDataFactory.SeedBudgetAsync(_repository, category, TestDataFactory.CreateBudget(1, TestDataFactory.CreateUser(), category, 300.00m, 1, 2024));

            // Act
            IEnumerable<Budget> result = await _repository.GetAllAsync(userId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count());
        }

        // ==================== TEST: GET BY CATEGORY ====================

        [Fact]
        public async Task GetByCategoryIdAsync_WithExistingCategory_ReturnsBudgets()
        {
            // Arrange
            int userId = 1;
            Category category1 = await TestDataFactory.SeedCategoryAsync(_categoryRepository);
            Category category2 = await TestDataFactory.SeedCategoryAsync(_categoryRepository, TestDataFactory.CreateCategory(1, TestDataFactory.CreateUser(), "Transporte"));

            await TestDataFactory.SeedBudgetAsync(_repository);
            await TestDataFactory.SeedBudgetAsync(_repository, category1, TestDataFactory.CreateBudget(1, TestDataFactory.CreateUser(), category1, 300.00m, 1, 2024));
            await TestDataFactory.SeedBudgetAsync(_repository, category2, TestDataFactory.CreateBudget(1, TestDataFactory.CreateUser(), category2, 200.00m, 1, 2024));

            // Act
            IEnumerable<Budget> result = await _repository.GetByCategoryIdAsync(category1.Id, userId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count());
            Assert.All(result, b => Assert.Equal(category1.Id, b.CategoryId));
        }

        [Fact]
        public async Task GetByCategoryIdAsync_WithNonExistingCategory_ReturnsEmptyList()
        {
            // Act
            IEnumerable<Budget> result = await _repository.GetByCategoryIdAsync(999, 1);

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result);
        }

        // ==================== TEST: GET BY PERIOD ====================

        [Fact]
        public async Task GetByPeriodAsync_WithExistingPeriod_ReturnsBudgets()
        {
            // Arrange
            int userId = 1;
            Category category = await TestDataFactory.SeedCategoryAsync(_categoryRepository);
            await TestDataFactory.SeedBudgetAsync(_repository);
            await TestDataFactory.SeedBudgetAsync(_repository, category, TestDataFactory.CreateBudget(1, TestDataFactory.CreateUser(), category, 300.00m, 1, 2024));
            await TestDataFactory.SeedBudgetAsync(_repository, category, TestDataFactory.CreateBudget(1, TestDataFactory.CreateUser(), category, 200.00m, 1, 2024));

            MonthlyPeriod period = new MonthlyPeriod(1, 2024);

            // Act
            IEnumerable<Budget> result = await _repository.GetByPeriodAsync(period, userId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count());
            Assert.All(result, b => Assert.Equal(1, b.Period.Month));
            Assert.All(result, b => Assert.Equal(2024, b.Period.Year));
        }

        [Fact]
        public async Task GetByPeriodAsync_WithNonExistingPeriod_ReturnsEmptyList()
        {
            // Arrange
            int userId = 1;
            MonthlyPeriod period = new MonthlyPeriod(12, 2025);

            // Act
            IEnumerable<Budget> result = await _repository.GetByPeriodAsync(period, userId);

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result);
        }

        [Fact]
        public async Task GetByPeriodAsync_WithNullPeriod_ThrowsArgumentNullException()
        {
            // Act & Assert
            await Assert.ThrowsAsync<ArgumentNullException>(() => _repository.GetByPeriodAsync(null, 1));
        }

        // ==================== TEST: GET BY CATEGORY AND PERIOD ====================

        [Fact]
        public async Task GetByCategoryAndPeriodAsync_WithExistingBudget_ReturnsBudget()
        {
            // Arrange
            int userId = 1;
            Category category = await TestDataFactory.SeedCategoryAsync(_categoryRepository);
            await TestDataFactory.SeedBudgetAsync(_repository);
            await TestDataFactory.SeedBudgetAsync(_repository, category, TestDataFactory.CreateBudget(1, TestDataFactory.CreateUser(), category, 300.00m, 1, 2024));

            MonthlyPeriod period = new MonthlyPeriod(1, 2024);

            // Act
            Budget? result = await _repository.GetByCategoryAndPeriodAsync(category.Id, period, userId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(500.00m, result.MonthlyAmount.Value);
            Assert.Equal(2024, result.Period.Year);
            Assert.Equal(1, result.Period.Month);
            Assert.Equal(category.Id, result.CategoryId);
        }

        [Fact]
        public async Task GetByCategoryAndPeriodAsync_WithNonExistingBudget_ReturnsNull()
        {
            // Arrange
            int userId = 1;
            Category category = await TestDataFactory.SeedCategoryAsync(_categoryRepository);
            MonthlyPeriod period = new MonthlyPeriod(12, 2025);

            // Act
            Budget? result = await _repository.GetByCategoryAndPeriodAsync(category.Id, period, userId);

            // Assert
            Assert.Null(result);
        }

        // ==================== TEST: EXISTS ====================

        [Fact]
        public async Task ExistsAsync_WithExistingId_ReturnsTrue()
        {
            // Arrange
            int userId = 1;
            Category category = await TestDataFactory.SeedCategoryAsync(_categoryRepository);
            Budget budget = await TestDataFactory.SeedBudgetAsync(_repository);

            // Act
            bool exists = await _repository.ExistsAsync(budget.Id, userId);

            // Assert
            Assert.True(exists);
        }

        [Fact]
        public async Task ExistsAsync_WithNonExistingId_ReturnsFalse()
        {
            // Act
            int userId = 1;
            bool exists = await _repository.ExistsAsync(999, userId);

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

        // ==================== TEST: EXISTS FOR CATEGORY AND PERIOD ====================

        [Fact]
        public async Task ExistsForCategoryAndPeriodAsync_WithExistingBudget_ReturnsTrue()
        {
            // Arrange
            int userId = 1;
            Category category = await TestDataFactory.SeedCategoryAsync(_categoryRepository);
            await TestDataFactory.SeedBudgetAsync(_repository);

            MonthlyPeriod period = new MonthlyPeriod(1, 2024);

            // Act
            bool exists = await _repository.ExistsForCategoryAndPeriodAsync(category.Id, period, userId);

            // Assert
            Assert.True(exists);
        }

        [Fact]
        public async Task ExistsForCategoryAndPeriodAsync_WithNonExistingBudget_ReturnsFalse()
        {
            // Arrange
            int userId = 1;
            Category category = await TestDataFactory.SeedCategoryAsync(_categoryRepository);
            MonthlyPeriod period = new MonthlyPeriod(12, 2025);

            // Act
            bool exists = await _repository.ExistsForCategoryAndPeriodAsync(category.Id, period, userId);

            // Assert
            Assert.False(exists);
        }

        [Fact]
        public async Task ExistsForCategoryAndPeriodAsync_WithInvalidCategoryId_ReturnsFalse()
        {
            // Arrange
            int userId = 1;
            MonthlyPeriod period = new MonthlyPeriod(1, 2024);

            // Act
            bool exists = await _repository.ExistsForCategoryAndPeriodAsync(0, period, userId);

            // Assert
            Assert.False(exists);
        }

        [Fact]
        public async Task ExistsForCategoryAndPeriodAsync_WithNullPeriod_ThrowsArgumentNullException()
        {
            // Arrange
            int userId = 1;
            Category category = await TestDataFactory.SeedCategoryAsync(_categoryRepository);

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentNullException>(() => _repository.ExistsForCategoryAndPeriodAsync(category.Id, null, userId));
        }

        // ==================== TEST: UPDATE ====================

        [Fact]
        public async Task UpdateAsync_ShouldUpdateBudget()
        {
            // Arrange
            int userId = 1;
            Category category = await TestDataFactory.SeedCategoryAsync(_categoryRepository);
            Budget budget = await TestDataFactory.SeedBudgetAsync(_repository);

            // Modificar la entidad
            Money newAmount = new Money(600.00m);
            budget.UpdateAmount(newAmount);

            // Act
            await _repository.UpdateAsync(budget);

            // Assert
            Budget? updated = await _repository.GetByIdAsync(budget.Id, userId);
            Assert.NotNull(updated);
            Assert.Equal(600.00m, updated.MonthlyAmount.Value);
            Assert.NotNull(updated.UpdatedAt);
        }

        // ==================== TEST: DELETE ====================

        [Fact]
        public async Task DeleteAsync_ShouldRemoveBudget()
        {
            // Arrange
            int userId = 1;
            Category category = await TestDataFactory.SeedCategoryAsync(_categoryRepository);
            Budget budget = await TestDataFactory.SeedBudgetAsync(_repository);
            int id = budget.Id;

            // Act
            await _repository.DeleteAsync(id, userId);

            // Assert
            Budget? deleted = await _repository.GetByIdAsync(id, userId);
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