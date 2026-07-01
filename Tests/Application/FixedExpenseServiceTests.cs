using Application.DTOs.Request;
using Application.DTOs.Response;
using Application.Interfaces;
using Application.Mappings;
using Application.Services;
using AutoMapper;
using Domain.Entities;
using Domain.Interfaces;
using Domain.ValueObjects;
using Microsoft.Extensions.Logging;
using Moq;

namespace Tests.Application
{
    /// <summary>
    /// Pruebas unitarias para FixedExpenseService
    /// </summary>
    public class FixedExpenseServiceTests : IDisposable
    {
        private readonly Mock<IFixedExpenseRepository> _fixedExpenseRepositoryMock;
        private readonly Mock<ICategoryRepository> _categoryRepositoryMock;
        private readonly IMapper _mapper;
        private readonly IFixedExpenseService _fixedExpenseService;

        public FixedExpenseServiceTests()
        {
            // Configurar AutoMapper
            MapperConfiguration mapperConfiguration = new MapperConfiguration
            (
                config =>
                {
                    config.AddProfile<AutoMapperProfile>();
                },
                new LoggerFactory()
            );

            _mapper = mapperConfiguration.CreateMapper();

            // Crear mocks
            _fixedExpenseRepositoryMock = new Mock<IFixedExpenseRepository>();
            _categoryRepositoryMock = new Mock<ICategoryRepository>();

            // Instanciar el servicio
            _fixedExpenseService = new FixedExpenseService(_fixedExpenseRepositoryMock.Object, _categoryRepositoryMock.Object, _mapper);
        }

        // ==================== HELPERS ====================

        private Category CreateCategory(int id, string name)
        {
            EntityInfo info = new EntityInfo(name, null);
            Category category = new Category(info);

            // Asignar ID por reflexión (ya que es private set)
            typeof(Category).GetProperty("Id")?.SetValue(category, id);

            return category;
        }

        private FixedExpense CreateFixedExpense(int id, Category category, string name, decimal amount, int month, int year)
        {
            EntityInfo info = new EntityInfo(name, null);
            Money money = new Money(amount);
            Period period = new Period(month, year);

            FixedExpense fixedExpense = new FixedExpense(category, info, money, period);

            // Asignar ID por reflexión
            typeof(FixedExpense).GetProperty("Id")?.SetValue(fixedExpense, id);

            return fixedExpense;
        }

        // ==================== TEST: GET BY ID ====================

        [Fact]
        public async Task GetByIdAsync_WithExistingId_ReturnsFixedExpense()
        {
            // Arrange
            int fixedExpenseId = 1;
            Category category = CreateCategory(1, "Suscripciones");
            FixedExpense fixedExpense = CreateFixedExpense(fixedExpenseId, category, "Netflix", 15.99m, 1, 2024);

            _fixedExpenseRepositoryMock
                .Setup(repo => repo.GetByIdAsync(fixedExpenseId))
                .ReturnsAsync(fixedExpense);

            // Act
            FixedExpenseResponseDTO result = await _fixedExpenseService.GetByIdAsync(fixedExpenseId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(fixedExpenseId, result.Id);
            Assert.Equal("Netflix", result.Name);
            Assert.Equal(15.99m, result.Amount);
            Assert.Equal(1, result.Month);
            Assert.Equal(2024, result.Year);
            Assert.Equal("Suscripciones", result.CategoryName);
            Assert.True(result.IsActive);
        }

        [Fact]
        public async Task GetByIdAsync_WithNonExistingId_ThrowsKeyNotFoundException()
        {
            // Arrange
            int fixedExpenseId = 999;

            _fixedExpenseRepositoryMock
                .Setup(repo => repo.GetByIdAsync(fixedExpenseId))
                .ReturnsAsync((FixedExpense)null);

            // Act & Assert
            await Assert.ThrowsAsync<KeyNotFoundException>(() => _fixedExpenseService.GetByIdAsync(fixedExpenseId));
        }

        [Fact]
        public async Task GetByIdAsync_WithInvalidId_ThrowsArgumentException()
        {
            // Arrange
            int invalidId = 0;

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() => _fixedExpenseService.GetByIdAsync(invalidId));
        }

        // ==================== TEST: GET ALL ====================

