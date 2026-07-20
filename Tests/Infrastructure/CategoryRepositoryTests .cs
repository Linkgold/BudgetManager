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
    /// Pruebas unitarias para el repositorio de categorías usando InMemory Database
    /// </summary>
    public class CategoryRepositoryTests : IDisposable
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly ICategoryRepository _repository;

        public CategoryRepositoryTests()
        {
            // Crear un nombre de base de datos único para cada prueba
            string databaseName = Guid.NewGuid().ToString();

            DbContextOptions<ApplicationDbContext> options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: databaseName)
                .Options;

            _dbContext = new ApplicationDbContext(options);
            _repository = new CategoryRepository(_dbContext);
        }

        // ==================== TEST: ADD ====================

        [Fact]
        public async Task AddAsync_ShouldAddCategoryToDatabase()
        {
            // Arrange & Act
            Category category = await TestDataFactory.SeedCategoryAsync(_repository, 1, TestDataFactory.CreateUser());

            // Assert
            Category? retrieved = await _dbContext.Categories.FindAsync(category.Id);
            Assert.NotNull(retrieved);
            Assert.Equal(TestDataFactory.DEFAULT_CATEGORY_NAME, retrieved.Info.Name);
            Assert.Equal(TestDataFactory.DEFAULT_CATEGORY_DESCRIPTION, retrieved.Info.Description);
            Assert.NotEqual(default, retrieved.CreatedAt);
        }

        // ==================== TEST: GET BY ID ====================

        [Fact]
        public async Task GetByIdAsync_WithExistingId_ReturnsCategory()
        {
            // Arrange
            int userId = 1;
            Category category = await TestDataFactory.SeedCategoryAsync(_repository, 1, TestDataFactory.CreateUser());

            // Act
            Category? retrieved = await _repository.GetByIdAsync(userId, category.Id);

            // Assert
            Assert.NotNull(retrieved);
            Assert.Equal(category.Id, retrieved.Id);
            Assert.Equal(TestDataFactory.DEFAULT_CATEGORY_NAME, retrieved.Info.Name);
        }

        [Fact]
        public async Task GetByIdAsync_WithNonExistingId_ReturnsNull()
        {
            // Act
            int userId = 1;
            Category? retrieved = await _repository.GetByIdAsync(userId, 999);

            // Assert
            Assert.Null(retrieved);
        }

        // ==================== TEST: GET ALL ====================

        [Fact]
        public async Task GetAllAsync_ReturnsAllCategories()
        {
            // Arrange
            int userId = 1;
            User user = TestDataFactory.CreateUser(userId);
            Category category1 = await TestDataFactory.SeedCategoryAsync(_repository, 1, user);
            Category category2 = await TestDataFactory.SeedCategoryAsync(_repository, 2, user);

            // Act
            IEnumerable<Category> result = await _repository.GetAllAsync(userId);

            // Assert
            Assert.Equal(2, result.Count());
            Assert.Contains(result, c => c.Id == category1.Id);
            Assert.Contains(result, c => c.Id == category2.Id);
        }

        // ==================== TEST: GET BY NAME ====================

        [Fact]
        public async Task GetByNameAsync_WithExistingName_ReturnsCategory()
        {
            // Arrange
            int userId = 1;
            Category category = await TestDataFactory.SeedCategoryAsync(_repository, 1, TestDataFactory.CreateUser());

            // Act
            Category? retrieved = await _repository.GetByNameAsync(userId, TestDataFactory.DEFAULT_CATEGORY_NAME);

            // Assert
            Assert.NotNull(retrieved);
            Assert.Equal(category.Id, retrieved.Id);
            Assert.Equal(TestDataFactory.DEFAULT_CATEGORY_NAME, retrieved.Info.Name);
        }

        [Fact]
        public async Task GetByNameAsync_WithNonExistingName_ReturnsNull()
        {
            // Act
            Category? retrieved = await _repository.GetByNameAsync(1, "NoExiste");

            // Assert
            Assert.Null(retrieved);
        }

        // ==================== TEST: UPDATE ====================

        [Fact]
        public async Task UpdateAsync_ShouldUpdateCategory()
        {
            // Arrange
            int userId = 1;
            string updatedName = "Modificado";
            string updatedDescription = "Nueva descripción";
            Category category = await TestDataFactory.SeedCategoryAsync(_repository, 1, TestDataFactory.CreateUser());

            // Modificar la entidad
            category.Update(TestDataFactory.CreateEntityInfo(updatedName, updatedDescription));

            // Act
            await _repository.UpdateAsync(category);

            // Assert
            Category? updated = await _repository.GetByIdAsync(userId, category.Id);
            Assert.NotNull(updated);
            Assert.Equal(updatedName, updated.Info.Name);
            Assert.Equal(updatedDescription, updated.Info.Description);
            Assert.NotNull(updated.UpdatedAt);
        }

        // ==================== TEST: DELETE ====================

        [Fact]
        public async Task DeleteAsync_ShouldRemoveCategory()
        {
            // Arrange
            int userId = 1;
            Category category = await TestDataFactory.SeedCategoryAsync(_repository, 1, TestDataFactory.CreateUser());

            // Act
            await _repository.DeleteAsync(userId, category.Id);

            // Assert
            Category? deleted = await _repository.GetByIdAsync(userId, category.Id);
            Assert.Null(deleted);
        }

        [Fact]
        public async Task DeleteAsync_WithNonExistingId_ThrowsKeyNotFoundException()
        {
            // Act & Assert
            await Assert.ThrowsAsync<KeyNotFoundException>(() => _repository.DeleteAsync(999, 1));
        }

        // ==================== TEST: EXISTS ====================

        [Fact]
        public async Task ExistsAsync_WithExistingId_ReturnsTrue()
        {
            // Arrange
            int userId = 1;
            Category category = await TestDataFactory.SeedCategoryAsync(_repository, 1, TestDataFactory.CreateUser());

            // Act
            bool exists = await _repository.ExistsAsync(userId, category.Id);

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

        // ==================== TEST: EXISTS BY NAME ====================

        [Fact]
        public async Task ExistsByNameAsync_WithExistingName_ReturnsTrue()
        {
            // Arrange
            int userId = 1;
            Category category = await TestDataFactory.SeedCategoryAsync(_repository, 1, TestDataFactory.CreateUser());

            // Act
            bool exists = await _repository.ExistsByNameAsync(userId, TestDataFactory.DEFAULT_CATEGORY_NAME);

            // Assert
            Assert.True(exists);
        }

        [Fact]
        public async Task ExistsByNameAsync_WithNonExistingName_ReturnsFalse()
        {
            // Act
            bool exists = await _repository.ExistsByNameAsync(1, "NoExiste");

            // Assert
            Assert.False(exists);
        }

        // ==================== TEST: HAS EXPENSES (sin implementar) ====================

        [Fact]
        public async Task HasExpensesAsync_ShouldReturnFalse_WhenNoExpensesExist()
        {
            // Arrange
            int userId = 1;
            Category category = await TestDataFactory.SeedCategoryAsync(_repository, 1, TestDataFactory.CreateUser());

            // Act
            bool hasExpenses = await _repository.HasDependenciesAsync(userId, category.Id);

            // Assert
            Assert.False(hasExpenses);
        }

        // ==================== DISPOSE ====================

        public void Dispose()
        {
            _dbContext?.Dispose();
        }
    }
}