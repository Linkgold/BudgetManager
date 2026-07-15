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

        public FixedExpenseRepositoryTests()
        {
            // Crear un nombre de base de datos único para cada prueba
            string databaseName = Guid.NewGuid().ToString();

            DbContextOptions<ApplicationDbContext> options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: databaseName)
                .Options;

            _dbContext = new ApplicationDbContext(options);
            _repository = new FixedExpenseRepository(_dbContext);
        }

        // ==================== TEST: ADD ====================

        [Fact]
        public async Task AddAsync_ShouldAddFixedExpenseToDatabase()
        {
            // Arrange
            User user = TestDataFactory.CreateUser();
            Category category = TestDataFactory.CreateCategory();
            FixedExpense fixedExpense = TestDataFactory.CreateFixedExpense(1, user, category);

            // Act
            await _repository.AddAsync(fixedExpense);

            // Assert
            FixedExpense? retrieved = await _dbContext.FixedExpenses
                .Include(f => f.Category)
                .FirstOrDefaultAsync(f => f.Id == fixedExpense.Id);

            Assert.NotNull(retrieved);
            Assert.Equal(TestDataFactory.DEFAULT_FIXED_EXPENSE_NAME, retrieved.Info.Name);
            Assert.Equal(TestDataFactory.DEFAULT_FIXED_EXPENSE_AMOUNT, retrieved.Amount.Value);
            Assert.Equal(TestDataFactory.DEFAULT_MONTHLY_MONTH, retrieved.ChargePeriod.Month);
            Assert.Equal(TestDataFactory.DEFAULT_YEAR, retrieved.ChargePeriod.Year);
            Assert.Equal(user.Id, retrieved.UserId);
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
            User user = TestDataFactory.CreateUser();
            Category category = TestDataFactory.CreateCategory();
            FixedExpense fixedExpense = await TestDataFactory.SeedFixedExpenseAsync(_repository, 1, user, category);

            // Act
            FixedExpense? retrieved = await _repository.GetByIdAsync(userId, fixedExpense.Id);

            // Assert
            Assert.NotNull(retrieved);
            Assert.Equal(fixedExpense.Id, retrieved.Id);
            Assert.Equal(TestDataFactory.DEFAULT_FIXED_EXPENSE_NAME, retrieved.Info.Name);
            Assert.Equal(TestDataFactory.DEFAULT_FIXED_EXPENSE_AMOUNT, retrieved.Amount.Value);
            Assert.Equal(TestDataFactory.DEFAULT_MONTHLY_MONTH, retrieved.ChargePeriod.Month);
            Assert.Equal(TestDataFactory.DEFAULT_YEAR, retrieved.ChargePeriod.Year);
            Assert.Equal(user.Id, retrieved.UserId);
            Assert.Equal(category.Id, retrieved.CategoryId);
            Assert.NotNull(retrieved.Category);
        }

        [Fact]
        public async Task GetByIdAsync_WithNonExistingId_ReturnsNull()
        {
            // Act
            int userId = 1;
            FixedExpense? retrieved = await _repository.GetByIdAsync(userId, 999);

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
            string otherFixedExpenseName = "Spotify";
            User user = TestDataFactory.CreateUser();
            Category category = TestDataFactory.CreateCategory(1, user);
            await TestDataFactory.SeedFixedExpenseAsync(_repository, 1, user, category);
            await TestDataFactory.SeedFixedExpenseAsync(_repository, 2, user, category, otherFixedExpenseName, amount: 9.99m);

            // Act
            IEnumerable<FixedExpense> result = await _repository.GetAllAsync(userId);

            // Assert
            Assert.Equal(2, result.Count());
            Assert.Contains(result, f => f.Info.Name == TestDataFactory.DEFAULT_FIXED_EXPENSE_NAME);
            Assert.Contains(result, f => f.Info.Name == otherFixedExpenseName);
        }

        // ==================== TEST: GET BY CATEGORY ====================

        [Fact]
        public async Task GetByCategoryAsync_WithExistingCategory_ReturnsFixedExpenses()
        {
            // Arrange
            int userId = 1;
            string otherFixedExpenseName = "Spotify";
            User user = TestDataFactory.CreateUser();
            Category category1 = TestDataFactory.CreateCategory(1, user);
            Category category2 = TestDataFactory.CreateCategory(2, user, "Seguros");

            await TestDataFactory.SeedFixedExpenseAsync(_repository, 1, user, category1);
            await TestDataFactory.SeedFixedExpenseAsync(_repository, 2, user, category1, otherFixedExpenseName);
            await TestDataFactory.SeedFixedExpenseAsync(_repository, 3, user, category2, "Seguro Coche");

            // Act
            IEnumerable<FixedExpense> result = await _repository.GetByCategoryAsync(userId, category1.Id);

            // Assert
            Assert.Equal(2, result.Count());
            Assert.All(result, f => Assert.Equal(category1.Id, f.CategoryId));
            Assert.Contains(result, f => f.Info.Name == TestDataFactory.DEFAULT_FIXED_EXPENSE_NAME);
            Assert.Contains(result, f => f.Info.Name == otherFixedExpenseName);
        }

        [Fact]
        public async Task GetByCategoryAsync_WithNonExistingCategory_ReturnsEmptyList()
        {
            // Act
            int userId = 1;
            IEnumerable<FixedExpense> result = await _repository.GetByCategoryAsync(userId, 999);

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
            User user = TestDataFactory.CreateUser();
            Category category = TestDataFactory.CreateCategory(1, user);

            FixedExpense active = await TestDataFactory.SeedFixedExpenseAsync(_repository, 1, user, category);
            FixedExpense inactive = await TestDataFactory.SeedFixedExpenseAsync(_repository, 2, user, category, "Disney+");
            inactive.Deactivate();
            await _repository.UpdateAsync(inactive);

            // Act
            IEnumerable<FixedExpense> result = await _repository.GetActiveAsync(userId);

            // Assert
            Assert.Single(result);
            Assert.Equal(TestDataFactory.DEFAULT_FIXED_EXPENSE_NAME, result.First().Info.Name);
            Assert.True(result.First().IsActive);
        }

        // ==================== TEST: GET ACTIVE BY CATEGORY ====================

        [Fact]
        public async Task GetActiveByCategoryAsync_ReturnsActiveFixedExpensesForCategory()
        {
            // Arrange
            int userId = 1;
            User user = TestDataFactory.CreateUser();
            Category category1 = TestDataFactory.CreateCategory(1, user);
            Category category2 = TestDataFactory.CreateCategory(2, user, "Seguros");

            FixedExpense active = await TestDataFactory.SeedFixedExpenseAsync(_repository, 1, user, category1);
            FixedExpense inactive = await TestDataFactory.SeedFixedExpenseAsync(_repository, 2, user, category1, "Disney+");
            inactive.Deactivate();
            await _repository.UpdateAsync(inactive);

            await TestDataFactory.SeedFixedExpenseAsync(_repository, 3, user, category2, "Seguro Coche");

            // Act
            IEnumerable<FixedExpense> result = await _repository.GetActiveByCategoryAsync(userId, category1.Id);

            // Assert
            Assert.Single(result);
            Assert.Equal(TestDataFactory.DEFAULT_FIXED_EXPENSE_NAME, result.First().Info.Name);
            Assert.Equal(category1.Id, result.First().CategoryId);
            Assert.True(result.First().IsActive);
        }

        // ==================== TEST: GET ACTIVE FOR PERIOD ====================

        [Fact]
        public async Task GetActiveForPeriodAsync_ReturnsActiveFixedExpensesForPeriod()
        {
            // Arrange
            int userId = 1;
            User user = TestDataFactory.CreateUser();
            Category category = TestDataFactory.CreateCategory(1, user);

            await TestDataFactory.SeedFixedExpenseAsync(_repository, 1, user, category);
            await TestDataFactory.SeedFixedExpenseAsync(_repository, 2, user, category, "Spotify", amount: 9.99m, month: 3, year: 2024);
            await TestDataFactory.SeedFixedExpenseAsync(_repository, 3, user, category, "Disney+", amount: 11.99m, month: 1, year: 2025);

            MonthlyPeriod period = TestDataFactory.CreateMonthlyPeriod();

            // Act
            IEnumerable<FixedExpense> result = await _repository.GetActiveForPeriodAsync(userId, period);

            // Assert
            // Solo Netflix debe estar activo en febrero 2024 (Spotify empieza en marzo, Disney+ en 2025)
            Assert.Single(result);
            Assert.Equal(TestDataFactory.DEFAULT_FIXED_EXPENSE_NAME, result.First().Info.Name);
        }

        [Fact]
        public async Task GetActiveForPeriodAsync_WithNullPeriod_ThrowsArgumentNullException()
        {
            // Act & Assert
            await Assert.ThrowsAsync<ArgumentNullException>(() => _repository.GetActiveForPeriodAsync(1, null));
        }

        // ==================== TEST: GET ACTIVE FOR PERIOD BY CATEGORY ====================

        [Fact]
        public async Task GetActiveForPeriodByCategoryAsync_ReturnsActiveFixedExpensesForCategoryAndPeriod()
        {
            // Arrange
            int userId = 1;
            User user = TestDataFactory.CreateUser();
            Category category1 = TestDataFactory.CreateCategory(1, user);
            Category category2 = TestDataFactory.CreateCategory(2, user, "Seguros");

            await TestDataFactory.SeedFixedExpenseAsync(_repository, 1, user, category1);
            await TestDataFactory.SeedFixedExpenseAsync(_repository, 2, user, category1, "Spotify", month: 3);
            await TestDataFactory.SeedFixedExpenseAsync(_repository, 3, user, category2, "Seguro Coche");

            MonthlyPeriod period = TestDataFactory.CreateMonthlyPeriod(2);

            // Act
            IEnumerable<FixedExpense> result = await _repository.GetActiveForPeriodByCategoryAsync(userId, category1.Id, period);

            // Assert
            Assert.Single(result);
            Assert.Equal(TestDataFactory.DEFAULT_FIXED_EXPENSE_NAME, result.First().Info.Name);
            Assert.Equal(category1.Id, result.First().CategoryId);
        }

        // ==================== TEST: GET TOTAL BY CATEGORY AND PERIOD ====================

        [Fact]
        public async Task GetTotalByCategoryAndPeriodAsync_ReturnsTotalForCategoryAndPeriod()
        {
            // Arrange
            int userId = 1;
            decimal otherAmount = 9.99m;
            User user = TestDataFactory.CreateUser();
            Category category = TestDataFactory.CreateCategory(1, user);

            await TestDataFactory.SeedFixedExpenseAsync(_repository, 1, user, category);
            await TestDataFactory.SeedFixedExpenseAsync(_repository, 2, user, category, "Spotify", amount: otherAmount);
            await TestDataFactory.SeedFixedExpenseAsync(_repository, 3, user, category, "Disney+", year: 2025);

            MonthlyPeriod period = TestDataFactory.CreateMonthlyPeriod(2, 2024);

            // Act
            decimal total = await _repository.GetTotalByCategoryAndPeriodAsync(userId, category.Id, period);

            // Assert
            Assert.Equal(otherAmount + TestDataFactory.DEFAULT_FIXED_EXPENSE_AMOUNT, total); // Netflix + Spotify = 15.99 + 9.99
        }

        // ==================== TEST: GET TOTAL ACTIVE ====================

        [Fact]
        public async Task GetTotalActiveAsync_ReturnsTotalOfAllActiveFixedExpenses()
        {
            // Arrange
            int userId = 1;
            User user = TestDataFactory.CreateUser();
            Category category = TestDataFactory.CreateCategory(1, user);

            await TestDataFactory.SeedFixedExpenseAsync(_repository, 1, user, category);

            FixedExpense inactive = await TestDataFactory.SeedFixedExpenseAsync(_repository, 2, user, category, "Disney+", amount: 11.99m);
            inactive.Deactivate();

            await _repository.UpdateAsync(inactive);

            // Act
            decimal total = await _repository.GetTotalActiveAsync(userId);

            // Assert
            Assert.Equal(TestDataFactory.DEFAULT_FIXED_EXPENSE_AMOUNT, total);
        }

        // ==================== TEST: GET TOTAL ACTIVE BY CATEGORY ====================

        [Fact]
        public async Task GetTotalActiveByCategoryAsync_ReturnsTotalActiveForCategory()
        {
            // Arrange
            int userId = 1;
            decimal otherAmount = 9.99m;
            User user = TestDataFactory.CreateUser();
            Category category1 = TestDataFactory.CreateCategory(1, user);
            Category category2 = TestDataFactory.CreateCategory(2, user, "Seguros");

            await TestDataFactory.SeedFixedExpenseAsync(_repository, 1, user, category1);
            await TestDataFactory.SeedFixedExpenseAsync(_repository, 2, user, category1, "Spotify", amount: otherAmount);

            FixedExpense inactive = await TestDataFactory.SeedFixedExpenseAsync(_repository, 3, user, category1, "Disney+", amount: 11.99m, year: 2025);
            inactive.Deactivate();
            await _repository.UpdateAsync(inactive);

            await TestDataFactory.SeedFixedExpenseAsync(_repository, 4, user, category2, "Seguro Coche", amount: 50.00m);

            // Act
            decimal total = await _repository.GetTotalActiveByCategoryAsync(userId, category1.Id);

            // Assert
            Assert.Equal(otherAmount + TestDataFactory.DEFAULT_FIXED_EXPENSE_AMOUNT, total); // Netflix + Spotify = 15.99 + 9.99
        }

        // ==================== TEST: EXISTS ====================

        [Fact]
        public async Task ExistsAsync_WithExistingId_ReturnsTrue()
        {
            // Arrange
            int userId = 1;
            User user = TestDataFactory.CreateUser();
            Category category = TestDataFactory.CreateCategory(1, user);
            FixedExpense fixedExpense = await TestDataFactory.SeedFixedExpenseAsync(_repository, 1, user, category);

            // Act
            bool exists = await _repository.ExistsAsync(userId, fixedExpense.Id);

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

        // ==================== TEST: IS ACTIVE ====================

        [Fact]
        public async Task IsActiveAsync_WithActiveFixedExpense_ReturnsTrue()
        {
            // Arrange
            int userId = 1;
            User user = TestDataFactory.CreateUser();
            Category category = TestDataFactory.CreateCategory(1, user);
            FixedExpense fixedExpense = await TestDataFactory.SeedFixedExpenseAsync(_repository, 1, user, category);

            // Act
            bool isActive = await _repository.IsActiveAsync(userId, fixedExpense.Id);

            // Assert
            Assert.True(isActive);
        }

        [Fact]
        public async Task IsActiveAsync_WithInactiveFixedExpense_ReturnsFalse()
        {
            // Arrange
            int userId = 1;
            User user = TestDataFactory.CreateUser();
            Category category = TestDataFactory.CreateCategory(1, user);

            FixedExpense fixedExpense = await TestDataFactory.SeedFixedExpenseAsync(_repository, 1, user, category);
            fixedExpense.Deactivate();

            await _repository.UpdateAsync(fixedExpense);

            // Act
            bool isActive = await _repository.IsActiveAsync(userId, fixedExpense.Id);

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
            bool result = await _repository.IsActiveAsync(userId, nonExistingId);

            // Assert
            Assert.False(result);
        }

        // ==================== TEST: UPDATE ====================

        [Fact]
        public async Task UpdateAsync_ShouldUpdateFixedExpense()
        {
            // Arrange
            int userId = 1;
            string updatedName = "Netflix Premium";
            string updatedDescription = "Suscripción Premium";
            decimal updatedAmount = 17.99m;
            int updatedMonth = 2;
            User user = TestDataFactory.CreateUser();
            Category category = TestDataFactory.CreateCategory(1, user);

            FixedExpense fixedExpense = await TestDataFactory.SeedFixedExpenseAsync(_repository, 1, user, category);

            // Modificar la entidad
            EntityInfo updatedInfoObject = TestDataFactory.CreateEntityInfo(updatedName, updatedDescription);
            Money updatedAmountObject = TestDataFactory.CreateMoney(updatedAmount);
            MonthlyPeriod updatedPeriodObject = TestDataFactory.CreateMonthlyPeriod(updatedMonth);

            fixedExpense.Update(updatedInfoObject, updatedAmountObject, updatedPeriodObject);

            // Act
            await _repository.UpdateAsync(fixedExpense);

            // Assert
            FixedExpense? updated = await _repository.GetByIdAsync(userId, fixedExpense.Id);
            Assert.NotNull(updated);
            Assert.Equal(updatedName, updated.Info.Name);
            Assert.Equal(updatedDescription, updated.Info.Description);
            Assert.Equal(updatedAmount, updated.Amount.Value);
            Assert.Equal(updatedMonth, updated.ChargePeriod.Month);
            Assert.Equal(TestDataFactory.DEFAULT_YEAR, updated.ChargePeriod.Year);
            Assert.NotNull(updated.UpdatedAt);
        }

        // ==================== TEST: DELETE ====================

        [Fact]
        public async Task DeleteAsync_ShouldRemoveFixedExpense()
        {
            // Arrange
            int userId = 1;
            User user = TestDataFactory.CreateUser();
            Category category = TestDataFactory.CreateCategory(1, user);

            FixedExpense fixedExpense = await TestDataFactory.SeedFixedExpenseAsync(_repository, 1, user, category);
            int id = fixedExpense.Id;

            // Act
            await _repository.DeleteAsync(userId, id);

            // Assert
            FixedExpense? deleted = await _repository.GetByIdAsync(userId, id);
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
            User user = TestDataFactory.CreateUser();
            Category category = TestDataFactory.CreateCategory(1, user);

            FixedExpense fixedExpense = await TestDataFactory.SeedFixedExpenseAsync(_repository, 1, user, category);
            fixedExpense.Deactivate();

            await _repository.UpdateAsync(fixedExpense);

            // Act
            await _repository.ActivateAsync(userId, fixedExpense.Id);

            // Assert
            bool isActive = await _repository.IsActiveAsync(userId, fixedExpense.Id);
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
            User user = TestDataFactory.CreateUser();
            Category category = TestDataFactory.CreateCategory(1, user);
            FixedExpense fixedExpense = await TestDataFactory.SeedFixedExpenseAsync(_repository, 1, user, category);

            // Act
            await _repository.DeactivateAsync(userId, fixedExpense.Id);

            // Assert
            bool isActive = await _repository.IsActiveAsync(userId, fixedExpense.Id);
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