        [Fact]
        public async Task GetAllAsync_ReturnsListOfFixedExpenses()
        {
            // Arrange
            Category category = CreateCategory(1, "Suscripciones");

            FixedExpense fixedExpense1 = CreateFixedExpense(1, category, "Netflix", 15.99m, 1, 2024);
            FixedExpense fixedExpense2 = CreateFixedExpense(2, category, "Spotify", 9.99m, 1, 2024);

            List<FixedExpense> fixedExpenses = new List<FixedExpense> { fixedExpense1, fixedExpense2 };

            _fixedExpenseRepositoryMock
                .Setup(repo => repo.GetAllAsync())
                .ReturnsAsync(fixedExpenses);

            // Act
            List<FixedExpenseResponseDTO> result = await _fixedExpenseService.GetAllAsync();

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count);
            Assert.Equal("Netflix", result[0].Name);
            Assert.Equal("Spotify", result[1].Name);
        }

        // ==================== TEST: GET BY CATEGORY ====================

        [Fact]
        public async Task GetByCategoryIdAsync_WithExistingCategory_ReturnsFixedExpenses()
        {
            // Arrange
            int categoryId = 1;
            Category category = CreateCategory(categoryId, "Suscripciones");

            FixedExpense fixedExpense1 = CreateFixedExpense(1, category, "Netflix", 15.99m, 1, 2024);
            FixedExpense fixedExpense2 = CreateFixedExpense(2, category, "Spotify", 9.99m, 1, 2024);

            List<FixedExpense> fixedExpenses = new List<FixedExpense> { fixedExpense1, fixedExpense2 };

            _categoryRepositoryMock
                .Setup(repo => repo.ExistsAsync(categoryId))
                .ReturnsAsync(true);

            _fixedExpenseRepositoryMock
                .Setup(repo => repo.GetByCategoryAsync(categoryId))
                .ReturnsAsync(fixedExpenses);

            // Act
            List<FixedExpenseResponseDTO> result = await _fixedExpenseService.GetByCategoryIdAsync(categoryId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count);
            _fixedExpenseRepositoryMock.Verify(repo => repo.GetByCategoryAsync(categoryId), Times.Once);
        }

        [Fact]
        public async Task GetByCategoryIdAsync_WithNonExistingCategory_ThrowsKeyNotFoundException()
        {
            // Arrange
            int categoryId = 999;

            _categoryRepositoryMock
                .Setup(repo => repo.ExistsAsync(categoryId))
                .ReturnsAsync(false);

            // Act & Assert
            await Assert.ThrowsAsync<KeyNotFoundException>(() => _fixedExpenseService.GetByCategoryIdAsync(categoryId));

            _fixedExpenseRepositoryMock.Verify(repo => repo.GetByCategoryAsync(It.IsAny<int>()), Times.Never);
        }

        // ==================== TEST: GET ACTIVE ====================

        [Fact]
        public async Task GetActiveAsync_ReturnsOnlyActiveFixedExpenses()
        {
            // Arrange
            Category category = CreateCategory(1, "Suscripciones");

            FixedExpense activeExpense = CreateFixedExpense(1, category, "Netflix", 15.99m, 1, 2024);
            FixedExpense inactiveExpense = CreateFixedExpense(2, category, "Disney+", 11.99m, 1, 2024);
            inactiveExpense.Deactivate(); // Desactivar uno

            List<FixedExpense> activeExpenses = new List<FixedExpense> { activeExpense };

            _fixedExpenseRepositoryMock
                .Setup(repo => repo.GetActiveAsync())
                .ReturnsAsync(activeExpenses);

            // Act
            List<FixedExpenseResponseDTO> result = await _fixedExpenseService.GetActiveAsync();

            // Assert
            Assert.NotNull(result);
            Assert.Single(result);
            Assert.Equal("Netflix", result[0].Name);
        }

        // ==================== TEST: CREATE ====================

