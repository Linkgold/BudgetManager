using Application.DTOs.Request;
using Application.DTOs.Response;
using Application.Interfaces;
using Application.Mappings;
using Application.Services;
using AutoMapper;
using Domain.Entities;
using Domain.Enums;
using Domain.Exceptions;
using Domain.Interfaces;
using Domain.ValueObjects;
using Microsoft.Extensions.Logging;
using Moq;
using Tests.Helpers;

namespace Tests.Application
{
    public class BudgetServiceTests : IDisposable
    {
        private readonly Mock<IUserRepository> _userRepositoryMock;
        private readonly Mock<ICurrentUserService> _currentUserServiceMock;
        private readonly Mock<IBudgetRepository> _budgetRepositoryMock;
        private readonly Mock<ICategoryRepository> _categoryRepositoryMock;
        private readonly IMapper _mapper;
        private readonly IBudgetService _budgetService;

        public BudgetServiceTests()
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
            _budgetRepositoryMock = new Mock<IBudgetRepository>();
            _categoryRepositoryMock = new Mock<ICategoryRepository>();

            // Instanciar el servicio
            _budgetService = new BudgetService(_currentUserServiceMock.Object, _budgetRepositoryMock.Object, _categoryRepositoryMock.Object, _userRepositoryMock.Object, _mapper);
        }

        // ==================== TEST: GET BY ID ====================

        [Fact]
        public async Task GetByIdAsync_WithExistingId_ReturnsBudget()
        {
            // Arrange
            int userId = 1;
            int budgetId = 1;
            Category category = TestDataFactory.CreateCategory(1);
            Budget budget = TestDataFactory.CreateBudget(budgetId);

            TestDataFactory.SetupAuthenticatedUser(_currentUserServiceMock, userId);

            _budgetRepositoryMock
                .Setup(repo => repo.GetByIdAsync(budgetId, userId))
                .ReturnsAsync(budget);

            // Act
            BudgetResponseDTO result = await _budgetService.GetByIdAsync(budgetId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(budgetId, result.Id);
            Assert.Equal(500.00m, result.Amount);
            Assert.Equal("Alimentación", result.CategoryName);
            Assert.Equal(2024, result.Year);
            Assert.Equal(1, result.Month);
        }

        [Fact]
        public async Task GetByIdAsync_WithNonExistingId_ThrowsKeyNotFoundException()
        {
            // Arrange
            int userId = 1;
            int budgetId = 999;

            TestDataFactory.SetupAuthenticatedUser(_currentUserServiceMock, userId);

            _budgetRepositoryMock
                .Setup(repo => repo.GetByIdAsync(budgetId, userId))
                .ReturnsAsync((Budget)null);

            // Act & Assert
            await Assert.ThrowsAsync<KeyNotFoundException>(() => _budgetService.GetByIdAsync(budgetId));
        }

        [Fact]
        public async Task GetByIdAsync_WithInvalidId_ThrowsArgumentException()
        {
            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() => _budgetService.GetByIdAsync(0));
        }

        // ==================== TEST: GET ALL ====================

