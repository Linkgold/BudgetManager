using Domain.Entities;
using Domain.Interfaces;
using Domain.ValueObjects;
using Infrastructure.Data;
using Infrastructure.Repositories;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Tests.Helpers;

namespace Tests.Infrastructure
{
    public class UserRepositoryTests : IDisposable
    {
        private readonly SqliteConnection _connection;
        private readonly ApplicationDbContext _dbContext;
        private readonly IUserRepository _repository;
        private readonly ICategoryRepository _categoryRepository;
        private readonly IBudgetRepository _budgetRepository;
        private readonly IFixedExpenseRepository _fixedExpenseRepository;
        private readonly ITransactionRepository _transactionRepository;

        public UserRepositoryTests()
        {
            _connection = new SqliteConnection("DataSource=:memory:");
            _connection.Open();

            DbContextOptions<ApplicationDbContext> options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseSqlite(_connection)
                .Options;

            _dbContext = new ApplicationDbContext(options);
            _dbContext.Database.EnsureCreated();

            _repository = new UserRepository(_dbContext);
            _categoryRepository = new CategoryRepository(_dbContext);
            _budgetRepository = new BudgetRepository(_dbContext);
            _fixedExpenseRepository = new FixedExpenseRepository(_dbContext);
            _transactionRepository = new TransactionRepository(_dbContext);
        }

        // ==================== TEST: ADD ====================

        [Fact]
        public async Task AddAsync_ShouldAddUserToDatabase()
        {
            // Arrange
            User user = await TestDataFactory.SeedUserAsync(_repository, 1);

            // Assert
            User? retrieved = await _dbContext.Users.FirstOrDefaultAsync(u => u.Id == user.Id);

            Assert.NotNull(retrieved);
            Assert.Equal(TestDataFactory.DEFAULT_USER_NAME, retrieved.Info.UserName);
            Assert.Equal(TestDataFactory.DEFAULT_USER_EMAIL, retrieved.Info.Email);
            Assert.NotEqual(default, retrieved.CreatedAt);
        }

        // ==================== TEST: GET BY ID ====================

        [Fact]
        public async Task GetByIdAsync_WithExistingId_ReturnsUser()
        {
            // Arrange
            User user = await TestDataFactory.SeedUserAsync(_repository, 1);

            // Act
            User? retrieved = await _repository.GetByIdAsync(user.Id);

            // Assert
            Assert.NotNull(retrieved);
            Assert.Equal(user.Id, retrieved.Id);
            Assert.Equal(TestDataFactory.DEFAULT_USER_NAME, retrieved.Info.UserName);
            Assert.Equal(TestDataFactory.DEFAULT_USER_EMAIL, retrieved.Info.Email);
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
            User user = await TestDataFactory.SeedUserAsync(_repository, 1);

            // Act
            User? retrieved = await _repository.GetByEmailAsync(TestDataFactory.DEFAULT_USER_EMAIL);

            // Assert
            Assert.NotNull(retrieved);
            Assert.Equal(user.Id, retrieved.Id);
            Assert.Equal(TestDataFactory.DEFAULT_USER_EMAIL, retrieved.Info.Email);
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
            User user = await TestDataFactory.SeedUserAsync(_repository, 1);

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
            User user = await TestDataFactory.SeedUserAsync(_repository, 1);

            // Act
            bool exists = await _repository.ExistsByEmailAsync(TestDataFactory.DEFAULT_USER_EMAIL);

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
            UserInfo updatedUserInfo = TestDataFactory.CreateUserInfo("María García", "maria@email.com");

            User user = await TestDataFactory.SeedUserAsync(_repository, 1);

            // Modificar el usuario
            user.Update(updatedUserInfo);

            // Act
            await _repository.UpdateAsync(user);

            // Assert
            User? updated = await _repository.GetByIdAsync(user.Id);
            Assert.NotNull(updated);
            Assert.Equal(updatedUserInfo.UserName, updated.Info.UserName);
            Assert.Equal(updatedUserInfo.Email, updated.Info.Email);
            Assert.NotNull(updated.UpdatedAt);
        }

        // ==================== TEST: DELETE ====================

        [Fact]
        public async Task DeleteAsync_ShouldRemoveUser()
        {
            // Arrange
            User user = await TestDataFactory.SeedUserAsync(_repository, 1);
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

        [Fact]
        public async Task DeleteAsync_WithUserHavingAllRelations_ShouldDeleteUserAndAllRelatedEntities()
        {
            // Arrange
            int userId = 1;
            User user = TestDataFactory.CreateUser(userId);
            Category category = TestDataFactory.CreateCategory(1, user, "Alimentación");
            Budget budget = TestDataFactory.CreateBudget(1, user, category);
            FixedExpense fixedExpense = TestDataFactory.CreateFixedExpense(1, user, category);
            Transaction transaction = TestDataFactory.CreateTransaction(1, user, category);

            // Persistir todas las entidades
            await _repository.AddAsync(user);
            await _categoryRepository.AddAsync(category);
            await _budgetRepository.AddAsync(budget);
            await _fixedExpenseRepository.AddAsync(fixedExpense);
            await _transactionRepository.AddAsync(transaction);

            // Verificar que existen
            Assert.NotNull(await _repository.GetByIdAsync(userId));
            Assert.NotNull(await _categoryRepository.GetByIdAsync(userId, category.Id));
            Assert.NotNull(await _budgetRepository.GetByIdAsync(userId, budget.Id));
            Assert.NotNull(await _fixedExpenseRepository.GetByIdAsync(userId, fixedExpense.Id));
            Assert.NotNull(await _transactionRepository.GetByIdAsync(userId, transaction.Id));

            // Act
            await _repository.DeleteAsync(userId);

            // Assert
            Assert.Null(await _repository.GetByIdAsync(userId));
            Assert.Null(await _categoryRepository.GetByIdAsync(userId, category.Id));
            Assert.Null(await _budgetRepository.GetByIdAsync(userId, budget.Id));
            Assert.Null(await _fixedExpenseRepository.GetByIdAsync(userId, fixedExpense.Id));
            Assert.Null(await _transactionRepository.GetByIdAsync(userId, transaction.Id));
        }

        // ==================== DISPOSE ====================

        public void Dispose()
        {
            _dbContext?.Dispose();
            _connection.Dispose();
        }
    }
}