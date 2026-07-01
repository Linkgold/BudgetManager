using Application.DTOs.Request;
using Application.DTOs.Response;
using Newtonsoft.Json;
using System.Net;
using System.Text;
using Tests.API.Fixtures;

namespace Tests.API.Controllers
{
    [Collection("ApiTestCollection")]
    public class CategoryControllerTests : IClassFixture<ApiTestFixture>, IDisposable
    {
        private readonly HttpClient _client;
        private readonly ApiTestFixture _fixture;

        public CategoryControllerTests(ApiTestFixture fixture)
        {
            _fixture = fixture;
            _client = fixture.Client;
        }

        // ==================== TEST: GET ALL ====================

        [Fact]
        public async Task GetAll_ReturnsOkWithCategories()
        {
            // Act
            HttpResponseMessage response = await _client.GetAsync("/api/category");

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            string content = await response.Content.ReadAsStringAsync();
            List<CategoryResponseDTO> categories = JsonConvert.DeserializeObject<List<CategoryResponseDTO>>(content);

            Assert.NotNull(categories);
        }

        // ==================== TEST: GET BY ID ====================

        [Fact]
        public async Task GetById_WithExistingId_ReturnsCategory()
        {
            // Arrange - Crear una categoría primero
            CreateCategoryRequestDTO createRequest = new CreateCategoryRequestDTO
            {
                Name = "TestCategory",
                Description = "Test Description"
            };

            string createJson = JsonConvert.SerializeObject(createRequest);
            StringContent createContent = new StringContent(createJson, Encoding.UTF8, "application/json");

            HttpResponseMessage createResponse = await _client.PostAsync("/api/category", createContent);
            string createContentString = await createResponse.Content.ReadAsStringAsync();
            CategoryResponseDTO createdCategory = JsonConvert.DeserializeObject<CategoryResponseDTO>(createContentString);

            // Act
            HttpResponseMessage response = await _client.GetAsync($"/api/category/{createdCategory.Id}");

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            string content = await response.Content.ReadAsStringAsync();
            CategoryResponseDTO category = JsonConvert.DeserializeObject<CategoryResponseDTO>(content);

            Assert.NotNull(category);
            Assert.Equal(createdCategory.Id, category.Id);
            Assert.Equal("TestCategory", category.Name);
        }

        [Fact]
        public async Task GetById_WithNonExistingId_ReturnsNotFound()
        {
            // Act
            HttpResponseMessage response = await _client.GetAsync("/api/category/999");

            // Assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task GetById_WithInvalidId_ReturnsBadRequest()
        {
            // Act
            HttpResponseMessage response = await _client.GetAsync("/api/category/0");

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        // ==================== TEST: CREATE ====================

        [Fact]
        public async Task Create_WithValidData_ReturnsCreatedCategory()
        {
            // Arrange
            CreateCategoryRequestDTO request = new CreateCategoryRequestDTO
            {
                Name = "NewCategory",
                Description = "New Description"
            };

            string json = JsonConvert.SerializeObject(request);
            StringContent content = new StringContent(json, Encoding.UTF8, "application/json");

            // Act
            HttpResponseMessage response = await _client.PostAsync("/api/category", content);

            // Assert
            Assert.Equal(HttpStatusCode.Created, response.StatusCode);

            string responseContent = await response.Content.ReadAsStringAsync();
            CategoryResponseDTO category = JsonConvert.DeserializeObject<CategoryResponseDTO>(responseContent);

            Assert.NotNull(category);
            Assert.Equal("NewCategory", category.Name);
            Assert.Equal("New Description", category.Description);
            Assert.True(category.IsActive);
        }

        [Fact]
        public async Task Create_WithEmptyName_ReturnsBadRequest()
        {
            // Arrange
            CreateCategoryRequestDTO request = new CreateCategoryRequestDTO
            {
                Name = "",
                Description = "Test"
            };

            string json = JsonConvert.SerializeObject(request);
            StringContent content = new StringContent(json, Encoding.UTF8, "application/json");

            // Act
            HttpResponseMessage response = await _client.PostAsync("/api/category", content);

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        // ==================== TEST: UPDATE ====================

        [Fact]
        public async Task Update_WithValidData_ReturnsUpdatedCategory()
        {
            // Arrange - Crear una categoría primero
            CreateCategoryRequestDTO createRequest = new CreateCategoryRequestDTO
            {
                Name = "Original",
                Description = "Original Description"
            };

            string createJson = JsonConvert.SerializeObject(createRequest);
            StringContent createContent = new StringContent(createJson, Encoding.UTF8, "application/json");

            HttpResponseMessage createResponse = await _client.PostAsync("/api/category", createContent);
            string createContentString = await createResponse.Content.ReadAsStringAsync();
            CategoryResponseDTO createdCategory = JsonConvert.DeserializeObject<CategoryResponseDTO>(createContentString);

            // Act - Actualizar
            UpdateCategoryRequestDTO updateRequest = new UpdateCategoryRequestDTO
            {
                Name = "Updated",
                Description = "Updated Description"
            };

            string updateJson = JsonConvert.SerializeObject(updateRequest);
            StringContent updateContent = new StringContent(updateJson, Encoding.UTF8, "application/json");

            HttpResponseMessage response = await _client.PutAsync($"/api/category/{createdCategory.Id}", updateContent);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            string responseContent = await response.Content.ReadAsStringAsync();
            CategoryResponseDTO updatedCategory = JsonConvert.DeserializeObject<CategoryResponseDTO>(responseContent);

            Assert.NotNull(updatedCategory);
            Assert.Equal("Updated", updatedCategory.Name);
            Assert.Equal("Updated Description", updatedCategory.Description);
        }

        // ==================== TEST: DELETE ====================

        [Fact]
        public async Task Delete_WithExistingId_ReturnsNoContent()
        {
            // Arrange - Crear una categoría primero
            CreateCategoryRequestDTO createRequest = new CreateCategoryRequestDTO
            {
                Name = "ToDelete",
                Description = "To Delete"
            };

            string createJson = JsonConvert.SerializeObject(createRequest);
            StringContent createContent = new StringContent(createJson, Encoding.UTF8, "application/json");

            HttpResponseMessage createResponse = await _client.PostAsync("/api/category", createContent);
            string createContentString = await createResponse.Content.ReadAsStringAsync();
            CategoryResponseDTO createdCategory = JsonConvert.DeserializeObject<CategoryResponseDTO>(createContentString);

            // Act
            HttpResponseMessage response = await _client.DeleteAsync($"/api/category/{createdCategory.Id}");

            // Assert
            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        }

        [Fact]
        public async Task Delete_WithNonExistingId_ReturnsNotFound()
        {
            // Act
            HttpResponseMessage response = await _client.DeleteAsync("/api/category/999");

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