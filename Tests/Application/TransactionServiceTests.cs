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

namespace Tests.Application
{
    public class TransactionServiceTests : IDisposable
    {
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
            _transactionRepositoryMock = new Mock<ITransactionRepository>();
            _categoryRepositoryMock = new Mock<ICategoryRepository>();

            // Instanciar el servicio
            _transactionService = new TransactionService(_transactionRepositoryMock.Object, _categoryRepositoryMock.Object, _mapper);
        }

        // ==================== HELPERS ====================

        private Category CreateCategory(int id, string name)
        {
            EntityInfo info = new EntityInfo(name, null);
            Category category = new Category(info);
            typeof(Category).GetProperty("Id")?.SetValue(category, id);

            return category;
        }

        private Transaction CreateTransaction(int id, Category category, string name, decimal amount, TransactionTypeEnum type, int day, int month, int year)
        {
            EntityInfo info = new EntityInfo(name, null);
            Money money = new Money(amount);
            DailyPeriod date = new DailyPeriod(day, month, year);
            Transaction transaction = new Transaction(category, info, money, type, date);
            typeof(Transaction).GetProperty("Id")?.SetValue(transaction, id);

            return transaction;
        }

        // ==================== TEST: GET BY ID ====================

        [Fact]
        public async Task GetByIdAsync_WithExistingId_ReturnsTransaction()
        {
            // Arrange
            int transactionId = 1;
            Category category = CreateCategory(1, "Alimentación");
            Transaction transaction = CreateTransaction(transactionId, category, "Compra supermercado", 45.75m, TransactionTypeEnum.Expense, 15, 6, 2024);

            _transactionRepositoryMock
                .Setup(repo => repo.GetByIdAsync(transactionId))
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
            int transactionId = 999;

            _transactionRepositoryMock
                .Setup(repo => repo.GetByIdAsync(transactionId))
                .ReturnsAsync((Transaction)null);

            // Act & Assert
            await Assert.ThrowsAsync<KeyNotFoundException>(() => _transactionService.GetByIdAsync(transactionId));
        }