        [Fact]
        public async Task CreateAsync_WithValidData_ReturnsCreatedFixedExpense()
        {
            // Arrange
            int categoryId = 1;
            CreateFixedExpenseRequestDTO request = new CreateFixedExpenseRequestDTO { CategoryId = categoryId, Name = "Netflix", Description = "Suscripción mensual", Amount = 15.99m, Month = 1, Year = 2024 };

            Category category = CreateCategory(categoryId, "Suscripciones");

            _categoryRepositoryMock
                .Setup(repo => repo.GetByIdAsync(categoryId, true))
                .ReturnsAsync(category);

            // Act
            FixedExpenseResponseDTO result = await _fixedExpenseService.CreateAsync(request);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Netflix", result.Name);
            Assert.Equal(15.99m, result.Amount);
            Assert.Equal(1, result.Month);
            Assert.Equal(2024, result.Year);
            Assert.Equal("Suscripciones", result.CategoryName);
            Assert.True(result.IsActive);

            _fixedExpenseRepositoryMock.Verify(repo => repo.AddAsync(It.IsAny<FixedExpense>()), Times.Once);
        }

        [Fact]
        public async Task CreateAsync_WithNonExistingCategory_ThrowsKeyNotFoundException()
        {
            // Arrange
            int categoryId = 999;
            CreateFixedExpenseRequestDTO request = new CreateFixedExpenseRequestDTO { CategoryId = categoryId, Name = "Netflix", Amount = 15.99m, Month = 1, Year = 2024 };

            _categoryRepositoryMock
                .Setup(repo => repo.GetByIdAsync(categoryId,true))
                .ReturnsAsync((Category)null);

            // Act & Assert
            await Assert.ThrowsAsync<KeyNotFoundException>(() => _fixedExpenseService.CreateAsync(request));

            _fixedExpenseRepositoryMock.Verify(repo => repo.AddAsync(It.IsAny<FixedExpense>()), Times.Never);
        }

        [Fact]
        public async Task CreateAsync_WithNegativeAmount_ThrowsArgumentException()
        {
            // Arrange
            int categoryId = 1;
            CreateFixedExpenseRequestDTO request = new CreateFixedExpenseRequestDTO { CategoryId = categoryId, Name = "Netflix", Amount = -15.99m, Month = 1, Year = 2024 };

            Category category = CreateCategory(categoryId, "Suscripciones");

            _categoryRepositoryMock
                .Setup(repo => repo.GetByIdAsync(categoryId, true))
                .ReturnsAsync(category);

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() => _fixedExpenseService.CreateAsync(request));

