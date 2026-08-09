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
    /// <summary>
    /// Pruebas unitarias para FixedExpenseRepository usando InMemory Database
    /// </summary>
    public class FixedExpenseRepositoryTests : IDisposable
    {
        private readonly SqliteConnection _connection;
        private readonly ApplicationDbContext _dbContext;
        private readonly IFixedExpenseRepository _repository;
        private readonly ICategoryRepository _categoryRepository;

        public FixedExpenseRepositoryTests()
        {
            _connection = new SqliteConnection("DataSource=:memory:");
            _connection.Open();

            DbContextOptions<ApplicationDbContext> options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseSqlite(_connection)
                .Options;

            _dbContext = new ApplicationDbContext(options);
            _dbContext.EnsureDatabaseCreated();

            _repository = new FixedExpenseRepository(_dbContext);
            _categoryRepository = new CategoryRepository(_dbContext);
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
            Assert.Equal(TestDataFactory.DEFAULT_CURRENCY, retrieved.Amount.Currency);
            Assert.Equal(TestDataFactory.DEFAULT_MONTHLY_MONTH, retrieved.ChargePeriod.Month);
            Assert.Equal(TestDataFactory.DEFAULT_YEAR, retrieved.ChargePeriod.Year);
            Assert.Equal(user.Id, retrieved.UserId);
            Assert.Equal(category.Id, retrieved.CategoryId);
            Assert.NotEqual(default, retrieved.CreatedAt);
        }

        [Fact]
        public async Task AddAsync_ShouldAddFixedExpenseWithCNYCurrency()
        {
            // Arrange
            string customCurrency = "CNY";
            FixedExpense fixedExpense = TestDataFactory.CreateFixedExpense(currency: customCurrency);

            // Act
            await _repository.AddAsync(fixedExpense);

            // Assert
            FixedExpense? retrieved = await _dbContext.FixedExpenses
                .Include(f => f.Category)
                .FirstOrDefaultAsync(f => f.Id == fixedExpense.Id);

            Assert.NotNull(retrieved);
            Assert.Equal(customCurrency, retrieved.Amount.Currency);
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
            await TestDataFactory.SeedFixedExpenseAsync(_repository, 1, user, category, month: 7, year: 2003);
            await TestDataFactory.SeedFixedExpenseAsync(_repository, 2, user, category, otherFixedExpenseName, amount: 9.99m);

            // Act
            IEnumerable<FixedExpense> result = await _repository.GetAllByYearAsync(userId, 2003);

            // Assert
            Assert.Single(result);
            Assert.Contains(result, f => f.Info.Name == TestDataFactory.DEFAULT_FIXED_EXPENSE_NAME);
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

        [Fact]
        public async Task ExistsByCategoryNameMonthYearAsync_WithExistingCombination_ReturnsTrue()
        {
            // Arrange
            int userId = 1;
            User user = TestDataFactory.CreateUser(userId);
            Category category = TestDataFactory.CreateCategory(1, user);
            string name = "Seguro";
            int month = 9;
            int year = 2024;

            await TestDataFactory.SeedFixedExpenseAsync(_repository, 1, user, category, name, month: month, year: year);

            // Act
            bool exists = await _repository.ExistsByCategoryNameMonthYearAsync(userId, category.Id, name, month, year);

            // Assert
            Assert.True(exists);
        }

        [Fact]
        public async Task ExistsByCategoryNameMonthYearAsync_WithDifferentCategory_ReturnsFalse()
        {
            // Arrange
            int userId = 1;
            User user = TestDataFactory.CreateUser(userId);
            Category category1 = TestDataFactory.CreateCategory(1, user, "Coche");
            Category category2 = TestDataFactory.CreateCategory(2, user, "Vida");
            string name = "Seguro";
            int month = 9;
            int year = 2024;

            await TestDataFactory.SeedFixedExpenseAsync(_repository, 1, user, category1, name, month: month, year: year);

            // Act
            bool exists = await _repository.ExistsByCategoryNameMonthYearAsync(userId, category2.Id, name, month, year);

            // Assert
            Assert.False(exists);
        }

        [Fact]
        public async Task ExistsByCategoryNameMonthYearAsync_WithDifferentMonth_ReturnsFalse()
        {
            // Arrange
            int userId = 1;
            User user = TestDataFactory.CreateUser(userId);
            Category category = TestDataFactory.CreateCategory(1, user);
            string name = "Seguro";
            int month = 9;
            int year = 2024;

            await TestDataFactory.SeedFixedExpenseAsync(_repository, 1, user, category, name, month: month, year: year);

            // Act
            bool exists = await _repository.ExistsByCategoryNameMonthYearAsync(userId, category.Id, name, 10, year);

            // Assert
            Assert.False(exists);
        }

        [Fact]
        public async Task ExistsByCategoryNameMonthYearAsync_WithDifferentYear_ReturnsFalse()
        {
            // Arrange
            int userId = 1;
            User user = TestDataFactory.CreateUser(userId);
            Category category = TestDataFactory.CreateCategory(1, user);
            string name = "Seguro";
            int month = 9;
            int year = 2024;

            await TestDataFactory.SeedFixedExpenseAsync(_repository, 1, user, category, name, month: month, year: year);

            // Act
            bool exists = await _repository.ExistsByCategoryNameMonthYearAsync(userId, category.Id, name, month, 2025);

            // Assert
            Assert.False(exists);
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

            Category updatedCategory = await TestDataFactory.SeedCategoryAsync(_categoryRepository, 2, user, "Test 2");
            FixedExpense fixedExpense = await TestDataFactory.SeedFixedExpenseAsync(_repository, 1, user, category);

            // Modificar la entidad
            EntityInfo updatedInfoObject = TestDataFactory.CreateEntityInfo(updatedName, updatedDescription);
            Money updatedAmountObject = TestDataFactory.CreateMoney(updatedAmount);
            MonthlyPeriod updatedPeriodObject = TestDataFactory.CreateMonthlyPeriod(updatedMonth);

            fixedExpense.Update(updatedCategory, updatedInfoObject, updatedAmountObject, updatedPeriodObject);

            // Act
            await _repository.UpdateAsync(fixedExpense);

            // Assert
            FixedExpense? updated = await _repository.GetByIdAsync(userId, fixedExpense.Id);
            Assert.NotNull(updated);
            Assert.Equal(updatedCategory.Id, updated.CategoryId);
            Assert.Equal(updatedCategory.Info.Name, updated.Category.Info.Name);
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

        // ==================== NUEVOS TESTS: GET DISTINCT YEARS ====================

        [Fact]
        public async Task GetDistinctYearsAsync_WithData_ReturnsYearsList()
        {
            // Arrange
            int userId = 1;
            User user = TestDataFactory.CreateUser(userId);
            Category category = TestDataFactory.CreateCategory(1, user, "Suscripciones", nature: CategoryNatureEnum.Expense);

            // Crear gastos fijos en diferentes años
            await TestDataFactory.SeedFixedExpenseAsync(_repository, 1, user, category, "Netflix", "", 15.99m, "EUR", 1, 2023);
            await TestDataFactory.SeedFixedExpenseAsync(_repository, 2, user, category, "Spotify", "", 9.99m, "EUR", 2, 2023);
            await TestDataFactory.SeedFixedExpenseAsync(_repository, 3, user, category, "Disney+", "", 12.99m, "EUR", 1, 2024);
            await TestDataFactory.SeedFixedExpenseAsync(_repository, 4, user, category, "Amazon", "", 4.99m, "EUR", 1, 2025);

            // Expected: años 2023, 2024, 2025
            List<int> expected = new List<int> { 2023, 2024, 2025 };

            // Act
            IEnumerable<int> result = await _repository.GetDistinctYearsAsync(userId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(expected.Count, result.Count());
            Assert.Equal(expected, result.Order().ToList());
        }

        [Fact]
        public async Task GetDistinctYearsAsync_WithNoData_ReturnsEmptyList()
        {
            // Arrange
            int userId = 1;

            // Act
            IEnumerable<int> result = await _repository.GetDistinctYearsAsync(userId);

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result);
        }

        [Fact]
        public async Task GetDistinctYearsAsync_WithInvalidUserId_ThrowsArgumentException()
        {
            // Arrange
            int invalidUserId = 0;

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() => _repository.GetDistinctYearsAsync(invalidUserId));
        }

        [Fact]
        public async Task GetDistinctYearsAsync_WithMultipleYears_ReturnsSortedYears()
        {
            // Arrange
            int userId = 1;
            User user = TestDataFactory.CreateUser(userId);
            Category category = TestDataFactory.CreateCategory(1, user, "Suscripciones");

            // Crear gastos fijos en años desordenados
            await TestDataFactory.SeedFixedExpenseAsync(_repository, 1, user, category, "Netflix", "", 15.99m, "EUR", 1, 2025);
            await TestDataFactory.SeedFixedExpenseAsync(_repository, 2, user, category, "Spotify", "", 9.99m, "EUR", 1, 2023);
            await TestDataFactory.SeedFixedExpenseAsync(_repository, 3, user, category, "Disney+", "", 12.99m, "EUR", 1, 2024);

            // Expected: años ordenados 2023, 2024, 2025
            List<int> expected = new List<int> { 2023, 2024, 2025 };

            // Act
            IEnumerable<int> result = await _repository.GetDistinctYearsAsync(userId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(expected, result);
        }

        [Fact]
        public async Task GetDistinctYearsAsync_WithDifferentUsers_ReturnsOnlyUserYears()
        {
            // Arrange
            int userId1 = 1;
            int userId2 = 2;
            User user1 = TestDataFactory.CreateUser(userId1);
            User user2 = TestDataFactory.CreateUser(userId2, email: "paquito@example.com");
            Category category1 = TestDataFactory.CreateCategory(1, user1, "Suscripciones");
            Category category2 = TestDataFactory.CreateCategory(2, user2, "Suscripciones");

            // Gastos fijos para usuario 1: años 2023 y 2024
            await TestDataFactory.SeedFixedExpenseAsync(_repository, 1, user1, category1, "Netflix", "", 15.99m, "EUR", 1, 2023);
            await TestDataFactory.SeedFixedExpenseAsync(_repository, 2, user1, category1, "Spotify", "", 9.99m, "EUR", 1, 2024);

            // Gastos fijos para usuario 2: año 2025
            await TestDataFactory.SeedFixedExpenseAsync(_repository, 3, user2, category2, "Disney+", "", 12.99m, "EUR", 1, 2025);

            // Expected para usuario 1: años 2023, 2024
            List<int> expected = new List<int> { 2023, 2024 };

            // Act
            IEnumerable<int> result = await _repository.GetDistinctYearsAsync(userId1);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(expected, result);
            Assert.DoesNotContain(2025, result);
        }

        // ==================== DISPOSE ====================

        public void Dispose()
        {
            _dbContext?.Dispose();
            _connection?.Dispose();
        }
    }
}