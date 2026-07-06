using Domain.Entities;
using Domain.Interfaces;
using Domain.ValueObjects;
using Infrastructure.Data;
using Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;

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

        // ==================== HELPERS ====================

        private Category CreateCategory(string name)
        {
            EntityInfo info = new EntityInfo(name, null);
            Category category = new Category(info);

            return category;
        }

        private FixedExpense CreateFixedExpense(Category category, string name, decimal amount, int year, int month)
        {
            EntityInfo info = new EntityInfo(name, null);
            Money money = new Money(amount);
            MonthlyPeriod period = new MonthlyPeriod(month, year);
            FixedExpense fixedExpense = new FixedExpense(category, info, money, period);

            return fixedExpense;
        }

        private async Task<Category> SeedCategoryAsync(string name)
        {
            Category category = CreateCategory(name);
            await _categoryRepository.AddAsync(category);

            return category;
        }

        private async Task<FixedExpense> SeedFixedExpenseAsync(Category category, string name, decimal amount, int year, int month)
        {
            FixedExpense fixedExpense = CreateFixedExpense(category, name, amount, year, month);
            await _repository.AddAsync(fixedExpense);

            return fixedExpense;
        }

        // ==================== TEST: ADD ====================

        [Fact]
        public async Task AddAsync_ShouldAddFixedExpenseToDatabase()
        {
            // Arrange
            Category category = await SeedCategoryAsync("Suscripciones");
            FixedExpense fixedExpense = CreateFixedExpense(category, "Netflix", 15.99m, 2024, 1);

            // Act
            await _repository.AddAsync(fixedExpense);

            // Assert
            FixedExpense retrieved = await _dbContext.FixedExpenses
                .Include(f => f.Category)
                .FirstOrDefaultAsync(f => f.Id == fixedExpense.Id);

            Assert.NotNull(retrieved);
            Assert.Equal("Netflix", retrieved.Info.Name);
            Assert.Equal(15.99m, retrieved.Amount.Value);
            Assert.Equal(2024, retrieved.ChargePeriod.Year);
            Assert.Equal(1, retrieved.ChargePeriod.Month);
            Assert.Equal(category.Id, retrieved.CategoryId);
            Assert.True(retrieved.IsActive);
            Assert.NotEqual(default(DateTime), retrieved.CreatedAt);
        }

        // ==================== TEST: GET BY ID ====================

        [Fact]
        public async Task GetByIdAsync_WithExistingId_ReturnsFixedExpense()
        {
            // Arrange
            Category category = await SeedCategoryAsync("Suscripciones");
            FixedExpense fixedExpense = await SeedFixedExpenseAsync(category, "Netflix", 15.99m, 2024, 1);

            // Act
            FixedExpense retrieved = await _repository.GetByIdAsync(fixedExpense.Id);

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
            FixedExpense retrieved = await _repository.GetByIdAsync(999);

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
        public async Task GetAllAsync_ReturnsAllFixedExpenses()
        {
            // Arrange
            Category category = await SeedCategoryAsync("Suscripciones");
            await SeedFixedExpenseAsync(category, "Netflix", 15.99m, 2024, 1);
            await SeedFixedExpenseAsync(category, "Spotify", 9.99m, 2024, 1);

            // Act
            List<FixedExpense> result = await _repository.GetAllAsync();

            // Assert
            Assert.Equal(2, result.Count);
            Assert.Contains(result, f => f.Info.Name == "Netflix");
            Assert.Contains(result, f => f.Info.Name == "Spotify");
        }

        // ==================== TEST: GET BY CATEGORY ====================

        [Fact]
        public async Task GetByCategoryAsync_WithExistingCategory_ReturnsFixedExpenses()
        {
            // Arrange
            Category category1 = await SeedCategoryAsync("Suscripciones");
            Category category2 = await SeedCategoryAsync("Seguros");

            await SeedFixedExpenseAsync(category1, "Netflix", 15.99m, 2024, 1);
            await SeedFixedExpenseAsync(category1, "Spotify", 9.99m, 2024, 1);
            await SeedFixedExpenseAsync(category2, "Seguro Coche", 50.00m, 2024, 1);

            // Act
            List<FixedExpense> result = await _repository.GetByCategoryAsync(category1.Id);

            // Assert
            Assert.Equal(2, result.Count);
            Assert.All(result, f => Assert.Equal(category1.Id, f.CategoryId));
            Assert.Contains(result, f => f.Info.Name == "Netflix");
            Assert.Contains(result, f => f.Info.Name == "Spotify");
        }

        [Fact]
        public async Task GetByCategoryAsync_WithNonExistingCategory_ReturnsEmptyList()
        {
            // Act
            List<FixedExpense> result = await _repository.GetByCategoryAsync(999);

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result);
        }

        // ==================== TEST: GET ACTIVE ====================

        [Fact]
        public async Task GetActiveAsync_ReturnsOnlyActiveFixedExpenses()
        {
            // Arrange
            Category category = await SeedCategoryAsync("Suscripciones");

            FixedExpense active = await SeedFixedExpenseAsync(category, "Netflix", 15.99m, 2024, 1);
            FixedExpense inactive = await SeedFixedExpenseAsync(category, "Disney+", 11.99m, 2024, 1);
            inactive.Deactivate();
            await _repository.UpdateAsync(inactive);

            // Act
            List<FixedExpense> result = await _repository.GetActiveAsync();

            // Assert
            Assert.Single(result);
            Assert.Equal("Netflix", result[0].Info.Name);
            Assert.True(result[0].IsActive);
        }

        // ==================== TEST: GET ACTIVE BY CATEGORY ====================

        [Fact]
        public async Task GetActiveByCategoryAsync_ReturnsActiveFixedExpensesForCategory()
        {
            // Arrange
            Category category1 = await SeedCategoryAsync("Suscripciones");
            Category category2 = await SeedCategoryAsync("Seguros");

            FixedExpense active = await SeedFixedExpenseAsync(category1, "Netflix", 15.99m, 2024, 1);
            FixedExpense inactive = await SeedFixedExpenseAsync(category1, "Disney+", 11.99m, 2024, 1);
            inactive.Deactivate();
            await _repository.UpdateAsync(inactive);

            await SeedFixedExpenseAsync(category2, "Seguro Coche", 50.00m, 2024, 1);

            // Act
            List<FixedExpense> result = await _repository.GetActiveByCategoryAsync(category1.Id);

            // Assert
            Assert.Single(result);
            Assert.Equal("Netflix", result[0].Info.Name);
            Assert.Equal(category1.Id, result[0].CategoryId);
            Assert.True(result[0].IsActive);
        }

        // ==================== TEST: GET ACTIVE FOR PERIOD ====================

        [Fact]
        public async Task GetActiveForPeriodAsync_ReturnsActiveFixedExpensesForPeriod()
        {
            // Arrange
            Category category = await SeedCategoryAsync("Suscripciones");

            await SeedFixedExpenseAsync(category, "Netflix", 15.99m, 2024, 1);
            await SeedFixedExpenseAsync(category, "Spotify", 9.99m, 2024, 3);
            await SeedFixedExpenseAsync(category, "Disney+", 11.99m, 2025, 1);

            MonthlyPeriod period = new MonthlyPeriod(2, 2024);

            // Act
            List<FixedExpense> result = await _repository.GetActiveForPeriodAsync(period);

            // Assert
            // Solo Netflix debe estar activo en febrero 2024 (Spotify empieza en marzo, Disney+ en 2025)
            Assert.Single(result);
            Assert.Equal("Netflix", result[0].Info.Name);
        }

        [Fact]
        public async Task GetActiveForPeriodAsync_WithNullPeriod_ThrowsArgumentNullException()
        {
            // Act & Assert
            await Assert.ThrowsAsync<ArgumentNullException>(() => _repository.GetActiveForPeriodAsync(null));
        }

        // ==================== TEST: GET ACTIVE FOR PERIOD BY CATEGORY ====================

        [Fact]
        public async Task GetActiveForPeriodByCategoryAsync_ReturnsActiveFixedExpensesForCategoryAndPeriod()
        {
            // Arrange
            Category category1 = await SeedCategoryAsync("Suscripciones");
            Category category2 = await SeedCategoryAsync("Seguros");

            await SeedFixedExpenseAsync(category1, "Netflix", 15.99m, 2024, 1);
            await SeedFixedExpenseAsync(category1, "Spotify", 9.99m, 2024, 3);
            await SeedFixedExpenseAsync(category2, "Seguro Coche", 50.00m, 2024, 1);

            MonthlyPeriod period = new MonthlyPeriod(2, 2024);

            // Act
            List<FixedExpense> result = await _repository.GetActiveForPeriodByCategoryAsync(category1.Id, period);

            // Assert
            Assert.Single(result);
            Assert.Equal("Netflix", result[0].Info.Name);
            Assert.Equal(category1.Id, result[0].CategoryId);
        }

        // ==================== TEST: GET TOTAL BY CATEGORY AND PERIOD ====================

        [Fact]
        public async Task GetTotalByCategoryAndPeriodAsync_ReturnsTotalForCategoryAndPeriod()
        {
            // Arrange
            Category category = await SeedCategoryAsync("Suscripciones");

            await SeedFixedExpenseAsync(category, "Netflix", 15.99m, 2024, 1);
            await SeedFixedExpenseAsync(category, "Spotify", 9.99m, 2024, 1);
            await SeedFixedExpenseAsync(category, "Disney+", 11.99m, 2025, 1);

            MonthlyPeriod period = new MonthlyPeriod(2, 2024);

            // Act
            decimal total = await _repository.GetTotalByCategoryAndPeriodAsync(category.Id, period);

            // Assert
            Assert.Equal(25.98m, total); // Netflix + Spotify = 15.99 + 9.99
        }

        // ==================== TEST: GET TOTAL ACTIVE ====================

        [Fact]
        public async Task GetTotalActiveAsync_ReturnsTotalOfAllActiveFixedExpenses()
        {
            // Arrange
            Category category = await SeedCategoryAsync("Suscripciones");

            await SeedFixedExpenseAsync(category, "Netflix", 15.99m, 2024, 1);

            FixedExpense inactive = await SeedFixedExpenseAsync(category, "Disney+", 11.99m, 2024, 1);
            inactive.Deactivate();
            await _repository.UpdateAsync(inactive);

            // Act
            decimal total = await _repository.GetTotalActiveAsync();

            // Assert
            Assert.Equal(15.99m, total);
        }

        // ==================== TEST: GET TOTAL ACTIVE BY CATEGORY ====================

        [Fact]
        public async Task GetTotalActiveByCategoryAsync_ReturnsTotalActiveForCategory()
        {
            // Arrange
            Category category1 = await SeedCategoryAsync("Suscripciones");
            Category category2 = await SeedCategoryAsync("Seguros");

            await SeedFixedExpenseAsync(category1, "Netflix", 15.99m, 2024, 1);
            await SeedFixedExpenseAsync(category1, "Spotify", 9.99m, 2024, 1);

            FixedExpense inactive = await SeedFixedExpenseAsync(category1, "Disney+", 11.99m, 2024, 1);
            inactive.Deactivate();
            await _repository.UpdateAsync(inactive);

            await SeedFixedExpenseAsync(category2, "Seguro Coche", 50.00m, 2024, 1);

            // Act
            decimal total = await _repository.GetTotalActiveByCategoryAsync(category1.Id);

            // Assert
            Assert.Equal(25.98m, total); // Netflix + Spotify = 15.99 + 9.99
        }

        // ==================== TEST: EXISTS ====================

        [Fact]
        public async Task ExistsAsync_WithExistingId_ReturnsTrue()
        {
            // Arrange
            Category category = await SeedCategoryAsync("Suscripciones");
            FixedExpense fixedExpense = await SeedFixedExpenseAsync(category, "Netflix", 15.99m, 2024, 1);

            // Act
            bool exists = await _repository.ExistsAsync(fixedExpense.Id);

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

        // ==================== TEST: IS ACTIVE ====================

        [Fact]
        public async Task IsActiveAsync_WithActiveFixedExpense_ReturnsTrue()
        {
            // Arrange
            Category category = await SeedCategoryAsync("Suscripciones");
            FixedExpense fixedExpense = await SeedFixedExpenseAsync(category, "Netflix", 15.99m, 2024, 1);

            // Act
            bool isActive = await _repository.IsActiveAsync(fixedExpense.Id);

            // Assert
            Assert.True(isActive);
        }

        [Fact]
        public async Task IsActiveAsync_WithInactiveFixedExpense_ReturnsFalse()
        {
            // Arrange
            Category category = await SeedCategoryAsync("Suscripciones");
            FixedExpense fixedExpense = await SeedFixedExpenseAsync(category, "Netflix", 15.99m, 2024, 1);
            fixedExpense.Deactivate();
            await _repository.UpdateAsync(fixedExpense);

            // Act
            bool isActive = await _repository.IsActiveAsync(fixedExpense.Id);

            // Assert
            Assert.False(isActive);
        }

        [Fact]
        public async Task IsActiveAsync_WithNonExistingId_ReturnsFalse()
        {
            // Arrange
            int nonExistingId = 999;

            // Act
            bool result = await _repository.IsActiveAsync(nonExistingId);

            // Assert
            Assert.False(result);
        }

        // ==================== TEST: UPDATE ====================

        [Fact]
        public async Task UpdateAsync_ShouldUpdateFixedExpense()
        {
            // Arrange
            Category category = await SeedCategoryAsync("Suscripciones");
            FixedExpense fixedExpense = await SeedFixedExpenseAsync(category, "Netflix", 15.99m, 2024, 1);

            // Modificar la entidad
            EntityInfo newInfo = new EntityInfo("Netflix Premium", "Suscripción Premium");
            Money newAmount = new Money(17.99m);
            MonthlyPeriod newPeriod = new MonthlyPeriod(2, 2024);
            fixedExpense.Update(newInfo, newAmount, newPeriod);

            // Act
            await _repository.UpdateAsync(fixedExpense);

            // Assert
            FixedExpense updated = await _repository.GetByIdAsync(fixedExpense.Id);
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
            Category category = await SeedCategoryAsync("Suscripciones");
            FixedExpense fixedExpense = await SeedFixedExpenseAsync(category, "Netflix", 15.99m, 2024, 1);
            int id = fixedExpense.Id;

            // Act
            await _repository.DeleteAsync(id);

            // Assert
            FixedExpense deleted = await _repository.GetByIdAsync(id);
            Assert.Null(deleted);
        }

        [Fact]
        public async Task DeleteAsync_WithNonExistingId_ThrowsKeyNotFoundException()
        {
            // Act & Assert
            await Assert.ThrowsAsync<KeyNotFoundException>(() => _repository.DeleteAsync(999));
        }

        // ==================== TEST: ACTIVATE ====================

        [Fact]
        public async Task ActivateAsync_ShouldActivateFixedExpense()
        {
            // Arrange
            Category category = await SeedCategoryAsync("Suscripciones");
            FixedExpense fixedExpense = await SeedFixedExpenseAsync(category, "Netflix", 15.99m, 2024, 1);
            fixedExpense.Deactivate();
            await _repository.UpdateAsync(fixedExpense);

            // Act
            await _repository.ActivateAsync(fixedExpense.Id);

            // Assert
            bool isActive = await _repository.IsActiveAsync(fixedExpense.Id);
            Assert.True(isActive);
        }

        [Fact]
        public async Task ActivateAsync_WithNonExistingId_ThrowsKeyNotFoundException()
        {
            // Act & Assert
            await Assert.ThrowsAsync<KeyNotFoundException>(() => _repository.ActivateAsync(999));
        }

        // ==================== TEST: DEACTIVATE ====================

        [Fact]
        public async Task DeactivateAsync_ShouldDeactivateFixedExpense()
        {
            // Arrange
            Category category = await SeedCategoryAsync("Suscripciones");
            FixedExpense fixedExpense = await SeedFixedExpenseAsync(category, "Netflix", 15.99m, 2024, 1);

            // Act
            await _repository.DeactivateAsync(fixedExpense.Id);

            // Assert
            bool isActive = await _repository.IsActiveAsync(fixedExpense.Id);
            Assert.False(isActive);
        }

        [Fact]
        public async Task DeactivateAsync_WithNonExistingId_ThrowsKeyNotFoundException()
        {
            // Act & Assert
            await Assert.ThrowsAsync<KeyNotFoundException>(() => _repository.DeactivateAsync(999));
        }

        // ==================== DISPOSE ====================

        public void Dispose()
        {
            _dbContext?.Dispose();
        }
    }
}