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
using Tests.Helpers;

namespace Tests.Application
{
    /// <summary>
    /// Pruebas unitarias para FixedExpenseService
    /// </summary>
    public class FixedExpenseServiceTests : IDisposable
    {
        private readonly Mock<IUserRepository> _userRepositoryMock;
        private readonly Mock<ICurrentUserService> _currentUserServiceMock;
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
            _userRepositoryMock = new Mock<IUserRepository>();
            _currentUserServiceMock = new Mock<ICurrentUserService>();
            _fixedExpenseRepositoryMock = new Mock<IFixedExpenseRepository>();
            _categoryRepositoryMock = new Mock<ICategoryRepository>();

            // Instanciar el servicio
            _fixedExpenseService = new FixedExpenseService(_currentUserServiceMock.Object, _fixedExpenseRepositoryMock.Object, _categoryRepositoryMock.Object, _userRepositoryMock.Object, _mapper);
        }

        // ==================== TEST: GET BY ID ====================

        [Fact]
        public async Task GetByIdAsync_WithExistingId_ReturnsFixedExpense()
        {
            // Arrange
            int userId = 1;
            int fixedExpenseId = 1;
            Category category = TestDataFactory.CreateCategory(1, TestDataFactory.CreateUser());
            FixedExpense fixedExpense = TestDataFactory.CreateFixedExpense();

            TestDataFactory.SetupAuthenticatedUser(_currentUserServiceMock, userId);

            _fixedExpenseRepositoryMock
                .Setup(repo => repo.GetByIdAsync(fixedExpenseId, userId))
                .ReturnsAsync(fixedExpense);

            // Act
            FixedExpenseResponseDTO result = await _fixedExpenseService.GetByIdAsync(fixedExpenseId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(fixedExpenseId, result.Id);
            Assert.Equal(TestDataFactory.DEFAULT_FIXED_EXPENSE_NAME, result.Name);
            Assert.Equal(TestDataFactory.DEFAULT_FIXED_EXPENSE_AMOUNT, result.Amount);
            Assert.Equal(TestDataFactory.DEFAULT_MONTHLY_MONTH, result.Month);
            Assert.Equal(TestDataFactory.DEFAULT_YEAR, result.Year);
            Assert.Equal(TestDataFactory.DEFAULT_FIXED_EXPENSE_DESCRIPTION, result.Description);
            Assert.True(result.IsActive);
        }

        [Fact]
        public async Task GetByIdAsync_WithNonExistingId_ThrowsKeyNotFoundException()
        {
            // Arrange
            int userId = 1;
            int fixedExpenseId = 999;

            TestDataFactory.SetupAuthenticatedUser(_currentUserServiceMock, userId);

            _fixedExpenseRepositoryMock
                .Setup(repo => repo.GetByIdAsync(fixedExpenseId, userId))
                .ReturnsAsync((FixedExpense?)null);

            // Act & Assert
            await Assert.ThrowsAsync<KeyNotFoundException>(() => _fixedExpenseService.GetByIdAsync(fixedExpenseId));
        }

        [Fact]
        public async Task GetByIdAsync_WithInvalidId_ThrowsArgumentException()
        {
            // Arrange
            int invalidId = 0;

            TestDataFactory.SetupAuthenticatedUser(_currentUserServiceMock, 1);

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() => _fixedExpenseService.GetByIdAsync(invalidId));
        }

        // ==================== TEST: GET ALL ====================

