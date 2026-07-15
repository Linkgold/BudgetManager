using Shared.DTOs.Request;
using Shared.DTOs.Response;
using Crypt = BCrypt.Net.BCrypt;
using Domain.Entities;
using Domain.ValueObjects;
using System.Net;
using Tests.API.Fixtures;

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
        //private readonly HttpClient _authenticatedClient;

        public UserControllerTests(ApiTestFixture fixture)
        {
            _fixture = fixture;
            _client = fixture.Client;
            //_authenticatedClient = fixture.AuthenticatedClient;
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
            Assert.True(user.IsActive);
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
            await RegisterTestUserAsync();

            LoginRequestDTO request = new LoginRequestDTO
            {
                Email = _testEmail,
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
            await RegisterTestUserAsync();

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
            await RegisterTestUserAsync();
            string token = await GetTokenAsync();

            // 🔥 Configurar el token en el cliente
            _client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            // Act
            HttpResponseMessage response = await _client.GetAsync("/api/user/me");

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            string responseContent = await response.Content.ReadAsStringAsync();
            UserResponseDTO? user = _fixture.DeserializeResponse<UserResponseDTO>(responseContent);

            Assert.NotNull(user);
            Assert.Equal(_testUser, user.UserName);
            Assert.Equal(_testEmail, user.Email);
        }

        // ==================== TEST: UPDATE CURRENT USER ====================

        [Fact]
        public async Task UpdateCurrentUser_WithValidData_ReturnsUpdatedUser()
        {
            // Arrange - Registrar y loguear un usuario
            await RegisterTestUserAsync();
            string token = await GetTokenAsync();

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
            await RegisterTestUserAsync();
            string token = await GetTokenAsync();

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
            await RegisterTestUserAsync();
            string token = await GetTokenAsync();

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

        // ==================== TEST: DELETE CURRENT USER ====================

        [Fact]
        public async Task DeleteCurrentUser_WithAuthenticatedUser_ReturnsNoContent()
        {
            // Arrange - Registrar y loguear un usuario
            await RegisterTestUserAsync();
            string token = await GetTokenAsync();

            _client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            // Act
            HttpResponseMessage response = await _client.DeleteAsync("/api/user/me");

            // Assert
            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        }

        // ==================== HELPERS ====================

        private async Task RegisterTestUserAsync()
        {
            string uniqueId = Guid.NewGuid().ToString("N").Substring(0, 6);
            string userName = $"TestUser_{uniqueId}";
            string email = $"test_{uniqueId}@example.com";

            // 🔥 Obtener el usuario del fixture (el que tiene Id = 1)
            User? user = await _fixture.DbContext.Users.FindAsync(1);
            if (user == null)
            {
                // Si no existe, crearlo
                UserInfo userInfo = new UserInfo("TestUser", "test@example.com");
                User newUser = new User(userInfo, Crypt.HashPassword("Password123!"));
                _fixture.DbContext.Users.Add(newUser);
                await _fixture.DbContext.SaveChangesAsync();
                _testEmail = "test@example.com";
                _testPassword = "Password123!";
                return;
            }

            // 🔥 Actualizar el usuario existente con datos de prueba
            user.Update(new UserInfo(userName, email));
            user.UpdatePassword(Crypt.HashPassword("Password123!"));
            await _fixture.DbContext.SaveChangesAsync();

            _testUser = userName;
            _testEmail = email;
            _testPassword = "Password123!";
        }

        string _testUser;
        string _testEmail;
        string _testPassword;


        private async Task<string> GetTokenAsync()
        {
            LoginRequestDTO request = new LoginRequestDTO
            {
                Email = _testEmail,
                Password = _testPassword
            };

            StringContent content = _fixture.SerializeRequest(request);
            HttpResponseMessage response = await _client.PostAsync("/api/user/login", content);
            string responseContent = await response.Content.ReadAsStringAsync();
            LoginResponseDTO? loginResponse = _fixture.DeserializeResponse<LoginResponseDTO>(responseContent);

            return loginResponse?.Token ?? string.Empty;
        }
    }
}