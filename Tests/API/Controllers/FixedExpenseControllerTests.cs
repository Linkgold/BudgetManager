using Application.DTOs.Request;
using Application.DTOs.Response;
using Newtonsoft.Json;
using System.Net;
using System.Text;
using Tests.API.Fixtures;

namespace Tests.API.Controllers
{
    [Collection("ApiTestCollection")]
    public class FixedExpenseControllerTests : IDisposable, IClassFixture<ApiTestFixture>
    {
        private readonly HttpClient _client;
        private readonly ApiTestFixture _fixture;

        public FixedExpenseControllerTests(ApiTestFixture fixture)
        {
            _fixture = fixture;
            _client = fixture.Client;
        }

        // ==================== HELPERS ====================

        private async Task<int> CreateCategoryAsync(string name)
        {
            CreateCategoryRequestDTO request = new CreateCategoryRequestDTO
            {
                Name = name,
                Description = "Test Category"
            };

            string json = JsonConvert.SerializeObject(request);
            StringContent content = new StringContent(json, Encoding.UTF8, "application/json");

            HttpResponseMessage response = await _client.PostAsync("/api/category", content);
            string responseContent = await response.Content.ReadAsStringAsync();
            CategoryResponseDTO category = JsonConvert.DeserializeObject<CategoryResponseDTO>(responseContent);

            return category.Id;
        }

        private async Task<int> CreateFixedExpenseAsync(int categoryId, string name, decimal amount, int year, int month)
        {
            CreateFixedExpenseRequestDTO request = new CreateFixedExpenseRequestDTO
            {
                CategoryId = categoryId,
                Name = name,
                Description = "Test Fixed Expense",
                Amount = amount,
                Year = year,
                Month = month
            };

            string json = JsonConvert.SerializeObject(request);
            StringContent content = new StringContent(json, Encoding.UTF8, "application/json");

            HttpResponseMessage response = await _client.PostAsync("/api/fixedexpense", content);
            string responseContent = await response.Content.ReadAsStringAsync();
            FixedExpenseResponseDTO fixedExpense = JsonConvert.DeserializeObject<FixedExpenseResponseDTO>(responseContent);

            return fixedExpense.Id;
        }

        // ==================== TEST: CREATE ====================

        [Fact]
        public async Task Create_WithValidData_ReturnsCreatedFixedExpense()
        {
            // Arrange
            int categoryId = await CreateCategoryAsync("Suscripciones");

            CreateFixedExpenseRequestDTO request = new CreateFixedExpenseRequestDTO
            {
                CategoryId = categoryId,
                Name = "Netflix",
                Description = "Suscripción mensual",
                Amount = 15.99m,
                Year = 2024,
                Month = 1
            };

            string json = JsonConvert.SerializeObject(request);
            StringContent content = new StringContent(json, Encoding.UTF8, "application/json");

            // Act
            HttpResponseMessage response = await _client.PostAsync("/api/fixedexpense", content);

            // Assert
            Assert.Equal(HttpStatusCode.Created, response.StatusCode);

            string responseContent = await response.Content.ReadAsStringAsync();
            FixedExpenseResponseDTO fixedExpense = JsonConvert.DeserializeObject<FixedExpenseResponseDTO>(responseContent);

            Assert.NotNull(fixedExpense);
            Assert.Equal("Netflix", fixedExpense.Name);
            Assert.Equal(15.99m, fixedExpense.Amount);
            Assert.Equal(2024, fixedExpense.Year);
            Assert.Equal(1, fixedExpense.Month);
            Assert.True(fixedExpense.IsActive);
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
                Year = 2024,
                Month = 1
            };

            string json = JsonConvert.SerializeObject(request);
            StringContent content = new StringContent(json, Encoding.UTF8, "application/json");

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
            int categoryId = await CreateCategoryAsync("Suscripciones");
            int fixedExpenseId = await CreateFixedExpenseAsync(categoryId, "Spotify", 9.99m, 2024, 1);

            // Act
            HttpResponseMessage response = await _client.GetAsync($"/api/fixedexpense/{fixedExpenseId}");

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            string content = await response.Content.ReadAsStringAsync();
            FixedExpenseResponseDTO fixedExpense = JsonConvert.DeserializeObject<FixedExpenseResponseDTO>(content);

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
            List<FixedExpenseResponseDTO> fixedExpenses = JsonConvert.DeserializeObject<List<FixedExpenseResponseDTO>>(content);

            Assert.NotNull(fixedExpenses);
        }

        // ==================== TEST: GET ACTIVE FOR PERIOD ====================

        [Fact]
        public async Task GetActiveForPeriod_ReturnsOkWithFixedExpenses()
        {
            // Arrange
            int categoryId = await CreateCategoryAsync("Suscripciones");
            await CreateFixedExpenseAsync(categoryId, "Netflix", 15.99m, 2024, 1);
            await CreateFixedExpenseAsync(categoryId, "Spotify", 9.99m, 2024, 3);

            // Act
            HttpResponseMessage response = await _client.GetAsync("/api/fixedexpense/active/period?year=2024&month=2");

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            string content = await response.Content.ReadAsStringAsync();
            List<FixedExpenseResponseDTO> fixedExpenses = JsonConvert.DeserializeObject<List<FixedExpenseResponseDTO>>(content);

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
            int categoryId = await CreateCategoryAsync("Suscripciones");
            int fixedExpenseId = await CreateFixedExpenseAsync(categoryId, "ToDelete", 10.00m, 2024, 1);

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

        // ==================== TEST: ACTIVATE ====================

        [Fact]
        public async Task Activate_WithExistingId_ReturnsNoContent()
        {
            // Arrange
            int categoryId = await CreateCategoryAsync("Suscripciones");
            int fixedExpenseId = await CreateFixedExpenseAsync(categoryId, "ToActivate", 10.00m, 2024, 1);

            // Act
            HttpResponseMessage response = await _client.PatchAsync($"/api/fixedexpense/{fixedExpenseId}/activate", null);

            // Assert
            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        }

        [Fact]
        public async Task Activate_WithNonExistingId_ReturnsNotFound()
        {
            // Act
            HttpResponseMessage response = await _client.PatchAsync("/api/fixedexpense/999/activate", null);

            // Assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        // ==================== TEST: DEACTIVATE ====================

        [Fact]
        public async Task Deactivate_WithExistingId_ReturnsNoContent()
        {
            // Arrange
            int categoryId = await CreateCategoryAsync("Suscripciones");
            int fixedExpenseId = await CreateFixedExpenseAsync(categoryId, "ToDeactivate", 10.00m, 2024, 1);

            // Act
            HttpResponseMessage response = await _client.PatchAsync($"/api/fixedexpense/{fixedExpenseId}/deactivate", null);

            // Assert
            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        }

        [Fact]
        public async Task Deactivate_WithNonExistingId_ReturnsNotFound()
        {
            // Act
            HttpResponseMessage response = await _client.PatchAsync("/api/fixedexpense/999/deactivate", null);

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