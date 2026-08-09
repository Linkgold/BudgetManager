using Shared.DTOs.Request;
using Shared.DTOs.Response;
using System.Net;
using Tests.API.Fixtures;
using Tests.Helpers;

namespace Tests.API.Controllers
{
    /// <summary>
    /// Pruebas de integración para UserController
    /// </summary>
    [Collection("ApiTestCollection")]
    public class UserControllerTests : IClassFixture<ApiTestFixture>, IDisposable
    {
        private readonly ApiTestFixture _fixture;
        private readonly HttpClient _client;

        public UserControllerTests(ApiTestFixture fixture)
        {
            _fixture = fixture;
            _client = fixture.Client;
        }

        public void Dispose()
        {
            _fixture.ClearDatabase();
        }

        // ==================== TEST: REGISTER ====================

        [Fact]
        public async Task Register_WithValidData_ReturnsCreatedUser()
        {
            // Arrange
            CreateUserRequestDTO request = new CreateUserRequestDTO
            {
                UserName = "TestValidUser",
                Email = "testvalid@example.com",
                Password = "Password123!",
                ConfirmPassword = "Password123!"
            };

            StringContent content = _fixture.SerializeRequest(request);

            // Act
            HttpResponseMessage response = await _client.PostAsync("/api/user/register", content);

            // Assert
            Assert.Equal(HttpStatusCode.Created, response.StatusCode);

            string responseContent = await response.Content.ReadAsStringAsync();
            UserResponseDTO? user = _fixture.DeserializeResponse<UserResponseDTO>(responseContent);

            Assert.NotNull(user);
            Assert.Equal("TestValidUser", user.UserName);
            Assert.Equal("testvalid@example.com", user.Email);
        }

        [Fact]
        public async Task Register_WithDuplicateEmail_ReturnsConflict()
        {
            // Arrange
            CreateUserRequestDTO request = new CreateUserRequestDTO
            {
                UserName = "TestUserDuplicate",
                Email = "duplicatedTest@example.com",
                Password = "Password123!",
                ConfirmPassword = "Password123!"
            };

            StringContent content = _fixture.SerializeRequest(request);

            // Act - Primera petición (debe funcionar)
            HttpResponseMessage firstResponse = await _client.PostAsync("/api/user/register", content);

            // Segunda petición (debe fallar)
            HttpResponseMessage secondResponse = await _client.PostAsync("/api/user/register", content);

            // Assert
            Assert.Equal(HttpStatusCode.Conflict, secondResponse.StatusCode);
        }