        [Fact]
        public async Task GetAllAsync_ReturnsListOfBudgets()
        {
            // Arrange
            int userId = 1;
            Budget budget1 = TestDataFactory.CreateBudget(1);
            Budget budget2 = TestDataFactory.CreateBudget(2);
            List<Budget> budgets = new List<Budget> { budget1, budget2 };

            TestDataFactory.SetupAuthenticatedUser(_currentUserServiceMock, userId);

            _budgetRepositoryMock
                .Setup(repo => repo.GetAllAsync(userId))
                .ReturnsAsync(budgets);

            // Act
            List<BudgetResponseDTO> result = await _budgetService.GetAllAsync();

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count);
            Assert.Equal(500.00m, result[0].Amount);
            Assert.Equal(300.00m, result[1].Amount);
        }

        // ==================== TEST: GET BY CATEGORY ====================

        [Fact]
        public async Task GetByCategoryIdAsync_WithExistingCategory_ReturnsBudgets()
        {
            // Arrange
            int userId = 1;
            int categoryId = 1;
            Category category = TestDataFactory.CreateCategory(categoryId);
            Budget budget1 = TestDataFactory.CreateBudget(1);
            Budget budget2 = TestDataFactory.CreateBudget(2);
            List<Budget> budgets = new List<Budget> { budget1, budget2 };

            TestDataFactory.SetupAuthenticatedUser(_currentUserServiceMock, userId);

            _categoryRepositoryMock
                .Setup(repo => repo.ExistsAsync(categoryId, userId))
                .ReturnsAsync(true);

            _budgetRepositoryMock
                .Setup(repo => repo.GetByCategoryIdAsync(categoryId, userId))
                .ReturnsAsync(budgets);

            // Act
            List<BudgetResponseDTO> result = await _budgetService.GetByCategoryIdAsync(categoryId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count);
            _budgetRepositoryMock.Verify(repo => repo.GetByCategoryIdAsync(categoryId, userId), Times.Once);
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
            await Assert.ThrowsAsync<KeyNotFoundException>(() => _budgetService.GetByCategoryIdAsync(categoryId));
        }

        // ==================== TEST: GET BY PERIOD ====================

        [Fact]
        public async Task GetByPeriodAsync_ReturnsBudgetsForPeriod()
        {
            // Arrange
            int userId = 1;
            int year = 2024;
            int month = 1;
            Budget budget1 = TestDataFactory.CreateBudget(1);
            Budget budget2 = TestDataFactory.CreateBudget(2);
            List<Budget> budgets = new List<Budget> { budget1, budget2 };

            TestDataFactory.SetupAuthenticatedUser(_currentUserServiceMock, userId);

            _budgetRepositoryMock
                .Setup(repo => repo.GetByPeriodAsync(It.IsAny<MonthlyPeriod>(), userId))
                .ReturnsAsync(budgets);

            // Act
            List<BudgetResponseDTO> result = await _budgetService.GetByPeriodAsync(month, year);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count);
        }

        // ==================== TEST: GET BY CATEGORY AND PERIOD ====================

        [Fact]
        public async Task GetByCategoryAndPeriodAsync_WithExistingBudget_ReturnsBudget()
        {
            // Arrange
            int userId = 1;
            int categoryId = 1;
            int year = 2024;
            int month = 1;
            Category category = TestDataFactory.CreateCategory(categoryId);
            Budget budget = TestDataFactory.CreateBudget(1);

            TestDataFactory.SetupAuthenticatedUser(_currentUserServiceMock, userId);

            _budgetRepositoryMock
                .Setup(repo => repo.GetByCategoryAndPeriodAsync(categoryId, It.IsAny<MonthlyPeriod>(), userId))
                .ReturnsAsync(budget);

            // Act
            BudgetResponseDTO result = await _budgetService.GetByCategoryAndPeriodAsync(categoryId, month, year);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(500.00m, result.Amount);
            Assert.Equal(2024, result.Year);
            Assert.Equal(1, result.Month);
        }

        [Fact]
        public async Task GetByCategoryAndPeriodAsync_WithNonExistingBudget_ThrowsKeyNotFoundException()
        {
            // Arrange
            int userId = 1;
            int categoryId = 1;
            int year = 2024;
            int month = 1;

            TestDataFactory.SetupAuthenticatedUser(_currentUserServiceMock, userId);

            _budgetRepositoryMock
                .Setup(repo => repo.GetByCategoryAndPeriodAsync(categoryId, It.IsAny<MonthlyPeriod>(), userId))
                .ReturnsAsync((Budget)null);

            // Act & Assert
            await Assert.ThrowsAsync<KeyNotFoundException>(() => _budgetService.GetByCategoryAndPeriodAsync(categoryId, month, year));
        }

        // ==================== TEST: GET SUMMARY ====================

        [Fact]
        public async Task GetSummaryByCategoryAndPeriodAsync_WithExistingBudget_ReturnsSummary()
        {
            // Arrange
            int userId = 1;
            int categoryId = 1;
            int year = 2024;
            int month = 1;
            Category category = TestDataFactory.CreateCategory(categoryId);
            Budget budget = TestDataFactory.CreateBudget(1);

            TestDataFactory.SetupAuthenticatedUser(_currentUserServiceMock, userId);

            _budgetRepositoryMock
                .Setup(repo => repo.GetByCategoryAndPeriodAsync(categoryId, It.IsAny<MonthlyPeriod>(), userId))
                .ReturnsAsync(budget);

            // Act
            BudgetSummaryDTO result = await _budgetService.GetSummaryByCategoryAndPeriodAsync(categoryId, month, year);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(1, result.BudgetId);
            Assert.Equal(500.00m, result.BudgetAmount);
            Assert.Equal(0, result.TotalSpent); // Por ahora siempre 0
            Assert.Equal(500.00m, result.Remaining);
            Assert.Equal(0, result.PercentageUsed);
            Assert.Equal(BudgetStatusEnum.Green, result.Status);
            Assert.False(result.IsOverBudget);
        }

        [Fact]
        public async Task GetSummaryByCategoryAndPeriodAsync_WithNonExistingBudget_ThrowsKeyNotFoundException()
        {
            // Arrange
            int userId = 1;
            int categoryId = 1;
            int year = 2024;
            int month = 1;

            TestDataFactory.SetupAuthenticatedUser(_currentUserServiceMock, userId);

            _budgetRepositoryMock
                .Setup(repo => repo.GetByCategoryAndPeriodAsync(categoryId, It.IsAny<MonthlyPeriod>(), userId))
                .ReturnsAsync((Budget)null);

            // Act & Assert
            await Assert.ThrowsAsync<KeyNotFoundException>(() => _budgetService.GetSummaryByCategoryAndPeriodAsync(categoryId, month, year));
        }

        // ==================== TEST: CREATE ====================

        [Fact]
        public async Task CreateAsync_WithValidData_ReturnsCreatedBudget()
        {
            // Arrange
            int userId = 1;
            int categoryId = 1;

            CreateBudgetRequestDTO request = new CreateBudgetRequestDTO
            {
                CategoryId = categoryId,
                Amount = 500.00m,
                Month = 1,
                Year = 2024
            };

            Category category = TestDataFactory.CreateCategory(categoryId);

            TestDataFactory.SetupAuthenticatedUser(_currentUserServiceMock, userId);

            _categoryRepositoryMock
                .Setup(repo => repo.GetByIdAsync(categoryId, userId, It.IsAny<bool>()))
                .ReturnsAsync(category);

            _budgetRepositoryMock
                .Setup(repo => repo.ExistsForCategoryAndPeriodAsync(categoryId, It.IsAny<MonthlyPeriod>(), userId))
                .ReturnsAsync(false);

            // Act
            BudgetResponseDTO result = await _budgetService.CreateAsync(request);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(500.00m, result.Amount);
            Assert.Equal(1, result.Month);
            Assert.Equal(2024, result.Year);
            Assert.Equal("Alimentación", result.CategoryName);

            _budgetRepositoryMock.Verify(repo => repo.AddAsync(It.IsAny<Budget>()), Times.Once);
        }

        [Fact]
        public async Task CreateAsync_WithNonExistingCategory_ThrowsKeyNotFoundException()
        {
            // Arrange
            int userId = 1;
            int categoryId = 999;

            CreateBudgetRequestDTO request = new CreateBudgetRequestDTO { CategoryId = categoryId, Amount = 500.00m, Month = 1, Year = 2024 };

            TestDataFactory.SetupAuthenticatedUser(_currentUserServiceMock, userId);

            _categoryRepositoryMock
                .Setup(repo => repo.GetByIdAsync(categoryId, userId))
                .ReturnsAsync((Category)null);

            // Act & Assert
            await Assert.ThrowsAsync<KeyNotFoundException>(() => _budgetService.CreateAsync(request));

            _budgetRepositoryMock.Verify(repo => repo.AddAsync(It.IsAny<Budget>()), Times.Never);
        }

        [Fact]
        public async Task CreateAsync_WithDuplicateBudget_ThrowsInvalidOperationException()
        {
            // Arrange
            int userId = 1;
            int categoryId = 1;

            CreateBudgetRequestDTO request = new CreateBudgetRequestDTO { CategoryId = categoryId, Amount = 500.00m, Month = 1, Year = 2024 };

            Category category = TestDataFactory.CreateCategory(categoryId);

            TestDataFactory.SetupAuthenticatedUser(_currentUserServiceMock, userId);

            _categoryRepositoryMock
                .Setup(repo => repo.GetByIdAsync(categoryId, userId, It.IsAny<bool>()))
                .ReturnsAsync(category);

            _budgetRepositoryMock
                .Setup(repo => repo.ExistsForCategoryAndPeriodAsync(categoryId, It.IsAny<MonthlyPeriod>(), userId))
                .ReturnsAsync(true);

            // Act & Assert
            await Assert.ThrowsAsync<ConflictException>(() => _budgetService.CreateAsync(request));

            _budgetRepositoryMock.Verify(repo => repo.AddAsync(It.IsAny<Budget>()), Times.Never);
        }

        // ==================== TEST: UPDATE ====================

        [Fact]
        public async Task UpdateAsync_WithValidData_ReturnsUpdatedBudget()
        {
            // Arrange
            int userId = 1;
            int budgetId = 1;

            UpdateBudgetRequestDTO request = new UpdateBudgetRequestDTO { Amount = 600.00m };

            Budget budget = TestDataFactory.CreateBudget(budgetId);

            TestDataFactory.SetupAuthenticatedUser(_currentUserServiceMock, userId);

            _budgetRepositoryMock
                .Setup(repo => repo.GetByIdAsync(budgetId, userId))
                .ReturnsAsync(budget);

            // Act
            BudgetResponseDTO result = await _budgetService.UpdateAsync(budgetId, request);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(budgetId, result.Id);
            Assert.Equal(600.00m, result.Amount);

            _budgetRepositoryMock.Verify(repo => repo.UpdateAsync(It.IsAny<Budget>()), Times.Once);
        }

        [Fact]
        public async Task UpdateAsync_WithNonExistingId_ThrowsKeyNotFoundException()
        {
            // Arrange
            int userId = 1;
            int budgetId = 999;

            UpdateBudgetRequestDTO request = new UpdateBudgetRequestDTO { Amount = 600.00m };

            TestDataFactory.SetupAuthenticatedUser(_currentUserServiceMock, userId);

            _budgetRepositoryMock
                .Setup(repo => repo.GetByIdAsync(budgetId, userId))
                .ReturnsAsync((Budget)null);

            // Act & Assert
            await Assert.ThrowsAsync<KeyNotFoundException>(() => _budgetService.UpdateAsync(budgetId, request));

            _budgetRepositoryMock.Verify(repo => repo.UpdateAsync(It.IsAny<Budget>()), Times.Never);
        }

        // ==================== TEST: DELETE ====================

        [Fact]
        public async Task DeleteAsync_WithExistingId_DeletesBudget()
        {
            // Arrange
            int userId = 1;
            int budgetId = 1;

            TestDataFactory.SetupAuthenticatedUser(_currentUserServiceMock, userId);

            _budgetRepositoryMock
                .Setup(repo => repo.ExistsAsync(budgetId, userId))
                .ReturnsAsync(true);

            // Act
            await _budgetService.DeleteAsync(budgetId);

            // Assert
            _budgetRepositoryMock.Verify(repo => repo.DeleteAsync(budgetId, userId), Times.Once);
        }

        [Fact]
        public async Task DeleteAsync_WithNonExistingId_ThrowsKeyNotFoundException()
        {
            // Arrange
            int userId = 1;
            int budgetId = 999;

            TestDataFactory.SetupAuthenticatedUser(_currentUserServiceMock, userId);

            _budgetRepositoryMock
                .Setup(repo => repo.ExistsAsync(budgetId, userId))
                .ReturnsAsync(false);

            // Act & Assert
            await Assert.ThrowsAsync<KeyNotFoundException>(() => _budgetService.DeleteAsync(budgetId));

            _budgetRepositoryMock.Verify(repo => repo.DeleteAsync(It.IsAny<int>(), userId), Times.Never);
        }

        // ==================== TEST: EXISTS ====================

        [Fact]
        public async Task ExistsAsync_WithExistingId_ReturnsTrue()
        {
            // Arrange
            int userId = 1;
            int budgetId = 1;

            TestDataFactory.SetupAuthenticatedUser(_currentUserServiceMock, userId);

            _budgetRepositoryMock
                .Setup(repo => repo.ExistsAsync(budgetId, userId))
                .ReturnsAsync(true);

            // Act
            bool result = await _budgetService.ExistsAsync(budgetId);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public async Task ExistsAsync_WithNonExistingId_ReturnsFalse()
        {
            // Arrange
            int userId = 1;
            int budgetId = 999;

            TestDataFactory.SetupAuthenticatedUser(_currentUserServiceMock, userId);

            _budgetRepositoryMock
                .Setup(repo => repo.ExistsAsync(budgetId, userId))
                .ReturnsAsync(false);

            // Act
            bool result = await _budgetService.ExistsAsync(budgetId);

            // Assert
            Assert.False(result);
        }

        // ==================== TEST: EXISTS FOR CATEGORY AND PERIOD ====================

        [Fact]
        public async Task ExistsForCategoryAndPeriodAsync_WithExistingBudget_ReturnsTrue()
        {
            // Arrange
            int userId = 1;
            int categoryId = 1;
            int month = 1;
            int year = 2024;

            TestDataFactory.SetupAuthenticatedUser(_currentUserServiceMock, userId);

            _budgetRepositoryMock
                .Setup(repo => repo.ExistsForCategoryAndPeriodAsync(categoryId, It.IsAny<MonthlyPeriod>(), userId))
                .ReturnsAsync(true);

            // Act
            bool result = await _budgetService.ExistsForCategoryAndPeriodAsync(categoryId, month, year);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public async Task ExistsForCategoryAndPeriodAsync_WithNonExistingBudget_ReturnsFalse()
        {
            // Arrange
            int userId = 1;
            int categoryId = 1;
            int year = 2024;
            int month = 1;

            TestDataFactory.SetupAuthenticatedUser(_currentUserServiceMock, userId);

            _budgetRepositoryMock
                .Setup(repo => repo.ExistsForCategoryAndPeriodAsync(categoryId, It.IsAny<MonthlyPeriod>(), userId))
                .ReturnsAsync(false);

            // Act
            bool result = await _budgetService.ExistsForCategoryAndPeriodAsync(categoryId, month, year);

            // Assert
            Assert.False(result);
        }

        // ==================== DISPOSE ====================

        public void Dispose()
        {
            // Limpiar recursos si es necesario
        }
    }
}