using Contracts.Enums;
using Shared.DTOs.Request;
using Shared.DTOs.Response;
using System.Globalization;
using System.Net;
using Tests.API.Fixtures;
using Tests.Helpers;

namespace Tests.API.Controllers
{
    /// <summary>
    /// Pruebas de integración para TransactionController
    /// </summary>
    [Collection("ApiTestCollection")]
    public class TransactionControllerTests : IClassFixture<ApiTestFixture>, IDisposable
    {
        private readonly HttpClient _client;
        private readonly ApiTestFixture _fixture;


        public TransactionControllerTests(ApiTestFixture fixture)
        {
            _fixture = fixture;
            _client = fixture.Client;
        }

        public void Dispose()
        {
            _fixture.ClearDatabase();
        }

        // ==================== TEST: CREATE ====================

        [Fact]
        public async Task Create_WithValidExpense_ReturnsCreatedTransaction()
        {
            // Arrange
            int categoryId = await TestDataFactory.CreateCategoryAsync(_fixture, "Alimentación");

            CreateTransactionRequestDTO request = new CreateTransactionRequestDTO
            {
                CategoryId = categoryId,
                Name = "Compra supermercado",
                Description = "Carrefour 15/06/2024",
                Amount = 45.75m,
                Currency = "EUR",
                Date = new DateTime(2024, 6, 15)
            };

            StringContent content = _fixture.SerializeRequest(request);

            // Act
            HttpResponseMessage response = await _client.PostAsync("/api/transaction", content);

            // Assert
            Assert.Equal(HttpStatusCode.Created, response.StatusCode);

            string responseContent = await response.Content.ReadAsStringAsync();
            TransactionResponseDTO? transaction = _fixture.DeserializeResponse<TransactionResponseDTO?>(responseContent);

            Assert.NotNull(transaction);
            Assert.Equal(categoryId, transaction.CategoryId);
            Assert.Equal(45.75m, transaction.Amount);
            Assert.Equal("EUR", transaction.Currency);
            Assert.Equal(15, transaction.Date.Day);
            Assert.Equal(6, transaction.Date.Month);
            Assert.Equal(2024, transaction.Date.Year);
            Assert.Equal("Alimentación", transaction.CategoryName);
        }

        [Fact]
        public async Task Create_WithValidIncome_ReturnsCreatedTransaction()
        {
            // Arrange
            int categoryId = await TestDataFactory.CreateCategoryAsync(_fixture, "Salario");

            CreateTransactionRequestDTO request = new CreateTransactionRequestDTO
            {
                CategoryId = categoryId,
                Name = "Salario",
                Description = "Nómina junio 2024",
                Amount = 1500.00m,
                Date = new DateTime(2024, 6, 1)
            };

            StringContent content = _fixture.SerializeRequest(request);

            // Act
            HttpResponseMessage response = await _client.PostAsync("/api/transaction", content);

            // Assert
            Assert.Equal(HttpStatusCode.Created, response.StatusCode);

            string responseContent = await response.Content.ReadAsStringAsync();
            TransactionResponseDTO? transaction = _fixture.DeserializeResponse<TransactionResponseDTO?>(responseContent);

            Assert.NotNull(transaction);
            Assert.Equal(categoryId, transaction.CategoryId);
            Assert.Equal(1500.00m, transaction.Amount);
            Assert.Equal(1, transaction.Date.Day);
            Assert.Equal(6, transaction.Date.Month);
            Assert.Equal(2024, transaction.Date.Year);
            Assert.Equal("Salario", transaction.CategoryName);
        }

