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
using System.Data;

namespace Tests.Application
{
    public class BudgetServiceTests : IDisposable
    {
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
            _budgetRepositoryMock = new Mock<IBudgetRepository>();
            _categoryRepositoryMock = new Mock<ICategoryRepository>();

            // Instanciar el servicio
            _budgetService = new BudgetService(_budgetRepositoryMock.Object, _categoryRepositoryMock.Object, _mapper);
        }

        // ==================== HELPERS ====================

        private Category CreateCategory(int id, string name)
        {
            EntityInfo info = new EntityInfo(name, null);
            Category category = new Category(info);
            typeof(Category).GetProperty("Id")?.SetValue(category, id);

            return category;
        }

        private Budget CreateBudget(int id, Category category, decimal amount, int month, int year)
        {
            Money money = new Money(amount);
            Period period = new Period(month, year);
            Budget budget = new Budget(category, money, period);
            typeof(Budget).GetProperty("Id")?.SetValue(budget, id);

            return budget;
        }

        // ==================== TEST: GET BY ID ====================

        [Fact]
        public async Task GetByIdAsync_WithExistingId_ReturnsBudget()
        {
            // Arrange
            int budgetId = 1;
            Category category = CreateCategory(1, "Alimentación");
            Budget budget = CreateBudget(budgetId, category, 500.00m, 1, 2024);

            _budgetRepositoryMock
                .Setup(repo => repo.GetByIdAsync(budgetId))
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
            int budgetId = 999;

            _budgetRepositoryMock
                .Setup(repo => repo.GetByIdAsync(budgetId))
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
            Category category = CreateCategory(1, "Alimentación");
            Budget budget1 = CreateBudget(1, category, 500.00m, 1, 2024);
            Budget budget2 = CreateBudget(2, category, 300.00m, 2, 2024);
            List<Budget> budgets = new List<Budget> { budget1, budget2 };

            _budgetRepositoryMock
                .Setup(repo => repo.GetAllAsync())
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
            int categoryId = 1;
            Category category = CreateCategory(categoryId, "Alimentación");
            Budget budget1 = CreateBudget(1, category, 500.00m, 1, 2024);
            Budget budget2 = CreateBudget(2, category, 300.00m, 2, 2024);
            List<Budget> budgets = new List<Budget> { budget1, budget2 };

            _categoryRepositoryMock
                .Setup(repo => repo.ExistsAsync(categoryId))
                .ReturnsAsync(true);

            _budgetRepositoryMock
                .Setup(repo => repo.GetByCategoryIdAsync(categoryId))
                .ReturnsAsync(budgets);

            // Act
            List<BudgetResponseDTO> result = await _budgetService.GetByCategoryIdAsync(categoryId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count);
            _budgetRepositoryMock.Verify(repo => repo.GetByCategoryIdAsync(categoryId), Times.Once);
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
            await Assert.ThrowsAsync<KeyNotFoundException>(() => _budgetService.GetByCategoryIdAsync(categoryId));
        }

        // ==================== TEST: GET BY PERIOD ====================

        [Fact]
        public async Task GetByPeriodAsync_ReturnsBudgetsForPeriod()
        {
            // Arrange
            int year = 2024;
            int month = 1;
            Category category = CreateCategory(1, "Alimentación");
            Budget budget1 = CreateBudget(1, category, 500.00m, month, year);
            Budget budget2 = CreateBudget(2, category, 300.00m, month, year);
            List<Budget> budgets = new List<Budget> { budget1, budget2 };

            _budgetRepositoryMock
                .Setup(repo => repo.GetByPeriodAsync(It.IsAny<Period>()))
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
            int categoryId = 1;
            int year = 2024;
            int month = 1;
            Category category = CreateCategory(categoryId, "Alimentación");
            Budget budget = CreateBudget(1, category, 500.00m, month, year);

            _budgetRepositoryMock
                .Setup(repo => repo.GetByCategoryAndPeriodAsync(categoryId, It.IsAny<Period>()))
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
            int categoryId = 1;
            int year = 2024;
            int month = 1;

            _budgetRepositoryMock
                .Setup(repo => repo.GetByCategoryAndPeriodAsync(categoryId, It.IsAny<Period>()))
                .ReturnsAsync((Budget)null);

            // Act & Assert
            await Assert.ThrowsAsync<KeyNotFoundException>(() => _budgetService.GetByCategoryAndPeriodAsync(categoryId, month, year));
        }

        // ==================== TEST: GET SUMMARY ====================

        [Fact]
        public async Task GetSummaryByCategoryAndPeriodAsync_WithExistingBudget_ReturnsSummary()
        {
            // Arrange
            int categoryId = 1;
            int year = 2024;
            int month = 1;
            Category category = CreateCategory(categoryId, "Alimentación");
            Budget budget = CreateBudget(1, category, 500.00m, month, year);

            _budgetRepositoryMock
                .Setup(repo => repo.GetByCategoryAndPeriodAsync(categoryId, It.IsAny<Period>()))
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
            int categoryId = 1;
            int year = 2024;
            int month = 1;

            _budgetRepositoryMock
                .Setup(repo => repo.GetByCategoryAndPeriodAsync(categoryId, It.IsAny<Period>()))
                .ReturnsAsync((Budget)null);

            // Act & Assert
            await Assert.ThrowsAsync<KeyNotFoundException>(() => _budgetService.GetSummaryByCategoryAndPeriodAsync(categoryId, month, year));
        }

