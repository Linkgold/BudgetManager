using Shared.DTOs.Request;
using Shared.DTOs.Response;
using Application.Interfaces;
using Application.Mappings;
using Application.Services;
using AutoMapper;
using Domain.Entities;
using Contracts.Enums;
using Domain.Interfaces;
using Domain.ValueObjects;
using Microsoft.Extensions.Logging;
using Moq;
using Tests.Helpers;

namespace Tests.Application
{
    public class TransactionServiceTests : IDisposable
    {
        private readonly Mock<IUserRepository> _userRepositoryMock;
        private readonly Mock<ICurrentUserService> _currentUserServiceMock;
        private readonly Mock<ITransactionRepository> _transactionRepositoryMock;
        private readonly Mock<ICategoryRepository> _categoryRepositoryMock;
        private readonly IMapper _mapper;
        private readonly ITransactionService _transactionService;

        public TransactionServiceTests()
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
            _transactionRepositoryMock = new Mock<ITransactionRepository>();
            _categoryRepositoryMock = new Mock<ICategoryRepository>();

            // Instanciar el servicio
            _transactionService = new TransactionService(_currentUserServiceMock.Object, _transactionRepositoryMock.Object, _categoryRepositoryMock.Object, _userRepositoryMock.Object, _mapper);
        }

        // ==================== TEST: GET BY ID ====================

        [Fact]
        public async Task GetByIdAsync_WithExistingId_ReturnsTransaction()
        {
            // Arrange
            int userId = 1;
            int transactionId = 1;

            TestDataFactory.SetupAuthenticatedUser(_currentUserServiceMock, userId);

            _transactionRepositoryMock
                .Setup(repo => repo.GetByIdAsync(userId, transactionId))
                .ReturnsAsync(TestDataFactory.CreateTransaction());

            // Act
            TransactionResponseDTO result = await _transactionService.GetByIdAsync(transactionId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(transactionId, result.Id);
            Assert.Equal(TestDataFactory.DEFAULT_TRANSACTION_AMOUNT, result.Amount);
            Assert.Equal(TransactionTypeEnum.Expense, result.Type);
            Assert.Equal(TestDataFactory.DEFAULT_DAILY_DAY, result.Date.Day);
            Assert.Equal(TestDataFactory.DEFAULT_DAILY_MONTH, result.Date.Month);
            Assert.Equal(TestDataFactory.DEFAULT_YEAR, result.Date.Year);
            Assert.Equal(TestDataFactory.DEFAULT_CATEGORY_NAME, result.CategoryName);
        }

        [Fact]
        public async Task GetByIdAsync_WithNonExistingId_ThrowsKeyNotFoundException()
        {
            // Arrange
            int userId = 1;
            int transactionId = 999;

            TestDataFactory.SetupAuthenticatedUser(_currentUserServiceMock, userId);

            _transactionRepositoryMock
                .Setup(repo => repo.GetByIdAsync(userId, transactionId))
                .ReturnsAsync((Transaction?)null);

            // Act & Assert
            await Assert.ThrowsAsync<KeyNotFoundException>(() => _transactionService.GetByIdAsync(transactionId));
        }

        [Fact]
        public async Task GetByIdAsync_WithInvalidId_ThrowsArgumentException()
        {
            TestDataFactory.SetupAuthenticatedUser(_currentUserServiceMock);

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() => _transactionService.GetByIdAsync(0));
        }

        // ==================== TEST: GET ALL ====================

