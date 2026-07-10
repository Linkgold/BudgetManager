using Application.DTOs.Request;
using Application.DTOs.Response;
using Application.Interfaces;
using Application.Mappings;
using Application.Services;
using AutoMapper;
using Domain.Entities;
using Domain.Enums;
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

            User user = TestDataFactory.CreateUser(userId);
            Category category = TestDataFactory.CreateCategory();
            Transaction transaction = TestDataFactory.CreateTransaction(transactionId, user, category, "Compra supermercado");

            TestDataFactory.SetupAuthenticatedUser(_currentUserServiceMock, userId);

            _transactionRepositoryMock
                .Setup(repo => repo.GetByIdAsync(transactionId, userId))
                .ReturnsAsync(transaction);

            // Act
            TransactionResponseDTO result = await _transactionService.GetByIdAsync(transactionId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(transactionId, result.Id);
            Assert.Equal(45.75m, result.Amount);
            Assert.Equal(TransactionTypeEnum.Expense, result.Type);
            Assert.Equal(15, result.Date.Day);
            Assert.Equal(6, result.Date.Month);
            Assert.Equal(2024, result.Date.Year);
            Assert.Equal("Alimentación", result.CategoryName);
        }

        [Fact]
        public async Task GetByIdAsync_WithNonExistingId_ThrowsKeyNotFoundException()
        {
            // Arrange
            int userId = 1;
            int transactionId = 999;

            TestDataFactory.SetupAuthenticatedUser(_currentUserServiceMock, userId);

            _transactionRepositoryMock
                .Setup(repo => repo.GetByIdAsync(transactionId, userId))
                .ReturnsAsync((Transaction)null);

            // Act & Assert
            await Assert.ThrowsAsync<KeyNotFoundException>(() => _transactionService.GetByIdAsync(transactionId));
        }

        [Fact]
        public async Task GetByIdAsync_WithInvalidId_ThrowsArgumentException()
        {
            int userId = 1;

            TestDataFactory.SetupAuthenticatedUser(_currentUserServiceMock, userId);

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() => _transactionService.GetByIdAsync(0));
        }

        // ==================== TEST: GET ALL ====================

        [Fact]
        public async Task GetAllAsync_ReturnsListOfTransactions()
        {
            // Arrange
            int userId = 1;
            User user = TestDataFactory.CreateUser(userId);
            Category category = TestDataFactory.CreateCategory();
            Transaction transaction1 = TestDataFactory.CreateTransaction(1, user, category, "Compra 1", "", 45.75m, TransactionTypeEnum.Expense, 15, 6, 2024);
            Transaction transaction2 = TestDataFactory.CreateTransaction(2, user, category, "Compra 2", "", 30.00m, TransactionTypeEnum.Expense, 20, 6, 2024);
            List<Transaction> transactions = new List<Transaction> { transaction1, transaction2 };

            TestDataFactory.SetupAuthenticatedUser(_currentUserServiceMock, userId);

            _transactionRepositoryMock
                .Setup(repo => repo.GetAllAsync(userId))
                .ReturnsAsync(transactions);

            // Act
            List<TransactionResponseDTO> result = await _transactionService.GetAllAsync();

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count);
            Assert.Equal(45.75m, result[0].Amount);
            Assert.Equal(30.00m, result[1].Amount);
        }

        // ==================== TEST: GET BY CATEGORY ====================

        [Fact]
        public async Task GetByCategoryIdAsync_WithExistingCategory_ReturnsTransactions()
        {
            // Arrange
            int userId = 1;
            int categoryId = 1;
            User user = TestDataFactory.CreateUser(userId);
            Category category = TestDataFactory.CreateCategory();
            Transaction transaction1 = TestDataFactory.CreateTransaction(1, user, category, "Compra 1", "", 45.75m, TransactionTypeEnum.Expense, 15, 6, 2024);
            Transaction transaction2 = TestDataFactory.CreateTransaction(2, user, category, "Compra 2", "", 30.00m, TransactionTypeEnum.Expense, 20, 6, 2024);
            List<Transaction> transactions = new List<Transaction> { transaction1, transaction2 };

            TestDataFactory.SetupAuthenticatedUser(_currentUserServiceMock, userId);

            _categoryRepositoryMock
                .Setup(repo => repo.ExistsAsync(categoryId, userId))
                .ReturnsAsync(true);

            _transactionRepositoryMock
                .Setup(repo => repo.GetByCategoryIdAsync(categoryId, userId))
                .ReturnsAsync(transactions);

            // Act
            List<TransactionResponseDTO> result = await _transactionService.GetByCategoryIdAsync(categoryId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count);
            _transactionRepositoryMock.Verify(repo => repo.GetByCategoryIdAsync(categoryId, userId), Times.Once);
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
            await Assert.ThrowsAsync<KeyNotFoundException>(() => _transactionService.GetByCategoryIdAsync(categoryId));

            _transactionRepositoryMock.Verify(repo => repo.GetByCategoryIdAsync(It.IsAny<int>(), userId), Times.Never);
        }

        // ==================== TEST: GET BY MONTHLY PERIOD ====================

        [Fact]
        public async Task GetByMonthlyPeriodAsync_ReturnsTransactionsForPeriod()
        {
            // Arrange
            int userId = 1;
            int month = 6;
            int year = 2024;
            User user = TestDataFactory.CreateUser(userId);
            Category category = TestDataFactory.CreateCategory();
            Transaction transaction1 = TestDataFactory.CreateTransaction(1, user, category, "Compra 1", "", 45.75m, TransactionTypeEnum.Expense, 15, month, year);
            Transaction transaction2 = TestDataFactory.CreateTransaction(2, user, category, "Compra 2", "", 30.00m, TransactionTypeEnum.Expense, 20, month, year);
            List<Transaction> transactions = new List<Transaction> { transaction1, transaction2 };

            TestDataFactory.SetupAuthenticatedUser(_currentUserServiceMock, userId);

            _transactionRepositoryMock
                .Setup(repo => repo.GetByMonthlyPeriodAsync(It.IsAny<MonthlyPeriod>(), userId))
                .ReturnsAsync(transactions);

            // Act
            List<TransactionResponseDTO> result = await _transactionService.GetByMonthlyPeriodAsync(month, year);

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
            int month = 6;
            int year = 2024;
            User user = TestDataFactory.CreateUser(userId);
            Category category = TestDataFactory.CreateCategory();
            Transaction transaction1 = TestDataFactory.CreateTransaction(1, user, category, "Compra 1", "", 45.75m, TransactionTypeEnum.Expense, 15, month, year);
            Transaction transaction2 = TestDataFactory.CreateTransaction(2, user, category, "Compra 2", "", 30.00m, TransactionTypeEnum.Expense, 20, month, year);
            List<Transaction> transactions = new List<Transaction> { transaction1, transaction2 };

            TestDataFactory.SetupAuthenticatedUser(_currentUserServiceMock, userId);

            _categoryRepositoryMock
                .Setup(repo => repo.ExistsAsync(categoryId, userId))
                .ReturnsAsync(true);

            _transactionRepositoryMock
                .Setup(repo => repo.GetByCategoryAndMonthlyPeriodAsync(categoryId, It.IsAny<MonthlyPeriod>(), userId))
                .ReturnsAsync(transactions);

            // Act
            List<TransactionResponseDTO> result = await _transactionService.GetByCategoryAndMonthlyPeriodAsync(categoryId, month, year);

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
            int month = 6;
            int year = 2024;

            TestDataFactory.SetupAuthenticatedUser(_currentUserServiceMock, userId);

            _categoryRepositoryMock
                .Setup(repo => repo.ExistsAsync(categoryId, userId))
                .ReturnsAsync(false);

            // Act & Assert
            await Assert.ThrowsAsync<KeyNotFoundException>(() => _transactionService.GetByCategoryAndMonthlyPeriodAsync(categoryId, month, year));
        }

        // ==================== TEST: GET BY DATE RANGE ====================

        [Fact]
        public async Task GetByDateRangeAsync_ReturnsTransactionsInRange()
        {
            // Arrange
            int userId = 1;
            DateTime from = new DateTime(2024, 6, 1);
            DateTime to = new DateTime(2024, 6, 30);
            User user = TestDataFactory.CreateUser(userId);
            Category category = TestDataFactory.CreateCategory();
            Transaction transaction1 = TestDataFactory.CreateTransaction(1, user, category, "Compra 1", "", 45.75m, TransactionTypeEnum.Expense, 15, 6, 2024);
            Transaction transaction2 = TestDataFactory.CreateTransaction(2, user, category, "Compra 2", "", 30.00m, TransactionTypeEnum.Expense, 20, 6, 2024);
            List<Transaction> transactions = new List<Transaction> { transaction1, transaction2 };

            TestDataFactory.SetupAuthenticatedUser(_currentUserServiceMock, userId);

            _transactionRepositoryMock
                .Setup(repo => repo.GetByDateRangeAsync(It.IsAny<DailyPeriod>(), It.IsAny<DailyPeriod>(), userId))
                .ReturnsAsync(transactions);

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
            int month = 6;
            int year = 2024;
            decimal expectedTotal = 75.75m;

            TestDataFactory.SetupAuthenticatedUser(_currentUserServiceMock, userId);

            _categoryRepositoryMock
                .Setup(repo => repo.ExistsAsync(categoryId, userId))
                .ReturnsAsync(true);

            _transactionRepositoryMock
                .Setup(repo => repo.GetTotalByCategoryAndMonthlyPeriodAsync(categoryId, It.IsAny<MonthlyPeriod>(), userId))
                .ReturnsAsync(expectedTotal);

            // Act
            decimal result = await _transactionService.GetTotalByCategoryAndMonthlyPeriodAsync(categoryId, month, year);

            // Assert
            Assert.Equal(expectedTotal, result);
        }

        [Fact]
        public async Task GetTotalByCategoryAndMonthlyPeriodAsync_WithNonExistingCategory_ThrowsKeyNotFoundException()
        {
            // Arrange
            int userId = 1;
            int categoryId = 999;
            int month = 6;
            int year = 2024;

            TestDataFactory.SetupAuthenticatedUser(_currentUserServiceMock, userId);

            _categoryRepositoryMock
                .Setup(repo => repo.ExistsAsync(categoryId, userId))
                .ReturnsAsync(false);

            // Act & Assert
            await Assert.ThrowsAsync<KeyNotFoundException>(() => _transactionService.GetTotalByCategoryAndMonthlyPeriodAsync(categoryId, month, year));
        }

        // ==================== TEST: CREATE ====================

        [Fact]
        public async Task CreateAsync_WithValidData_ReturnsCreatedTransaction()
        {
            // Arrange
            int userId = 1;
            int categoryId = 1;
            CreateTransactionRequestDTO request = new CreateTransactionRequestDTO
            {
                CategoryId = categoryId,
                Name = "Compra supermercado",
                Description = "Carrefour 15/06/2024",
                Amount = 45.75m,
                Type = TransactionTypeEnum.Expense,
                Date = new DateTime(2024, 6, 15)
            };

            User user = TestDataFactory.CreateUser(userId);
            Category category = TestDataFactory.CreateCategory();

            TestDataFactory.SetupAuthenticatedUser(_currentUserServiceMock, userId);

            _categoryRepositoryMock
                .Setup(repo => repo.GetByIdAsync(categoryId, userId, It.IsAny<bool>()))
                .ReturnsAsync(category);

            // Act
            TransactionResponseDTO result = await _transactionService.CreateAsync(request);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(45.75m, result.Amount);
            Assert.Equal(TransactionTypeEnum.Expense, result.Type);
            Assert.Equal(15, result.Date.Day);
            Assert.Equal(6, result.Date.Month);
            Assert.Equal(2024, result.Date.Year);
            Assert.Equal("Alimentación", result.CategoryName);

            _transactionRepositoryMock.Verify(repo => repo.AddAsync(It.IsAny<Transaction>()), Times.Once);
        }

        [Fact]
        public async Task CreateAsync_WithNonExistingCategory_ThrowsKeyNotFoundException()
        {
            // Arrange
            int userId = 1;
            int categoryId = 999;
            CreateTransactionRequestDTO request = new CreateTransactionRequestDTO
            {
                CategoryId = categoryId,
                Name = "Compra supermercado",
                Amount = 45.75m,
                Type = TransactionTypeEnum.Expense,
                Date = new DateTime(2024, 6, 15)
            };

            TestDataFactory.SetupAuthenticatedUser(_currentUserServiceMock, userId);

            _categoryRepositoryMock
                .Setup(repo => repo.GetByIdAsync(categoryId, userId, It.IsAny<bool>()))
                .ReturnsAsync((Category)null);

            // Act & Assert
            await Assert.ThrowsAsync<KeyNotFoundException>(() => _transactionService.CreateAsync(request));

            _transactionRepositoryMock.Verify(repo => repo.AddAsync(It.IsAny<Transaction>()), Times.Never);
        }

        // ==================== TEST: UPDATE ====================

        [Fact]
        public async Task UpdateAsync_WithValidData_ReturnsUpdatedTransaction()
        {
            // Arrange
            int userId = 1;
            int transactionId = 1;

            User user = TestDataFactory.CreateUser(userId);
            Category category = TestDataFactory.CreateCategory();
            Transaction transaction = TestDataFactory.CreateTransaction(transactionId, user, category, "Compra supermercado", "", 45.75m, TransactionTypeEnum.Expense, 15, 6, 2024);

            UpdateTransactionRequestDTO request = new UpdateTransactionRequestDTO
            {
                Name = "Compra supermercado actualizada",
                Description = "Carrefour 20/06/2024",
                Amount = 50.00m,
                Type = TransactionTypeEnum.Income,
                Date = new DateTime(2024, 6, 20)
            };

            TestDataFactory.SetupAuthenticatedUser(_currentUserServiceMock, userId);

            _transactionRepositoryMock
                .Setup(repo => repo.GetByIdAsync(transactionId, userId))
                .ReturnsAsync(transaction);

            // Act
            TransactionResponseDTO result = await _transactionService.UpdateAsync(transactionId, request);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(transactionId, result.Id);
            Assert.Equal(50.00m, result.Amount);
            Assert.Equal(TransactionTypeEnum.Income, result.Type);
            Assert.Equal(20, result.Date.Day);
            Assert.Equal(6, result.Date.Month);
            Assert.Equal(2024, result.Date.Year);

            _transactionRepositoryMock.Verify(repo => repo.UpdateAsync(It.IsAny<Transaction>()), Times.Once);
        }

        [Fact]
        public async Task UpdateAsync_WithNonExistingId_ThrowsKeyNotFoundException()
        {
            // Arrange
            int userId = 1;
            int transactionId = 999;
            UpdateTransactionRequestDTO request = new UpdateTransactionRequestDTO
            {
                Name = "Compra supermercado",
                Amount = 50.00m,
                Type = TransactionTypeEnum.Expense,
                Date = new DateTime(2024, 6, 20)
            };

            TestDataFactory.SetupAuthenticatedUser(_currentUserServiceMock, userId);

            _transactionRepositoryMock
                .Setup(repo => repo.GetByIdAsync(transactionId, userId))
                .ReturnsAsync((Transaction)null);

            // Act & Assert
            await Assert.ThrowsAsync<KeyNotFoundException>(() => _transactionService.UpdateAsync(transactionId, request));

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
                .Setup(repo => repo.ExistsAsync(transactionId, userId))
                .ReturnsAsync(true);

            // Act
            await _transactionService.DeleteAsync(transactionId);

            // Assert
            _transactionRepositoryMock.Verify(repo => repo.DeleteAsync(transactionId, userId), Times.Once);
        }

        [Fact]
        public async Task DeleteAsync_WithNonExistingId_ThrowsKeyNotFoundException()
        {
            // Arrange
            int userId = 1;
            int transactionId = 999;

            TestDataFactory.SetupAuthenticatedUser(_currentUserServiceMock, userId);

            _transactionRepositoryMock
                .Setup(repo => repo.ExistsAsync(transactionId, userId))
                .ReturnsAsync(false);

            // Act & Assert
            await Assert.ThrowsAsync<KeyNotFoundException>(() => _transactionService.DeleteAsync(transactionId));

            _transactionRepositoryMock.Verify(repo => repo.DeleteAsync(It.IsAny<int>(), userId), Times.Never);
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
                .Setup(repo => repo.ExistsAsync(transactionId, userId))
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
                .Setup(repo => repo.ExistsAsync(transactionId, userId))
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
                repo => repo.ExistsAsync(It.IsAny<int>(), userId),
                Times.Never);
        }

        // ==================== DISPOSE ====================

        public void Dispose()
        {
            // Limpiar recursos si es necesario
        }
    }
}