        [Fact]
        public async Task Create_WithNonExistingCategory_ReturnsNotFound()
        {
            // Arrange
            CreateTransactionRequestDTO request = new CreateTransactionRequestDTO
            {
                CategoryId = 999,
                Name = "Compra supermercado",
                Amount = 45.75m,
                Date = new DateTime(2024, 6, 15)
            };

            StringContent content = _fixture.SerializeRequest(request);

            // Act
            HttpResponseMessage response = await _client.PostAsync("/api/transaction", content);

            // Assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        // ==================== TEST: GET BY ID ====================

        [Fact]
        public async Task GetById_WithExistingId_ReturnsTransaction()
        {
            // Arrange
            int categoryId = await TestDataFactory.CreateCategoryAsync(_fixture, "Alimentación");
            int transactionId = await TestDataFactory.CreateTransactionAsync(_fixture, categoryId, "Compra supermercado", 45.75m, "EUR", 15, 6, 2024);

            // Act
            HttpResponseMessage response = await _client.GetAsync($"/api/transaction/{transactionId}");

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            string responseContent = await response.Content.ReadAsStringAsync();
            TransactionResponseDTO? transaction = _fixture.DeserializeResponse<TransactionResponseDTO?>(responseContent);

            Assert.NotNull(transaction);
            Assert.Equal(transactionId, transaction.Id);
            Assert.Equal(45.75m, transaction.Amount);
            Assert.Equal(15, transaction.Date.Day);
            Assert.Equal(6, transaction.Date.Month);
            Assert.Equal(2024, transaction.Date.Year);
            Assert.Equal("Alimentación", transaction.CategoryName);
            Assert.Equal(CategoryNatureEnum.Expense, transaction.CategoryNature);
        }

        [Fact]
        public async Task GetById_WithNonExistingId_ReturnsNotFound()
        {
            // Act
            HttpResponseMessage response = await _client.GetAsync("/api/transaction/999");

            // Assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        // ==================== TEST: GET ALL ====================

        [Fact]
        public async Task GetAll_ReturnsOkWithTransactions()
        {
            // Arrange
            int categoryId = await TestDataFactory.CreateCategoryAsync(_fixture, "Alimentación");
            await TestDataFactory.CreateTransactionAsync(_fixture, categoryId, "Compra 1", 45.75m, "EUR", 15, 6, 2024);
            await TestDataFactory.CreateTransactionAsync(_fixture, categoryId, "Compra 2", 30.00m, "EUR", 20, 6, 2024);

            // Act
            HttpResponseMessage response = await _client.GetAsync("/api/transaction");

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            string responseContent = await response.Content.ReadAsStringAsync();
            List<TransactionResponseDTO>? transactions = _fixture.DeserializeResponse<List<TransactionResponseDTO>?>(responseContent);

            Assert.NotNull(transactions);
            Assert.NotEmpty(transactions);
            Assert.All(transactions, t => Assert.Equal(CategoryNatureEnum.Expense, t.CategoryNature));
            Assert.Equal(2, transactions.Count);
        }

        // ==================== TEST: GET BY CATEGORY ====================

        [Fact]
        public async Task GetByCategory_WithExistingCategory_ReturnsTransactions()
        {
            // Arrange
            int categoryId1 = await TestDataFactory.CreateCategoryAsync(_fixture, "Alimentación");
            int categoryId2 = await TestDataFactory.CreateCategoryAsync(_fixture, "Transporte");

            await TestDataFactory.CreateTransactionAsync(_fixture, categoryId1, "Compra 1", 45.75m, "EUR", 15, 6, 2024);
            await TestDataFactory.CreateTransactionAsync(_fixture, categoryId1, "Compra 2", 30.00m, "EUR", 20, 6, 2024);
            await TestDataFactory.CreateTransactionAsync(_fixture, categoryId2, "Gasolina", 50.00m, "EUR", 10, 6, 2024);

            // Act
            HttpResponseMessage response = await _client.GetAsync($"/api/transaction/by-category/{categoryId1}");

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            string responseContent = await response.Content.ReadAsStringAsync();
            List<TransactionResponseDTO>? transactions = _fixture.DeserializeResponse<List<TransactionResponseDTO>?>(responseContent);

            Assert.NotNull(transactions);
            Assert.Equal(2, transactions.Count);
            Assert.All(transactions, t => Assert.Equal(categoryId1, t.CategoryId));
        }

        [Fact]
        public async Task GetByCategory_WithNonExistingCategory_ReturnsNotFound()
        {
            // Act
            HttpResponseMessage response = await _client.GetAsync("/api/transaction/by-category/999");

            // Assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        // ==================== TEST: GET BY MONTHLY PERIOD ====================

        [Fact]
        public async Task GetByMonthlyPeriod_ReturnsTransactionsForPeriod()
        {
            // Arrange
            int categoryId = await TestDataFactory.CreateCategoryAsync(_fixture, "Alimentación");
            await TestDataFactory.CreateTransactionAsync(_fixture, categoryId, "Compra 1", 45.75m, "EUR", 15, 6, 2024);
            await TestDataFactory.CreateTransactionAsync(_fixture, categoryId, "Compra 2", 30.00m, "EUR", 20, 6, 2024);
            await TestDataFactory.CreateTransactionAsync(_fixture, categoryId, "Compra 3", 20.00m, "EUR", 10, 7, 2024);

            // Act
            HttpResponseMessage response = await _client.GetAsync("/api/transaction/by-monthly-period?month=6&year=2024");

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            string responseContent = await response.Content.ReadAsStringAsync();
            List<TransactionResponseDTO>? transactions = _fixture.DeserializeResponse<List<TransactionResponseDTO>?>(responseContent);

            Assert.NotNull(transactions);
            Assert.Equal(2, transactions.Count);
            Assert.All(transactions, t => Assert.Equal(6, t.Date.Month));
            Assert.All(transactions, t => Assert.Equal(2024, t.Date.Year));
        }

        // ==================== TEST: GET BY CATEGORY AND MONTHLY PERIOD ====================

        [Fact]
        public async Task GetByCategoryAndMonthlyPeriod_WithExistingCategoryAndPeriod_ReturnsTransactions()
        {
            // Arrange
            int categoryId1 = await TestDataFactory.CreateCategoryAsync(_fixture, "Alimentación");
            int categoryId2 = await TestDataFactory.CreateCategoryAsync(_fixture, "Transporte");

            await TestDataFactory.CreateTransactionAsync(_fixture, categoryId1, "Compra 1", 45.75m, "EUR", 15, 6, 2024);
            await TestDataFactory.CreateTransactionAsync(_fixture, categoryId1, "Compra 2", 30.00m, "EUR", 20, 6, 2024);
            await TestDataFactory.CreateTransactionAsync(_fixture, categoryId2, "Gasolina", 50.00m, "EUR", 10, 6, 2024);

            // Act
            HttpResponseMessage response = await _client.GetAsync(
                $"/api/transaction/by-category-monthly-period?categoryId={categoryId1}&month=6&year=2024");

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            string responseContent = await response.Content.ReadAsStringAsync();
            List<TransactionResponseDTO>? transactions = _fixture.DeserializeResponse<List<TransactionResponseDTO>?>(responseContent);

            Assert.NotNull(transactions);
            Assert.Equal(2, transactions.Count);
            Assert.All(transactions, t => Assert.Equal(categoryId1, t.CategoryId));
            Assert.All(transactions, t => Assert.Equal(6, t.Date.Month));
            Assert.All(transactions, t => Assert.Equal(2024, t.Date.Year));
        }

        [Fact]
        public async Task GetByCategoryAndMonthlyPeriod_WithNonExistingCategory_ReturnsNotFound()
        {
            // Act
            HttpResponseMessage response = await _client.GetAsync("/api/transaction/by-category-monthly-period?categoryId=999&month=6&year=2024");

            // Assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        // ==================== TEST: GET BY DATE RANGE ====================

        [Fact]
        public async Task GetByDateRange_WithExistingRange_ReturnsTransactions()
        {
            // Arrange
            int categoryId = await TestDataFactory.CreateCategoryAsync(_fixture, "Alimentación");
            await TestDataFactory.CreateTransactionAsync(_fixture, categoryId, "Compra 1", 45.75m, "EUR", 15, 6, 2024);
            await TestDataFactory.CreateTransactionAsync(_fixture, categoryId, "Compra 2", 30.00m, "EUR", 20, 6, 2024);
            await TestDataFactory.CreateTransactionAsync(_fixture, categoryId, "Compra 3", 20.00m, "EUR", 10, 7, 2024);

            // Act
            HttpResponseMessage response = await _client.GetAsync("/api/transaction/by-date-range?from=2024-06-01&to=2024-06-30");

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            string responseContent = await response.Content.ReadAsStringAsync();
            List<TransactionResponseDTO>? transactions = _fixture.DeserializeResponse<List<TransactionResponseDTO>?>(responseContent);

            Assert.NotNull(transactions);
            Assert.Equal(2, transactions.Count);
            Assert.All(transactions, t => Assert.Equal(6, t.Date.Month));
            Assert.All(transactions, t => Assert.Equal(2024, t.Date.Year));
        }

        [Fact]
        public async Task GetByDateRange_WithInvalidRange_ReturnsBadRequest()
        {
            // Act
            HttpResponseMessage response = await _client.GetAsync("/api/transaction/by-date-range?from=2024-06-30&to=2024-06-01");

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        // ==================== TEST: GET TOTAL ====================

        [Fact]
        public async Task GetTotalByCategoryAndMonthlyPeriod_ReturnsTotal()
        {
            // Arrange
            int categoryId = await TestDataFactory.CreateCategoryAsync(_fixture, "Alimentación");
            await TestDataFactory.CreateTransactionAsync(_fixture, categoryId, "Compra 1", 45.75m, "EUR", 15, 6, 2024);
            await TestDataFactory.CreateTransactionAsync(_fixture, categoryId, "Compra 2", 30.00m, "EUR", 20, 6, 2024);

            // Act
            HttpResponseMessage response = await _client.GetAsync($"/api/transaction/total-by-category-monthly-period?categoryId={categoryId}&month=6&year=2024");

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            string responseContent = await response.Content.ReadAsStringAsync();
            Dictionary<string, object>? result = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, object>>(responseContent);

            Assert.NotNull(result);
            Assert.True(result.ContainsKey("total"));
            decimal total = Convert.ToDecimal(result["total"].ToString(), CultureInfo.InvariantCulture);

            Assert.Equal(75.75m, total);
        }

        [Fact]
        public async Task GetTotalByCategoryAndMonthlyPeriod_WithNonExistingCategory_ReturnsNotFound()
        {
            // Act
            HttpResponseMessage response = await _client.GetAsync("/api/transaction/total-by-category-monthly-period?categoryId=999&month=6&year=2024");

            // Assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        // ==================== TEST: UPDATE ====================

        [Fact]
        public async Task Update_WithValidData_ReturnsUpdatedTransaction()
        {
            // Arrange
            int categoryId = await TestDataFactory.CreateCategoryAsync(_fixture, "Alimentación");
            int updatedCategoryId = await TestDataFactory.CreateCategoryAsync(_fixture, "Suscripción");
            int transactionId = await TestDataFactory.CreateTransactionAsync(_fixture, categoryId, "Compra supermercado", 45.75m, "EUR", 15, 6, 2024);

            UpdateTransactionRequestDTO request = new UpdateTransactionRequestDTO
            {
                CategoryId = updatedCategoryId,
                Name = "Compra actualizada",
                Description = "Carrefour 20/06/2024",
                Amount = 50.00m,
                TransactionType = TransactionTypeEnum.Expense,
                Currency = "CNY",
                Date = new DateTime(2024, 6, 20)
            };

            StringContent content = _fixture.SerializeRequest(request);

            // Act
            HttpResponseMessage response = await _client.PutAsync($"/api/transaction/{transactionId}", content);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            string responseContent = await response.Content.ReadAsStringAsync();
            TransactionResponseDTO? transaction = _fixture.DeserializeResponse<TransactionResponseDTO?>(responseContent);

            Assert.NotNull(transaction);
            Assert.Equal(transactionId, transaction.Id);
            Assert.Equal(updatedCategoryId, transaction.CategoryId);
            Assert.Equal(50.00m, transaction.Amount);
            Assert.Equal("CNY", transaction.Currency);
            Assert.Equal(20, transaction.Date.Day);
            Assert.Equal(6, transaction.Date.Month);
            Assert.Equal(2024, transaction.Date.Year);
        }

        [Fact]
        public async Task Update_WithNonExistingId_ReturnsNotFound()
        {
            // Arrange
            UpdateTransactionRequestDTO request = new UpdateTransactionRequestDTO
            {
                Name = "Compra actualizada",
                Amount = 50.00m,
                TransactionType = TransactionTypeEnum.Expense,
                Date = new DateTime(2024, 6, 20)
            };

            StringContent content = _fixture.SerializeRequest(request);

            // Act
            HttpResponseMessage response = await _client.PutAsync("/api/transaction/999", content);

            // Assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        // ==================== TEST: DELETE ====================

        [Fact]
        public async Task Delete_WithExistingId_ReturnsNoContent()
        {
            // Arrange
            int categoryId = await TestDataFactory.CreateCategoryAsync(_fixture, "Alimentación");
            int transactionId = await TestDataFactory.CreateTransactionAsync(_fixture, categoryId, "Compra supermercado", 45.75m, "EUR", 15, 6, 2024);

            // Act
            HttpResponseMessage response = await _client.DeleteAsync($"/api/transaction/{transactionId}");

            // Assert
            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        }

        [Fact]
        public async Task Delete_WithNonExistingId_ReturnsNotFound()
        {
            // Act
            HttpResponseMessage response = await _client.DeleteAsync("/api/transaction/999");

            // Assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }
    }
}