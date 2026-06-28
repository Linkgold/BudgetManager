using Domain.Entities;
using Domain.Interfaces;
using Domain.ValueObjects;
using Infrastructure.Data;
using Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;

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
            Category category = new Category(new EntityInfo("Prueba", "Descripción de prueba"));

            // Act
            await _repository.AddAsync(category);

            // Assert
            Category retrieved = await _dbContext.Categories.FindAsync(category.Id);
            Assert.NotNull(retrieved);
            Assert.Equal("Prueba", retrieved.Info.Name);
            Assert.Equal("Descripción de prueba", retrieved.Info.Description);
            Assert.True(retrieved.IsActive);
            Assert.NotEqual(default(DateTime), retrieved.CreatedAt);
        }

        // ==================== TEST: GET BY ID ====================

        [Fact]
        public async Task GetByIdAsync_WithExistingId_ReturnsCategory()
        {
            // Arrange
            Category category = new Category(new EntityInfo("Existente", null));
            await _repository.AddAsync(category);

            // Act
            Category retrieved = await _repository.GetByIdAsync(category.Id);

            // Assert
            Assert.NotNull(retrieved);
            Assert.Equal(category.Id, retrieved.Id);
            Assert.Equal("Existente", retrieved.Info.Name);
        }

        [Fact]
        public async Task GetByIdAsync_WithNonExistingId_ReturnsNull()
        {
            // Act
            Category retrieved = await _repository.GetByIdAsync(999);

            // Assert
            Assert.Null(retrieved);
        }

        // ==================== TEST: GET ALL ====================

        [Fact]
        public async Task GetAllAsync_ReturnsAllCategories()
        {
            // Arrange
            Category cat1 = new Category(new EntityInfo("Cat1", null));
            Category cat2 = new Category(new EntityInfo("Cat2", null));
            await _repository.AddAsync(cat1);
            await _repository.AddAsync(cat2);

            // Act
            List<Category> result = await _repository.GetAllAsync();

            // Assert
            Assert.Equal(2, result.Count);
            Assert.Contains(result, c => c.Info.Name == "Cat1");
            Assert.Contains(result, c => c.Info.Name == "Cat2");
        }

        // ==================== TEST: GET BY NAME ====================

        [Fact]
        public async Task GetByNameAsync_WithExistingName_ReturnsCategory()
        {
            // Arrange
            Category category = new Category(new EntityInfo("NombreUnico", null));
            await _repository.AddAsync(category);

            // Act
            Category retrieved = await _repository.GetByNameAsync("NombreUnico");

            // Assert
            Assert.NotNull(retrieved);
            Assert.Equal(category.Id, retrieved.Id);
            Assert.Equal("NombreUnico", retrieved.Info.Name);
        }

        [Fact]
        public async Task GetByNameAsync_WithNonExistingName_ReturnsNull()
        {
            // Act
            Category retrieved = await _repository.GetByNameAsync("NoExiste");

            // Assert
            Assert.Null(retrieved);
        }

        // ==================== TEST: GET ACTIVE ====================

        [Fact]
        public async Task GetActiveCategoriesAsync_ReturnsOnlyActiveCategories()
        {
            // Arrange
            Category active = new Category(new EntityInfo("Activa", null));
            Category inactive = new Category(new EntityInfo("Inactiva", null));
            inactive.Deactivate(); // Desactivamos

            await _repository.AddAsync(active);
            await _repository.AddAsync(inactive);

            // Act
            List<Category> result = await _repository.GetActiveCategoriesAsync();

            // Assert
            Assert.Single(result);
            Assert.Equal("Activa", result[0].Info.Name);
        }

        // ==================== TEST: UPDATE ====================

        [Fact]
        public async Task UpdateAsync_ShouldUpdateCategory()
        {
            // Arrange
            Category category = new Category(new EntityInfo("Original", "Descripción original"));
            await _repository.AddAsync(category);

            // Modificar la entidad
            category.Update(new EntityInfo("Modificado", "Nueva descripción"));

            // Act
            await _repository.UpdateAsync(category);

            // Assert
            Category updated = await _repository.GetByIdAsync(category.Id);
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
            Category category = new Category(new EntityInfo("Eliminar", null));
            await _repository.AddAsync(category);
            int id = category.Id;

            /*// Desprender la entidad del contexto para evitar conflicto de tracking
            _dbContext.Entry(category).State = EntityState.Detached;*/

            // Act
            await _repository.DeleteAsync(id);

            // Assert
            Category deleted = await _repository.GetByIdAsync(id);
            Assert.Null(deleted);
        }

        [Fact]
        public async Task DeleteAsync_WithNonExistingId_ThrowsKeyNotFoundException()
        {
            // Act & Assert
            await Assert.ThrowsAsync<KeyNotFoundException>(() =>
                _repository.DeleteAsync(999));
        }

        // ==================== TEST: EXISTS ====================

        [Fact]
        public async Task ExistsAsync_WithExistingId_ReturnsTrue()
        {
            // Arrange
            Category category = new Category(new EntityInfo("Existe", null));
            await _repository.AddAsync(category);

            // Act
            bool exists = await _repository.ExistsAsync(category.Id);

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

        // ==================== TEST: EXISTS BY NAME ====================

        [Fact]
        public async Task ExistsByNameAsync_WithExistingName_ReturnsTrue()
        {
            // Arrange
            Category category = new Category(new EntityInfo("NombreTest", null));
            await _repository.AddAsync(category);

            // Act
            bool exists = await _repository.ExistsByNameAsync("NombreTest");

            // Assert
            Assert.True(exists);
        }

        [Fact]
        public async Task ExistsByNameAsync_WithNonExistingName_ReturnsFalse()
        {
            // Act
            bool exists = await _repository.ExistsByNameAsync("NoExiste");

            // Assert
            Assert.False(exists);
        }

        // ==================== TEST: HAS EXPENSES (sin implementar) ====================

        [Fact]
        public async Task HasExpensesAsync_ShouldReturnFalse_WhenNoExpensesExist()
        {
            // Arrange
            Category category = new Category(new EntityInfo("SinGastos", null));
            await _repository.AddAsync(category);

            // Act
            bool hasExpenses = await _repository.HasExpensesAsync(category.Id);

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
