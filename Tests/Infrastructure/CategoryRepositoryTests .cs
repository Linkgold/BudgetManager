using Application.Interfaces;
using Domain.Entities;
using Domain.Interfaces;
using Domain.ValueObjects;
using Infrastructure.Data;
using Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Moq;
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
            // Arrange
            Category category = TestDataFactory.CreateCategory(1, TestDataFactory.CreateUser());

            // Act
            await _repository.AddAsync(category);

            // Assert
            Category? retrieved = await _dbContext.Categories.FindAsync(category.Id);
            Assert.NotNull(retrieved);
            Assert.Equal("Alimentación", retrieved.Info.Name);
            Assert.Equal("Gastos de comida", retrieved.Info.Description);
            Assert.True(retrieved.IsActive);
            Assert.NotEqual(default(DateTime), retrieved.CreatedAt);
        }

        // ==================== TEST: GET BY ID ====================

        [Fact]
        public async Task GetByIdAsync_WithExistingId_ReturnsCategory()
        {
            // Arrange
            int userId = 1;
            Category category = TestDataFactory.CreateCategory(1, TestDataFactory.CreateUser());
            await _repository.AddAsync(category);

            // Act
            Category? retrieved = await _repository.GetByIdAsync(category.Id, userId);

            // Assert
            Assert.NotNull(retrieved);
            Assert.Equal(category.Id, retrieved.Id);
            Assert.Equal("Existente", retrieved.Info.Name);
        }

        [Fact]
        public async Task GetByIdAsync_WithNonExistingId_ReturnsNull()
        {
            // Act
            int userId = 1;
            Category? retrieved = await _repository.GetByIdAsync(999,userId);

            // Assert
            Assert.Null(retrieved);
        }

        // ==================== TEST: GET ALL ====================

        [Fact]
        public async Task GetAllAsync_ReturnsAllCategories()
        {
            // Arrange
            int userId = 1;
            List<Category> categories = TestDataFactory.CreateCategories(2, TestDataFactory.CreateUser(userId));

            await _repository.AddAsync(categories[0]);
            await _repository.AddAsync(categories[1]);

            // Act
            IEnumerable<Category> result = await _repository.GetAllAsync(userId);

            // Assert
            Assert.Equal(2, result.Count());
            Assert.Contains(result, c => c.Info.Name == "Categoría 1");
            Assert.Contains(result, c => c.Info.Name == "Categoría 2");
        }

        // ==================== TEST: GET BY NAME ====================

        [Fact]
        public async Task GetByNameAsync_WithExistingName_ReturnsCategory()
        {
            // Arrange
            int userId = 1;
            Category category = TestDataFactory.CreateCategory();
            await _repository.AddAsync(category);

            // Act
            Category? retrieved = await _repository.GetByNameAsync("Alimentación", userId);

            // Assert
            Assert.NotNull(retrieved);
            Assert.Equal(category.Id, retrieved.Id);
            Assert.Equal("Alimentación", retrieved.Info.Name);
        }

        [Fact]
        public async Task GetByNameAsync_WithNonExistingName_ReturnsNull()
        {
            // Act
            Category? retrieved = await _repository.GetByNameAsync("NoExiste",1);

            // Assert
            Assert.Null(retrieved);
        }

        // ==================== TEST: GET ACTIVE ====================

        [Fact]
        public async Task GetActiveCategoriesAsync_ReturnsOnlyActiveCategories()
        {
            // Arrange
            int userId = 1;
            List<Category> categories = TestDataFactory.CreateCategories(2, TestDataFactory.CreateUser(userId));
            categories[1].Deactivate(); // Desactivamos

            await _repository.AddAsync(categories[0]);
            await _repository.AddAsync(categories[1]);

            // Act
            IEnumerable<Category> result = await _repository.GetActiveCategoriesAsync(userId);

            // Assert
            Assert.Single(result);
            Assert.Equal("Activa", result.First().Info.Name);
        }

        // ==================== TEST: UPDATE ====================

        [Fact]
        public async Task UpdateAsync_ShouldUpdateCategory()
        {
            // Arrange
            int userId = 1;
            Category category = TestDataFactory.CreateCategory();
            await _repository.AddAsync(category);

            // Modificar la entidad
            category.Update(new EntityInfo("Modificado", "Nueva descripción"));

            // Act
            await _repository.UpdateAsync(category);

            // Assert
            Category? updated = await _repository.GetByIdAsync(category.Id, userId);
            Assert.NotNull(updated);
            Assert.Equal("Modificado", updated.Info.Name);
            Assert.Equal("Nueva descripción", updated.Info.Description);
            Assert.NotNull(updated.UpdatedAt);
        }

        // ==================== TEST: DELETE ====================

        [Fact]
        public async Task DeleteAsync_ShouldRemoveCategory()
        {
            // Arrange
            int userId = 1;
            Category category = TestDataFactory.CreateCategory();
            await _repository.AddAsync(category);
            int id = category.Id;

            // Act
            await _repository.DeleteAsync(id,   userId);

            // Assert
            Category? deleted = await _repository.GetByIdAsync(id,userId);
            Assert.Null(deleted);
        }

        [Fact]
        public async Task DeleteAsync_WithNonExistingId_ThrowsKeyNotFoundException()
        {
            // Act & Assert
            await Assert.ThrowsAsync<KeyNotFoundException>(() =>                _repository.DeleteAsync(999,1));
        }

        // ==================== TEST: EXISTS ====================

        [Fact]
        public async Task ExistsAsync_WithExistingId_ReturnsTrue()
        {
            // Arrange
            int userId = 1;
            Category category = TestDataFactory.CreateCategory();
            await _repository.AddAsync(category);

            // Act
            bool exists = await _repository.ExistsAsync(category.Id, userId);

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

        // ==================== TEST: EXISTS BY NAME ====================

        [Fact]
        public async Task ExistsByNameAsync_WithExistingName_ReturnsTrue()
        {
            // Arrange
            int userId = 1;
            Category category = TestDataFactory.CreateCategory();
            await _repository.AddAsync(category);

            // Act
            bool exists = await _repository.ExistsByNameAsync("Alimentación",userId);

            // Assert
            Assert.True(exists);
        }

        [Fact]
        public async Task ExistsByNameAsync_WithNonExistingName_ReturnsFalse()
        {
            // Act
            bool exists = await _repository.ExistsByNameAsync("NoExiste", 1);

            // Assert
            Assert.False(exists);
        }

        // ==================== TEST: HAS EXPENSES (sin implementar) ====================

        [Fact]
        public async Task HasExpensesAsync_ShouldReturnFalse_WhenNoExpensesExist()
        {
            // Arrange
            int userId = 1;
            Category category = TestDataFactory.CreateCategory();
            await _repository.AddAsync(category);

            // Act
            bool hasExpenses = await _repository.HasDependenciesAsync(category.Id, userId);

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
