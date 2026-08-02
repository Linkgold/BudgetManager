using Shared.DTOs.Request;
using Shared.DTOs.Response;
using System.Net;
using Tests.API.Fixtures;
using Tests.Helpers;

namespace Tests.API.Controllers
{
    [Collection("ApiTestCollection")]
    public class FixedExpenseControllerTests : IClassFixture<ApiTestFixture>, IDisposable
    {
        private readonly HttpClient _client;
        private readonly ApiTestFixture _fixture;

        public FixedExpenseControllerTests(ApiTestFixture fixture)
        {
            _fixture = fixture;
            _client = fixture.Client;
        }

        // ==================== TEST: CREATE ====================

        [Fact]
        public async Task Create_WithValidData_ReturnsCreatedFixedExpense()
        {
            // Arrange
            int categoryId = await TestDataFactory.CreateCategoryAsync(_fixture, "Suscripciones");

            CreateFixedExpenseRequestDTO request = new CreateFixedExpenseRequestDTO
            {
                CategoryId = categoryId,
                Name = "Netflix",
                Description = "Suscripción mensual",
                Amount = 15.99m,
                Currency = "EUR",
                Year = 2024,
                Month = 1
            };

            StringContent content = _fixture.SerializeRequest(request);

            // Act
            HttpResponseMessage response = await _client.PostAsync("/api/fixedexpense", content);

            // Assert
            Assert.Equal(HttpStatusCode.Created, response.StatusCode);

            string responseContent = await response.Content.ReadAsStringAsync();
            FixedExpenseResponseDTO? fixedExpense = _fixture.DeserializeResponse<FixedExpenseResponseDTO>(responseContent);

            Assert.NotNull(fixedExpense);
            Assert.Equal("Netflix", fixedExpense.Name);
            Assert.Equal(15.99m, fixedExpense.Amount);
            Assert.Equal("EUR", fixedExpense.Currency);
            Assert.Equal(1, fixedExpense.Month);
            Assert.Equal(2024, fixedExpense.Year);
        }

