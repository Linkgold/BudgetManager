using Shared.DTOs.Request;
using Shared.DTOs.Response;
using Application.Interfaces;
using Application.Mappings;
using Application.Services;
using AutoMapper;
using Domain.Entities;
using Contracts.Enums;
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
        private readonly Mock<ITransactionManager> _transactionManagerMock;
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
            _transactionManagerMock = new Mock<ITransactionManager>();
            _userRepositoryMock = new Mock<IUserRepository>();
            _currentUserServiceMock = new Mock<ICurrentUserService>();
            _budgetRepositoryMock = new Mock<IBudgetRepository>();
            _categoryRepositoryMock = new Mock<ICategoryRepository>();

            // Instanciar el servicio
            _budgetService = new BudgetService(_transactionManagerMock.Object, _currentUserServiceMock.Object, _budgetRepositoryMock.Object, _categoryRepositoryMock.Object, _userRepositoryMock.Object, _mapper);
        }

        // ==================== TEST: GET BY ID ====================

        [Fact]
        public async Task GetByIdAsync_WithExistingId_ReturnsBudget()
        {
            // Arrange
            int userId = 1;
            int budgetId = 1;
            Category category = TestDataFactory.CreateCategory();
            Budget budget = TestDataFactory.CreateBudget();

            TestDataFactory.SetupAuthenticatedUser(_currentUserServiceMock, userId);

            _budgetRepositoryMock
                .Setup(repo => repo.GetByIdAsync(userId, budgetId))
                .ReturnsAsync(budget);

            // Act
            BudgetResponseDTO result = await _budgetService.GetByIdAsync(budgetId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(budgetId, result.Id);
            Assert.Equal(TestDataFactory.DEFAULT_BUDGET_AMOUNT, result.Amount);
            Assert.Equal(TestDataFactory.DEFAULT_CATEGORY_NAME, result.CategoryName);
            Assert.Equal(TestDataFactory.DEFAULT_YEAR, result.Year);
            Assert.Equal(TestDataFactory.DEFAULT_MONTHLY_MONTH, result.Month);
        }

        [Fact]
        public async Task GetByIdAsync_WithNonExistingId_ThrowsKeyNotFoundException()
        {
            // Arrange
            int userId = 1;
            int budgetId = 999;

            TestDataFactory.SetupAuthenticatedUser(_currentUserServiceMock, userId);

            _budgetRepositoryMock
                .Setup(repo => repo.GetByIdAsync(userId, budgetId))
                .ReturnsAsync((Budget?)null);

            // Act & Assert
            await Assert.ThrowsAsync<KeyNotFoundException>(() => _budgetService.GetByIdAsync(budgetId));
        }

        [Fact]
        public async Task GetByIdAsync_WithInvalidId_ThrowsArgumentException()
        {
            // Act & Assert
            TestDataFactory.SetupAuthenticatedUser(_currentUserServiceMock);

            await Assert.ThrowsAsync<ArgumentException>(() => _budgetService.GetByIdAsync(0));
        }

        // ==================== TEST: GET ALL ====================

        [Fact]
        public async Task GetAllAsync_ReturnsListOfBudgets()
        {
            // Arrange
            int userId = 1;
            decimal secondBudgetAmount = 300.00m;
            Budget budget1 = TestDataFactory.CreateBudget(1);
            Budget budget2 = TestDataFactory.CreateBudget(2, TestDataFactory.CreateUser(), TestDataFactory.CreateCategory(), secondBudgetAmount);
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
            Assert.Equal(TestDataFactory.DEFAULT_BUDGET_AMOUNT, result[0].Amount);
            Assert.Equal(secondBudgetAmount, result[1].Amount);
        }

        // ==================== TEST: GET BY CATEGORY ====================

        [Fact]
        public async Task GetByCategoryIdAsync_WithExistingCategory_ReturnsBudgets()
        {
            // Arrange
            int userId = 1;
            int categoryId = 1;
            Category category = TestDataFactory.CreateCategory();
            Budget budget1 = TestDataFactory.CreateBudget(1);
            Budget budget2 = TestDataFactory.CreateBudget(2);
            List<Budget> budgets = new List<Budget> { budget1, budget2 };

            TestDataFactory.SetupAuthenticatedUser(_currentUserServiceMock, userId);

            _categoryRepositoryMock
                .Setup(repo => repo.ExistsAsync(userId, categoryId))
                .ReturnsAsync(true);

            _budgetRepositoryMock
                .Setup(repo => repo.GetByCategoryIdAsync(userId, categoryId))
                .ReturnsAsync(budgets);

            // Act
            List<BudgetResponseDTO> result = await _budgetService.GetByCategoryIdAsync(categoryId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count);
            _budgetRepositoryMock.Verify(repo => repo.GetByCategoryIdAsync(userId, categoryId), Times.Once);
        }

        [Fact]
        public async Task GetByCategoryIdAsync_WithNonExistingCategory_ThrowsKeyNotFoundException()
        {
            // Arrange
            int userId = 1;
            int categoryId = 999;

            TestDataFactory.SetupAuthenticatedUser(_currentUserServiceMock, userId);

            _categoryRepositoryMock
                .Setup(repo => repo.ExistsAsync(userId, categoryId))
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
            Budget budget1 = TestDataFactory.CreateBudget(1);
            Budget budget2 = TestDataFactory.CreateBudget(2);
            List<Budget> budgets = new List<Budget> { budget1, budget2 };

            TestDataFactory.SetupAuthenticatedUser(_currentUserServiceMock, userId);

            _budgetRepositoryMock
                .Setup(repo => repo.GetByPeriodAsync(userId, It.IsAny<MonthlyPeriod>()))
                .ReturnsAsync(budgets);

            // Act
            List<BudgetResponseDTO> result = await _budgetService.GetByPeriodAsync(TestDataFactory.DEFAULT_MONTHLY_MONTH, TestDataFactory.DEFAULT_YEAR);

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
            Category category = TestDataFactory.CreateCategory();
            Budget budget = TestDataFactory.CreateBudget(1);

            TestDataFactory.SetupAuthenticatedUser(_currentUserServiceMock, userId);

            _budgetRepositoryMock
                .Setup(repo => repo.GetByCategoryAndPeriodAsync(userId, categoryId, It.IsAny<MonthlyPeriod>()))
                .ReturnsAsync(budget);

            // Act
            BudgetResponseDTO result = await _budgetService.GetByCategoryAndPeriodAsync(categoryId, TestDataFactory.DEFAULT_MONTHLY_MONTH, TestDataFactory.DEFAULT_YEAR);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(TestDataFactory.DEFAULT_BUDGET_AMOUNT, result.Amount);
            Assert.Equal(TestDataFactory.DEFAULT_YEAR, result.Year);
            Assert.Equal(TestDataFactory.DEFAULT_MONTHLY_MONTH, result.Month);
        }

        [Fact]
        public async Task GetByCategoryAndPeriodAsync_WithNonExistingBudget_ThrowsKeyNotFoundException()
        {
            // Arrange
            int userId = 1;
            int categoryId = 1;

            TestDataFactory.SetupAuthenticatedUser(_currentUserServiceMock, userId);

            _budgetRepositoryMock
                .Setup(repo => repo.GetByCategoryAndPeriodAsync(userId, categoryId, It.IsAny<MonthlyPeriod>()))
                .ReturnsAsync((Budget?)null);

            // Act & Assert
            await Assert.ThrowsAsync<KeyNotFoundException>(() => _budgetService.GetByCategoryAndPeriodAsync(categoryId, TestDataFactory.DEFAULT_MONTHLY_MONTH, TestDataFactory.DEFAULT_YEAR));
        }

        // ==================== TEST: GET SUMMARY ====================

        [Fact]
        public async Task GetSummaryByCategoryAndPeriodAsync_WithExistingBudget_ReturnsSummary()
        {
            // Arrange
            int userId = 1;
            int categoryId = 1;
            int budgetId = 1;
            Category category = TestDataFactory.CreateCategory();
            Budget budget = TestDataFactory.CreateBudget(budgetId);

            TestDataFactory.SetupAuthenticatedUser(_currentUserServiceMock, userId);

            _budgetRepositoryMock
                .Setup(repo => repo.GetByCategoryAndPeriodAsync(userId, categoryId, It.IsAny<MonthlyPeriod>()))
                .ReturnsAsync(budget);

            // Act
            BudgetSummaryDTO result = await _budgetService.GetSummaryByCategoryAndPeriodAsync(categoryId, TestDataFactory.DEFAULT_MONTHLY_MONTH, TestDataFactory.DEFAULT_YEAR);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(budgetId, result.BudgetId);
            Assert.Equal(TestDataFactory.DEFAULT_BUDGET_AMOUNT, result.BudgetAmount);
            Assert.Equal(0, result.TotalSpent);
            Assert.Equal(TestDataFactory.DEFAULT_BUDGET_AMOUNT, result.Remaining);
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

            TestDataFactory.SetupAuthenticatedUser(_currentUserServiceMock, userId);

            _budgetRepositoryMock
                .Setup(repo => repo.GetByCategoryAndPeriodAsync(userId, categoryId, It.IsAny<MonthlyPeriod>()))
                .ReturnsAsync((Budget?)null);

            // Act & Assert
            await Assert.ThrowsAsync<KeyNotFoundException>(() => _budgetService.GetSummaryByCategoryAndPeriodAsync(categoryId, TestDataFactory.DEFAULT_MONTHLY_MONTH, TestDataFactory.DEFAULT_YEAR));
        }

        // ==================== TEST: CREATE BULK ====================

        [Fact]
        public async Task CreateBulkAsync_WithValidData_ShouldCreateMultipleBudgets()
        {
            // Arrange
            int userId = 1;
            int categoryId = 1;
            int year = 2026;

            User user = TestDataFactory.CreateUser(userId);
            Category category = TestDataFactory.CreateCategory(categoryId, user);

            CreateBulkBudgetRequestDTO request = new CreateBulkBudgetRequestDTO
            {
                CategoryId = categoryId,
                Year = year,
                MonthlyBudgets = new List<MonthlyBudgetDTO>
                {
                    new() { Month = 1, Amount = 500.00m },
                    new() { Month = 2, Amount = 600.00m },
                    new() { Month = 3, Amount = 700.00m }
                }
            };

            _currentUserServiceMock.Setup(service => service.UserId).Returns(userId);

            _userRepositoryMock
                .Setup(repo => repo.GetByIdAsync(userId, It.IsAny<bool>()))
                .ReturnsAsync(user);

            _categoryRepositoryMock
                .Setup(repo => repo.GetByIdAsync(userId, categoryId, true))
                .ReturnsAsync(category);

            _budgetRepositoryMock
                .Setup(repo => repo.ExistsForCategoryAndPeriodAsync(userId, categoryId, It.IsAny<MonthlyPeriod>()))
                .ReturnsAsync(false);

            _transactionManagerMock
                .Setup(tm => tm.BeginTransactionAsync())
                .Returns(Task.CompletedTask);

            _transactionManagerMock
                .Setup(tm => tm.CommitTransactionAsync())
                .Returns(Task.CompletedTask);

            // Act
            BulkBudgetResponseDTO result = await _budgetService.CreateBulkAsync(request);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(categoryId, result.CategoryId);
            Assert.Equal(year, result.Year);
            Assert.Equal(3, result.TotalCreated);
            Assert.Equal(3, result.AfectedIds.Count);

            _budgetRepositoryMock.Verify(repo => repo.AddAsync(It.IsAny<Budget>()), Times.Exactly(3));
        }

        [Fact]
        public async Task CreateBulkAsync_WithDuplicateBudgets_ShouldOnlyCreateNewOnes()
        {
            // Arrange
            int userId = 1;
            int categoryId = 1;
            int year = 2026;

            User user = TestDataFactory.CreateUser(userId);
            Category category = TestDataFactory.CreateCategory(categoryId, user);

            CreateBulkBudgetRequestDTO request = new CreateBulkBudgetRequestDTO
            {
                CategoryId = categoryId,
                Year = year,
                MonthlyBudgets = new List<MonthlyBudgetDTO>
                {
                    new() { Month = 1, Amount = 500.00m },
                    new() { Month = 2, Amount = 600.00m },
                    new() { Month = 3, Amount = 700.00m }
                }
            };

            _currentUserServiceMock.Setup(service => service.UserId).Returns(userId);

            _userRepositoryMock
                .Setup(repo => repo.GetByIdAsync(userId, It.IsAny<bool>()))
                .ReturnsAsync(user);

            _categoryRepositoryMock
                .Setup(repo => repo.GetByIdAsync(userId, categoryId, true))
                .ReturnsAsync(category);

            // 🔥 Simular que el mes 2 ya existe
            _budgetRepositoryMock
                .Setup(repo => repo.ExistsForCategoryAndPeriodAsync(userId, categoryId, It.Is<MonthlyPeriod>(p => p.Month == 2)))
                .ReturnsAsync(true);

            _budgetRepositoryMock
                .Setup(repo => repo.ExistsForCategoryAndPeriodAsync(userId, categoryId, It.Is<MonthlyPeriod>(p => p.Month != 2)))
                .ReturnsAsync(false);

            _transactionManagerMock
                .Setup(tm => tm.BeginTransactionAsync())
                .Returns(Task.CompletedTask);

            _transactionManagerMock
                .Setup(tm => tm.CommitTransactionAsync())
                .Returns(Task.CompletedTask);

            // Act
            BulkBudgetResponseDTO result = await _budgetService.CreateBulkAsync(request);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.TotalCreated); // Solo 2 creados (meses 1 y 3)
            Assert.Equal(2, result.AfectedIds.Count);

            _budgetRepositoryMock.Verify(repo => repo.AddAsync(It.IsAny<Budget>()), Times.Exactly(2));
        }

        [Fact]
        public async Task CreateBulkAsync_WithNoValidMonths_ShouldThrowArgumentException()
        {
            // Arrange
            int userId = 1;
            int categoryId = 1;
            int year = 2026;

            User user = TestDataFactory.CreateUser(userId);
            Category category = TestDataFactory.CreateCategory(categoryId, user);

            CreateBulkBudgetRequestDTO request = new CreateBulkBudgetRequestDTO
            {
                CategoryId = categoryId,
                Year = year,
                MonthlyBudgets = new List<MonthlyBudgetDTO>
                {
                    new() { Month = 1, Amount = 0 },
                    new() { Month = 2, Amount = 0 },
                    new() { Month = 3, Amount = 0 }
                }
            };

            _currentUserServiceMock.Setup(service => service.UserId).Returns(userId);

            _userRepositoryMock
                .Setup(repo => repo.GetByIdAsync(userId, It.IsAny<bool>()))
                .ReturnsAsync(user);

            _categoryRepositoryMock
                .Setup(repo => repo.GetByIdAsync(userId, categoryId, true))
                .ReturnsAsync(category);

            // Act & Assert
            ArgumentException exception = await Assert.ThrowsAsync<ArgumentException>(
                () => _budgetService.CreateBulkAsync(request));

            Assert.Contains("At least one month with amount > 0 is required", exception.Message);

            _budgetRepositoryMock.Verify(repo => repo.AddAsync(It.IsAny<Budget>()), Times.Never);
        }

        [Fact]
        public async Task CreateBulkAsync_WithInvalidMonth_ShouldThrowArgumentException()
        {
            // Arrange
            int userId = 1;
            int categoryId = 1;
            int year = 2026;

            User user = TestDataFactory.CreateUser(userId);
            Category category = TestDataFactory.CreateCategory(categoryId, user);

            CreateBulkBudgetRequestDTO request = new CreateBulkBudgetRequestDTO
            {
                CategoryId = categoryId,
                Year = year,
                MonthlyBudgets = [new() { Month = 13, Amount = 500.00m }]
            };

            _currentUserServiceMock.Setup(service => service.UserId).Returns(userId);

            _userRepositoryMock
                .Setup(repo => repo.GetByIdAsync(userId, It.IsAny<bool>()))
                .ReturnsAsync(user);

            _categoryRepositoryMock
                .Setup(repo => repo.GetByIdAsync(userId, categoryId, true))
                .ReturnsAsync(category);

            // Act & Assert
            ArgumentException exception = await Assert.ThrowsAsync<ArgumentException>(() => _budgetService.CreateBulkAsync(request));

            Assert.Contains("Invalid month: 13", exception.Message);

            _budgetRepositoryMock.Verify(repo => repo.AddAsync(It.IsAny<Budget>()), Times.Never);
        }

        [Fact]
        public async Task CreateBulkAsync_WithNonExistingCategory_ShouldThrowKeyNotFoundException()
        {
            // Arrange
            int userId = 1;
            int categoryId = 999;
            int year = 2026;

            User user = TestDataFactory.CreateUser(userId);

            CreateBulkBudgetRequestDTO request = new CreateBulkBudgetRequestDTO
            {
                CategoryId = categoryId,
                Year = year,
                MonthlyBudgets = [new() { Month = 1, Amount = 500.00m }]
            };

            _currentUserServiceMock.Setup(service => service.UserId).Returns(userId);

            _userRepositoryMock
                .Setup(repo => repo.GetByIdAsync(userId))
                .ReturnsAsync(user);

            _categoryRepositoryMock
                .Setup(repo => repo.GetByIdAsync(userId, categoryId, true))
                .ReturnsAsync((Category?)null);

            // Act & Assert
            await Assert.ThrowsAsync<KeyNotFoundException>(() => _budgetService.CreateBulkAsync(request));

            _budgetRepositoryMock.Verify(repo => repo.AddAsync(It.IsAny<Budget>()), Times.Never);
        }

        // ==================== TEST: CREATE ====================

        [Fact]
        public async Task CreateAsync_WithValidData_ReturnsCreatedBudget()
        {
            // Arrange
            int userId = 1;
            int categoryId = 1;

            TestDataFactory.SetupAuthenticatedUser(_currentUserServiceMock, userId);

            _categoryRepositoryMock
                .Setup(repo => repo.GetByIdAsync(userId, categoryId, It.IsAny<bool>()))
                .ReturnsAsync(TestDataFactory.CreateCategory(categoryId));

            _budgetRepositoryMock
                .Setup(repo => repo.ExistsForCategoryAndPeriodAsync(userId, categoryId, It.IsAny<MonthlyPeriod>()))
                .ReturnsAsync(false);

            _userRepositoryMock
                .Setup(repo => repo.GetByIdAsync(userId, It.IsAny<bool>()))
                .ReturnsAsync(TestDataFactory.CreateUser(userId));

            // Act
            BudgetResponseDTO result = await _budgetService.CreateAsync
            (
                new CreateBudgetRequestDTO
                {
                    CategoryId = categoryId,
                    Amount = TestDataFactory.DEFAULT_BUDGET_AMOUNT,
                    Month = TestDataFactory.DEFAULT_MONTHLY_MONTH,
                    Year = TestDataFactory.DEFAULT_YEAR
                }
            );

            // Assert
            Assert.NotNull(result);
            Assert.Equal(TestDataFactory.DEFAULT_BUDGET_AMOUNT, result.Amount);
            Assert.Equal(TestDataFactory.DEFAULT_MONTHLY_MONTH, result.Month);
            Assert.Equal(TestDataFactory.DEFAULT_YEAR, result.Year);
            Assert.Equal(TestDataFactory.DEFAULT_CATEGORY_NAME, result.CategoryName);

            _budgetRepositoryMock.Verify(repo => repo.AddAsync(It.IsAny<Budget>()), Times.Once);
        }

        [Fact]
        public async Task CreateAsync_WithNonExistingCategory_ThrowsKeyNotFoundException()
        {
            // Arrange
            int userId = 1;
            int categoryId = 999;

            TestDataFactory.SetupAuthenticatedUser(_currentUserServiceMock, userId);

            _categoryRepositoryMock
                .Setup(repo => repo.GetByIdAsync(userId, categoryId))
                .ReturnsAsync((Category?)null);

            // Act & Assert
            await Assert.ThrowsAsync<KeyNotFoundException>
            (
                () => _budgetService.CreateAsync(new CreateBudgetRequestDTO { CategoryId = categoryId, Amount = TestDataFactory.DEFAULT_BUDGET_AMOUNT, Month = TestDataFactory.DEFAULT_MONTHLY_MONTH, Year = TestDataFactory.DEFAULT_YEAR })
            );

            _budgetRepositoryMock.Verify(repo => repo.AddAsync(It.IsAny<Budget>()), Times.Never);
        }

        [Fact]
        public async Task CreateAsync_WithDuplicateBudget_ThrowsInvalidOperationException()
        {
            // Arrange
            int userId = 1;
            int categoryId = 1;

            Category category = TestDataFactory.CreateCategory(categoryId);

            TestDataFactory.SetupAuthenticatedUser(_currentUserServiceMock, userId);

            _userRepositoryMock
                .Setup(repo => repo.GetByIdAsync(userId, It.IsAny<bool>()))
                .ReturnsAsync(TestDataFactory.CreateUser(userId));

            _categoryRepositoryMock
                .Setup(repo => repo.GetByIdAsync(userId, categoryId, It.IsAny<bool>()))
                .ReturnsAsync(category);

            _budgetRepositoryMock
                .Setup(repo => repo.ExistsForCategoryAndPeriodAsync(userId, categoryId, It.IsAny<MonthlyPeriod>()))
                .ReturnsAsync(true);

            // Act & Assert
            await Assert.ThrowsAsync<ConflictException>
            (
                () => _budgetService.CreateAsync(new CreateBudgetRequestDTO { CategoryId = categoryId, Amount = TestDataFactory.DEFAULT_BUDGET_AMOUNT, Month = TestDataFactory.DEFAULT_MONTHLY_MONTH, Year = TestDataFactory.DEFAULT_YEAR })
            );

            _budgetRepositoryMock.Verify(repo => repo.AddAsync(It.IsAny<Budget>()), Times.Never);
        }

        // ==================== TEST: UPDATE BULK ====================

        [Fact]
        public async Task UpdateBulkAsync_WithValidData_ShouldUpdateExistingBudgets()
        {
            // Arrange
            int userId = 1;
            int categoryId = 1;
            int year = 2026;

            User user = TestDataFactory.CreateUser(userId);
            Category category = TestDataFactory.CreateCategory(categoryId, user);

            CreateBulkBudgetRequestDTO createRequest = new CreateBulkBudgetRequestDTO
            {
                CategoryId = categoryId,
                Year = year,
                MonthlyBudgets = new List<MonthlyBudgetDTO>
                {
                    new() { Month = 1, Amount = 500.00m },
                    new() { Month = 2, Amount = 600.00m },
                    new() { Month = 3, Amount = 700.00m }
                }
            };

            // Simular que los presupuestos existen
            List<Budget> existingBudgets = new List<Budget>
            {
                TestDataFactory.CreateBudget(1, user, category, 500.00m, 1, year),
                TestDataFactory.CreateBudget(2, user, category, 600.00m, 2, year),
                TestDataFactory.CreateBudget(3, user, category, 700.00m, 3, year)
            };

            UpdateBulkBudgetRequestDTO updateRequest = new UpdateBulkBudgetRequestDTO
            {
                CategoryId = categoryId,
                Year = year,
                MonthlyBudgets = new List<MonthlyBudgetDTO>
                {
                    new() { Month = 1, Amount = 550.00m },
                    new() { Month = 2, Amount = 650.00m }
                }
            };

            _currentUserServiceMock.Setup(service => service.UserId).Returns(userId);

            _userRepositoryMock
                .Setup(repo => repo.GetByIdAsync(userId, It.IsAny<bool>()))
                .ReturnsAsync(user);

            _categoryRepositoryMock
                .Setup(repo => repo.GetByIdAsync(userId, categoryId, true))
                .ReturnsAsync(category);

            _budgetRepositoryMock
                .Setup(repo => repo.GetByCategoryAndPeriodAsync(userId, categoryId, It.IsAny<MonthlyPeriod>(), It.IsAny<bool>()))
                .ReturnsAsync((int u, int c, MonthlyPeriod p, bool b) => { return existingBudgets.FirstOrDefault(b => b.Period.Month == p.Month); });

            _transactionManagerMock
                .Setup(tm => tm.BeginTransactionAsync())
                .Returns(Task.CompletedTask);

            _transactionManagerMock
                .Setup(tm => tm.CommitTransactionAsync())
                .Returns(Task.CompletedTask);

            // Act
            BulkBudgetResponseDTO result = await _budgetService.UpdateBulkAsync(updateRequest);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(categoryId, result.CategoryId);
            Assert.Equal(year, result.Year);
            Assert.Equal(2, result.TotalCreated);
            Assert.Equal(2, result.AfectedIds.Count);

            _budgetRepositoryMock.Verify(repo => repo.UpdateAsync(It.IsAny<Budget>()), Times.Exactly(2));
        }

        [Fact]
        public async Task UpdateBulkAsync_WithNoValidMonths_ShouldThrowArgumentException()
        {
            // Arrange
            int userId = 1;
            int categoryId = 1;
            int year = 2026;

            User user = TestDataFactory.CreateUser(userId);
            Category category = TestDataFactory.CreateCategory(categoryId, user);

            UpdateBulkBudgetRequestDTO updateRequest = new UpdateBulkBudgetRequestDTO
            {
                CategoryId = categoryId,
                Year = year,
                MonthlyBudgets = new List<MonthlyBudgetDTO>
                {
                    new() { Month = 1, Amount = 0 },
                    new() { Month = 2, Amount = 0 },
                    new() { Month = 3, Amount = 0 }
                }
            };

            _currentUserServiceMock.Setup(service => service.UserId).Returns(userId);

            _userRepositoryMock
                .Setup(repo => repo.GetByIdAsync(userId, It.IsAny<bool>()))
                .ReturnsAsync(user);

            _categoryRepositoryMock
                .Setup(repo => repo.GetByIdAsync(userId, categoryId, true))
                .ReturnsAsync(category);

            // Act & Assert
            ArgumentException exception = await Assert.ThrowsAsync<ArgumentException>(() => _budgetService.UpdateBulkAsync(updateRequest));

            Assert.Contains("At least one month with amount > 0 is required", exception.Message);

            _budgetRepositoryMock.Verify(repo => repo.UpdateAsync(It.IsAny<Budget>()), Times.Never);
        }

        [Fact]
        public async Task UpdateBulkAsync_WithNonExistingCategory_ShouldThrowKeyNotFoundException()
        {
            // Arrange
            int userId = 1;
            int categoryId = 999;
            int year = 2026;

            User user = TestDataFactory.CreateUser(userId);

            UpdateBulkBudgetRequestDTO updateRequest = new UpdateBulkBudgetRequestDTO
            {
                CategoryId = categoryId,
                Year = year,
                MonthlyBudgets = new List<MonthlyBudgetDTO> { new MonthlyBudgetDTO { Month = 1, Amount = 550.00m } }
            };

            _currentUserServiceMock.Setup(service => service.UserId).Returns(userId);

            _userRepositoryMock
                .Setup(repo => repo.GetByIdAsync(userId, It.IsAny<bool>()))
                .ReturnsAsync(user);

            _categoryRepositoryMock
                .Setup(repo => repo.GetByIdAsync(userId, categoryId, true))
                .ReturnsAsync((Category?)null);

            // Act & Assert
            await Assert.ThrowsAsync<KeyNotFoundException>(() => _budgetService.UpdateBulkAsync(updateRequest));

            _budgetRepositoryMock.Verify(repo => repo.UpdateAsync(It.IsAny<Budget>()), Times.Never);
        }

        // ==================== TEST: UPDATE ====================

        [Fact]
        public async Task UpdateAsync_WithValidData_ReturnsUpdatedBudget()
        {
            // Arrange
            int userId = 1;
            int budgetId = 1;
            decimal updatedAmount = 600.00m;

            Budget budget = TestDataFactory.CreateBudget(budgetId);

            TestDataFactory.SetupAuthenticatedUser(_currentUserServiceMock, userId);

            _budgetRepositoryMock
                .Setup(repo => repo.GetByIdAsync(userId, budgetId))
                .ReturnsAsync(budget);

            // Act
            BudgetResponseDTO result = await _budgetService.UpdateAsync(budgetId, new UpdateBudgetRequestDTO { Amount = updatedAmount });

            // Assert
            Assert.NotNull(result);
            Assert.Equal(budgetId, result.Id);
            Assert.Equal(updatedAmount, result.Amount);

            _budgetRepositoryMock.Verify(repo => repo.UpdateAsync(It.IsAny<Budget>()), Times.Once);
        }

        [Fact]
        public async Task UpdateAsync_WithNonExistingId_ThrowsKeyNotFoundException()
        {
            // Arrange
            int userId = 1;
            int budgetId = 999;
            decimal updatedAmount = 600.00m;

            TestDataFactory.SetupAuthenticatedUser(_currentUserServiceMock, userId);

            _budgetRepositoryMock
                .Setup(repo => repo.GetByIdAsync(userId, budgetId))
                .ReturnsAsync((Budget?)null);

            // Act & Assert
            await Assert.ThrowsAsync<KeyNotFoundException>(() => _budgetService.UpdateAsync(budgetId, new UpdateBudgetRequestDTO { Amount = updatedAmount }));

            _budgetRepositoryMock.Verify(repo => repo.UpdateAsync(It.IsAny<Budget>()), Times.Never);
        }

        // ==================== TEST: DELETE BULK ====================

        [Fact]
        public async Task DeleteBulkAsync_WithValidData_ShouldDeleteExistingBudgets()
        {
            // Arrange
            int userId = 1;
            int categoryId = 1;
            int year = 2026;

            User user = TestDataFactory.CreateUser(userId);
            Category category = TestDataFactory.CreateCategory(categoryId, user);

            // Simular que los presupuestos existen
            List<Budget> existingBudgets = new List<Budget>
            {
                TestDataFactory.CreateBudget(1, user, category, 500.00m, 1, year),
                TestDataFactory.CreateBudget(2, user, category, 600.00m, 2, year),
                TestDataFactory.CreateBudget(3, user, category, 700.00m, 3, year)
            };

            DeleteBulkBudgetRequestDTO deleteRequest = new DeleteBulkBudgetRequestDTO
            {
                CategoryId = categoryId,
                Year = year,
                MonthsToDelete = new List<int> { 1, 3 }
            };

            _currentUserServiceMock.Setup(service => service.UserId).Returns(userId);

            _userRepositoryMock
                .Setup(repo => repo.GetByIdAsync(userId, It.IsAny<bool>()))
                .ReturnsAsync(user);

            _categoryRepositoryMock
                .Setup(repo => repo.GetByIdAsync(userId, categoryId, true))
                .ReturnsAsync(category);

            _budgetRepositoryMock
                .Setup(repo => repo.GetByCategoryAndPeriodAsync(userId, categoryId, It.IsAny<MonthlyPeriod>(), It.IsAny<bool>()))
                .ReturnsAsync((int u, int c, MonthlyPeriod p, bool b) => { return existingBudgets.FirstOrDefault(b => b.Period.Month == p.Month); });

            _transactionManagerMock
                .Setup(tm => tm.BeginTransactionAsync())
                .Returns(Task.CompletedTask);

            _transactionManagerMock
                .Setup(tm => tm.CommitTransactionAsync())
                .Returns(Task.CompletedTask);

            // Act
            BulkBudgetResponseDTO result = await _budgetService.DeleteBulkAsync(deleteRequest);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(categoryId, result.CategoryId);
            Assert.Equal(year, result.Year);
            Assert.Equal(2, result.TotalCreated);
            Assert.Equal(2, result.AfectedIds.Count);

            _budgetRepositoryMock.Verify(repo => repo.DeleteAsync(userId, It.IsAny<int>()), Times.Exactly(2));
        }

        [Fact]
        public async Task DeleteBulkAsync_WithNoMonthsToDelete_ShouldThrowArgumentException()
        {
            // Arrange
            int userId = 1;
            int categoryId = 1;
            int year = 2026;

            User user = TestDataFactory.CreateUser(userId);
            Category category = TestDataFactory.CreateCategory(categoryId, user);

            DeleteBulkBudgetRequestDTO deleteRequest = new DeleteBulkBudgetRequestDTO
            {
                CategoryId = categoryId,
                Year = year,
                MonthsToDelete = new List<int>()
            };

            _currentUserServiceMock.Setup(service => service.UserId).Returns(userId);

            _userRepositoryMock
                .Setup(repo => repo.GetByIdAsync(userId, It.IsAny<bool>()))
                .ReturnsAsync(user);

            _categoryRepositoryMock
                .Setup(repo => repo.GetByIdAsync(userId, categoryId, true))
                .ReturnsAsync(category);

            // Act & Assert
            ArgumentException exception = await Assert.ThrowsAsync<ArgumentException>(() => _budgetService.DeleteBulkAsync(deleteRequest));

            Assert.Contains("At least one month to delete is required", exception.Message);

            _budgetRepositoryMock.Verify(repo => repo.DeleteAsync(It.IsAny<int>(), It.IsAny<int>()), Times.Never);
        }

        [Fact]
        public async Task DeleteBulkAsync_WithNonExistingCategory_ShouldThrowKeyNotFoundException()
        {
            // Arrange
            int userId = 1;
            int categoryId = 999;
            int year = 2026;

            User user = TestDataFactory.CreateUser(userId);

            DeleteBulkBudgetRequestDTO deleteRequest = new DeleteBulkBudgetRequestDTO
            {
                CategoryId = categoryId,
                Year = year,
                MonthsToDelete = new List<int> { 1, 3 }
            };

            _currentUserServiceMock.Setup(service => service.UserId).Returns(userId);

            _userRepositoryMock
                .Setup(repo => repo.GetByIdAsync(userId, It.IsAny<bool>()))
                .ReturnsAsync(user);

            _categoryRepositoryMock
                .Setup(repo => repo.GetByIdAsync(userId, categoryId, true))
                .ReturnsAsync((Category?)null);

            // Act & Assert
            await Assert.ThrowsAsync<KeyNotFoundException>(() => _budgetService.DeleteBulkAsync(deleteRequest));

            _budgetRepositoryMock.Verify(repo => repo.DeleteAsync(It.IsAny<int>(), It.IsAny<int>()), Times.Never);
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
                .Setup(repo => repo.ExistsAsync(userId, budgetId))
                .ReturnsAsync(true);

            // Act
            await _budgetService.DeleteAsync(budgetId);

            // Assert
            _budgetRepositoryMock.Verify(repo => repo.DeleteAsync(userId, budgetId), Times.Once);
        }

        [Fact]
        public async Task DeleteAsync_WithNonExistingId_ThrowsKeyNotFoundException()
        {
            // Arrange
            int userId = 1;
            int budgetId = 999;

            TestDataFactory.SetupAuthenticatedUser(_currentUserServiceMock, userId);

            _budgetRepositoryMock
                .Setup(repo => repo.ExistsAsync(userId, budgetId))
                .ReturnsAsync(false);

            // Act & Assert
            await Assert.ThrowsAsync<KeyNotFoundException>(() => _budgetService.DeleteAsync(budgetId));

            _budgetRepositoryMock.Verify(repo => repo.DeleteAsync(userId, It.IsAny<int>()), Times.Never);
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
                .Setup(repo => repo.ExistsAsync(userId, budgetId))
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
                .Setup(repo => repo.ExistsAsync(userId, budgetId))
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

            TestDataFactory.SetupAuthenticatedUser(_currentUserServiceMock, userId);

            _budgetRepositoryMock
                .Setup(repo => repo.ExistsForCategoryAndPeriodAsync(userId, categoryId, It.IsAny<MonthlyPeriod>()))
                .ReturnsAsync(true);

            // Act
            bool result = await _budgetService.ExistsForCategoryAndPeriodAsync(categoryId, TestDataFactory.DEFAULT_MONTHLY_MONTH, TestDataFactory.DEFAULT_YEAR);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public async Task ExistsForCategoryAndPeriodAsync_WithNonExistingBudget_ReturnsFalse()
        {
            // Arrange
            int userId = 1;
            int categoryId = 1;

            TestDataFactory.SetupAuthenticatedUser(_currentUserServiceMock, userId);

            _budgetRepositoryMock
                .Setup(repo => repo.ExistsForCategoryAndPeriodAsync(userId, categoryId, It.IsAny<MonthlyPeriod>()))
                .ReturnsAsync(false);

            // Act
            bool result = await _budgetService.ExistsForCategoryAndPeriodAsync(categoryId, TestDataFactory.DEFAULT_MONTHLY_MONTH, TestDataFactory.DEFAULT_YEAR);

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