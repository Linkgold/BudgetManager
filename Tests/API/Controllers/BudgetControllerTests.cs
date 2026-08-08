using Contracts.Enums;
using Microsoft.AspNetCore.Http;
using Shared.DTOs.Request;
using Shared.DTOs.Response;
using System.Net;
using Tests.API.Fixtures;
using Tests.Helpers;

namespace Tests.API.Controllers
{
    /// <summary>
    /// Pruebas de integración para BudgetController
    /// </summary>
    [Collection("ApiTestCollection")]
    public class BudgetControllerTests : IClassFixture<ApiTestFixture>, IDisposable
    {
        private readonly ApiTestFixture _fixture;
        private readonly HttpClient _client;

        public BudgetControllerTests(ApiTestFixture fixture)
        {
            _fixture = fixture;
            _client = fixture.Client;
        }

        public void Dispose()
        {
            _fixture.ClearDatabase();
        }

        // ==================== TEST: CREATE BULK ====================

        [Fact]
        public async Task CreateBulk_WithValidData_ReturnsCreated()
        {
            // Arrange
            int categoryId = await TestDataFactory.CreateCategoryAsync(_fixture, "Alimentación");

            CreateBulkBudgetRequestDTO request = new CreateBulkBudgetRequestDTO
            {
                CategoryId = categoryId,
                Year = 2026,
                MonthlyBudgets = new List<MonthlyBudgetDTO>
                {
                    new() { Month = 1, Amount = 500.00m },
                    new() { Month = 2, Amount = 600.00m },
                    new() { Month = 3, Amount = 700.00m }
                }
            };

            StringContent content = _fixture.SerializeRequest(request);

            // Act
            HttpResponseMessage response = await _client.PostAsync("/api/budget/bulk", content);

            // Assert
            Assert.Equal(HttpStatusCode.Created, response.StatusCode);

            string responseContent = await response.Content.ReadAsStringAsync();
            BulkBudgetResponseDTO? result = _fixture.DeserializeResponse<BulkBudgetResponseDTO>(responseContent);

            Assert.NotNull(result);
            Assert.Equal(categoryId, result.CategoryId);
            Assert.Equal(2026, result.Year);
            Assert.Equal(3, result.TotalCreated);
        }

