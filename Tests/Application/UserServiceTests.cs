using Shared.DTOs.Request;
using Shared.DTOs.Response;
using Application.Interfaces;
using Application.Mappings;
using Application.Services;
using AutoMapper;
using Domain.Entities;
using Domain.Exceptions;
using Domain.Interfaces;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Tests.Helpers;

namespace Tests.Application
{
    public class UserServiceTests : IDisposable
    {
        private readonly Mock<IUserRepository> _userRepositoryMock;
        private readonly Mock<ICurrentUserService> _currentUserServiceMock;
        private readonly Mock<IJwtSettings> _jwtSettingsMock;
        private readonly IMapper _mapper;
        private readonly IUserService _userService;

        public UserServiceTests()
        {
            // Configurar AutoMapper
            MapperConfiguration mapperConfiguration = new MapperConfiguration
            (
                config =>
                {
                    config.AddProfile<AutoMapperProfile>();
                },
                new NullLoggerFactory()
            );

            _mapper = mapperConfiguration.CreateMapper();

            // Crear mocks
            _userRepositoryMock = new Mock<IUserRepository>();
            _currentUserServiceMock = new Mock<ICurrentUserService>();
            _jwtSettingsMock = new Mock<IJwtSettings>();

            // Instanciar el servicio
            _userService = new UserService(_userRepositoryMock.Object, _mapper, _currentUserServiceMock.Object, _jwtSettingsMock.Object);
        }

        // ==================== TEST: CREATE ====================

        [Fact]
        public async Task CreateAsync_WithValidData_ReturnsCreatedUser()
        {
            // Arrange
            CreateUserRequestDTO request = new CreateUserRequestDTO
            {
                UserName = TestDataFactory.DEFAULT_USER_NAME,
                Email = TestDataFactory.DEFAULT_USER_EMAIL,
                Password = TestDataFactory.DEFAULT_PASSWORD,
                ConfirmPassword = TestDataFactory.DEFAULT_PASSWORD
            };

            _userRepositoryMock
                .Setup(repo => repo.ExistsByEmailAsync(request.Email))
                .ReturnsAsync(false);

            // Act
            UserResponseDTO result = await _userService.CreateAsync(request);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(TestDataFactory.DEFAULT_USER_NAME, result.UserName);
            Assert.Equal(TestDataFactory.DEFAULT_USER_EMAIL, result.Email);

            _userRepositoryMock.Verify(repo => repo.AddAsync(It.IsAny<User>()), Times.Once);
        }

        [Fact]
        public async Task CreateAsync_WithExistingEmail_ThrowsInvalidOperationException()
        {
            // Arrange
            CreateUserRequestDTO request = new CreateUserRequestDTO
            {
                UserName = TestDataFactory.DEFAULT_USER_NAME,
                Email = TestDataFactory.DEFAULT_USER_EMAIL,
                Password = TestDataFactory.DEFAULT_PASSWORD,
                ConfirmPassword = TestDataFactory.DEFAULT_PASSWORD
            };

            _userRepositoryMock
                .Setup(repo => repo.ExistsByEmailAsync(request.Email))
                .ReturnsAsync(true);

            // Act & Assert
            await Assert.ThrowsAsync<ConflictException>(() => _userService.CreateAsync(request));

            _userRepositoryMock.Verify(repo => repo.AddAsync(It.IsAny<User>()), Times.Never);
        }

        // ==================== TEST: GET BY ID ====================