        [Fact]
        public async Task GetAllAsync_ReturnsListOfFixedExpenses()
        {
            // Arrange
            int userId = 1;
            int categoryId = 1;
            string customFixedExpenseName = "Spotify";

            User user = TestDataFactory.CreateUser(userId);
            Category category = TestDataFactory.CreateCategory(categoryId, user, "Suscripciones");
            
            FixedExpense fixedExpense1 = TestDataFactory.CreateFixedExpense(1, user, category);
            FixedExpense fixedExpense2 = TestDataFactory.CreateFixedExpense(2, user, category, customFixedExpenseName);

            List<FixedExpense> fixedExpenses = new List<FixedExpense> { fixedExpense1, fixedExpense2 };

            TestDataFactory.SetupAuthenticatedUser(_currentUserServiceMock, userId);

            _fixedExpenseRepositoryMock
                .Setup(repo => repo.GetAllAsync(userId))
                .ReturnsAsync(fixedExpenses);

            // Act
            List<FixedExpenseResponseDTO> result = await _fixedExpenseService.GetAllAsync();

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count);
            Assert.Equal(TestDataFactory.DEFAULT_FIXED_EXPENSE_NAME, result[0].Name);
            Assert.Equal(customFixedExpenseName, result[1].Name);
        }

        // ==================== TEST: GET BY CATEGORY ====================

        [Fact]
        public async Task GetByCategoryIdAsync_WithExistingCategory_ReturnsFixedExpenses()
        {
            // Arrange
            int userId = 1;
            int categoryId = 1;
            string customFixedExpenseName = "Spotify";

            User user = TestDataFactory.CreateUser(userId);
            Category category = TestDataFactory.CreateCategory(categoryId, user, "Suscripciones");

            FixedExpense fixedExpense1 = TestDataFactory.CreateFixedExpense(1, user, category);
            FixedExpense fixedExpense2 = TestDataFactory.CreateFixedExpense(2, user, category, customFixedExpenseName);

            List<FixedExpense> fixedExpenses = new List<FixedExpense> { fixedExpense1, fixedExpense2 };

            TestDataFactory.SetupAuthenticatedUser(_currentUserServiceMock, userId);

            _categoryRepositoryMock
                .Setup(repo => repo.ExistsAsync(categoryId, userId))
                .ReturnsAsync(true);

            _fixedExpenseRepositoryMock
                .Setup(repo => repo.GetByCategoryAsync(categoryId, userId))
                .ReturnsAsync(fixedExpenses);

            // Act
            List<FixedExpenseResponseDTO> result = await _fixedExpenseService.GetByCategoryIdAsync(categoryId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count);
            _fixedExpenseRepositoryMock.Verify(repo => repo.GetByCategoryAsync(categoryId, userId), Times.Once);
        }

        [Fact]
        public async Task GetByCategoryIdAsync_WithNonExistingCategory_ThrowsKeyNotFoundException()
        {
            // Arrange
            int userId = 1;
            int categoryId = 999;

            TestDataFactory.SetupAuthenticatedUser(_currentUserServiceMock, userId);

            _categoryRepositoryMock
                .Setup(repo => repo.ExistsAsync(categoryId, userId))
                .ReturnsAsync(false);

            // Act & Assert
            await Assert.ThrowsAsync<KeyNotFoundException>(() => _fixedExpenseService.GetByCategoryIdAsync(categoryId));

            _fixedExpenseRepositoryMock.Verify(repo => repo.GetByCategoryAsync(It.IsAny<int>(), userId), Times.Never);
        }

        // ==================== TEST: GET ACTIVE ====================

        [Fact]
        public async Task GetActiveAsync_ReturnsOnlyActiveFixedExpenses()
        {
            // Arrange
            int userId = 1;
            int categoryId = 1;
            string customFixedExpenseName = "Disney+";

            User user = TestDataFactory.CreateUser(userId);
            Category category = TestDataFactory.CreateCategory(categoryId, user, "Suscripciones");

            FixedExpense activeExpense = TestDataFactory.CreateFixedExpense(1, user, category);
            FixedExpense inactiveExpense = TestDataFactory.CreateFixedExpense(2, user, category, customFixedExpenseName);

            TestDataFactory.SetupAuthenticatedUser(_currentUserServiceMock, userId);

            inactiveExpense.Deactivate(); // Desactivar uno

            List<FixedExpense> activeExpenses = new List<FixedExpense> { activeExpense };

            TestDataFactory.SetupAuthenticatedUser(_currentUserServiceMock, userId);

            _fixedExpenseRepositoryMock
                .Setup(repo => repo.GetActiveAsync(userId))
                .ReturnsAsync(activeExpenses);

            // Act
            List<FixedExpenseResponseDTO> result = await _fixedExpenseService.GetActiveAsync();

            // Assert
            Assert.NotNull(result);
            Assert.Single(result);
            Assert.Equal(TestDataFactory.DEFAULT_FIXED_EXPENSE_NAME, result[0].Name);
        }

        // ==================== TEST: CREATE ====================

        [Fact]
        public async Task CreateAsync_WithValidData_ReturnsCreatedFixedExpense()
        {
            // Arrange
            int userId = 1;
            int categoryId = 1;
            string customCatogoryName = "Suscripciones";

            CreateFixedExpenseRequestDTO request = new CreateFixedExpenseRequestDTO 
            { 
                CategoryId = categoryId, 
                Name = TestDataFactory.DEFAULT_FIXED_EXPENSE_NAME, 
                Description = TestDataFactory.DEFAULT_FIXED_EXPENSE_DESCRIPTION, 
                Amount = TestDataFactory.DEFAULT_FIXED_EXPENSE_AMOUNT, 
                Month = TestDataFactory.DEFAULT_MONTHLY_MONTH, 
                Year = TestDataFactory.DEFAULT_YEAR 
            };

            User user = TestDataFactory.CreateUser(userId);
            Category category = TestDataFactory.CreateCategory(categoryId, user, customCatogoryName);

            TestDataFactory.SetupAuthenticatedUser(_currentUserServiceMock, userId);

            _categoryRepositoryMock
                .Setup(repo => repo.GetByIdAsync(categoryId, userId, true))
                .ReturnsAsync(category);

            _userRepositoryMock
                .Setup(repo => repo.GetByIdAsync(userId))
                .ReturnsAsync(user);

            // Act
            FixedExpenseResponseDTO result = await _fixedExpenseService.CreateAsync(request);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(TestDataFactory.DEFAULT_FIXED_EXPENSE_NAME, result.Name);
            Assert.Equal(TestDataFactory.DEFAULT_FIXED_EXPENSE_AMOUNT, result.Amount);
            Assert.Equal(TestDataFactory.DEFAULT_MONTHLY_MONTH, result.Month);
            Assert.Equal(TestDataFactory.DEFAULT_YEAR, result.Year);
            Assert.Equal(customCatogoryName, result.CategoryName);
            Assert.True(result.IsActive);

            _fixedExpenseRepositoryMock.Verify(repo => repo.AddAsync(It.IsAny<FixedExpense>()), Times.Once);
        }

        [Fact]
        public async Task CreateAsync_WithNonExistingCategory_ThrowsKeyNotFoundException()
        {
            // Arrange
            int userId = 1;
            int categoryId = 999;
            CreateFixedExpenseRequestDTO request = new CreateFixedExpenseRequestDTO { CategoryId = categoryId };

            TestDataFactory.SetupAuthenticatedUser(_currentUserServiceMock, userId);

            _categoryRepositoryMock
                .Setup(repo => repo.GetByIdAsync(categoryId, userId, true))
                .ReturnsAsync((Category?)null);

            // Act & Assert
            await Assert.ThrowsAsync<KeyNotFoundException>(() => _fixedExpenseService.CreateAsync(request));

            _fixedExpenseRepositoryMock.Verify(repo => repo.AddAsync(It.IsAny<FixedExpense>()), Times.Never);
        }

        [Fact]
        public async Task CreateAsync_WithNegativeAmount_ThrowsArgumentException()
        {
            // Arrange
            int userId = 1;
            int categoryId = 1;
            CreateFixedExpenseRequestDTO request = new CreateFixedExpenseRequestDTO { CategoryId = categoryId, Amount = -15.99m };

            User user = TestDataFactory.CreateUser(userId);
            Category category = TestDataFactory.CreateCategory(categoryId, user, "Suscripciones");

            TestDataFactory.SetupAuthenticatedUser(_currentUserServiceMock, userId);

            _categoryRepositoryMock
                .Setup(repo => repo.GetByIdAsync(categoryId, userId, true))
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
            int userId = 1;
            int fixedExpenseId = 1;
            int categoryId = 1;
            string updatedName = "Netflix Premium";
            string updatedDescription = "Suscripción mensual Premium";
            decimal updatedAmount = 17.99m;
            int updatedMonth = 2;
            int updatedYear = 2025;

            UpdateFixedExpenseRequestDTO request = new UpdateFixedExpenseRequestDTO { Name = updatedName, Description = updatedDescription, Amount = updatedAmount, Month = updatedMonth, Year = updatedYear };

            User user = TestDataFactory.CreateUser(userId);
            Category category = TestDataFactory.CreateCategory(categoryId, user, "Suscripciones");
            FixedExpense existingFixedExpense = TestDataFactory.CreateFixedExpense(fixedExpenseId, user, category);

            TestDataFactory.SetupAuthenticatedUser(_currentUserServiceMock, userId);

            _fixedExpenseRepositoryMock
                .Setup(repo => repo.GetByIdAsync(fixedExpenseId, userId))
                .ReturnsAsync(existingFixedExpense);

            // Act
            FixedExpenseResponseDTO result = await _fixedExpenseService.UpdateAsync(fixedExpenseId, request);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(fixedExpenseId, result.Id);
            Assert.Equal(updatedName, result.Name);
            Assert.Equal(updatedDescription, result.Description);
            Assert.Equal(updatedAmount, result.Amount);
            Assert.Equal(updatedMonth, result.Month);
            Assert.Equal(updatedYear, result.Year);

            _fixedExpenseRepositoryMock.Verify(repo => repo.UpdateAsync(It.IsAny<FixedExpense>()), Times.Once);
        }

        [Fact]
        public async Task UpdateAsync_WithNonExistingId_ThrowsKeyNotFoundException()
        {
            // Arrange
            int userId = 1;
            int fixedExpenseId = 999;
            UpdateFixedExpenseRequestDTO request = new UpdateFixedExpenseRequestDTO { };

            TestDataFactory.SetupAuthenticatedUser(_currentUserServiceMock, userId);

            _fixedExpenseRepositoryMock
                .Setup(repo => repo.GetByIdAsync(fixedExpenseId, userId))
                .ReturnsAsync((FixedExpense?)null);

            // Act & Assert
            await Assert.ThrowsAsync<KeyNotFoundException>(() => _fixedExpenseService.UpdateAsync(fixedExpenseId, request));

            _fixedExpenseRepositoryMock.Verify(repo => repo.UpdateAsync(It.IsAny<FixedExpense>()), Times.Never);
        }

        // ==================== TEST: DELETE ====================

        [Fact]
        public async Task DeleteAsync_WithExistingId_DeletesFixedExpense()
        {
            // Arrange
            int userId = 1;
            int fixedExpenseId = 1;

            TestDataFactory.SetupAuthenticatedUser(_currentUserServiceMock, userId);

            _fixedExpenseRepositoryMock
                .Setup(repo => repo.ExistsAsync(fixedExpenseId, userId))
                .ReturnsAsync(true);

            // Act
            await _fixedExpenseService.DeleteAsync(fixedExpenseId);

            // Assert
            _fixedExpenseRepositoryMock.Verify(repo => repo.DeleteAsync(fixedExpenseId, userId), Times.Once);
        }

        [Fact]
        public async Task DeleteAsync_WithNonExistingId_ThrowsKeyNotFoundException()
        {
            // Arrange
            int userId = 1;
            int fixedExpenseId = 999;

            TestDataFactory.SetupAuthenticatedUser(_currentUserServiceMock, userId);

            _fixedExpenseRepositoryMock
                .Setup(repo => repo.ExistsAsync(fixedExpenseId, userId))
                .ReturnsAsync(false);

            // Act & Assert
            await Assert.ThrowsAsync<KeyNotFoundException>(() => _fixedExpenseService.DeleteAsync(fixedExpenseId));

            _fixedExpenseRepositoryMock.Verify(repo => repo.DeleteAsync(It.IsAny<int>(), userId), Times.Never);
        }

        // ==================== TEST: ACTIVATE ====================

        [Fact]
        public async Task ActivateAsync_WithExistingId_ActivatesFixedExpense()
        {
            // Arrange
            int userId = 1;
            int fixedExpenseId = 1;

            TestDataFactory.SetupAuthenticatedUser(_currentUserServiceMock, userId);

            _fixedExpenseRepositoryMock
                .Setup(repo => repo.ExistsAsync(fixedExpenseId, userId))
                .ReturnsAsync(true);

            // Act
            await _fixedExpenseService.ActivateAsync(fixedExpenseId);

            // Assert
            _fixedExpenseRepositoryMock.Verify(repo => repo.ActivateAsync(fixedExpenseId,userId), Times.Once);
        }

        [Fact]
        public async Task ActivateAsync_WithNonExistingId_ThrowsKeyNotFoundException()
        {
            // Arrange
            int userId = 1;
            int fixedExpenseId = 999;

            TestDataFactory.SetupAuthenticatedUser(_currentUserServiceMock, userId);

            _fixedExpenseRepositoryMock
                .Setup(repo => repo.ExistsAsync(fixedExpenseId,userId))
                .ReturnsAsync(false);

            // Act & Assert
            await Assert.ThrowsAsync<KeyNotFoundException>(() => _fixedExpenseService.ActivateAsync(fixedExpenseId));

            _fixedExpenseRepositoryMock.Verify(repo => repo.ActivateAsync(It.IsAny<int>(), userId), Times.Never);
        }

        // ==================== TEST: DEACTIVATE ====================

        [Fact]
        public async Task DeactivateAsync_WithExistingId_DeactivatesFixedExpense()
        {
            // Arrange
            int userId = 1;
            int fixedExpenseId = 1;

            TestDataFactory.SetupAuthenticatedUser(_currentUserServiceMock, userId);

            _fixedExpenseRepositoryMock
                .Setup(repo => repo.ExistsAsync(fixedExpenseId, userId))
                .ReturnsAsync(true);

            // Act
            await _fixedExpenseService.DeactivateAsync(fixedExpenseId);

            // Assert
            _fixedExpenseRepositoryMock.Verify(repo => repo.DeactivateAsync(fixedExpenseId, userId), Times.Once);
        }

        [Fact]
        public async Task DeactivateAsync_WithNonExistingId_ThrowsKeyNotFoundException()
        {
            // Arrange
            int userId = 1;
            int fixedExpenseId = 999;

            TestDataFactory.SetupAuthenticatedUser(_currentUserServiceMock, userId);

            _fixedExpenseRepositoryMock
                .Setup(repo => repo.ExistsAsync(fixedExpenseId, userId))
                .ReturnsAsync(false);

            // Act & Assert
            await Assert.ThrowsAsync<KeyNotFoundException>(() => _fixedExpenseService.DeactivateAsync(fixedExpenseId));

            _fixedExpenseRepositoryMock.Verify(repo => repo.DeactivateAsync(It.IsAny<int>(), userId), Times.Never);
        }

        // ==================== TEST: GET TOTAL FOR PERIOD ====================

        [Fact]
        public async Task GetTotalForPeriodByCategoryAsync_WithValidData_ReturnsTotal()
        {
            // Arrange
            int userId = 1;
            int categoryId = 1;
            decimal expectedTotal = 25.98m;

            TestDataFactory.SetupAuthenticatedUser(_currentUserServiceMock, userId);

            _categoryRepositoryMock
                .Setup(repo => repo.ExistsAsync(categoryId, userId))
                .ReturnsAsync(true);

            _fixedExpenseRepositoryMock
                .Setup(repo => repo.GetTotalByCategoryAndPeriodAsync(categoryId, It.IsAny<MonthlyPeriod>(), userId))
                .ReturnsAsync(expectedTotal);

            // Act
            decimal result = await _fixedExpenseService.GetTotalForPeriodByCategoryAsync(categoryId, TestDataFactory.DEFAULT_MONTHLY_MONTH, TestDataFactory.DEFAULT_YEAR);

            // Assert
            Assert.Equal(expectedTotal, result);
            _fixedExpenseRepositoryMock.Verify(repo => repo.GetTotalByCategoryAndPeriodAsync(categoryId, It.IsAny<MonthlyPeriod>(), userId), Times.Once);
        }

        [Fact]
        public async Task GetTotalForPeriodByCategoryAsync_WithNonExistingCategory_ThrowsKeyNotFoundException()
        {
            // Arrange
            int userId = 1;
            int categoryId = 999;

            TestDataFactory.SetupAuthenticatedUser(_currentUserServiceMock, userId);

            _categoryRepositoryMock
                .Setup(repo => repo.ExistsAsync(categoryId, userId))
                .ReturnsAsync(false);

            // Act & Assert
            await Assert.ThrowsAsync<KeyNotFoundException>(() => _fixedExpenseService.GetTotalForPeriodByCategoryAsync(categoryId, TestDataFactory.DEFAULT_MONTHLY_MONTH, TestDataFactory.DEFAULT_YEAR));

            _fixedExpenseRepositoryMock.Verify(repo => repo.GetTotalByCategoryAndPeriodAsync(It.IsAny<int>(), It.IsAny<MonthlyPeriod>(), userId), Times.Never);
        }

        // ==================== TEST: EXISTS ====================

        [Fact]
        public async Task ExistsAsync_WithExistingId_ReturnsTrue()
        {
            // Arrange
            int userId = 1;
            int fixedExpenseId = 1;

            TestDataFactory.SetupAuthenticatedUser(_currentUserServiceMock, userId);

            _fixedExpenseRepositoryMock
                .Setup(repo => repo.ExistsAsync(fixedExpenseId, userId))
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
            int userId = 1;
            int fixedExpenseId = 999;

            TestDataFactory.SetupAuthenticatedUser(_currentUserServiceMock, userId);

            _fixedExpenseRepositoryMock
                .Setup(repo => repo.ExistsAsync(fixedExpenseId, userId))
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
            int userId = 1;
            int invalidId = 0;

            TestDataFactory.SetupAuthenticatedUser(_currentUserServiceMock, userId);

            // Act
            bool result = await _fixedExpenseService.ExistsAsync(invalidId);

            // Assert
            Assert.False(result);
            _fixedExpenseRepositoryMock.Verify(repo => repo.ExistsAsync(It.IsAny<int>(), userId), Times.Never);
        }

        // ==================== TEST: IS ACTIVE ====================

        [Fact]
        public async Task IsActiveAsync_WithExistingId_ReturnsTrue()
        {
            // Arrange
            int userId = 1;
            int fixedExpenseId = 1;

            TestDataFactory.SetupAuthenticatedUser(_currentUserServiceMock, userId);

            _fixedExpenseRepositoryMock
                .Setup(repo => repo.ExistsAsync(fixedExpenseId, userId))
                .ReturnsAsync(true);

            _fixedExpenseRepositoryMock
                .Setup(repo => repo.IsActiveAsync(fixedExpenseId, userId))
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
            int userId = 1;
            int fixedExpenseId = 999;

            TestDataFactory.SetupAuthenticatedUser(_currentUserServiceMock, userId);

            _fixedExpenseRepositoryMock
                .Setup(repo => repo.ExistsAsync(fixedExpenseId, userId))
                .ReturnsAsync(false);

            // Act & Assert
            await Assert.ThrowsAsync<KeyNotFoundException>(() => _fixedExpenseService.IsActiveAsync(fixedExpenseId));

            _fixedExpenseRepositoryMock.Verify(repo => repo.IsActiveAsync(It.IsAny<int>(), userId), Times.Never);
        }

        [Fact]
        public async Task IsActiveAsync_WithInvalidId_ReturnsFalse()
        {
            // Arrange
            int userId = 1;
            int invalidId = 0;

            TestDataFactory.SetupAuthenticatedUser(_currentUserServiceMock, userId);

            // Act
            bool result = await _fixedExpenseService.IsActiveAsync(invalidId);

            // Assert
            Assert.False(result);
            _fixedExpenseRepositoryMock.Verify(repo => repo.ExistsAsync(It.IsAny<int>(), userId), Times.Never);
        }

        public void Dispose()
        {
            // Limpiar recursos si es necesario
        }
    }
}