        [Fact]
        public async Task CreateBulk_WithInvalidData_ReturnsBadRequest()
        {
            // Arrange
            int categoryId = await TestDataFactory.CreateCategoryAsync(_fixture, "Alimentación");

            CreateBulkBudgetRequestDTO request = new CreateBulkBudgetRequestDTO
            {
                CategoryId = categoryId,
                Year = 2026,
                MonthlyBudgets = [new() { Month = 13, Amount = 500.00m }]// Mes inválido
            };

            StringContent content = _fixture.SerializeRequest(request);

            // Act
            HttpResponseMessage response = await _client.PostAsync("/api/budget/bulk", content);

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task CreateBulk_WithNonExistingCategory_ReturnsNotFound()
        {
            // Arrange
            CreateBulkBudgetRequestDTO request = new CreateBulkBudgetRequestDTO
            {
                CategoryId = 999,
                Year = 2026,
                MonthlyBudgets = [new() { Month = 1, Amount = 500.00m }]
            };

            StringContent content = _fixture.SerializeRequest(request);

            // Act
            HttpResponseMessage response = await _client.PostAsync("/api/budget/bulk", content);

            // Assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        // ==================== TEST: CREATE ====================

        [Fact]
        public async Task Create_WithValidData_ReturnsCreatedBudget()
        {
            // Arrange
            int categoryId = await TestDataFactory.CreateCategoryAsync(_fixture, "Alimentación");

            CreateBudgetRequestDTO request = new CreateBudgetRequestDTO
            {
                CategoryId = categoryId,
                Amount = 500.00m,
                Currency = "EUR",
                Month = 1,
                Year = 2024
            };

            StringContent content = _fixture.SerializeRequest(request);

            // Act
            HttpResponseMessage response = await _client.PostAsync("/api/budget", content);

            // Assert
            Assert.Equal(HttpStatusCode.Created, response.StatusCode);

            string responseContent = await response.Content.ReadAsStringAsync();
            BudgetResponseDTO? budget = _fixture.DeserializeResponse<BudgetResponseDTO>(responseContent);

            Assert.NotNull(budget);
            Assert.Equal(categoryId, budget.CategoryId);
            Assert.Equal(500.00m, budget.Amount);
            Assert.Equal("EUR", budget.Currency);
            Assert.Equal(2024, budget.Year);
            Assert.Equal(1, budget.Month);
            Assert.Equal("Alimentación", budget.CategoryName);
        }

        [Fact]
        public async Task Create_WithDuplicateBudget_ReturnsConflict()
        {
            // Arrange
            int categoryId = await TestDataFactory.CreateCategoryAsync(_fixture, "Alimentación");
            await TestDataFactory.CreateBudgetAsync(_fixture, categoryId, 500.00m, "EUR", 1, 2024);

            CreateBudgetRequestDTO request = new CreateBudgetRequestDTO
            {
                CategoryId = categoryId,
                Amount = 600.00m,
                Month = 1,
                Year = 2024
            };

            StringContent content = _fixture.SerializeRequest(request);

            // Act
            HttpResponseMessage response = await _client.PostAsync("/api/budget", content);

            // Assert
            Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
        }

        [Fact]
        public async Task Create_WithNonExistingCategory_ReturnsNotFound()
        {
            // Arrange
            CreateBudgetRequestDTO request = new CreateBudgetRequestDTO
            {
                CategoryId = 999,
                Amount = 500.00m,
                Month = 1,
                Year = 2024
            };

            StringContent content = _fixture.SerializeRequest(request);

            // Act
            HttpResponseMessage response = await _client.PostAsync("/api/budget", content);

            // Assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        // ==================== TEST: GET BY ID ====================

        [Fact]
        public async Task GetById_WithExistingId_ReturnsBudget()
        {
            // Arrange
            int categoryId = await TestDataFactory.CreateCategoryAsync(_fixture, "Alimentación");
            int budgetId = await TestDataFactory.CreateBudgetAsync(_fixture, categoryId, 500.00m, "EUR", 1, 2024);

            // Act
            HttpResponseMessage response = await _client.GetAsync($"/api/budget/{budgetId}");

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            string responseContent = await response.Content.ReadAsStringAsync();
            BudgetResponseDTO? budget = _fixture.DeserializeResponse<BudgetResponseDTO>(responseContent);

            Assert.NotNull(budget);
            Assert.Equal(budgetId, budget.Id);
            Assert.Equal(500.00m, budget.Amount);
            Assert.Equal("Alimentación", budget.CategoryName);
        }

        [Fact]
        public async Task GetById_WithNonExistingId_ReturnsNotFound()
        {
            // Act
            HttpResponseMessage response = await _client.GetAsync("/api/budget/999");

            // Assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        // ==================== TEST: GET ALL ====================

        [Fact]
        public async Task GetAll_ReturnsOkWithBudgets()
        {
            // Arrange
            int categoryId = await TestDataFactory.CreateCategoryAsync(_fixture, "Alimentación");
            await TestDataFactory.CreateBudgetAsync(_fixture, categoryId, 500.00m, "EUR", 1, 2024);

            // Act
            HttpResponseMessage response = await _client.GetAsync("/api/budget/year/2024");

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            string responseContent = await response.Content.ReadAsStringAsync();
            List<BudgetResponseDTO>? budgets = _fixture.DeserializeResponse<List<BudgetResponseDTO>>(responseContent);

            Assert.NotNull(budgets);
            Assert.NotEmpty(budgets);
        }

        // ==================== TEST: UPDATE BULK ====================

        [Fact]
        public async Task UpdateBulk_WithValidData_ReturnsOk()
        {
            // Arrange
            int categoryId = await TestDataFactory.CreateCategoryAsync(_fixture, "Alimentación");

            // Crear presupuestos iniciales para el mes 1 y 2
            await TestDataFactory.CreateBudgetAsync(_fixture, categoryId, 500.00m, "EUR", 1, 2026);
            await TestDataFactory.CreateBudgetAsync(_fixture, categoryId, 600.00m, "EUR", 2, 2026);

            UpdateBulkBudgetRequestDTO request = new UpdateBulkBudgetRequestDTO
            {
                CategoryId = categoryId,
                Year = 2026,
                MonthlyBudgets = new List<MonthlyBudgetDTO>
                {
                    new() { Month = 1, Amount = 550.00m },
                    new() { Month = 2, Amount = 650.00m }
                }
            };

            StringContent content = _fixture.SerializeRequest(request);

            // Act
            HttpResponseMessage response = await _client.PutAsync("/api/budget/bulk", content);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            string responseContent = await response.Content.ReadAsStringAsync();
            BulkBudgetResponseDTO? result = _fixture.DeserializeResponse<BulkBudgetResponseDTO>(responseContent);

            Assert.NotNull(result);
            Assert.Equal(categoryId, result.CategoryId);
            Assert.Equal(2026, result.Year);
            Assert.Equal(2, result.TotalCreated);
        }

        [Fact]
        public async Task UpdateBulk_WithInvalidData_ReturnsBadRequest()
        {
            // Arrange
            int categoryId = await TestDataFactory.CreateCategoryAsync(_fixture, "Alimentación");

            UpdateBulkBudgetRequestDTO request = new UpdateBulkBudgetRequestDTO
            {
                CategoryId = categoryId,
                Year = 2026,
                MonthlyBudgets = new List<MonthlyBudgetDTO> { new MonthlyBudgetDTO { Month = 13, Amount = 550.00m } } // Mes inválido
            };

            StringContent content = _fixture.SerializeRequest(request);

            // Act
            HttpResponseMessage response = await _client.PutAsync("/api/budget/bulk", content);

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task UpdateBulk_WithNonExistingCategory_ReturnsNotFound()
        {
            // Arrange
            UpdateBulkBudgetRequestDTO request = new UpdateBulkBudgetRequestDTO
            {
                CategoryId = 999,
                Year = 2026,
                MonthlyBudgets = new List<MonthlyBudgetDTO> { new MonthlyBudgetDTO { Month = 1, Amount = 550.00m } }
            };

            StringContent content = _fixture.SerializeRequest(request);

            // Act
            HttpResponseMessage response = await _client.PutAsync("/api/budget/bulk", content);

            // Assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        // ==================== TEST: UPDATE ====================

        [Fact]
        public async Task Update_WithValidData_ReturnsUpdatedBudget()
        {
            // Arrange
            int categoryId = await TestDataFactory.CreateCategoryAsync(_fixture, "Alimentación");
            int budgetId = await TestDataFactory.CreateBudgetAsync(_fixture, categoryId, 500.00m, "EUR", 1, 2024);

            UpdateBudgetRequestDTO request = new UpdateBudgetRequestDTO { Amount = 600.00m, Currency = "CNY" };

            StringContent content = _fixture.SerializeRequest(request);

            // Act
            HttpResponseMessage response = await _client.PutAsync($"/api/budget/{budgetId}", content);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            string responseContent = await response.Content.ReadAsStringAsync();
            BudgetResponseDTO? budget = _fixture.DeserializeResponse<BudgetResponseDTO>(responseContent);

            Assert.NotNull(budget);
            Assert.Equal(budgetId, budget.Id);
            Assert.Equal(600.00m, budget.Amount);
            Assert.Equal("CNY", budget.Currency);
        }

        [Fact]
        public async Task Update_WithNonExistingId_ReturnsNotFound()
        {
            // Arrange
            UpdateBudgetRequestDTO request = new UpdateBudgetRequestDTO { Amount = 600.00m };

            StringContent content = _fixture.SerializeRequest(request);

            // Act
            HttpResponseMessage response = await _client.PutAsync("/api/budget/999", content);

            // Assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        // ==================== TEST: DELETE BULK ====================

        [Fact]
        public async Task DeleteBulk_WithValidData_ReturnsOk()
        {
            // Arrange
            int categoryId = await TestDataFactory.CreateCategoryAsync(_fixture, "Alimentación");

            // Crear presupuestos para los meses 1, 2 y 3
            await TestDataFactory.CreateBudgetAsync(_fixture, categoryId, 500.00m, "EUR", 1, 2026);
            await TestDataFactory.CreateBudgetAsync(_fixture, categoryId, 600.00m, "EUR", 2, 2026);
            await TestDataFactory.CreateBudgetAsync(_fixture, categoryId, 700.00m, "EUR", 3, 2026);

            DeleteBulkBudgetRequestDTO request = new DeleteBulkBudgetRequestDTO
            {
                CategoryId = categoryId,
                Year = 2026,
                MonthsToDelete = new List<int> { 1, 3 }
            };

            StringContent content = _fixture.SerializeRequest(request);

            HttpRequestMessage httpRequest = new HttpRequestMessage
            {
                Method = HttpMethod.Delete,
                RequestUri = new Uri("/api/budget/bulk", UriKind.Relative),
                Content = content
            };

            // Act
            HttpResponseMessage response = await _client.SendAsync(httpRequest);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            string responseContent = await response.Content.ReadAsStringAsync();
            BulkBudgetResponseDTO? result = _fixture.DeserializeResponse<BulkBudgetResponseDTO>(responseContent);

            Assert.NotNull(result);
            Assert.Equal(categoryId, result.CategoryId);
            Assert.Equal(2026, result.Year);
            Assert.Equal(2, result.TotalCreated);
        }

        [Fact]
        public async Task DeleteBulk_WithNoMonthsToDelete_ReturnsBadRequest()
        {
            // Arrange
            int categoryId = await TestDataFactory.CreateCategoryAsync(_fixture, "Alimentación");

            DeleteBulkBudgetRequestDTO request = new DeleteBulkBudgetRequestDTO
            {
                CategoryId = categoryId,
                Year = 2026,
                MonthsToDelete = new List<int>() // Lista vacía
            };

            StringContent content = _fixture.SerializeRequest(request);

            HttpRequestMessage httpRequest = new HttpRequestMessage
            {
                Method = HttpMethod.Delete,
                RequestUri = new Uri("/api/budget/bulk", UriKind.Relative),
                Content = content
            };

            // Act
            HttpResponseMessage response = await _client.SendAsync(httpRequest);

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task DeleteBulk_WithNonExistingCategory_ReturnsNotFound()
        {
            // Arrange
            DeleteBulkBudgetRequestDTO request = new DeleteBulkBudgetRequestDTO
            {
                CategoryId = 999,
                Year = 2026,
                MonthsToDelete = new List<int> { 1, 3 }
            };

            StringContent content = _fixture.SerializeRequest(request);

            HttpRequestMessage httpRequest = new HttpRequestMessage
            {
                Method = HttpMethod.Delete,
                RequestUri = new Uri("/api/budget/bulk", UriKind.Relative),
                Content = content
            };

            // Act
            HttpResponseMessage response = await _client.SendAsync(httpRequest);

            // Assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        // ==================== TEST: DELETE ====================

        [Fact]
        public async Task Delete_WithExistingId_ReturnsNoContent()
        {
            // Arrange
            int categoryId = await TestDataFactory.CreateCategoryAsync(_fixture, "Alimentación");
            int budgetId = await TestDataFactory.CreateBudgetAsync(_fixture, categoryId, 500.00m, "EUR", 1, 2024);

            // Act
            HttpResponseMessage response = await _client.DeleteAsync($"/api/budget/{budgetId}");

            // Assert
            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        }

        [Fact]
        public async Task Delete_WithNonExistingId_ReturnsNotFound()
        {
            // Act
            HttpResponseMessage response = await _client.DeleteAsync("/api/budget/999");

            // Assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }
    }
}