            _fixedExpenseRepositoryMock.Verify(repo => repo.AddAsync(It.IsAny<FixedExpense>()), Times.Never);
        }

        // ==================== TEST: UPDATE ====================

        [Fact]
        public async Task UpdateAsync_WithValidData_ReturnsUpdatedFixedExpense()
        {
            // Arrange
            int fixedExpenseId = 1;
            int categoryId = 1;

            UpdateFixedExpenseRequestDTO request = new UpdateFixedExpenseRequestDTO { Name = "Netflix Premium", Description = "Suscripción mensual Premium", Amount = 17.99m, Month = 2, Year = 2024 };

            Category category = CreateCategory(categoryId, "Suscripciones");
            FixedExpense existingFixedExpense = CreateFixedExpense(fixedExpenseId, category, "Netflix", 15.99m, 1,2024);

            _fixedExpenseRepositoryMock
                .Setup(repo => repo.GetByIdAsync(fixedExpenseId))
                .ReturnsAsync(existingFixedExpense);

            // Act
            FixedExpenseResponseDTO result = await _fixedExpenseService.UpdateAsync(fixedExpenseId, request);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(fixedExpenseId, result.Id);
            Assert.Equal("Netflix Premium", result.Name);
            Assert.Equal(17.99m, result.Amount);
            Assert.Equal(2, result.Month);
            Assert.Equal(2024, result.Year);

            _fixedExpenseRepositoryMock.Verify(repo => repo.UpdateAsync(It.IsAny<FixedExpense>()), Times.Once);
        }

        [Fact]
        public async Task UpdateAsync_WithNonExistingId_ThrowsKeyNotFoundException()
        {
            // Arrange
            int fixedExpenseId = 999;
            UpdateFixedExpenseRequestDTO request = new UpdateFixedExpenseRequestDTO { Name = "Netflix Premium", Amount = 17.99m, Month = 2, Year = 2024 };

            _fixedExpenseRepositoryMock
                .Setup(repo => repo.GetByIdAsync(fixedExpenseId))
                .ReturnsAsync((FixedExpense)null);

            // Act & Assert
            await Assert.ThrowsAsync<KeyNotFoundException>(() => _fixedExpenseService.UpdateAsync(fixedExpenseId, request));

            _fixedExpenseRepositoryMock.Verify(repo => repo.UpdateAsync(It.IsAny<FixedExpense>()), Times.Never);
        }

        // ==================== TEST: DELETE ====================

        [Fact]
        public async Task DeleteAsync_WithExistingId_DeletesFixedExpense()
        {
            // Arrange
            int fixedExpenseId = 1;

            _fixedExpenseRepositoryMock
                .Setup(repo => repo.ExistsAsync(fixedExpenseId))
                .ReturnsAsync(true);

            // Act
            await _fixedExpenseService.DeleteAsync(fixedExpenseId);

            // Assert
            _fixedExpenseRepositoryMock.Verify(repo => repo.DeleteAsync(fixedExpenseId), Times.Once);
        }

        [Fact]
        public async Task DeleteAsync_WithNonExistingId_ThrowsKeyNotFoundException()
        {
            // Arrange
            int fixedExpenseId = 999;

            _fixedExpenseRepositoryMock
                .Setup(repo => repo.ExistsAsync(fixedExpenseId))
                .ReturnsAsync(false);

            // Act & Assert
            await Assert.ThrowsAsync<KeyNotFoundException>(() => _fixedExpenseService.DeleteAsync(fixedExpenseId));

            _fixedExpenseRepositoryMock.Verify(repo => repo.DeleteAsync(It.IsAny<int>()), Times.Never);
        }

        // ==================== TEST: ACTIVATE ====================

        [Fact]
        public async Task ActivateAsync_WithExistingId_ActivatesFixedExpense()
        {
            // Arrange
            int fixedExpenseId = 1;

            _fixedExpenseRepositoryMock
                .Setup(repo => repo.ExistsAsync(fixedExpenseId))
                .ReturnsAsync(true);

            // Act
            await _fixedExpenseService.ActivateAsync(fixedExpenseId);

            // Assert
            _fixedExpenseRepositoryMock.Verify(repo => repo.ActivateAsync(fixedExpenseId), Times.Once);
        }

        [Fact]
        public async Task ActivateAsync_WithNonExistingId_ThrowsKeyNotFoundException()
        {
            // Arrange
            int fixedExpenseId = 999;

            _fixedExpenseRepositoryMock
                .Setup(repo => repo.ExistsAsync(fixedExpenseId))
                .ReturnsAsync(false);

            // Act & Assert
            await Assert.ThrowsAsync<KeyNotFoundException>(() => _fixedExpenseService.ActivateAsync(fixedExpenseId));

            _fixedExpenseRepositoryMock.Verify(repo => repo.ActivateAsync(It.IsAny<int>()), Times.Never);
        }

        // ==================== TEST: DEACTIVATE ====================

        [Fact]
        public async Task DeactivateAsync_WithExistingId_DeactivatesFixedExpense()
        {
            // Arrange
            int fixedExpenseId = 1;

            _fixedExpenseRepositoryMock
                .Setup(repo => repo.ExistsAsync(fixedExpenseId))
                .ReturnsAsync(true);

            // Act
            await _fixedExpenseService.DeactivateAsync(fixedExpenseId);

            // Assert
            _fixedExpenseRepositoryMock.Verify(repo => repo.DeactivateAsync(fixedExpenseId), Times.Once);
        }

        [Fact]
        public async Task DeactivateAsync_WithNonExistingId_ThrowsKeyNotFoundException()
        {
            // Arrange
            int fixedExpenseId = 999;

            _fixedExpenseRepositoryMock
                .Setup(repo => repo.ExistsAsync(fixedExpenseId))
                .ReturnsAsync(false);

            // Act & Assert
            await Assert.ThrowsAsync<KeyNotFoundException>(() => _fixedExpenseService.DeactivateAsync(fixedExpenseId));

            _fixedExpenseRepositoryMock.Verify(repo => repo.DeactivateAsync(It.IsAny<int>()), Times.Never);
        }

        // ==================== TEST: GET TOTAL FOR PERIOD ====================

        [Fact]
        public async Task GetTotalForPeriodByCategoryAsync_WithValidData_ReturnsTotal()
        {
            // Arrange
            int categoryId = 1;
            int year = 2024;
            int month = 1;
            decimal expectedTotal = 25.98m;

            _categoryRepositoryMock
                .Setup(repo => repo.ExistsAsync(categoryId))
                .ReturnsAsync(true);

            _fixedExpenseRepositoryMock
                .Setup(repo => repo.GetTotalByCategoryAndPeriodAsync(categoryId, It.IsAny<Period>()))
                .ReturnsAsync(expectedTotal);

            // Act
            decimal result = await _fixedExpenseService.GetTotalForPeriodByCategoryAsync(categoryId, month, year);

            // Assert
            Assert.Equal(expectedTotal, result);
            _fixedExpenseRepositoryMock.Verify(repo => repo.GetTotalByCategoryAndPeriodAsync(categoryId, It.IsAny<Period>()), Times.Once);
        }

        [Fact]
        public async Task GetTotalForPeriodByCategoryAsync_WithNonExistingCategory_ThrowsKeyNotFoundException()
        {
            // Arrange
            int categoryId = 999;

            _categoryRepositoryMock
                .Setup(repo => repo.ExistsAsync(categoryId))
                .ReturnsAsync(false);

            // Act & Assert
            await Assert.ThrowsAsync<KeyNotFoundException>(() => _fixedExpenseService.GetTotalForPeriodByCategoryAsync(categoryId, 1, 2024));

            _fixedExpenseRepositoryMock.Verify(repo => repo.GetTotalByCategoryAndPeriodAsync(It.IsAny<int>(), It.IsAny<Period>()), Times.Never);
        }

        // ==================== TEST: EXISTS ====================

        [Fact]
        public async Task ExistsAsync_WithExistingId_ReturnsTrue()
        {
            // Arrange
            int fixedExpenseId = 1;

            _fixedExpenseRepositoryMock
                .Setup(repo => repo.ExistsAsync(fixedExpenseId))
                .ReturnsAsync(true);

            // Act
            bool result = await _fixedExpenseService.ExistsAsync(fixedExpenseId);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public async Task ExistsAsync_WithNonExistingId_ReturnsFalse()
        {
            // Arrange
            int fixedExpenseId = 999;

            _fixedExpenseRepositoryMock
                .Setup(repo => repo.ExistsAsync(fixedExpenseId))
                .ReturnsAsync(false);

            // Act
            bool result = await _fixedExpenseService.ExistsAsync(fixedExpenseId);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public async Task ExistsAsync_WithInvalidId_ReturnsFalse()
        {
            // Arrange
            int invalidId = 0;

            // Act
            bool result = await _fixedExpenseService.ExistsAsync(invalidId);

            // Assert
            Assert.False(result);
            _fixedExpenseRepositoryMock.Verify(repo => repo.ExistsAsync(It.IsAny<int>()), Times.Never);
        }

        // ==================== TEST: IS ACTIVE ====================

        [Fact]
        public async Task IsActiveAsync_WithExistingId_ReturnsTrue()
        {
            // Arrange
            int fixedExpenseId = 1;

            _fixedExpenseRepositoryMock
                .Setup(repo => repo.ExistsAsync(fixedExpenseId))
                .ReturnsAsync(true);

            _fixedExpenseRepositoryMock
                .Setup(repo => repo.IsActiveAsync(fixedExpenseId))
                .ReturnsAsync(true);

            // Act
            bool result = await _fixedExpenseService.IsActiveAsync(fixedExpenseId);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public async Task IsActiveAsync_WithNonExistingId_ThrowsKeyNotFoundException()
        {
            // Arrange
            int fixedExpenseId = 999;

            _fixedExpenseRepositoryMock
                .Setup(repo => repo.ExistsAsync(fixedExpenseId))
                .ReturnsAsync(false);

            // Act & Assert
            await Assert.ThrowsAsync<KeyNotFoundException>(() => _fixedExpenseService.IsActiveAsync(fixedExpenseId));

            _fixedExpenseRepositoryMock.Verify(repo => repo.IsActiveAsync(It.IsAny<int>()), Times.Never);
        }

        [Fact]
        public async Task IsActiveAsync_WithInvalidId_ReturnsFalse()
        {
            // Arrange
            int invalidId = 0;

            // Act
            bool result = await _fixedExpenseService.IsActiveAsync(invalidId);

            // Assert
            Assert.False(result);
            _fixedExpenseRepositoryMock.Verify(repo => repo.ExistsAsync(It.IsAny<int>()), Times.Never);
        }

        // ==================== DISPOSE ====================

        public void Dispose()
        {
            // Limpiar recursos si es necesario
        }
    }
}