        [Fact]
        public async Task GetByIdAsync_WithExistingId_ReturnsUser()
        {
            // Arrange
            int userId = 1;

            _userRepositoryMock
                .Setup(repo => repo.GetByIdAsync(userId))
                .ReturnsAsync(TestDataFactory.CreateUser());

            // Act
            UserResponseDTO result = await _userService.GetByIdAsync(userId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(userId, result.Id);
            Assert.Equal(TestDataFactory.DEFAULT_USER_NAME, result.UserName);
            Assert.Equal(TestDataFactory.DEFAULT_USER_EMAIL, result.Email);
        }

        [Fact]
        public async Task GetByIdAsync_WithNonExistingId_ThrowsKeyNotFoundException()
        {
            // Arrange
            int userId = 999;

            _userRepositoryMock
                .Setup(repo => repo.GetByIdAsync(userId))
                .ReturnsAsync((User?)null);

            // Act & Assert
            await Assert.ThrowsAsync<KeyNotFoundException>(() => _userService.GetByIdAsync(userId));
        }

        // ==================== TEST: GET BY EMAIL ====================

        [Fact]
        public async Task GetByEmailAsync_WithExistingEmail_ReturnsUser()
        {
            // Arrange
            _userRepositoryMock
                .Setup(repo => repo.GetByEmailAsync(TestDataFactory.DEFAULT_USER_EMAIL))
                .ReturnsAsync(TestDataFactory.CreateUser());

            // Act
            UserResponseDTO result = await _userService.GetByEmailAsync(TestDataFactory.DEFAULT_USER_EMAIL);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(TestDataFactory.DEFAULT_USER_EMAIL, result.Email);
        }

        [Fact]
        public async Task GetByEmailAsync_WithNonExistingEmail_ThrowsKeyNotFoundException()
        {
            // Arrange
            string email = "noexiste@email.com";

            _userRepositoryMock
                .Setup(repo => repo.GetByEmailAsync(email))
                .ReturnsAsync((User?)null);

            // Act & Assert
            await Assert.ThrowsAsync<KeyNotFoundException>(() => _userService.GetByEmailAsync(email));
        }

        // ==================== TEST: GET CURRENT USER ====================

        [Fact]
        public async Task GetCurrentUserAsync_WhenAuthenticated_ReturnsUser()
        {
            // Arrange
            int userId = 1;

            _currentUserServiceMock
                .Setup(service => service.UserId)
                .Returns(userId);

            _userRepositoryMock
                .Setup(repo => repo.GetByIdAsync(userId))
                .ReturnsAsync(TestDataFactory.CreateUser());

            // Act
            UserResponseDTO result = await _userService.GetCurrentUserAsync();

            // Assert
            Assert.NotNull(result);
            Assert.Equal(userId, result.Id);
        }

        [Fact]
        public async Task GetCurrentUserAsync_WhenNotAuthenticated_ThrowsUnauthorizedAccessException()
        {
            // Arrange
            _currentUserServiceMock
                .Setup(service => service.UserId)
                .Returns(0);

            // Act & Assert
            await Assert.ThrowsAsync<UnauthorizedAccessException>(() => _userService.GetCurrentUserAsync());
        }

        // ==================== TEST: UPDATE ====================

        [Fact]
        public async Task UpdateAsync_WithValidData_ReturnsUpdatedUser()
        {
            // Arrange
            int userId = 1;
            string updatedUserName = "María García";
            string updatedEmail = "maria@email.com";

            UpdateUserRequestDTO request = new UpdateUserRequestDTO { UserName = updatedUserName, Email = updatedEmail };

            _currentUserServiceMock
                .Setup(service => service.UserId)
                .Returns(userId);

            _userRepositoryMock
                .Setup(repo => repo.GetByIdAsync(userId))
                .ReturnsAsync(TestDataFactory.CreateUser());

            _userRepositoryMock
                .Setup(repo => repo.GetByEmailAsync(request.Email))
                .ReturnsAsync((User?)null);

            // Act
            UserResponseDTO result = await _userService.UpdateAsync(request);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(updatedUserName, result.UserName);
            Assert.Equal(updatedEmail, result.Email);

            _userRepositoryMock.Verify(repo => repo.UpdateAsync(It.IsAny<User>()), Times.Once);
        }

        [Fact]
        public async Task UpdateAsync_WithEmailAlreadyInUse_ThrowsInvalidOperationException()
        {
            // Arrange
            int userId = 1;

            UpdateUserRequestDTO request = new UpdateUserRequestDTO
            {
                UserName = TestDataFactory.DEFAULT_USER_NAME,
                Email = "maria@email.com"
            };

            _currentUserServiceMock
                .Setup(service => service.UserId)
                .Returns(userId);

            _userRepositoryMock
                .Setup(repo => repo.GetByIdAsync(userId))
                .ReturnsAsync(TestDataFactory.CreateUser());

            _userRepositoryMock
                .Setup(repo => repo.GetByEmailAsync(request.Email))
                .ReturnsAsync(TestDataFactory.CreateUser(2, "Otro Usuario", "maria@email.com"));

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() => _userService.UpdateAsync(request));
        }

        // ==================== TEST: DELETE ====================

        [Fact]
        public async Task DeleteAsync_WithExistingId_DeletesUser()
        {
            // Arrange
            int userId = 1;

            _currentUserServiceMock
                .Setup(service => service.UserId)
                .Returns(userId);

            _userRepositoryMock
                .Setup(repo => repo.ExistsAsync(userId))
                .ReturnsAsync(true);

            // Act
            await _userService.DeleteAsync();

            // Assert
            _userRepositoryMock.Verify(repo => repo.DeleteAsync(userId), Times.Once);
        }

        [Fact]
        public async Task DeleteAsync_WithNonExistingId_ThrowsKeyNotFoundException()
        {
            // Arrange
            int userId = 999;

            _userRepositoryMock
                .Setup(repo => repo.ExistsAsync(userId))
                .ReturnsAsync(false);

            // Act & Assert
            await Assert.ThrowsAsync<KeyNotFoundException>(() => _userService.DeleteAsync());
        }

        // ==================== TEST: LOGIN ====================

