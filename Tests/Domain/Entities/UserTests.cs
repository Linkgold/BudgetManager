using Domain.Entities;
using Domain.ValueObjects;
using Tests.Helpers;

namespace Tests.Domain.Entities
{
    public class UserTests
    {
        // ==================== CONSTRUCTOR ====================

        [Fact]
        public void Constructor_WithValidValues_ShouldCreateUser()
        {
            // Arrange
            UserInfo userInfo = TestDataFactory.CreateUserInfo();
            string passwordHash = "hashed_password";

            // Act
            User user = TestDataFactory.CreateUser(userInfo, passwordHash);

            // Assert
            Assert.NotNull(user);
            Assert.Equal(TestDataFactory.DEFAULT_USER_NAME, user.Info.UserName);
            Assert.Equal(TestDataFactory.DEFAULT_USER_EMAIL, user.Info.Email);
            Assert.Equal(passwordHash, user.PasswordHash);
            Assert.True(user.IsActive);
            Assert.NotEqual(default, user.CreatedAt);
            Assert.Null(user.UpdatedAt);
            Assert.NotNull(user.Categories);
            Assert.NotNull(user.FixedExpenses);
            Assert.NotNull(user.Budgets);
            Assert.NotNull(user.Transactions);
        }

        [Fact]
        public void Constructor_WithNullUserInfo_ShouldThrowArgumentNullException()
        {
            // Act & Assert
            ArgumentNullException exception = Assert.Throws<ArgumentNullException>(() => new User(null, "hashed_password"));

            Assert.Equal("info", exception.ParamName);
        }

        [Fact]
        public void Constructor_WithNullPasswordHash_ShouldThrowArgumentNullException()
        {
            // Arrange
            UserInfo userInfo = TestDataFactory.CreateUserInfo();

            // Act & Assert
            ArgumentNullException exception = Assert.Throws<ArgumentNullException>(() => new User(userInfo, null));

            Assert.Equal("passwordHash", exception.ParamName);
        }

        [Fact]
        public void Constructor_WithEmptyPasswordHash_ShouldThrowArgumentException()
        {
            // Arrange
            UserInfo userInfo = TestDataFactory.CreateUserInfo();

            // Act & Assert
            ArgumentException exception = Assert.Throws<ArgumentException>(() => new User(userInfo, ""));

            Assert.Contains("Password hash cannot be empty", exception.Message);
        }

        // ==================== UPDATE ====================

        [Fact]
        public void Update_WithValidValues_ShouldUpdateUser()
        {
            // Arrange
            User user = TestDataFactory.CreateUser();
            UserInfo newUserInfo = TestDataFactory.CreateUserInfo("María García", "maria@email.com");

            // Act
            user.Update(newUserInfo);

            // Assert
            Assert.Equal("María García", user.Info.UserName);
            Assert.Equal("maria@email.com", user.Info.Email);
            Assert.NotNull(user.UpdatedAt);
        }

        [Fact]
        public void Update_WithNullUserInfo_ShouldThrowArgumentNullException()
        {
            // Arrange
            User user = TestDataFactory.CreateUser();

            // Act & Assert
            ArgumentNullException exception = Assert.Throws<ArgumentNullException>(() => user.Update(null));

            Assert.Equal("newInfo", exception.ParamName);
        }

        // ==================== UPDATE PASSWORD ====================

        [Fact]
        public void UpdatePassword_WithValidValue_ShouldUpdatePassword()
        {
            // Arrange
            User user = TestDataFactory.CreateUser();
            string newPasswordHash = "new_hashed_password";

            // Act
            user.UpdatePassword(newPasswordHash);

            // Assert
            Assert.Equal(newPasswordHash, user.PasswordHash);
            Assert.NotNull(user.UpdatedAt);
        }

        [Fact]
        public void UpdatePassword_WithNullPasswordHash_ShouldThrowArgumentNullException()
        {
            // Arrange
            User user = TestDataFactory.CreateUser();

            // Act & Assert
            ArgumentNullException exception = Assert.Throws<ArgumentNullException>(() => user.UpdatePassword(null));

            Assert.Equal("newPasswordHash", exception.ParamName);
        }

        [Fact]
        public void UpdatePassword_WithEmptyPasswordHash_ShouldThrowArgumentException()
        {
            // Arrange
            User user = TestDataFactory.CreateUser();

            // Act & Assert
            ArgumentException exception = Assert.Throws<ArgumentException>(() => user.UpdatePassword(""));

            Assert.Contains("Password hash cannot be empty", exception.Message);
        }

        // ==================== ACTIVATE / DEACTIVATE ====================

        [Fact]
        public void Activate_ShouldActivateUser()
        {
            // Arrange
            User user = TestDataFactory.CreateUser();
            user.Deactivate();

            // Act
            user.Activate();

            // Assert
            Assert.True(user.IsActive);
            Assert.NotNull(user.UpdatedAt);
        }

        [Fact]
        public void Deactivate_ShouldDeactivateUser()
        {
            // Arrange
            User user = TestDataFactory.CreateUser();

            // Act
            user.Deactivate();

            // Assert
            Assert.False(user.IsActive);
            Assert.NotNull(user.UpdatedAt);
        }

        // ==================== TO STRING ====================

        [Fact]
        public void ToString_ShouldReturnFormattedString()
        {
            // Arrange
            User user = TestDataFactory.CreateUser();

            // Act
            string result = user.ToString();

            // Assert
            Assert.Equal($"{TestDataFactory.DEFAULT_USER_NAME} <{TestDataFactory.DEFAULT_USER_EMAIL}>", result);
        }

        // ==================== NAVIGATION PROPERTIES ====================

        [Fact]
        public void NavigationProperties_ShouldBeInitialized()
        {
            // Arrange
            User user = TestDataFactory.CreateUser();

            // Assert
            Assert.NotNull(user.Categories);
            Assert.NotNull(user.FixedExpenses);
            Assert.NotNull(user.Budgets);
            Assert.NotNull(user.Transactions);
            Assert.Empty(user.Categories);
            Assert.Empty(user.FixedExpenses);
            Assert.Empty(user.Budgets);
            Assert.Empty(user.Transactions);
        }
    }
}