        // ==================== TEST: CREATE ====================

        [Fact]
        public async Task CreateAsync_WithValidData_ReturnsCreatedBudget()
        {
            // Arrange
            int categoryId = 1;
            CreateBudgetRequestDTO request = new CreateBudgetRequestDTO
            {
                CategoryId = categoryId,
                Amount = 500.00m,
                Month = 1,
                Year = 2024
            };

            Category category = CreateCategory(categoryId, "Alimentación");

            _categoryRepositoryMock
                .Setup(repo => repo.GetByIdAsync(categoryId, It.IsAny<bool>()))
                .ReturnsAsync(category);

            _budgetRepositoryMock
                .Setup(repo => repo.ExistsForCategoryAndPeriodAsync(categoryId, It.IsAny<Period>()))
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
            int categoryId = 999;
            CreateBudgetRequestDTO request = new CreateBudgetRequestDTO { CategoryId = categoryId, Amount = 500.00m, Month = 1, Year = 2024 };

            _categoryRepositoryMock
                .Setup(repo => repo.GetByIdAsync(categoryId))
                .ReturnsAsync((Category)null);

            // Act & Assert
            await Assert.ThrowsAsync<KeyNotFoundException>(() => _budgetService.CreateAsync(request));

            _budgetRepositoryMock.Verify(repo => repo.AddAsync(It.IsAny<Budget>()), Times.Never);
        }

        [Fact]
        public async Task CreateAsync_WithDuplicateBudget_ThrowsInvalidOperationException()
        {
            // Arrange
            int categoryId = 1;
            CreateBudgetRequestDTO request = new CreateBudgetRequestDTO { CategoryId = categoryId, Amount = 500.00m, Month = 1, Year = 2024 };

            Category category = CreateCategory(categoryId, "Alimentación");

            _categoryRepositoryMock
                .Setup(repo => repo.GetByIdAsync(categoryId, It.IsAny<bool>()))
                .ReturnsAsync(category);

            _budgetRepositoryMock
                .Setup(repo => repo.ExistsForCategoryAndPeriodAsync(categoryId, It.IsAny<Period>()))
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
            int budgetId = 1;
            UpdateBudgetRequestDTO request = new UpdateBudgetRequestDTO { Amount = 600.00m };

            Category category = CreateCategory(1, "Alimentación");
            Budget budget = CreateBudget(budgetId, category, 500.00m, 1, 2024);

            _budgetRepositoryMock
                .Setup(repo => repo.GetByIdAsync(budgetId))
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
            int budgetId = 999;
            UpdateBudgetRequestDTO request = new UpdateBudgetRequestDTO { Amount = 600.00m };

            _budgetRepositoryMock
                .Setup(repo => repo.GetByIdAsync(budgetId))
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
            int budgetId = 1;

            _budgetRepositoryMock
                .Setup(repo => repo.ExistsAsync(budgetId))
                .ReturnsAsync(true);

            // Act
            await _budgetService.DeleteAsync(budgetId);

            // Assert
            _budgetRepositoryMock.Verify(repo => repo.DeleteAsync(budgetId), Times.Once);
        }

        [Fact]
        public async Task DeleteAsync_WithNonExistingId_ThrowsKeyNotFoundException()
        {
            // Arrange
            int budgetId = 999;

            _budgetRepositoryMock
                .Setup(repo => repo.ExistsAsync(budgetId))
                .ReturnsAsync(false);

            // Act & Assert
            await Assert.ThrowsAsync<KeyNotFoundException>(() => _budgetService.DeleteAsync(budgetId));

            _budgetRepositoryMock.Verify(repo => repo.DeleteAsync(It.IsAny<int>()), Times.Never);
        }

        // ==================== TEST: EXISTS ====================

        [Fact]
        public async Task ExistsAsync_WithExistingId_ReturnsTrue()
        {
            // Arrange
            int budgetId = 1;

            _budgetRepositoryMock
                .Setup(repo => repo.ExistsAsync(budgetId))
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
            int budgetId = 999;

            _budgetRepositoryMock
                .Setup(repo => repo.ExistsAsync(budgetId))
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
            int categoryId = 1;
            int month = 1;
            int year = 2024;

            _budgetRepositoryMock
                .Setup(repo => repo.ExistsForCategoryAndPeriodAsync(categoryId, It.IsAny<Period>()))
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
            int categoryId = 1;
            int year = 2024;
            int month = 1;

            _budgetRepositoryMock
                .Setup(repo => repo.ExistsForCategoryAndPeriodAsync(categoryId, It.IsAny<Period>()))
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