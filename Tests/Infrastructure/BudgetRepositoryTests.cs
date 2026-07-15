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

        public BudgetRepositoryTests()
        {
            // Crear un nombre de base de datos único para cada prueba
            string databaseName = Guid.NewGuid().ToString();

            DbContextOptions<ApplicationDbContext> options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: databaseName)
                .Options;

            _dbContext = new ApplicationDbContext(options);
            _repository = new BudgetRepository(_dbContext);
        }

        // ==================== TEST: ADD ====================

        [Fact]
        public async Task AddAsync_ShouldAddBudgetToDatabase()
        {
            // Arrange

            Budget budget = TestDataFactory.CreateBudget();

            // Act
            await _repository.AddAsync(budget);

            // Assert
            Budget? retrieved = await _dbContext.Budgets
                .Include(b => b.Category)
                .FirstOrDefaultAsync(b => b.Id == budget.Id);

            Assert.NotNull(retrieved);
            Assert.Equal(TestDataFactory.DEFAULT_BUDGET_AMOUNT, retrieved.MonthlyAmount.Value);
            Assert.Equal(TestDataFactory.DEFAULT_YEAR, retrieved.Period.Year);
            Assert.Equal(TestDataFactory.DEFAULT_MONTHLY_MONTH, retrieved.Period.Month);
            Assert.NotEqual(default, retrieved.CreatedAt);
        }

        // ==================== TEST: GET BY ID ====================

        [Fact]
        public async Task GetByIdAsync_WithExistingId_ReturnsBudget()
        {
            // Arrange
            int userId = 1;
            Budget budget = await TestDataFactory.SeedBudgetAsync(_repository, 1, TestDataFactory.CreateUser(), TestDataFactory.CreateCategory());

            // Act
            Budget? retrieved = await _repository.GetByIdAsync(userId, budget.Id);

            // Assert
            Assert.NotNull(retrieved);
            Assert.Equal(budget.Id, retrieved.Id);
            Assert.Equal(TestDataFactory.DEFAULT_BUDGET_AMOUNT, retrieved.MonthlyAmount.Value);
            Assert.Equal(TestDataFactory.DEFAULT_YEAR, retrieved.Period.Year);
            Assert.Equal(TestDataFactory.DEFAULT_MONTHLY_MONTH, retrieved.Period.Month);
            Assert.NotNull(retrieved.Category);
        }

        [Fact]
        public async Task GetByIdAsync_WithNonExistingId_ReturnsNull()
        {
            // Act
            int userId = 1;
            Budget? retrieved = await _repository.GetByIdAsync(userId, 999);

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
            User user = TestDataFactory.CreateUser();
            Category category1 = TestDataFactory.CreateCategory(1, user);
            Category category2 = TestDataFactory.CreateCategory(2, user);

            await TestDataFactory.SeedBudgetAsync(_repository, 1, user, category1);
            await TestDataFactory.SeedBudgetAsync(_repository, 2, user, category2);

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
            User user = TestDataFactory.CreateUser();
            Category category1 = TestDataFactory.CreateCategory(1, user);
            Category category2 = TestDataFactory.CreateCategory(2, user);

            await TestDataFactory.SeedBudgetAsync(_repository, 1, user, category1);
            await TestDataFactory.SeedBudgetAsync(_repository, 2, user, category1);
            await TestDataFactory.SeedBudgetAsync(_repository, 3, user, category2);

            // Act
            IEnumerable<Budget> result = await _repository.GetByCategoryIdAsync(userId, category1.Id);

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
            User user = TestDataFactory.CreateUser();
            Category category = TestDataFactory.CreateCategory(1, user);

            await TestDataFactory.SeedBudgetAsync(_repository, 1, user, category);
            await TestDataFactory.SeedBudgetAsync(_repository, 2, user, category);
            await TestDataFactory.SeedBudgetAsync(_repository, 3, user, category, month: 2);

            MonthlyPeriod period = TestDataFactory.CreateMonthlyPeriod();

            // Act
            IEnumerable<Budget> result = await _repository.GetByPeriodAsync(userId, period);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count());
            Assert.All(result, b => Assert.Equal(TestDataFactory.DEFAULT_MONTHLY_MONTH, b.Period.Month));
            Assert.All(result, b => Assert.Equal(TestDataFactory.DEFAULT_YEAR, b.Period.Year));
        }

        [Fact]
        public async Task GetByPeriodAsync_WithNonExistingPeriod_ReturnsEmptyList()
        {
            // Arrange
            int userId = 1;
            MonthlyPeriod period = TestDataFactory.CreateMonthlyPeriod(12, 2025);

            // Act
            IEnumerable<Budget> result = await _repository.GetByPeriodAsync(userId, period);

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result);
        }

        [Fact]
        public async Task GetByPeriodAsync_WithNullPeriod_ThrowsArgumentNullException()
        {
            // Act & Assert
            await Assert.ThrowsAsync<ArgumentNullException>(() => _repository.GetByPeriodAsync(1, null));
        }

        // ==================== TEST: GET BY CATEGORY AND PERIOD ====================

        [Fact]
        public async Task GetByCategoryAndPeriodAsync_WithExistingBudget_ReturnsBudget()
        {
            // Arrange
            int userId = 1;
            User user = TestDataFactory.CreateUser();
            Category category = TestDataFactory.CreateCategory(1, user);
            Category category2 = TestDataFactory.CreateCategory(2, user);

            await TestDataFactory.SeedBudgetAsync(_repository, 1, user, category);
            await TestDataFactory.SeedBudgetAsync(_repository, 2, user, category2);

            MonthlyPeriod period = TestDataFactory.CreateMonthlyPeriod();

            // Act
            Budget? result = await _repository.GetByCategoryAndPeriodAsync(userId, category.Id, period);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(TestDataFactory.DEFAULT_BUDGET_AMOUNT, result.MonthlyAmount.Value);
            Assert.Equal(TestDataFactory.DEFAULT_MONTHLY_MONTH, result.Period.Month);
            Assert.Equal(TestDataFactory.DEFAULT_YEAR, result.Period.Year);
            Assert.Equal(category.Id, result.CategoryId);
        }

        [Fact]
        public async Task GetByCategoryAndPeriodAsync_WithNonExistingBudget_ReturnsNull()
        {
            // Arrange
            int userId = 1;
            Category category = TestDataFactory.CreateCategory();
            MonthlyPeriod period = TestDataFactory.CreateMonthlyPeriod(12, 2025);

            // Act
            Budget? result = await _repository.GetByCategoryAndPeriodAsync(userId, category.Id, period);

            // Assert
            Assert.Null(result);
        }

        // ==================== TEST: EXISTS ====================

        [Fact]
        public async Task ExistsAsync_WithExistingId_ReturnsTrue()
        {
            // Arrange
            int userId = 1;
            Budget budget = await TestDataFactory.SeedBudgetAsync(_repository, 1, TestDataFactory.CreateUser(), TestDataFactory.CreateCategory());

            // Act
            bool exists = await _repository.ExistsAsync(userId, budget.Id);

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

        // ==================== TEST: EXISTS FOR CATEGORY AND PERIOD ====================

        [Fact]
        public async Task ExistsForCategoryAndPeriodAsync_WithExistingBudget_ReturnsTrue()
        {
            // Arrange
            int userId = 1;

            User user = TestDataFactory.CreateUser();
            Category category = TestDataFactory.CreateCategory(1, user);
            await TestDataFactory.SeedBudgetAsync(_repository, 1, user, category);

            MonthlyPeriod period = TestDataFactory.CreateMonthlyPeriod();

            // Act
            bool exists = await _repository.ExistsForCategoryAndPeriodAsync(userId, category.Id, period);

            // Assert
            Assert.True(exists);
        }

        [Fact]
        public async Task ExistsForCategoryAndPeriodAsync_WithNonExistingBudget_ReturnsFalse()
        {
            // Arrange
            int userId = 1;
            Category category = TestDataFactory.CreateCategory();
            MonthlyPeriod period = TestDataFactory.CreateMonthlyPeriod(12, 2025);

            // Act
            bool exists = await _repository.ExistsForCategoryAndPeriodAsync(userId, category.Id, period);

            // Assert
            Assert.False(exists);
        }

        [Fact]
        public async Task ExistsForCategoryAndPeriodAsync_WithInvalidCategoryId_ReturnsFalse()
        {
            // Arrange
            int userId = 1;
            MonthlyPeriod period = TestDataFactory.CreateMonthlyPeriod();

            // Act
            bool exists = await _repository.ExistsForCategoryAndPeriodAsync(userId, 0, period);

            // Assert
            Assert.False(exists);
        }

        [Fact]
        public async Task ExistsForCategoryAndPeriodAsync_WithNullPeriod_ThrowsArgumentNullException()
        {
            // Arrange
            int userId = 1;
            Category category = TestDataFactory.CreateCategory();

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentNullException>(() => _repository.ExistsForCategoryAndPeriodAsync(userId, category.Id, null));
        }

        // ==================== TEST: UPDATE ====================

        [Fact]
        public async Task UpdateAsync_ShouldUpdateBudget()
        {
            // Arrange
            int userId = 1;
            decimal updatedAmount = 600.00m;
            Budget budget = await TestDataFactory.SeedBudgetAsync(_repository, 1, TestDataFactory.CreateUser(), TestDataFactory.CreateCategory());

            // Modificar la entidad
            budget.UpdateAmount(TestDataFactory.CreateMoney(updatedAmount));

            // Act
            await _repository.UpdateAsync(budget);

            // Assert
            Budget? updated = await _repository.GetByIdAsync(userId, budget.Id);
            Assert.NotNull(updated);
            Assert.Equal(updatedAmount, updated.MonthlyAmount.Value);
            Assert.NotNull(updated.UpdatedAt);
        }

        // ==================== TEST: DELETE ====================

        [Fact]
        public async Task DeleteAsync_ShouldRemoveBudget()
        {
            // Arrange
            int userId = 1;
            Budget budget = await TestDataFactory.SeedBudgetAsync(_repository, 1, TestDataFactory.CreateUser(), TestDataFactory.CreateCategory());
            int id = budget.Id;

            // Act
            await _repository.DeleteAsync(userId, id);

            // Assert
            Budget? deleted = await _repository.GetByIdAsync(userId, id);
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