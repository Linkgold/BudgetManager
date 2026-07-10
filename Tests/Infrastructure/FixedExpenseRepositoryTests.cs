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
    /// Pruebas unitarias para FixedExpenseRepository usando InMemory Database
    /// </summary>
    public class FixedExpenseRepositoryTests : IDisposable
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly IFixedExpenseRepository _repository;
        private readonly ICategoryRepository _categoryRepository;

        public FixedExpenseRepositoryTests()
        {
            // Crear un nombre de base de datos único para cada prueba
            string databaseName = Guid.NewGuid().ToString();

            DbContextOptions<ApplicationDbContext> options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: databaseName)
                .Options;

            _dbContext = new ApplicationDbContext(options);
            _repository = new FixedExpenseRepository(_dbContext);
            _categoryRepository = new CategoryRepository(_dbContext);
        }

        // ==================== TEST: ADD ====================

        [Fact]
        public async Task AddAsync_ShouldAddFixedExpenseToDatabase()
        {
            // Arrange
            Category category = await TestDataFactory.SeedCategoryAsync(_categoryRepository);
            FixedExpense fixedExpense = TestDataFactory.CreateFixedExpense();

            // Act
            await _repository.AddAsync(fixedExpense);

            // Assert
            FixedExpense? retrieved = await _dbContext.FixedExpenses
                .Include(f => f.Category)
                .FirstOrDefaultAsync(f => f.Id == fixedExpense.Id);

            Assert.NotNull(retrieved);
            Assert.Equal("Netflix", retrieved.Info.Name);
            Assert.Equal(15.99m, retrieved.Amount.Value);
            Assert.Equal(2024, retrieved.ChargePeriod.Year);
            Assert.Equal(1, retrieved.ChargePeriod.Month);
            Assert.Equal(category.Id, retrieved.CategoryId);
            Assert.True(retrieved.IsActive);
            Assert.NotEqual(default, retrieved.CreatedAt);
        }

        // ==================== TEST: GET BY ID ====================

        [Fact]
        public async Task GetByIdAsync_WithExistingId_ReturnsFixedExpense()
        {
            // Arrange
            int userId = 1;
            Category category = await TestDataFactory.SeedCategoryAsync(_categoryRepository);
            FixedExpense fixedExpense = await TestDataFactory.SeedFixedExpenseAsync(_repository);

            // Act
            FixedExpense? retrieved = await _repository.GetByIdAsync(fixedExpense.Id, userId);

            // Assert
            Assert.NotNull(retrieved);
            Assert.Equal(fixedExpense.Id, retrieved.Id);
            Assert.Equal("Netflix", retrieved.Info.Name);
            Assert.Equal(15.99m, retrieved.Amount.Value);
            Assert.Equal(2024, retrieved.ChargePeriod.Year);
            Assert.Equal(1, retrieved.ChargePeriod.Month);
            Assert.Equal(category.Id, retrieved.CategoryId);
            Assert.NotNull(retrieved.Category);
        }

        [Fact]
        public async Task GetByIdAsync_WithNonExistingId_ReturnsNull()
        {
            // Act
            int userId = 1;
            FixedExpense? retrieved = await _repository.GetByIdAsync(999, userId);

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
        public async Task GetAllAsync_ReturnsAllFixedExpenses()
        {
            // Arrange
            int userId = 1;
            Category category = await TestDataFactory.SeedCategoryAsync(_categoryRepository);
            await TestDataFactory.SeedFixedExpenseAsync(_repository);
            await TestDataFactory.SeedFixedExpenseAsync(_repository, TestDataFactory.CreateFixedExpense(2, TestDataFactory.CreateUser(), category, "Spotify", "", 9.99m, 2024, 1));

            // Act
            IEnumerable<FixedExpense> result = await _repository.GetAllAsync(userId);

            // Assert
            Assert.Equal(2, result.Count());
            Assert.Contains(result, f => f.Info.Name == "Netflix");
            Assert.Contains(result, f => f.Info.Name == "Spotify");
        }

        // ==================== TEST: GET BY CATEGORY ====================

        [Fact]
        public async Task GetByCategoryAsync_WithExistingCategory_ReturnsFixedExpenses()
        {
            // Arrange
            int userId = 1;
            Category category1 = await TestDataFactory.SeedCategoryAsync(_categoryRepository);
            Category category2 = await TestDataFactory.SeedCategoryAsync(_categoryRepository, TestDataFactory.CreateCategory(2, TestDataFactory.CreateUser(), "Seguros"));

            await TestDataFactory.SeedFixedExpenseAsync(_repository);
            await TestDataFactory.SeedFixedExpenseAsync(_repository, TestDataFactory.CreateFixedExpense(2, TestDataFactory.CreateUser(), category1, "Spotify", "", 9.99m, 2024, 1));
            await TestDataFactory.SeedFixedExpenseAsync(_repository, TestDataFactory.CreateFixedExpense(3, TestDataFactory.CreateUser(), category2, "Seguro Coche", "", 50.00m, 2024, 1));

            // Act
            IEnumerable<FixedExpense> result = await _repository.GetByCategoryAsync(category1.Id, userId);

            // Assert
            Assert.Equal(2, result.Count());
            Assert.All(result, f => Assert.Equal(category1.Id, f.CategoryId));
            Assert.Contains(result, f => f.Info.Name == "Netflix");
            Assert.Contains(result, f => f.Info.Name == "Spotify");
        }

        [Fact]
        public async Task GetByCategoryAsync_WithNonExistingCategory_ReturnsEmptyList()
        {
            // Act
            int userId = 1;
            IEnumerable<FixedExpense> result = await _repository.GetByCategoryAsync(999, userId);

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result);
        }

        // ==================== TEST: GET ACTIVE ====================

        [Fact]
        public async Task GetActiveAsync_ReturnsOnlyActiveFixedExpenses()
        {
            // Arrange
            int userId = 1;
            Category category = await TestDataFactory.SeedCategoryAsync(_categoryRepository);

            FixedExpense active = await TestDataFactory.SeedFixedExpenseAsync(_repository);
            FixedExpense inactive = await TestDataFactory.SeedFixedExpenseAsync(_repository, TestDataFactory.CreateFixedExpense(1, TestDataFactory.CreateUser(), category, "Disney+", "", 11.99m, 2024, 1));
            inactive.Deactivate();
            await _repository.UpdateAsync(inactive);

            // Act
            IEnumerable<FixedExpense> result = await _repository.GetActiveAsync(userId);

            // Assert
            Assert.Single(result);
            Assert.Equal("Netflix", result.First().Info.Name);
            Assert.True(result.First().IsActive);
        }

        // ==================== TEST: GET ACTIVE BY CATEGORY ====================

        [Fact]
        public async Task GetActiveByCategoryAsync_ReturnsActiveFixedExpensesForCategory()
        {
            // Arrange
            int userId = 1;
            Category category1 = await TestDataFactory.SeedCategoryAsync(_categoryRepository);
            Category category2 = await TestDataFactory.SeedCategoryAsync(_categoryRepository, TestDataFactory.CreateCategory(2, TestDataFactory.CreateUser(), "Seguros"));

            FixedExpense active = await TestDataFactory.SeedFixedExpenseAsync(_repository);
            FixedExpense inactive = await TestDataFactory.SeedFixedExpenseAsync(_repository, TestDataFactory.CreateFixedExpense(1, TestDataFactory.CreateUser(), category1, "Disney+", "", 11.99m, 2024, 1));
            inactive.Deactivate();
            await _repository.UpdateAsync(inactive);

            await TestDataFactory.SeedFixedExpenseAsync(_repository, TestDataFactory.CreateFixedExpense(1, TestDataFactory.CreateUser(), category2, "Seguro Coche", "", 50.00m, 2024, 1));

            // Act
            IEnumerable<FixedExpense> result = await _repository.GetActiveByCategoryAsync(category1.Id, userId);

            // Assert
            Assert.Single(result);
            Assert.Equal("Netflix", result.First().Info.Name);
            Assert.Equal(category1.Id, result.First().CategoryId);
            Assert.True(result.First().IsActive);
        }

        // ==================== TEST: GET ACTIVE FOR PERIOD ====================

        [Fact]
        public async Task GetActiveForPeriodAsync_ReturnsActiveFixedExpensesForPeriod()
        {
            // Arrange
            int userId = 1;
            Category category = await TestDataFactory.SeedCategoryAsync(_categoryRepository);

            await TestDataFactory.SeedFixedExpenseAsync(_repository);
            await TestDataFactory.SeedFixedExpenseAsync(_repository, TestDataFactory.CreateFixedExpense(2, TestDataFactory.CreateUser(), category, "Spotify", "", 9.99m, 2024, 1));
            await TestDataFactory.SeedFixedExpenseAsync(_repository, TestDataFactory.CreateFixedExpense(3, TestDataFactory.CreateUser(), category, "Disney+", "", 11.99m, 2024, 1));

            MonthlyPeriod period = new MonthlyPeriod(2, 2024);

            // Act
            IEnumerable<FixedExpense> result = await _repository.GetActiveForPeriodAsync(period, userId);

            // Assert
            // Solo Netflix debe estar activo en febrero 2024 (Spotify empieza en marzo, Disney+ en 2025)
            Assert.Single(result);
            Assert.Equal("Netflix", result.First().Info.Name);
        }

        [Fact]
        public async Task GetActiveForPeriodAsync_WithNullPeriod_ThrowsArgumentNullException()
        {
            // Act & Assert
            await Assert.ThrowsAsync<ArgumentNullException>(() => _repository.GetActiveForPeriodAsync(null, 1));
        }

        // ==================== TEST: GET ACTIVE FOR PERIOD BY CATEGORY ====================

        [Fact]
        public async Task GetActiveForPeriodByCategoryAsync_ReturnsActiveFixedExpensesForCategoryAndPeriod()
        {
            // Arrange
            int userId = 1;
            Category category1 = await TestDataFactory.SeedCategoryAsync(_categoryRepository);
            Category category2 = await TestDataFactory.SeedCategoryAsync(_categoryRepository, TestDataFactory.CreateCategory(2, TestDataFactory.CreateUser(), "Seguros"));

            await TestDataFactory.SeedFixedExpenseAsync(_repository);
            await TestDataFactory.SeedFixedExpenseAsync(_repository, TestDataFactory.CreateFixedExpense(2, TestDataFactory.CreateUser(), category1, "Spotify", "", 9.99m, 2024, 1));
            await TestDataFactory.SeedFixedExpenseAsync(_repository, TestDataFactory.CreateFixedExpense(3, TestDataFactory.CreateUser(), category2, "Seguro Coche", "", 50.00m, 2024, 1));

            MonthlyPeriod period = new MonthlyPeriod(2, 2024);

            // Act
            IEnumerable<FixedExpense> result = await _repository.GetActiveForPeriodByCategoryAsync(category1.Id, period, userId);

            // Assert
            Assert.Single(result);
            Assert.Equal("Netflix", result.First().Info.Name);
            Assert.Equal(category1.Id, result.First().CategoryId);
        }

        // ==================== TEST: GET TOTAL BY CATEGORY AND PERIOD ====================

        [Fact]
        public async Task GetTotalByCategoryAndPeriodAsync_ReturnsTotalForCategoryAndPeriod()
        {
            // Arrange
            int userId = 1;
            Category category = await TestDataFactory.SeedCategoryAsync(_categoryRepository);

            await TestDataFactory.SeedFixedExpenseAsync(_repository);
            await TestDataFactory.SeedFixedExpenseAsync(_repository, TestDataFactory.CreateFixedExpense(2, TestDataFactory.CreateUser(), category, "Spotify", "", 9.99m, 2024, 1));
            await TestDataFactory.SeedFixedExpenseAsync(_repository, TestDataFactory.CreateFixedExpense(3, TestDataFactory.CreateUser(), category, "Disney+", "", 11.99m, 2025, 1));

            MonthlyPeriod period = new MonthlyPeriod(2, 2024);

            // Act
            decimal total = await _repository.GetTotalByCategoryAndPeriodAsync(category.Id, period, userId);

            // Assert
            Assert.Equal(25.98m, total); // Netflix + Spotify = 15.99 + 9.99
        }

        // ==================== TEST: GET TOTAL ACTIVE ====================

        [Fact]
        public async Task GetTotalActiveAsync_ReturnsTotalOfAllActiveFixedExpenses()
        {
            // Arrange
            int userId = 1;
            Category category = await TestDataFactory.SeedCategoryAsync(_categoryRepository);

            await TestDataFactory.SeedFixedExpenseAsync(_repository);

            FixedExpense inactive = await TestDataFactory.SeedFixedExpenseAsync(_repository, TestDataFactory.CreateFixedExpense(2, TestDataFactory.CreateUser(), category, "Disney+", "", 11.99m, 2024, 1));
            inactive.Deactivate();
            await _repository.UpdateAsync(inactive);

            // Act
            decimal total = await _repository.GetTotalActiveAsync(userId);

            // Assert
            Assert.Equal(15.99m, total);
        }

        // ==================== TEST: GET TOTAL ACTIVE BY CATEGORY ====================

        [Fact]
        public async Task GetTotalActiveByCategoryAsync_ReturnsTotalActiveForCategory()
        {
            // Arrange
            int userId = 1;
            Category category1 = await TestDataFactory.SeedCategoryAsync(_categoryRepository);
            Category category2 = await TestDataFactory.SeedCategoryAsync(_categoryRepository, TestDataFactory.CreateCategory(2, TestDataFactory.CreateUser(), "Seguros"));

            await TestDataFactory.SeedFixedExpenseAsync(_repository);
            await TestDataFactory.SeedFixedExpenseAsync(_repository, TestDataFactory.CreateFixedExpense(2, TestDataFactory.CreateUser(), category1, "Spotify", "", 9.99m, 2024, 1));

            FixedExpense inactive = await TestDataFactory.SeedFixedExpenseAsync(_repository, TestDataFactory.CreateFixedExpense(3, TestDataFactory.CreateUser(), category1, "Disney+", "", 11.99m, 2025, 1));
            inactive.Deactivate();
            await _repository.UpdateAsync(inactive);

            await TestDataFactory.SeedFixedExpenseAsync(_repository, TestDataFactory.CreateFixedExpense(4, TestDataFactory.CreateUser(), category2, "Seguro Coche", "", 50.00m, 2024, 1));

            // Act
            decimal total = await _repository.GetTotalActiveByCategoryAsync(category1.Id, userId);

            // Assert
            Assert.Equal(25.98m, total); // Netflix + Spotify = 15.99 + 9.99
        }

        // ==================== TEST: EXISTS ====================

        [Fact]
        public async Task ExistsAsync_WithExistingId_ReturnsTrue()
        {
            // Arrange
            int userId = 1;
            Category category = await TestDataFactory.SeedCategoryAsync(_categoryRepository);
            FixedExpense fixedExpense = await TestDataFactory.SeedFixedExpenseAsync(_repository);

            // Act
            bool exists = await _repository.ExistsAsync(fixedExpense.Id, userId);

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

        // ==================== TEST: IS ACTIVE ====================

        [Fact]
        public async Task IsActiveAsync_WithActiveFixedExpense_ReturnsTrue()
        {
            // Arrange
            int userId = 1;
            Category category = await TestDataFactory.SeedCategoryAsync(_categoryRepository);
            FixedExpense fixedExpense = await TestDataFactory.SeedFixedExpenseAsync(_repository);

            // Act
            bool isActive = await _repository.IsActiveAsync(fixedExpense.Id, userId);

            // Assert
            Assert.True(isActive);
        }

        [Fact]
        public async Task IsActiveAsync_WithInactiveFixedExpense_ReturnsFalse()
        {
            // Arrange
            int userId = 1;
            Category category = await TestDataFactory.SeedCategoryAsync(_categoryRepository);
            FixedExpense fixedExpense = await TestDataFactory.SeedFixedExpenseAsync(_repository);
            fixedExpense.Deactivate();
            await _repository.UpdateAsync(fixedExpense);

            // Act
            bool isActive = await _repository.IsActiveAsync(fixedExpense.Id, userId);

            // Assert
            Assert.False(isActive);
        }

        [Fact]
        public async Task IsActiveAsync_WithNonExistingId_ReturnsFalse()
        {
            // Arrange
            int userId = 1;
            int nonExistingId = 999;

            // Act
            bool result = await _repository.IsActiveAsync(nonExistingId, userId);

            // Assert
            Assert.False(result);
        }

        // ==================== TEST: UPDATE ====================

        [Fact]
        public async Task UpdateAsync_ShouldUpdateFixedExpense()
        {
            // Arrange
            int userId = 1;
            Category category = await TestDataFactory.SeedCategoryAsync(_categoryRepository);
            FixedExpense fixedExpense = await TestDataFactory.SeedFixedExpenseAsync(_repository);

            // Modificar la entidad
            EntityInfo newInfo = new EntityInfo("Netflix Premium", "Suscripción Premium");
            Money newAmount = new Money(17.99m);
            MonthlyPeriod newPeriod = new MonthlyPeriod(2, 2024);
            fixedExpense.Update(newInfo, newAmount, newPeriod);

            // Act
            await _repository.UpdateAsync(fixedExpense);

            // Assert
            FixedExpense? updated = await _repository.GetByIdAsync(fixedExpense.Id, userId);
            Assert.NotNull(updated);
            Assert.Equal("Netflix Premium", updated.Info.Name);
            Assert.Equal("Suscripción Premium", updated.Info.Description);
            Assert.Equal(17.99m, updated.Amount.Value);
            Assert.Equal(2024, updated.ChargePeriod.Year);
            Assert.Equal(2, updated.ChargePeriod.Month);
            Assert.NotNull(updated.UpdatedAt);
        }

        // ==================== TEST: DELETE ====================

        [Fact]
        public async Task DeleteAsync_ShouldRemoveFixedExpense()
        {
            // Arrange
            int userId = 1;
            Category category = await TestDataFactory.SeedCategoryAsync(_categoryRepository);
            FixedExpense fixedExpense = await TestDataFactory.SeedFixedExpenseAsync(_repository);
            int id = fixedExpense.Id;

            // Act
            await _repository.DeleteAsync(id, userId);

            // Assert
            FixedExpense? deleted = await _repository.GetByIdAsync(id, userId);
            Assert.Null(deleted);
        }

        [Fact]
        public async Task DeleteAsync_WithNonExistingId_ThrowsKeyNotFoundException()
        {
            // Act & Assert
            await Assert.ThrowsAsync<KeyNotFoundException>(() => _repository.DeleteAsync(999, 1));
        }

        // ==================== TEST: ACTIVATE ====================

        [Fact]
        public async Task ActivateAsync_ShouldActivateFixedExpense()
        {
            // Arrange
            int userId = 1;
            Category category = await TestDataFactory.SeedCategoryAsync(_categoryRepository);
            FixedExpense fixedExpense = await TestDataFactory.SeedFixedExpenseAsync(_repository);
            fixedExpense.Deactivate();
            await _repository.UpdateAsync(fixedExpense);

            // Act
            await _repository.ActivateAsync(fixedExpense.Id, userId);

            // Assert
            bool isActive = await _repository.IsActiveAsync(fixedExpense.Id, userId);
            Assert.True(isActive);
        }

        [Fact]
        public async Task ActivateAsync_WithNonExistingId_ThrowsKeyNotFoundException()
        {
            // Act & Assert
            await Assert.ThrowsAsync<KeyNotFoundException>(() => _repository.ActivateAsync(999, 1));
        }

        // ==================== TEST: DEACTIVATE ====================

        [Fact]
        public async Task DeactivateAsync_ShouldDeactivateFixedExpense()
        {
            // Arrange
            int userId = 1;
            Category category = await TestDataFactory.SeedCategoryAsync(_categoryRepository);
            FixedExpense fixedExpense = await TestDataFactory.SeedFixedExpenseAsync(_repository);

            // Act
            await _repository.DeactivateAsync(fixedExpense.Id, userId);

            // Assert
            bool isActive = await _repository.IsActiveAsync(fixedExpense.Id, userId);
            Assert.False(isActive);
        }

        [Fact]
        public async Task DeactivateAsync_WithNonExistingId_ThrowsKeyNotFoundException()
        {
            // Act & Assert
            await Assert.ThrowsAsync<KeyNotFoundException>(() => _repository.DeactivateAsync(999, 1));
        }

        // ==================== DISPOSE ====================

        public void Dispose()
        {
            _dbContext?.Dispose();
        }
    }
}