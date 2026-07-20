using Shared.DTOs.Request;
using Shared.DTOs.Response;
using System.Net;
using Tests.API.Fixtures;

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

        // ==================== HELPERS ====================

        private async Task<int> CreateCategoryAsync(string name)
        {
            CreateCategoryRequestDTO request = new CreateCategoryRequestDTO
            {
                Name = name,
                Description = "Test Category"
            };

            StringContent content = _fixture.SerializeRequest(request);
            HttpResponseMessage response = await _client.PostAsync("/api/category", content);
            string responseContent = await response.Content.ReadAsStringAsync();
            CategoryResponseDTO? category = _fixture.DeserializeResponse<CategoryResponseDTO>(responseContent);

            return category?.Id ?? -1;
        }

        private async Task<int> CreateBudgetAsync(int categoryId, decimal amount, int month, int year)
        {
            CreateBudgetRequestDTO request = new CreateBudgetRequestDTO
            {
                CategoryId = categoryId,
                Amount = amount,
                Month = month,
                Year = year
            };

            StringContent content = _fixture.SerializeRequest(request);
            HttpResponseMessage response = await _client.PostAsync("/api/budget", content);
            string responseContent = await response.Content.ReadAsStringAsync();
            BudgetResponseDTO? budget = _fixture.DeserializeResponse<BudgetResponseDTO>(responseContent);

            return budget?.Id ?? -1;
        }

        // ==================== TEST: CREATE BULK ====================

        [Fact]
        public async Task CreateBulk_WithValidData_ReturnsCreated()
        {
            // Arrange
            int categoryId = await CreateCategoryAsync("Alimentación");

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
            int categoryId = await CreateCategoryAsync("Alimentación");

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
            int categoryId = await CreateCategoryAsync("Alimentación");

            CreateBudgetRequestDTO request = new CreateBudgetRequestDTO
            {
                CategoryId = categoryId,
                Amount = 500.00m,
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
            Assert.Equal(2024, budget.Year);
            Assert.Equal(1, budget.Month);
            Assert.Equal("Alimentación", budget.CategoryName);
        }

        [Fact]
        public async Task Create_WithDuplicateBudget_ReturnsConflict()
        {
            // Arrange
            int categoryId = await CreateCategoryAsync("Alimentación");
            await CreateBudgetAsync(categoryId, 500.00m, 1, 2024);

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
            int categoryId = await CreateCategoryAsync("Alimentación");
            int budgetId = await CreateBudgetAsync(categoryId, 500.00m, 1, 2024);

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
            int categoryId = await CreateCategoryAsync("Alimentación");
            await CreateBudgetAsync(categoryId, 500.00m, 1, 2024);

            // Act
            HttpResponseMessage response = await _client.GetAsync("/api/budget");

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            string responseContent = await response.Content.ReadAsStringAsync();
            List<BudgetResponseDTO>? budgets = _fixture.DeserializeResponse<List<BudgetResponseDTO>>(responseContent);

            Assert.NotNull(budgets);
            Assert.NotEmpty(budgets);
        }

        // ==================== TEST: GET BY CATEGORY ====================

        [Fact]
        public async Task GetByCategory_WithExistingCategory_ReturnsBudgets()
        {
            // Arrange
            int categoryId1 = await CreateCategoryAsync("Alimentación");
            int categoryId2 = await CreateCategoryAsync("Transporte");

            await CreateBudgetAsync(categoryId1, 500.00m, 1, 2024);
            await CreateBudgetAsync(categoryId1, 300.00m, 2, 2024);
            await CreateBudgetAsync(categoryId2, 200.00m, 1, 2024);

            // Act
            HttpResponseMessage response = await _client.GetAsync($"/api/budget/by-category/{categoryId1}");

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            string responseContent = await response.Content.ReadAsStringAsync();
            List<BudgetResponseDTO>? budgets = _fixture.DeserializeResponse<List<BudgetResponseDTO>>(responseContent);

            Assert.NotNull(budgets);
            Assert.Equal(2, budgets.Count);
            Assert.All(budgets, b => Assert.Equal(categoryId1, b.CategoryId));
        }

        [Fact]
        public async Task GetByCategory_WithNonExistingCategory_ReturnsNotFound()
        {
            // Act
            HttpResponseMessage response = await _client.GetAsync("/api/budget/by-category/999");

            // Assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        // ==================== TEST: GET BY PERIOD ====================

        [Fact]
        public async Task GetByPeriod_ReturnsBudgetsForPeriod()
        {
            // Arrange
            int categoryId = await CreateCategoryAsync("Alimentación");
            await CreateBudgetAsync(categoryId, 500.00m, 1, 2024);
            await CreateBudgetAsync(categoryId, 300.00m, 2, 2024);

            // Act
            HttpResponseMessage response = await _client.GetAsync("/api/budget/by-period?month=1&year=2024");

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            string responseContent = await response.Content.ReadAsStringAsync();
            List<BudgetResponseDTO>? budgets = _fixture.DeserializeResponse<List<BudgetResponseDTO>>(responseContent);

            Assert.NotNull(budgets);
            Assert.Single(budgets);
            Assert.Equal(500.00m, budgets[0].Amount);
        }

        // ==================== TEST: GET BY CATEGORY AND PERIOD ====================

        [Fact]
        public async Task GetByCategoryAndPeriod_WithExistingBudget_ReturnsBudget()
        {
            // Arrange
            int categoryId = await CreateCategoryAsync("Alimentación");
            await CreateBudgetAsync(categoryId, 500.00m, 1, 2024);

            // Act
            HttpResponseMessage response = await _client.GetAsync($"/api/budget/by-category-period?categoryId={categoryId}&month=1&year=2024");

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            string responseContent = await response.Content.ReadAsStringAsync();
            BudgetResponseDTO? budget = _fixture.DeserializeResponse<BudgetResponseDTO>(responseContent);

            Assert.NotNull(budget);
            Assert.Equal(categoryId, budget.CategoryId);
            Assert.Equal(500.00m, budget.Amount);
        }

        [Fact]
        public async Task GetByCategoryAndPeriod_WithNonExistingBudget_ReturnsNotFound()
        {
            // Arrange
            int categoryId = await CreateCategoryAsync("Alimentación");

            // Act
            HttpResponseMessage response = await _client.GetAsync($"/api/budget/by-category-period?categoryId={categoryId}&month=1&year=2024");

            // Assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        // ==================== TEST: GET SUMMARY ====================

        [Fact]
        public async Task GetSummary_WithExistingBudget_ReturnsSummary()
        {
            // Arrange
            int categoryId = await CreateCategoryAsync("Alimentación");
            await CreateBudgetAsync(categoryId, 500.00m, 1,2024);

            // Act
            HttpResponseMessage response = await _client.GetAsync($"/api/budget/summary?categoryId={categoryId}&month=1&year=2024");

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            string responseContent = await response.Content.ReadAsStringAsync();
            BudgetSummaryDTO? summary = _fixture.DeserializeResponse<BudgetSummaryDTO>(responseContent);

            Assert.NotNull(summary);
            Assert.Equal(categoryId, summary.CategoryId);
            Assert.Equal(500.00m, summary.BudgetAmount);
            Assert.Equal(0, summary.TotalSpent); // Por ahora siempre 0
        }

        [Fact]
        public async Task GetSummary_WithNonExistingBudget_ReturnsNotFound()
        {
            // Arrange
            int categoryId = await CreateCategoryAsync("Alimentación");

            // Act
            HttpResponseMessage response = await _client.GetAsync($"/api/budget/summary?categoryId={categoryId}&month=1&year=2024");

            // Assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        // ==================== TEST: UPDATE ====================

        [Fact]
        public async Task Update_WithValidData_ReturnsUpdatedBudget()
        {
            // Arrange
            int categoryId = await CreateCategoryAsync("Alimentación");
            int budgetId = await CreateBudgetAsync(categoryId, 500.00m, 1, 2024);

            UpdateBudgetRequestDTO request = new UpdateBudgetRequestDTO { Amount = 600.00m };

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

        // ==================== TEST: DELETE ====================

        [Fact]
        public async Task Delete_WithExistingId_ReturnsNoContent()
        {
            // Arrange
            int categoryId = await CreateCategoryAsync("Alimentación");
            int budgetId = await CreateBudgetAsync(categoryId, 500.00m, 1, 2024);

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