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
            int fixedExpenseId = await TestDataFactory.CreateFixedExpenseAsync(_fixture, categoryId, "Spotify", 9.99m, "EUR", 2024, 1);

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
            await TestDataFactory.CreateFixedExpenseAsync(_fixture, categoryId, "Netflix", 15.99m, "EUR", 2024, 1);
            await TestDataFactory.CreateFixedExpenseAsync(_fixture, categoryId, "Spotify", 9.99m, "EUR", 2024, 3);

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

        // ==================== TEST: DELETE ====================

        [Fact]
        public async Task Delete_WithExistingId_ReturnsNoContent()
        {
            // Arrange
            int categoryId = await TestDataFactory.CreateCategoryAsync(_fixture, "Suscripciones");
            int fixedExpenseId = await TestDataFactory.CreateFixedExpenseAsync(_fixture, categoryId, "ToDelete", 10.00m, "EUR", 2024, 1);

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