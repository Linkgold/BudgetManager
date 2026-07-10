using Application.DTOs.Request;
using Application.DTOs.Response;
using Application.Interfaces;
using Application.Mappings;
using Application.Services;
using AutoMapper;
using Domain.Entities;
using Domain.Interfaces;
using Domain.ValueObjects;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Crypt = BCrypt.Net.BCrypt;

namespace Tests.Application
{
    public class UserServiceTests : IDisposable
    {
        private readonly Mock<IUserRepository> _userRepositoryMock;
        private readonly Mock<ICurrentUserService> _currentUserServiceMock;
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

            // Instanciar el servicio
            _userService = new UserService(_userRepositoryMock.Object, _mapper, _currentUserServiceMock.Object);
        }

        // ==================== HELPERS ====================

        private User CreateUser(int id, string userName = "Juan Pérez", string email = "juan@email.com")
        {
            UserInfo info = new UserInfo(userName, email);
            User user = new User(info, "hashed_password");
            typeof(User).GetProperty("Id")?.SetValue(user, id);
            return user;
        }

        private User CreateUserWithPassword(string password = "Password123!")
        {
            string passwordHash = Crypt.HashPassword(password);
            UserInfo info = new UserInfo("Juan Pérez", "juan@email.com");
            User user = new User(info, passwordHash);
            typeof(User).GetProperty("Id")?.SetValue(user, 1);
            return user;
        }

        // ==================== TEST: CREATE ====================

        [Fact]
        public async Task CreateAsync_WithValidData_ReturnsCreatedUser()
        {
            // Arrange
            CreateUserRequestDTO request = new CreateUserRequestDTO
            {
                UserName = "Juan Pérez",
                Email = "juan@email.com",
                Password = "Password123!",
                ConfirmPassword = "Password123!"
            };

            _userRepositoryMock
                .Setup(repo => repo.ExistsByEmailAsync(request.Email))
                .ReturnsAsync(false);

            // Act
            UserResponseDTO result = await _userService.CreateAsync(request);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Juan Pérez", result.UserName);
            Assert.Equal("juan@email.com", result.Email);

            _userRepositoryMock.Verify(repo => repo.AddAsync(It.IsAny<User>()), Times.Once);
        }

        [Fact]
        public async Task CreateAsync_WithExistingEmail_ThrowsInvalidOperationException()
        {
            // Arrange
            CreateUserRequestDTO request = new CreateUserRequestDTO
            {
                UserName = "Juan Pérez",
                Email = "juan@email.com",
                Password = "Password123!",
                ConfirmPassword = "Password123!"
            };

            _userRepositoryMock
                .Setup(repo => repo.ExistsByEmailAsync(request.Email))
                .ReturnsAsync(true);

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() => _userService.CreateAsync(request));