        [Fact]
        public async Task GetByIdAsync_WithInvalidId_ThrowsArgumentException()
        {
            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() => _transactionService.GetByIdAsync(0));
        }

        // ==================== TEST: GET ALL ====================

        [Fact]
        public async Task GetAllAsync_ReturnsListOfTransactions()
        {
            // Arrange
            Category category = CreateCategory(1, "Alimentación");
            Transaction transaction1 = CreateTransaction(1, category, "Compra 1", 45.75m, TransactionTypeEnum.Expense, 15, 6, 2024);
            Transaction transaction2 = CreateTransaction(2, category, "Compra 2", 30.00m, TransactionTypeEnum.Expense, 20, 6, 2024);
            List<Transaction> transactions = new List<Transaction> { transaction1, transaction2 };

            _transactionRepositoryMock
                .Setup(repo => repo.GetAllAsync())
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
            int categoryId = 1;
            Category category = CreateCategory(categoryId, "Alimentación");
            Transaction transaction1 = CreateTransaction(1, category, "Compra 1", 45.75m, TransactionTypeEnum.Expense, 15, 6, 2024);
            Transaction transaction2 = CreateTransaction(2, category, "Compra 2", 30.00m, TransactionTypeEnum.Expense, 20, 6, 2024);
            List<Transaction> transactions = new List<Transaction> { transaction1, transaction2 };

            _categoryRepositoryMock
                .Setup(repo => repo.ExistsAsync(categoryId))
                .ReturnsAsync(true);

            _transactionRepositoryMock
                .Setup(repo => repo.GetByCategoryIdAsync(categoryId))
                .ReturnsAsync(transactions);

            // Act
            List<TransactionResponseDTO> result = await _transactionService.GetByCategoryIdAsync(categoryId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count);
            _transactionRepositoryMock.Verify(repo => repo.GetByCategoryIdAsync(categoryId), Times.Once);
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
            await Assert.ThrowsAsync<KeyNotFoundException>(() => _transactionService.GetByCategoryIdAsync(categoryId));

            _transactionRepositoryMock.Verify(repo => repo.GetByCategoryIdAsync(It.IsAny<int>()), Times.Never);
        }

        // ==================== TEST: GET BY MONTHLY PERIOD ====================

        [Fact]
        public async Task GetByMonthlyPeriodAsync_ReturnsTransactionsForPeriod()
        {
            // Arrange
            int month = 6;
            int year = 2024;
            Category category = CreateCategory(1, "Alimentación");
            Transaction transaction1 = CreateTransaction(1, category, "Compra 1", 45.75m, TransactionTypeEnum.Expense, 15, month, year);
            Transaction transaction2 = CreateTransaction(2, category, "Compra 2", 30.00m, TransactionTypeEnum.Expense, 20, month, year);
            List<Transaction> transactions = new List<Transaction> { transaction1, transaction2 };

            _transactionRepositoryMock
                .Setup(repo => repo.GetByMonthlyPeriodAsync(It.IsAny<MonthlyPeriod>()))
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
            int categoryId = 1;
            int month = 6;
            int year = 2024;
            Category category = CreateCategory(categoryId, "Alimentación");
            Transaction transaction1 = CreateTransaction(1, category, "Compra 1", 45.75m, TransactionTypeEnum.Expense, 15, month, year);
            Transaction transaction2 = CreateTransaction(2, category, "Compra 2", 30.00m, TransactionTypeEnum.Expense, 20, month, year);
            List<Transaction> transactions = new List<Transaction> { transaction1, transaction2 };

            _categoryRepositoryMock
                .Setup(repo => repo.ExistsAsync(categoryId))
                .ReturnsAsync(true);

            _transactionRepositoryMock
                .Setup(repo => repo.GetByCategoryAndMonthlyPeriodAsync(categoryId, It.IsAny<MonthlyPeriod>()))
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
            int categoryId = 999;
            int month = 6;
            int year = 2024;

            _categoryRepositoryMock
                .Setup(repo => repo.ExistsAsync(categoryId))
                .ReturnsAsync(false);

            // Act & Assert
            await Assert.ThrowsAsync<KeyNotFoundException>(() => _transactionService.GetByCategoryAndMonthlyPeriodAsync(categoryId, month, year));
        }

        // ==================== TEST: GET BY DATE RANGE ====================

        [Fact]
        public async Task GetByDateRangeAsync_ReturnsTransactionsInRange()
        {
            // Arrange
            DateTime from = new DateTime(2024, 6, 1);
            DateTime to = new DateTime(2024, 6, 30);
            Category category = CreateCategory(1, "Alimentación");
            Transaction transaction1 = CreateTransaction(1, category, "Compra 1", 45.75m, TransactionTypeEnum.Expense, 15, 6, 2024);
            Transaction transaction2 = CreateTransaction(2, category, "Compra 2", 30.00m, TransactionTypeEnum.Expense, 20, 6, 2024);
            List<Transaction> transactions = new List<Transaction> { transaction1, transaction2 };

            _transactionRepositoryMock
                .Setup(repo => repo.GetByDateRangeAsync(It.IsAny<DailyPeriod>(), It.IsAny<DailyPeriod>()))
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
            DateTime from = new DateTime(2024, 6, 30);
            DateTime to = new DateTime(2024, 6, 1);

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() => _transactionService.GetByDateRangeAsync(from, to));
        }

        // ==================== TEST: GET TOTAL ====================

        [Fact]
        public async Task GetTotalByCategoryAndMonthlyPeriodAsync_ReturnsTotal()
        {
            // Arrange
            int categoryId = 1;
            int month = 6;
            int year = 2024;
            decimal expectedTotal = 75.75m;

            _categoryRepositoryMock
                .Setup(repo => repo.ExistsAsync(categoryId))
                .ReturnsAsync(true);

            _transactionRepositoryMock
                .Setup(repo => repo.GetTotalByCategoryAndMonthlyPeriodAsync(categoryId, It.IsAny<MonthlyPeriod>()))
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
            int categoryId = 999;
            int month = 6;
            int year = 2024;

            _categoryRepositoryMock
                .Setup(repo => repo.ExistsAsync(categoryId))
                .ReturnsAsync(false);

            // Act & Assert
            await Assert.ThrowsAsync<KeyNotFoundException>(() => _transactionService.GetTotalByCategoryAndMonthlyPeriodAsync(categoryId, month, year));
        }

        // ==================== TEST: CREATE ====================

        [Fact]
        public async Task CreateAsync_WithValidData_ReturnsCreatedTransaction()
        {
            // Arrange
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

            Category category = CreateCategory(categoryId, "Alimentación");

            _categoryRepositoryMock
                .Setup(repo => repo.GetByIdAsync(categoryId, It.IsAny<bool>()))
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
            int categoryId = 999;
            CreateTransactionRequestDTO request = new CreateTransactionRequestDTO
            {
                CategoryId = categoryId,
                Name = "Compra supermercado",
                Amount = 45.75m,
                Type = TransactionTypeEnum.Expense,
                Date = new DateTime(2024, 6, 15)
            };

            _categoryRepositoryMock
                .Setup(repo => repo.GetByIdAsync(categoryId, It.IsAny<bool>()))
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
            int transactionId = 1;
            Category category = CreateCategory(1, "Alimentación");
            Transaction transaction = CreateTransaction(transactionId, category, "Compra supermercado", 45.75m, TransactionTypeEnum.Expense, 15, 6, 2024);

            UpdateTransactionRequestDTO request = new UpdateTransactionRequestDTO
            {
                Name = "Compra supermercado actualizada",
                Description = "Carrefour 20/06/2024",
                Amount = 50.00m,
                Type = TransactionTypeEnum.Income,
                Date = new DateTime(2024, 6, 20)
            };

            _transactionRepositoryMock
                .Setup(repo => repo.GetByIdAsync(transactionId))
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
            int transactionId = 999;
            UpdateTransactionRequestDTO request = new UpdateTransactionRequestDTO
            {
                Name = "Compra supermercado",
                Amount = 50.00m,
                Type = TransactionTypeEnum.Expense,
                Date = new DateTime(2024, 6, 20)
            };

            _transactionRepositoryMock
                .Setup(repo => repo.GetByIdAsync(transactionId))
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
            int transactionId = 1;

            _transactionRepositoryMock
                .Setup(repo => repo.ExistsAsync(transactionId))
                .ReturnsAsync(true);

            // Act
            await _transactionService.DeleteAsync(transactionId);

            // Assert
            _transactionRepositoryMock.Verify(repo => repo.DeleteAsync(transactionId), Times.Once);
        }

        [Fact]
        public async Task DeleteAsync_WithNonExistingId_ThrowsKeyNotFoundException()
        {
            // Arrange
            int transactionId = 999;

            _transactionRepositoryMock
                .Setup(repo => repo.ExistsAsync(transactionId))
                .ReturnsAsync(false);

            // Act & Assert
            await Assert.ThrowsAsync<KeyNotFoundException>(() => _transactionService.DeleteAsync(transactionId));

            _transactionRepositoryMock.Verify(repo => repo.DeleteAsync(It.IsAny<int>()), Times.Never);
        }

        // ==================== TEST: EXISTS ====================

        [Fact]
        public async Task ExistsAsync_WithExistingId_ReturnsTrue()
        {
            // Arrange
            int transactionId = 1;

            _transactionRepositoryMock
                .Setup(repo => repo.ExistsAsync(transactionId))
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
            int transactionId = 999;

            _transactionRepositoryMock
                .Setup(repo => repo.ExistsAsync(transactionId))
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
            int invalidId = 0;

            // Act
            bool result = await _transactionService.ExistsAsync(invalidId);

            // Assert
            Assert.False(result);
            _transactionRepositoryMock.Verify(
                repo => repo.ExistsAsync(It.IsAny<int>()),
                Times.Never);
        }

        // ==================== DISPOSE ====================

        public void Dispose()
        {
            // Limpiar recursos si es necesario
        }
    }
}