        [Fact]
        public async Task Create_WithNonExistingCategory_ReturnsNotFound()
        {
            // Arrange
            CreateFixedExpenseRequestDTO request = new CreateFixedExpenseRequestDTO
            {
                CategoryId = 999,
                Name = "Netflix",
                Amount = 15.99m,
                Month = 1,
                Year = 2024
            };

            StringContent content = _fixture.SerializeRequest(request);

            // Act
            HttpResponseMessage response = await _client.PostAsync("/api/fixedexpense", content);

            // Assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        // ==================== TEST: GET BY ID ====================

        [Fact]
        public async Task GetById_WithExistingId_ReturnsFixedExpense()
        {
            // Arrange
            int categoryId = await TestDataFactory.CreateCategoryAsync(_fixture, "Suscripciones");
            int fixedExpenseId = await TestDataFactory.CreateFixedExpenseAsync(_fixture, categoryId, "Spotify", 9.99m, "EUR", 1, 2024);

            // Act
            HttpResponseMessage response = await _client.GetAsync($"/api/fixedexpense/{fixedExpenseId}");

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            string content = await response.Content.ReadAsStringAsync();
            FixedExpenseResponseDTO? fixedExpense = _fixture.DeserializeResponse<FixedExpenseResponseDTO>(content);

            Assert.NotNull(fixedExpense);
            Assert.Equal(fixedExpenseId, fixedExpense.Id);
            Assert.Equal("Spotify", fixedExpense.Name);
            Assert.Equal(9.99m, fixedExpense.Amount);
        }

        [Fact]
        public async Task GetById_WithNonExistingId_ReturnsNotFound()
        {
            // Act
            HttpResponseMessage response = await _client.GetAsync("/api/fixedexpense/999");

            // Assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        // ==================== TEST: GET ALL ====================

        [Fact]
        public async Task GetAll_ReturnsOkWithFixedExpenses()
        {
            // Act
            HttpResponseMessage response = await _client.GetAsync("/api/fixedexpense");

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            string content = await response.Content.ReadAsStringAsync();
            List<FixedExpenseResponseDTO>? fixedExpenses = _fixture.DeserializeResponse<List<FixedExpenseResponseDTO>>(content);

            Assert.NotNull(fixedExpenses);
        }

        // ==================== TEST: GET ACTIVE FOR PERIOD ====================

        [Fact]
        public async Task GetActiveForPeriod_ReturnsOkWithFixedExpenses()
        {
            // Arrange
            int categoryId = await TestDataFactory.CreateCategoryAsync(_fixture, "Suscripciones");
            await TestDataFactory.CreateFixedExpenseAsync(_fixture, categoryId, "Netflix", 15.99m, "EUR", 1, 2024);
            await TestDataFactory.CreateFixedExpenseAsync(_fixture, categoryId, "Spotify", 9.99m, "EUR", 3, 2024);

            // Act
            HttpResponseMessage response = await _client.GetAsync("/api/fixedexpense/active/period?year=2024&month=2");

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            string content = await response.Content.ReadAsStringAsync();
            List<FixedExpenseResponseDTO>? fixedExpenses = _fixture.DeserializeResponse<List<FixedExpenseResponseDTO>>(content);

            Assert.NotNull(fixedExpenses);
            // Solo Netflix debe estar activo en febrero (Spotify empieza en marzo)
            Assert.Single(fixedExpenses);
            Assert.Equal("Netflix", fixedExpenses[0].Name);
        }

        // ==================== TEST: UPDATE ====================

        [Fact]
        public async Task Update_WithValidData_ReturnsUpdatedFixedExpensesn()
        {
            // Arrange
            int categoryId = await TestDataFactory.CreateCategoryAsync(_fixture, "Alimentación");
            int updatedCategoryId = await TestDataFactory.CreateCategoryAsync(_fixture, "Suscripción");
            int fixedExpenseId = await TestDataFactory.CreateFixedExpenseAsync(_fixture, categoryId, "Compra supermercado", 45.75m, "EUR", 6, 2024);

            UpdateFixedExpenseRequestDTO request = new UpdateFixedExpenseRequestDTO
            {
                CategoryId = updatedCategoryId,
                Name = "Compra actualizada",
                Description = "Carrefour 20/06/2024",
                Amount = 50.00m,
                Currency = "CNY",
                Month = 6,
                Year = 2024
            };

            StringContent content = _fixture.SerializeRequest(request);

            // Act
            HttpResponseMessage response = await _client.PutAsync($"/api/fixedExpense/{fixedExpenseId}", content);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            string responseContent = await response.Content.ReadAsStringAsync();
            FixedExpenseResponseDTO? fixedExpense = _fixture.DeserializeResponse<FixedExpenseResponseDTO?>(responseContent);

            Assert.NotNull(fixedExpense);
            Assert.Equal(fixedExpenseId, fixedExpense.Id);
            Assert.Equal(updatedCategoryId, fixedExpense.CategoryId);
            Assert.Equal(50.00m, fixedExpense.Amount);
            Assert.Equal("CNY", fixedExpense.Currency);
            Assert.Equal(6, fixedExpense.Month);
            Assert.Equal(2024, fixedExpense.Year);
        }

        [Fact]
        public async Task Update_WithNonExistingId_ReturnsNotFound()
        {
            // Arrange
            int categoryId = await TestDataFactory.CreateCategoryAsync(_fixture, "Alimentación");

            UpdateFixedExpenseRequestDTO request = new UpdateFixedExpenseRequestDTO
            {
                CategoryId = categoryId,
                Name = "Compra actualizada",
                Amount = 50.00m,
                Month= 1,
                Year= 2024
            };

            StringContent content = _fixture.SerializeRequest(request);

            // Act
            HttpResponseMessage response = await _client.PutAsync("/api/fixedExpense/999", content);

            // Assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        // ==================== TEST: DELETE ====================

        [Fact]
        public async Task Delete_WithExistingId_ReturnsNoContent()
        {
            // Arrange
            int categoryId = await TestDataFactory.CreateCategoryAsync(_fixture, "Suscripciones");
            int fixedExpenseId = await TestDataFactory.CreateFixedExpenseAsync(_fixture, categoryId, "ToDelete", 10.00m, "EUR", 1, 2024);

            // Act
            HttpResponseMessage response = await _client.DeleteAsync($"/api/fixedexpense/{fixedExpenseId}");

            // Assert
            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        }

        [Fact]
        public async Task Delete_WithNonExistingId_ReturnsNotFound()
        {
            // Act
            HttpResponseMessage response = await _client.DeleteAsync("/api/fixedexpense/999");

            // Assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        public void Dispose()
        {
            // 🔥 Limpiar al final de CADA prueba
            _fixture.ClearDatabase();
        }
    }
}