        [Fact]
        public async Task GetAllAsync_ReturnsListOfTransactions()
        {
            // Arrange
            int userId = 1;
            decimal customAmount = 30.00m;

            TestDataFactory.SetupAuthenticatedUser(_currentUserServiceMock, userId);

            _transactionRepositoryMock
                .Setup(repo => repo.GetAllAsync(userId))
                .ReturnsAsync
                (
                    new List<Transaction>
                    {
                        TestDataFactory.CreateTransaction(),
                        TestDataFactory.CreateTransaction(2, user: TestDataFactory.CreateUser(), category: TestDataFactory.CreateCategory(), name: "Compra 2", amount: customAmount)
                    }
                );

            // Act
            List<TransactionResponseDTO> result = await _transactionService.GetAllAsync();

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count);
            Assert.Equal(TestDataFactory.DEFAULT_TRANSACTION_AMOUNT, result[0].Amount);
            Assert.Equal(customAmount, result[1].Amount);
        }

        // ==================== TEST: GET BY CATEGORY ====================

        [Fact]
        public async Task GetByCategoryIdAsync_WithExistingCategory_ReturnsTransactions()
        {
            // Arrange
            int userId = 1;
            int categoryId = 1;

            TestDataFactory.SetupAuthenticatedUser(_currentUserServiceMock, userId);

            _categoryRepositoryMock
                .Setup(repo => repo.ExistsAsync(userId, categoryId))
                .ReturnsAsync(true);

            _transactionRepositoryMock
                .Setup(repo => repo.GetByCategoryIdAsync(userId, categoryId))
                .ReturnsAsync
                (
                    new List<Transaction>
                    {
                        TestDataFactory.CreateTransaction(),
                        TestDataFactory.CreateTransaction(2)
                    }
                );

            // Act
            List<TransactionResponseDTO> result = await _transactionService.GetByCategoryIdAsync(categoryId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count);

            _transactionRepositoryMock.Verify(repo => repo.GetByCategoryIdAsync(userId, categoryId), Times.Once);
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
            await Assert.ThrowsAsync<KeyNotFoundException>(() => _transactionService.GetByCategoryIdAsync(categoryId));

            _transactionRepositoryMock.Verify(repo => repo.GetByCategoryIdAsync(userId, It.IsAny<int>()), Times.Never);
        }

        // ==================== TEST: GET BY MONTHLY PERIOD ====================

        [Fact]
        public async Task GetByMonthlyPeriodAsync_ReturnsTransactionsForPeriod()
        {
            // Arrange
            int userId = 1;
            Transaction transaction1 = TestDataFactory.CreateTransaction();
            Transaction transaction2 = TestDataFactory.CreateTransaction(2);

            TestDataFactory.SetupAuthenticatedUser(_currentUserServiceMock, userId);

            _transactionRepositoryMock
                .Setup(repo => repo.GetByMonthlyPeriodAsync(userId, It.IsAny<MonthlyPeriod>()))
                .ReturnsAsync
                (
                    new List<Transaction>
                    {
                        TestDataFactory.CreateTransaction(),
                        TestDataFactory.CreateTransaction(2)
                    }
                );

            // Act
            List<TransactionResponseDTO> result = await _transactionService.GetByMonthlyPeriodAsync(TestDataFactory.DEFAULT_DAILY_MONTH, TestDataFactory.DEFAULT_YEAR);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count);
        }

        // ==================== TEST: GET BY CATEGORY AND MONTHLY PERIOD ====================

        [Fact]
        public async Task GetByCategoryAndMonthlyPeriodAsync_WithExistingCategory_ReturnsTransactions()
        {
            // Arrange
            int userId = 1;
            int categoryId = 1;

            TestDataFactory.SetupAuthenticatedUser(_currentUserServiceMock, userId);

            _categoryRepositoryMock
                .Setup(repo => repo.ExistsAsync(userId, categoryId))
                .ReturnsAsync(true);

            _transactionRepositoryMock
                .Setup(repo => repo.GetByCategoryAndMonthlyPeriodAsync(userId, categoryId, It.IsAny<MonthlyPeriod>()))
                .ReturnsAsync
                (
                    new List<Transaction>
                    {
                        TestDataFactory.CreateTransaction(),
                        TestDataFactory.CreateTransaction(2)
                    }
                );

            // Act
            List<TransactionResponseDTO> result = await _transactionService.GetByCategoryAndMonthlyPeriodAsync(categoryId, TestDataFactory.DEFAULT_MONTHLY_MONTH, TestDataFactory.DEFAULT_YEAR);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count);
        }

        [Fact]
        public async Task GetByCategoryAndMonthlyPeriodAsync_WithNonExistingCategory_ThrowsKeyNotFoundException()
        {
            // Arrange
            int userId = 1;
            int categoryId = 999;

            TestDataFactory.SetupAuthenticatedUser(_currentUserServiceMock, userId);

            _categoryRepositoryMock
                .Setup(repo => repo.ExistsAsync(userId, categoryId))
                .ReturnsAsync(false);

            // Act & Assert
            await Assert.ThrowsAsync<KeyNotFoundException>(() => _transactionService.GetByCategoryAndMonthlyPeriodAsync(categoryId, TestDataFactory.DEFAULT_MONTHLY_MONTH, TestDataFactory.DEFAULT_YEAR));
        }

        // ==================== TEST: GET BY DATE RANGE ====================

        [Fact]
        public async Task GetByDateRangeAsync_ReturnsTransactionsInRange()
        {
            // Arrange
            int userId = 1;
            DateTime from = new DateTime(2024, 6, 1);
            DateTime to = new DateTime(2024, 6, 30);

            TestDataFactory.SetupAuthenticatedUser(_currentUserServiceMock, userId);

            _transactionRepositoryMock
                .Setup(repo => repo.GetByDateRangeAsync(userId, It.IsAny<DailyPeriod>(), It.IsAny<DailyPeriod>()))
                .ReturnsAsync
                (
                    new List<Transaction>
                    {
                        TestDataFactory.CreateTransaction(),
                        TestDataFactory.CreateTransaction(2)
                    }
                );

            // Act
            List<TransactionResponseDTO> result = await _transactionService.GetByDateRangeAsync(from, to);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count);
        }

        [Fact]
        public async Task GetByDateRangeAsync_WithInvalidRange_ThrowsArgumentException()
        {
            // Arrange
            int userId = 1;
            DateTime from = new DateTime(2024, 6, 30);
            DateTime to = new DateTime(2024, 6, 1);

            TestDataFactory.SetupAuthenticatedUser(_currentUserServiceMock, userId);

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() => _transactionService.GetByDateRangeAsync(from, to));
        }

        // ==================== TEST: GET TOTAL ====================

        [Fact]
        public async Task GetTotalByCategoryAndMonthlyPeriodAsync_ReturnsTotal()
        {
            // Arrange
            int userId = 1;
            int categoryId = 1;
            decimal expectedTotal = 75.75m;

            TestDataFactory.SetupAuthenticatedUser(_currentUserServiceMock, userId);

            _categoryRepositoryMock
                .Setup(repo => repo.ExistsAsync(userId, categoryId))
                .ReturnsAsync(true);

            _transactionRepositoryMock
                .Setup(repo => repo.GetTotalByCategoryAndMonthlyPeriodAsync(userId, categoryId, It.IsAny<MonthlyPeriod>()))
                .ReturnsAsync(expectedTotal);

            // Act
            decimal result = await _transactionService.GetTotalByCategoryAndMonthlyPeriodAsync(categoryId, TestDataFactory.DEFAULT_DAILY_MONTH, TestDataFactory.DEFAULT_YEAR);

            // Assert
            Assert.Equal(expectedTotal, result);
        }

        [Fact]
        public async Task GetTotalByCategoryAndMonthlyPeriodAsync_WithNonExistingCategory_ThrowsKeyNotFoundException()
        {
            // Arrange
            int userId = 1;
            int categoryId = 999;

            TestDataFactory.SetupAuthenticatedUser(_currentUserServiceMock, userId);

            _categoryRepositoryMock
                .Setup(repo => repo.ExistsAsync(userId, categoryId))
                .ReturnsAsync(false);

            // Act & Assert
            await Assert.ThrowsAsync<KeyNotFoundException>(() => _transactionService.GetTotalByCategoryAndMonthlyPeriodAsync(categoryId, TestDataFactory.DEFAULT_DAILY_MONTH, TestDataFactory.DEFAULT_YEAR));
        }

        // ==================== TEST: CREATE ====================

        [Fact]
        public async Task CreateAsync_WithValidData_ReturnsCreatedTransaction()
        {
            // Arrange
            int userId = 1;
            int categoryId = 1;

            TestDataFactory.SetupAuthenticatedUser(_currentUserServiceMock, userId);

            _userRepositoryMock
                .Setup(repo => repo.GetByIdAsync(userId, It.IsAny<bool>()))
                .ReturnsAsync(TestDataFactory.CreateUser());

            _categoryRepositoryMock
                .Setup(repo => repo.GetByIdAsync(userId, categoryId, It.IsAny<bool>()))
                .ReturnsAsync(TestDataFactory.CreateCategory());

            // Act
            TransactionResponseDTO result = await _transactionService.CreateAsync
            (
                new CreateTransactionRequestDTO
                {
                    CategoryId = categoryId,
                    Name = TestDataFactory.DEFAULT_TRANSACTION_NAME,
                    Description = TestDataFactory.DEFAULT_TRANSACTION_DESCRIPTION,
                    Amount = TestDataFactory.DEFAULT_TRANSACTION_AMOUNT,
                    Type = TransactionTypeEnum.Expense,
                    Date = new DateTime(TestDataFactory.DEFAULT_YEAR, TestDataFactory.DEFAULT_DAILY_MONTH, TestDataFactory.DEFAULT_DAILY_DAY)
                }
            );

            // Assert
            Assert.NotNull(result);
            Assert.Equal(TestDataFactory.DEFAULT_TRANSACTION_AMOUNT, result.Amount);
            Assert.Equal(TransactionTypeEnum.Expense, result.Type);
            Assert.Equal(TestDataFactory.DEFAULT_DAILY_DAY, result.Date.Day);
            Assert.Equal(TestDataFactory.DEFAULT_DAILY_MONTH, result.Date.Month);
            Assert.Equal(TestDataFactory.DEFAULT_YEAR, result.Date.Year);
            Assert.Equal(TestDataFactory.DEFAULT_CATEGORY_NAME, result.CategoryName);

            _transactionRepositoryMock.Verify(repo => repo.AddAsync(It.IsAny<Transaction>()), Times.Once);
        }

        [Fact]
        public async Task CreateAsync_WithNonExistingCategory_ThrowsKeyNotFoundException()
        {
            // Arrange
            int userId = 1;
            int categoryId = 999;

            TestDataFactory.SetupAuthenticatedUser(_currentUserServiceMock, userId);

            _categoryRepositoryMock
                .Setup(repo => repo.GetByIdAsync(userId, categoryId, It.IsAny<bool>()))
                .ReturnsAsync((Category?)null);

            // Act & Assert
            await Assert.ThrowsAsync<KeyNotFoundException>
            (
                () => _transactionService.CreateAsync
                (
                    new CreateTransactionRequestDTO
                    {
                        CategoryId = categoryId,
                        Name = TestDataFactory.DEFAULT_TRANSACTION_NAME,
                        Amount = TestDataFactory.DEFAULT_TRANSACTION_AMOUNT,
                        Type = TransactionTypeEnum.Expense,
                        Date = new DateTime(TestDataFactory.DEFAULT_YEAR, TestDataFactory.DEFAULT_DAILY_MONTH, TestDataFactory.DEFAULT_DAILY_DAY)
                    }
                )
            );

            _transactionRepositoryMock.Verify(repo => repo.AddAsync(It.IsAny<Transaction>()), Times.Never);
        }

        // ==================== TEST: UPDATE ====================

        [Fact]
        public async Task UpdateAsync_WithValidData_ReturnsUpdatedTransaction()
        {
            // Arrange
            int userId = 1;
            int transactionId = 1;
            string updatedName = "Compra supermercado actualizada";
            string updatedDescription = "Carrefour 20/06/2024";
            decimal updatedAmount = 50.00m;
            int updatedDay = 20;

            TestDataFactory.SetupAuthenticatedUser(_currentUserServiceMock, userId);

            _transactionRepositoryMock
                .Setup(repo => repo.GetByIdAsync(userId, transactionId))
                .ReturnsAsync(TestDataFactory.CreateTransaction());

            // Act
            TransactionResponseDTO result = await _transactionService.UpdateAsync
            (
                transactionId,
                new UpdateTransactionRequestDTO
                {
                    Name = updatedName,
                    Description = updatedDescription,
                    Amount = updatedAmount,
                    Type = TransactionTypeEnum.Income,
                    Date = new DateTime(TestDataFactory.DEFAULT_YEAR, TestDataFactory.DEFAULT_DAILY_MONTH, updatedDay)
                }
            );

            // Assert
            Assert.NotNull(result);
            Assert.Equal(transactionId, result.Id);
            Assert.Equal(updatedAmount, result.Amount);
            Assert.Equal(updatedName, result.Name);
            Assert.Equal(updatedDescription, result.Description);
            Assert.Equal(TransactionTypeEnum.Income, result.Type);
            Assert.Equal(updatedDay, result.Date.Day);
            Assert.Equal(TestDataFactory.DEFAULT_DAILY_MONTH, result.Date.Month);
            Assert.Equal(TestDataFactory.DEFAULT_YEAR, result.Date.Year);

            _transactionRepositoryMock.Verify(repo => repo.UpdateAsync(It.IsAny<Transaction>()), Times.Once);
        }

        [Fact]
        public async Task UpdateAsync_WithNonExistingId_ThrowsKeyNotFoundException()
        {
            // Arrange
            int userId = 1;
            int transactionId = 999;

            TestDataFactory.SetupAuthenticatedUser(_currentUserServiceMock, userId);

            _transactionRepositoryMock
                .Setup(repo => repo.GetByIdAsync(userId, transactionId))
                .ReturnsAsync((Transaction?)null);

            // Act & Assert
            await Assert.ThrowsAsync<KeyNotFoundException>
            (
                () => _transactionService.UpdateAsync
                (
                    transactionId,
                    new UpdateTransactionRequestDTO
                    {
                        Name = "Compra supermercado",
                        Amount = 50.00m,
                        Type = TransactionTypeEnum.Expense,
                        Date = new DateTime(2024, 6, 20)
                    }
                )
            );

            _transactionRepositoryMock.Verify(repo => repo.UpdateAsync(It.IsAny<Transaction>()), Times.Never);
        }

        // ==================== TEST: DELETE ====================

        [Fact]
        public async Task DeleteAsync_WithExistingId_DeletesTransaction()
        {
            // Arrange
            int userId = 1;
            int transactionId = 1;

            TestDataFactory.SetupAuthenticatedUser(_currentUserServiceMock, userId);

            _transactionRepositoryMock
                .Setup(repo => repo.ExistsAsync(userId, transactionId))
                .ReturnsAsync(true);

            // Act
            await _transactionService.DeleteAsync(transactionId);

            // Assert
            _transactionRepositoryMock.Verify(repo => repo.DeleteAsync(userId, transactionId), Times.Once);
        }

        [Fact]
        public async Task DeleteAsync_WithNonExistingId_ThrowsKeyNotFoundException()
        {
            // Arrange
            int userId = 1;
            int transactionId = 999;

            TestDataFactory.SetupAuthenticatedUser(_currentUserServiceMock, userId);

            _transactionRepositoryMock
                .Setup(repo => repo.ExistsAsync(userId, transactionId))
                .ReturnsAsync(false);

            // Act & Assert
            await Assert.ThrowsAsync<KeyNotFoundException>(() => _transactionService.DeleteAsync(transactionId));

            _transactionRepositoryMock.Verify(repo => repo.DeleteAsync(userId, It.IsAny<int>()), Times.Never);
        }

        // ==================== TEST: EXISTS ====================

        [Fact]
        public async Task ExistsAsync_WithExistingId_ReturnsTrue()
        {
            // Arrange
            int userId = 1;
            int transactionId = 1;

            TestDataFactory.SetupAuthenticatedUser(_currentUserServiceMock, userId);

            _transactionRepositoryMock
                .Setup(repo => repo.ExistsAsync(userId, transactionId))
                .ReturnsAsync(true);

            // Act
            bool result = await _transactionService.ExistsAsync(transactionId);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public async Task ExistsAsync_WithNonExistingId_ReturnsFalse()
        {
            // Arrange
            int userId = 1;
            int transactionId = 999;

            TestDataFactory.SetupAuthenticatedUser(_currentUserServiceMock, userId);

            _transactionRepositoryMock
                .Setup(repo => repo.ExistsAsync(userId, transactionId))
                .ReturnsAsync(false);

            // Act
            bool result = await _transactionService.ExistsAsync(transactionId);

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
            bool result = await _transactionService.ExistsAsync(invalidId);

            // Assert
            Assert.False(result);
            _transactionRepositoryMock.Verify(
                repo => repo.ExistsAsync(userId, It.IsAny<int>()),
                Times.Never);
        }

        // ==================== DISPOSE ====================

        public void Dispose()
        {
            // Limpiar recursos si es necesario
        }
    }
}