        [Fact]
        public async Task Register_WithInvalidEmail_ReturnsBadRequest()
        {
            // Arrange
            CreateUserRequestDTO request = new CreateUserRequestDTO
            {
                UserName = "TestUser",
                Email = "invalid-email",
                Password = "Password123!",
                ConfirmPassword = "Password123!"
            };

            StringContent content = _fixture.SerializeRequest(request);

            // Act
            HttpResponseMessage response = await _client.PostAsync("/api/user/register", content);

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task Register_WithPasswordMismatch_ReturnsBadRequest()
        {
            // Arrange
            CreateUserRequestDTO request = new CreateUserRequestDTO
            {
                UserName = "TestUser",
                Email = "test@example.com",
                Password = "Password123!",
                ConfirmPassword = "DifferentPassword!"
            };

            StringContent content = _fixture.SerializeRequest(request);

            // Act
            HttpResponseMessage response = await _client.PostAsync("/api/user/register", content);

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        // ==================== TEST: LOGIN ====================

        [Fact]
        public async Task Login_WithValidCredentials_ReturnsToken()
        {
            // Arrange - Registrar un usuario primero
            string uniqueId = await TestDataFactory.RegisterTestUserAsync(_fixture);

            LoginRequestDTO request = new LoginRequestDTO
            {
                Email = TestDataFactory.GetEmail(uniqueId),
                Password = "Password123!"
            };

            StringContent content = _fixture.SerializeRequest(request);

            // Act
            HttpResponseMessage response = await _client.PostAsync("/api/user/login", content);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            string responseContent = await response.Content.ReadAsStringAsync();
            LoginResponseDTO? loginResponse = _fixture.DeserializeResponse<LoginResponseDTO>(responseContent);

            Assert.NotNull(loginResponse);
            Assert.NotEmpty(loginResponse.Token);
        }

        [Fact]
        public async Task Login_WithInvalidEmail_ReturnsUnauthorized()
        {
            // Arrange
            LoginRequestDTO request = new LoginRequestDTO
            {
                Email = "nonexistent@example.com",
                Password = "Password123!"
            };

            StringContent content = _fixture.SerializeRequest(request);

            // Act
            HttpResponseMessage response = await _client.PostAsync("/api/user/login", content);

            // Assert
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Fact]
        public async Task Login_WithInvalidPassword_ReturnsUnauthorized()
        {
            // Arrange - Registrar un usuario primero
            await TestDataFactory.RegisterTestUserAsync(_fixture);

            LoginRequestDTO request = new LoginRequestDTO
            {
                Email = "test@example.com",
                Password = "WrongPassword!"
            };

            StringContent content = _fixture.SerializeRequest(request);

            // Act
            HttpResponseMessage response = await _client.PostAsync("/api/user/login", content);

            // Assert
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        // ==================== TEST: GET CURRENT USER ====================

        [Fact]
        public async Task GetCurrentUser_WithAuthenticatedUser_ReturnsUser()
        {
            // Arrange - Registrar y loguear un usuario
            string uniqueId = await TestDataFactory.RegisterTestUserAsync(_fixture);
            string token = await TestDataFactory.GetTokenAsync(_fixture, _client, uniqueId);

            // 🔥 Configurar el token en el cliente
            _client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            // Act
            HttpResponseMessage response = await _client.GetAsync("/api/user/me");

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            string responseContent = await response.Content.ReadAsStringAsync();
            UserResponseDTO? user = _fixture.DeserializeResponse<UserResponseDTO>(responseContent);

            Assert.NotNull(user);
            Assert.Equal(TestDataFactory.GetUserName(uniqueId), user.UserName);
            Assert.Equal(TestDataFactory.GetEmail(uniqueId), user.Email);
        }

        // ==================== TEST: UPDATE CURRENT USER ====================

        [Fact]
        public async Task UpdateCurrentUser_WithValidData_ReturnsUpdatedUser()
        {
            // Arrange - Registrar y loguear un usuario
            string uniqueId = await TestDataFactory.RegisterTestUserAsync(_fixture);
            string token = await TestDataFactory.GetTokenAsync(_fixture, _client, uniqueId);

            _client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            UpdateUserRequestDTO request = new UpdateUserRequestDTO
            {
                UserName = "UpdatedUser",
                Email = "updated@example.com"
            };

            StringContent content = _fixture.SerializeRequest(request);

            // Act
            HttpResponseMessage response = await _client.PutAsync("/api/user/me", content);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            string responseContent = await response.Content.ReadAsStringAsync();
            UserResponseDTO? user = _fixture.DeserializeResponse<UserResponseDTO>(responseContent);

            Assert.NotNull(user);
            Assert.Equal("UpdatedUser", user.UserName);
            Assert.Equal("updated@example.com", user.Email);
        }

        // ==================== TEST: CHANGE PASSWORD ====================

        [Fact]
        public async Task ChangePassword_WithValidData_ReturnsNoContent()
        {
            // Arrange - Registrar y loguear un usuario
            string uniqueId = await TestDataFactory.RegisterTestUserAsync(_fixture);
            string token = await TestDataFactory.GetTokenAsync(_fixture, _client, uniqueId);

            _client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            ChangePasswordRequestDTO request = new ChangePasswordRequestDTO
            {
                CurrentPassword = "Password123!",
                NewPassword = "NewPassword456!",
                ConfirmNewPassword = "NewPassword456!"
            };

            StringContent content = _fixture.SerializeRequest(request);

            // Act
            HttpResponseMessage response = await _client.PostAsync("/api/user/change-password", content);

            // Assert
            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        }

        [Fact]
        public async Task ChangePassword_WithInvalidCurrentPassword_ReturnsBadRequest()
        {
            // Arrange - Registrar y loguear un usuario
            string uniqueId = await TestDataFactory.RegisterTestUserAsync(_fixture);
            string token = await TestDataFactory.GetTokenAsync(_fixture, _client, uniqueId);

            _client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            ChangePasswordRequestDTO request = new ChangePasswordRequestDTO
            {
                CurrentPassword = "WrongPassword!",
                NewPassword = "NewPassword456!",
                ConfirmNewPassword = "NewPassword456!"
            };

            StringContent content = _fixture.SerializeRequest(request);

            // Act
            HttpResponseMessage response = await _client.PostAsync("/api/user/change-password", content);

            // Assert
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }
    }
}