        [Fact]
        public async Task LoginAsync_WithValidCredentials_ReturnsToken()
        {
            // Arrange
            _userRepositoryMock
                .Setup(repo => repo.GetByEmailAsync(TestDataFactory.DEFAULT_USER_EMAIL))
                .ReturnsAsync(TestDataFactory.CreateUserWithPassword());

            // Act
            LoginResponseDTO loginResponse = await _userService.LoginAsync(TestDataFactory.DEFAULT_USER_EMAIL, TestDataFactory.DEFAULT_PASSWORD);

            // Assert
            Assert.NotNull(loginResponse);
            Assert.StartsWith("token_", loginResponse.Token);
            Assert.Equal(TestDataFactory.DEFAULT_USER_NAME, loginResponse.UserName);
            Assert.Equal(TestDataFactory.DEFAULT_USER_EMAIL, loginResponse.Email);
        }

        [Fact]
        public async Task LoginAsync_WithInvalidEmail_ThrowsUnauthorizedAccessException()
        {
            // Arrange
            string email = "noexiste@email.com";

            _userRepositoryMock
                .Setup(repo => repo.GetByEmailAsync(email))
                .ReturnsAsync((User?)null);

            // Act & Assert
            await Assert.ThrowsAsync<UnauthorizedAccessException>(() => _userService.LoginAsync(email, TestDataFactory.DEFAULT_PASSWORD));
        }

        [Fact]
        public async Task LoginAsync_WithInvalidPassword_ThrowsUnauthorizedAccessException()
        {
            // Arrange
            _userRepositoryMock
                .Setup(repo => repo.GetByEmailAsync(TestDataFactory.DEFAULT_USER_EMAIL))
                .ReturnsAsync(TestDataFactory.CreateUserWithPassword());

            // Act & Assert
            await Assert.ThrowsAsync<UnauthorizedAccessException>(() => _userService.LoginAsync(TestDataFactory.DEFAULT_USER_EMAIL, "WrongPassword"));
        }

        [Fact]
        public async Task LoginAsync_WithInactiveUser_ThrowsUnauthorizedAccessException()
        {
            // Arrange
            User user = TestDataFactory.CreateUserWithPassword();
            user.Deactivate();

            _userRepositoryMock
                .Setup(repo => repo.GetByEmailAsync(TestDataFactory.DEFAULT_USER_EMAIL))
                .ReturnsAsync(user);

            // Act & Assert
            await Assert.ThrowsAsync<UnauthorizedAccessException>(() => _userService.LoginAsync(TestDataFactory.DEFAULT_USER_EMAIL, "Password123!"));
        }

        // ==================== TEST: CHANGE PASSWORD ====================

        [Fact]
        public async Task ChangePasswordAsync_WithValidData_UpdatesPassword()
        {
            // Arrange
            int userId = 1;
            string newPassword = "NewPassword456!";

            _currentUserServiceMock
                .Setup(service => service.UserId)
                .Returns(userId);

            _userRepositoryMock
                .Setup(repo => repo.GetByIdAsync(userId))
                .ReturnsAsync(TestDataFactory.CreateUserWithPassword(TestDataFactory.DEFAULT_PASSWORD));

            // Act
            await _userService.ChangePasswordAsync(TestDataFactory.DEFAULT_PASSWORD, newPassword);

            // Assert
            _userRepositoryMock.Verify(repo => repo.UpdateAsync(It.IsAny<User>()), Times.Once);
        }

        [Fact]
        public async Task ChangePasswordAsync_WithInvalidCurrentPassword_ThrowsUnauthorizedAccessException()
        {
            // Arrange
            int userId = 1;

            _currentUserServiceMock
                .Setup(service => service.UserId)
                .Returns(userId);

            _userRepositoryMock
                .Setup(repo => repo.GetByIdAsync(userId))
                .ReturnsAsync(TestDataFactory.CreateUserWithPassword());

            // Act & Assert
            await Assert.ThrowsAsync<UnauthorizedAccessException>(() => _userService.ChangePasswordAsync("WrongPassword", "NewPassword456!"));
        }

        // ==================== TEST: EXISTS ====================

        [Fact]
        public async Task ExistsAsync_WithExistingId_ReturnsTrue()
        {
            // Arrange
            int userId = 1;

            _userRepositoryMock
                .Setup(repo => repo.ExistsAsync(userId))
                .ReturnsAsync(true);

            // Act
            bool result = await _userService.ExistsAsync(userId);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public async Task ExistsAsync_WithNonExistingId_ReturnsFalse()
        {
            // Arrange
            int userId = 999;

            _userRepositoryMock
                .Setup(repo => repo.ExistsAsync(userId))
                .ReturnsAsync(false);

            // Act
            bool result = await _userService.ExistsAsync(userId);

            // Assert
            Assert.False(result);
        }

        // ==================== DISPOSE ====================

        public void Dispose()
        {
            // Limpiar recursos si es necesario
        }
    }
}