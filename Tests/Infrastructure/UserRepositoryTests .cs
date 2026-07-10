using Domain.Entities;
using Domain.Interfaces;
using Domain.ValueObjects;
using Infrastructure.Data;
using Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Tests.Helpers;

namespace Tests.Infrastructure
{
    public class UserRepositoryTests : IDisposable
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly IUserRepository _repository;

        public UserRepositoryTests()
        {
            string databaseName = Guid.NewGuid().ToString();

            DbContextOptions<ApplicationDbContext> options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: databaseName)
                .Options;

            _dbContext = new ApplicationDbContext(options);
            _repository = new UserRepository(_dbContext);
        }

        // ==================== HELPERS ====================

        private async Task<User> SeedUserAsync(User user)
        {
            await _repository.AddAsync(user);
            return user;
        }

        // ==================== TEST: ADD ====================

        [Fact]
        public async Task AddAsync_ShouldAddUserToDatabase()
        {
            // Arrange<
            User user = TestDataFactory.CreateUser();

            // Act
            await _repository.AddAsync(user);

            // Assert
            User? retrieved = await _dbContext.Users.FirstOrDefaultAsync(u => u.Id == user.Id);

            Assert.NotNull(retrieved);
            Assert.Equal("Juan Pérez", retrieved.Info.UserName);
            Assert.Equal("juan@email.com", retrieved.Info.Email);
            Assert.NotEqual(default, retrieved.CreatedAt);
        }

        // ==================== TEST: GET BY ID ====================

        [Fact]
        public async Task GetByIdAsync_WithExistingId_ReturnsUser()
        {
            // Arrange
            User user = TestDataFactory.CreateUser();
            await SeedUserAsync(user);

            // Act
            User? retrieved = await _repository.GetByIdAsync(user.Id);

            // Assert
            Assert.NotNull(retrieved);
            Assert.Equal(user.Id, retrieved.Id);
            Assert.Equal("Juan Pérez", retrieved.Info.UserName);
            Assert.Equal("juan@email.com", retrieved.Info.Email);
        }

        [Fact]
        public async Task GetByIdAsync_WithNonExistingId_ReturnsNull()
        {
            // Act
            User? retrieved = await _repository.GetByIdAsync(999);

            // Assert
            Assert.Null(retrieved);
        }

        // ==================== TEST: GET BY EMAIL ====================

        [Fact]
        public async Task GetByEmailAsync_WithExistingEmail_ReturnsUser()
        {
            // Arrange
            User user = TestDataFactory.CreateUser();
            await SeedUserAsync(user);

            // Act
            User? retrieved = await _repository.GetByEmailAsync("juan@email.com");

            // Assert
            Assert.NotNull(retrieved);
            Assert.Equal(user.Id, retrieved.Id);
            Assert.Equal("juan@email.com", retrieved.Info.Email);
        }

        [Fact]
        public async Task GetByEmailAsync_WithNonExistingEmail_ReturnsNull()
        {
            // Act
            User? retrieved = await _repository.GetByEmailAsync("noexiste@email.com");

            // Assert
            Assert.Null(retrieved);
        }

        // ==================== TEST: EXISTS ====================

        [Fact]
        public async Task ExistsAsync_WithExistingId_ReturnsTrue()
        {
            // Arrange
            User user = TestDataFactory.CreateUser();
            await SeedUserAsync(user);

            // Act
            bool exists = await _repository.ExistsAsync(user.Id);

            // Assert
            Assert.True(exists);
        }

        [Fact]
        public async Task ExistsAsync_WithNonExistingId_ReturnsFalse()
        {
            // Act
            bool exists = await _repository.ExistsAsync(999);

            // Assert
            Assert.False(exists);
        }

        // ==================== TEST: EXISTS BY EMAIL ====================

        [Fact]
        public async Task ExistsByEmailAsync_WithExistingEmail_ReturnsTrue()
        {
            // Arrange
            User user = TestDataFactory.CreateUser();
            await SeedUserAsync(user);

            // Act
            bool exists = await _repository.ExistsByEmailAsync("juan@email.com");

            // Assert
            Assert.True(exists);
        }

        [Fact]
        public async Task ExistsByEmailAsync_WithNonExistingEmail_ReturnsFalse()
        {
            // Act
            bool exists = await _repository.ExistsByEmailAsync("noexiste@email.com");

            // Assert
            Assert.False(exists);
        }

        // ==================== TEST: UPDATE ====================

        [Fact]
        public async Task UpdateAsync_ShouldUpdateUser()
        {
            // Arrange
            User user = TestDataFactory.CreateUser();
            await SeedUserAsync(user);

            // Modificar el usuario
            UserInfo newUserInfo = new UserInfo("María García", "maria@email.com");
            user.Update(newUserInfo);

            // Act
            await _repository.UpdateAsync(user);

            // Assert
            User? updated = await _repository.GetByIdAsync(user.Id);
            Assert.NotNull(updated);
            Assert.Equal("María García", updated.Info.UserName);
            Assert.Equal("maria@email.com", updated.Info.Email);
            Assert.NotNull(updated.UpdatedAt);
        }

        // ==================== TEST: DELETE ====================

        [Fact]
        public async Task DeleteAsync_ShouldRemoveUser()
        {
            // Arrange
            User user = TestDataFactory.CreateUser();
            await SeedUserAsync(user);
            int id = user.Id;

            // Act
            await _repository.DeleteAsync(id);

            // Assert
            User? deleted = await _repository.GetByIdAsync(id);
            Assert.Null(deleted);
        }

        [Fact]
        public async Task DeleteAsync_WithNonExistingId_ThrowsKeyNotFoundException()
        {
            // Act & Assert
            await Assert.ThrowsAsync<KeyNotFoundException>(() => _repository.DeleteAsync(999));
        }

        // ==================== DISPOSE ====================

        public void Dispose()
        {
            _dbContext?.Dispose();
        }
    }
}