            _userRepositoryMock.Verify(repo => repo.AddAsync(It.IsAny<User>()), Times.Never);
        }

        // ==================== TEST: GET BY ID ====================

        [Fact]
        public async Task GetByIdAsync_WithExistingId_ReturnsUser()
        {
            // Arrange
            int userId = 1;
            User user = CreateUser(userId);

            _userRepositoryMock
                .Setup(repo => repo.GetByIdAsync(userId))
                .ReturnsAsync(user);

            // Act
            UserResponseDTO result = await _userService.GetByIdAsync(userId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(userId, result.Id);
            Assert.Equal("Juan Pérez", result.UserName);
            Assert.Equal("juan@email.com", result.Email);
        }

        [Fact]
        public async Task GetByIdAsync_WithNonExistingId_ThrowsKeyNotFoundException()
        {
            // Arrange
            int userId = 999;

            _userRepositoryMock
                .Setup(repo => repo.GetByIdAsync(userId))
                .ReturnsAsync((User)null);

            // Act & Assert
            await Assert.ThrowsAsync<KeyNotFoundException>(() => _userService.GetByIdAsync(userId));
        }

        // ==================== TEST: GET BY EMAIL ====================

        [Fact]
        public async Task GetByEmailAsync_WithExistingEmail_ReturnsUser()
        {
            // Arrange
            string email = "juan@email.com";
            User user = CreateUser(1, "Juan Pérez", email);

            _userRepositoryMock
                .Setup(repo => repo.GetByEmailAsync(email))
                .ReturnsAsync(user);

            // Act
            UserResponseDTO result = await _userService.GetByEmailAsync(email);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(email, result.Email);
        }

        [Fact]
        public async Task GetByEmailAsync_WithNonExistingEmail_ThrowsKeyNotFoundException()
        {
            // Arrange
            string email = "noexiste@email.com";

            _userRepositoryMock
                .Setup(repo => repo.GetByEmailAsync(email))
                .ReturnsAsync((User)null);

            // Act & Assert
            await Assert.ThrowsAsync<KeyNotFoundException>(() => _userService.GetByEmailAsync(email));
        }

        // ==================== TEST: GET CURRENT USER ====================

        [Fact]
        public async Task GetCurrentUserAsync_WhenAuthenticated_ReturnsUser()
        {
            // Arrange
            int userId = 1;
            User user = CreateUser(userId);

            _currentUserServiceMock
                .Setup(service => service.UserId)
                .Returns(userId);

            _userRepositoryMock
                .Setup(repo => repo.GetByIdAsync(userId))
                .ReturnsAsync(user);

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
            User user = CreateUser(userId);

            UpdateUserRequestDTO request = new UpdateUserRequestDTO
            {
                UserName = "María García",
                Email = "maria@email.com"
            };

            _userRepositoryMock
                .Setup(repo => repo.GetByIdAsync(userId))
                .ReturnsAsync(user);

            _userRepositoryMock
                .Setup(repo => repo.GetByEmailAsync(request.Email))
                .ReturnsAsync((User)null);

            // Act
            UserResponseDTO result = await _userService.UpdateAsync(request);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("María García", result.UserName);
            Assert.Equal("maria@email.com", result.Email);

            _userRepositoryMock.Verify(repo => repo.UpdateAsync(It.IsAny<User>()), Times.Once);
        }

        [Fact]
        public async Task UpdateAsync_WithEmailAlreadyInUse_ThrowsInvalidOperationException()
        {
            // Arrange
            int userId = 1;
            User user = CreateUser(userId);
            User otherUser = CreateUser(2, "Otro Usuario", "maria@email.com");

            UpdateUserRequestDTO request = new UpdateUserRequestDTO
            {
                UserName = "Juan Pérez",
                Email = "maria@email.com"
            };

            _userRepositoryMock
                .Setup(repo => repo.GetByIdAsync(userId))
                .ReturnsAsync(user);

            _userRepositoryMock
                .Setup(repo => repo.GetByEmailAsync(request.Email))
                .ReturnsAsync(otherUser);

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() => _userService.UpdateAsync(request));
        }

        // ==================== TEST: DELETE ====================

        [Fact]
        public async Task DeleteAsync_WithExistingId_DeletesUser()
        {
            // Arrange
            int userId = 1;

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
            string email = "juan@email.com";
            string password = "Password123!";
            User user = CreateUserWithPassword(password);

            _userRepositoryMock
                .Setup(repo => repo.GetByEmailAsync(email))
                .ReturnsAsync(user);

            // Act
            string token = await _userService.LoginAsync(email, password);

            // Assert
            Assert.NotNull(token);
            Assert.StartsWith("token_", token);
        }

        [Fact]
        public async Task LoginAsync_WithInvalidEmail_ThrowsUnauthorizedAccessException()
        {
            // Arrange
            string email = "noexiste@email.com";

            _userRepositoryMock
                .Setup(repo => repo.GetByEmailAsync(email))
                .ReturnsAsync((User)null);

            // Act & Assert
            await Assert.ThrowsAsync<UnauthorizedAccessException>(() => _userService.LoginAsync(email, "Password123!"));
        }

        [Fact]
        public async Task LoginAsync_WithInvalidPassword_ThrowsUnauthorizedAccessException()
        {
            // Arrange
            string email = "juan@email.com";
            User user = CreateUserWithPassword("Password123!");

            _userRepositoryMock
                .Setup(repo => repo.GetByEmailAsync(email))
                .ReturnsAsync(user);

            // Act & Assert
            await Assert.ThrowsAsync<UnauthorizedAccessException>(() => _userService.LoginAsync(email, "WrongPassword"));
        }

        [Fact]
        public async Task LoginAsync_WithInactiveUser_ThrowsUnauthorizedAccessException()
        {
            // Arrange
            string email = "juan@email.com";
            User user = CreateUserWithPassword("Password123!");
            user.Deactivate();

            _userRepositoryMock
                .Setup(repo => repo.GetByEmailAsync(email))
                .ReturnsAsync(user);

            // Act & Assert
            await Assert.ThrowsAsync<UnauthorizedAccessException>(() => _userService.LoginAsync(email, "Password123!"));
        }

        // ==================== TEST: CHANGE PASSWORD ====================

        [Fact]
        public async Task ChangePasswordAsync_WithValidData_UpdatesPassword()
        {
            // Arrange
            int userId = 1;
            string currentPassword = "Password123!";
            string newPassword = "NewPassword456!";
            User user = CreateUserWithPassword(currentPassword);

            _userRepositoryMock
                .Setup(repo => repo.GetByIdAsync(userId))
                .ReturnsAsync(user);

            // Act
            await _userService.ChangePasswordAsync(currentPassword, newPassword);

            // Assert
            _userRepositoryMock.Verify(repo => repo.UpdateAsync(It.IsAny<User>()), Times.Once);
        }

        [Fact]
        public async Task ChangePasswordAsync_WithInvalidCurrentPassword_ThrowsUnauthorizedAccessException()
        {
            // Arrange
            int userId = 1;
            User user = CreateUserWithPassword("Password123!");

            _userRepositoryMock
                .Setup(repo => repo.GetByIdAsync(userId))
                .ReturnsAsync(user);

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