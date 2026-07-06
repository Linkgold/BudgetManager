using Domain.Entities;
using Domain.Interfaces;
using Domain.ValueObjects;
using Infrastructure.Data;
using Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;

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

        // ==================== HELPERS ====================

        private Category CreateCategory(string name)
        {
            EntityInfo info = new EntityInfo(name, null);
            Category category = new Category(info);

            return category;
        }

        private Budget CreateBudget(Category category, decimal amount, int month, int year)
        {
            Money money = new Money(amount);
            MonthlyPeriod period = new MonthlyPeriod(month, year);
            Budget budget = new Budget(category, money, period);

            return budget;
        }

        private async Task<Category> SeedCategoryAsync(string name)
        {
            Category category = CreateCategory(name);
            await _categoryRepository.AddAsync(category);

            return category;
        }

        private async Task<Budget> SeedBudgetAsync(Category category, decimal amount, int month, int year)
        {
            Budget budget = CreateBudget(category, amount, month, year);
            await _repository.AddAsync(budget);

            return budget;
        }

        // ==================== TEST: ADD ====================

        [Fact]
        public async Task AddAsync_ShouldAddBudgetToDatabase()
        {
            // Arrange
            Category category = await SeedCategoryAsync("Alimentación");
            Budget budget = CreateBudget(category, 500.00m, 1, 2024);

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
            Category category = await SeedCategoryAsync("Alimentación");
            Budget budget = await SeedBudgetAsync(category, 500.00m, 1, 2024);

            // Act
            Budget retrieved = await _repository.GetByIdAsync(budget.Id);

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
            Budget retrieved = await _repository.GetByIdAsync(999);

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
        public async Task GetAllAsync_ReturnsAllBudgets()
        {
            // Arrange
            Category category = await SeedCategoryAsync("Alimentación");
            await SeedBudgetAsync(category, 500.00m, 1, 2024);
            await SeedBudgetAsync(category, 300.00m, 1, 2024);

            // Act
            IEnumerable<Budget> result = await _repository.GetAllAsync();

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count());
        }

        // ==================== TEST: GET BY CATEGORY ====================

        [Fact]
        public async Task GetByCategoryIdAsync_WithExistingCategory_ReturnsBudgets()
        {
            // Arrange
            Category category1 = await SeedCategoryAsync("Alimentación");
            Category category2 = await SeedCategoryAsync("Transporte");

            await SeedBudgetAsync(category1, 500.00m, 1, 2024);
            await SeedBudgetAsync(category1, 300.00m, 1, 2024);
            await SeedBudgetAsync(category2, 200.00m, 1, 2024);

            // Act
            IEnumerable<Budget> result = await _repository.GetByCategoryIdAsync(category1.Id);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count());
            Assert.All(result, b => Assert.Equal(category1.Id, b.CategoryId));
        }

        [Fact]
        public async Task GetByCategoryIdAsync_WithNonExistingCategory_ReturnsEmptyList()
        {
            // Act
            IEnumerable<Budget> result = await _repository.GetByCategoryIdAsync(999);

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result);
        }

        // ==================== TEST: GET BY PERIOD ====================

        [Fact]
        public async Task GetByPeriodAsync_WithExistingPeriod_ReturnsBudgets()
        {
            // Arrange
            Category category = await SeedCategoryAsync("Alimentación");
            await SeedBudgetAsync(category, 500.00m, 1, 2024);
            await SeedBudgetAsync(category, 300.00m, 1, 2024);
            await SeedBudgetAsync(category, 200.00m, 2, 2024);

            MonthlyPeriod period = new MonthlyPeriod(1, 2024);

            // Act
            IEnumerable<Budget> result = await _repository.GetByPeriodAsync(period);

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
            MonthlyPeriod period = new MonthlyPeriod(12, 2025);

            // Act
            IEnumerable<Budget> result = await _repository.GetByPeriodAsync(period);

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result);
        }

        [Fact]
        public async Task GetByPeriodAsync_WithNullPeriod_ThrowsArgumentNullException()
        {
            // Act & Assert
            await Assert.ThrowsAsync<ArgumentNullException>(() => _repository.GetByPeriodAsync(null));
        }

        // ==================== TEST: GET BY CATEGORY AND PERIOD ====================

        [Fact]
        public async Task GetByCategoryAndPeriodAsync_WithExistingBudget_ReturnsBudget()
        {
            // Arrange
            Category category = await SeedCategoryAsync("Alimentación");
            await SeedBudgetAsync(category, 500.00m, 1, 2024);
            await SeedBudgetAsync(category, 300.00m, 2, 2024);

            MonthlyPeriod period = new MonthlyPeriod(1, 2024);

            // Act
            Budget? result = await _repository.GetByCategoryAndPeriodAsync(category.Id, period);

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
            Category category = await SeedCategoryAsync("Alimentación");
            MonthlyPeriod period = new MonthlyPeriod(12, 2025);

            // Act
            Budget? result = await _repository.GetByCategoryAndPeriodAsync(category.Id, period);

            // Assert
            Assert.Null(result);
        }

        // ==================== TEST: EXISTS ====================

        [Fact]
        public async Task ExistsAsync_WithExistingId_ReturnsTrue()
        {
            // Arrange
            Category category = await SeedCategoryAsync("Alimentación");
            Budget budget = await SeedBudgetAsync(category, 500.00m, 1, 2024);

            // Act
            bool exists = await _repository.ExistsAsync(budget.Id);

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

        // ==================== TEST: EXISTS FOR CATEGORY AND PERIOD ====================

        [Fact]
        public async Task ExistsForCategoryAndPeriodAsync_WithExistingBudget_ReturnsTrue()
        {
            // Arrange
            Category category = await SeedCategoryAsync("Alimentación");
            await SeedBudgetAsync(category, 500.00m, 1, 2024);

            MonthlyPeriod period = new MonthlyPeriod(1, 2024);

            // Act
            bool exists = await _repository.ExistsForCategoryAndPeriodAsync(category.Id, period);

            // Assert
            Assert.True(exists);
        }

        [Fact]
        public async Task ExistsForCategoryAndPeriodAsync_WithNonExistingBudget_ReturnsFalse()
        {
            // Arrange
            Category category = await SeedCategoryAsync("Alimentación");
            MonthlyPeriod period = new MonthlyPeriod(12, 2025);

            // Act
            bool exists = await _repository.ExistsForCategoryAndPeriodAsync(category.Id, period);

            // Assert
            Assert.False(exists);
        }

        [Fact]
        public async Task ExistsForCategoryAndPeriodAsync_WithInvalidCategoryId_ReturnsFalse()
        {
            // Arrange
            MonthlyPeriod period = new MonthlyPeriod(1, 2024);

            // Act
            bool exists = await _repository.ExistsForCategoryAndPeriodAsync(0, period);

            // Assert
            Assert.False(exists);
        }

        [Fact]
        public async Task ExistsForCategoryAndPeriodAsync_WithNullPeriod_ThrowsArgumentNullException()
        {
            // Arrange
            Category category = await SeedCategoryAsync("Alimentación");

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentNullException>(() => _repository.ExistsForCategoryAndPeriodAsync(category.Id, null));
        }

        // ==================== TEST: UPDATE ====================

        [Fact]
        public async Task UpdateAsync_ShouldUpdateBudget()
        {
            // Arrange
            Category category = await SeedCategoryAsync("Alimentación");
            Budget budget = await SeedBudgetAsync(category, 500.00m, 1, 2024);

            // Modificar la entidad
            Money newAmount = new Money(600.00m);
            budget.UpdateAmount(newAmount);

            // Act
            await _repository.UpdateAsync(budget);

            // Assert
            Budget updated = await _repository.GetByIdAsync(budget.Id);
            Assert.NotNull(updated);
            Assert.Equal(600.00m, updated.MonthlyAmount.Value);
            Assert.NotNull(updated.UpdatedAt);
        }

        // ==================== TEST: DELETE ====================

        [Fact]
        public async Task DeleteAsync_ShouldRemoveBudget()
        {
            // Arrange
            Category category = await SeedCategoryAsync("Alimentación");
            Budget budget = await SeedBudgetAsync(category, 500.00m, 1, 2024);
            int id = budget.Id;

            // Act
            await _repository.DeleteAsync(id);

            // Assert
            Budget deleted = await _repository.GetByIdAsync(id);
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