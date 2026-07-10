using Domain.ValueObjects;

namespace Tests.Domain.ValueObjects
{
    public class UserInfoTests
    {
        // ==================== CONSTRUCTOR ====================

        [Fact]
        public void Constructor_WithValidValues_ShouldCreateUserInfo()
        {
            // Act
            UserInfo userInfo = new UserInfo("Juan Pérez", "juan@email.com");

            // Assert
            Assert.Equal("Juan Pérez", userInfo.UserName);
            Assert.Equal("juan@email.com", userInfo.Email);
        }

        [Fact]
        public void Constructor_WithEmptyUserName_ShouldThrowArgumentException()
        {
            // Act & Assert
            ArgumentException exception = Assert.Throws<ArgumentException>(() => new UserInfo("", "juan@email.com"));

            Assert.Contains("User name cannot be empty", exception.Message);
        }

        [Fact]
        public void Constructor_WithUserNameTooShort_ShouldThrowArgumentException()
        {
            // Act & Assert
            ArgumentException exception = Assert.Throws<ArgumentException>(() => new UserInfo("A", "juan@email.com"));

            Assert.Contains("User name must be between 2 and 50 characters", exception.Message);
        }

        [Fact]
        public void Constructor_WithUserNameTooLong_ShouldThrowArgumentException()
        {
            // Arrange
            string longName = new string('a', 51);

            // Act & Assert
            ArgumentException exception = Assert.Throws<ArgumentException>(() => new UserInfo(longName, "juan@email.com"));

            Assert.Contains("User name must be between 2 and 50 characters", exception.Message);
        }

        [Fact]
        public void Constructor_WithEmptyEmail_ShouldThrowArgumentException()
        {
            // Act & Assert
            ArgumentException exception = Assert.Throws<ArgumentException>(() => new UserInfo("Juan Pérez", ""));

            Assert.Contains("Email cannot be empty", exception.Message);
        }

        [Fact]
        public void Constructor_WithInvalidEmail_ShouldThrowArgumentException()
        {
            // Act & Assert
            ArgumentException exception = Assert.Throws<ArgumentException>(() => new UserInfo("Juan Pérez", "email-invalido"));

            Assert.Contains("Invalid email format", exception.Message);
        }

        [Fact]
        public void Constructor_ShouldTrimAndLowerEmail()
        {
            // Act
            UserInfo userInfo = new UserInfo("Juan Pérez", "  JUAN@EMAIL.COM  ");

            // Assert
            Assert.Equal("Juan Pérez", userInfo.UserName);
            Assert.Equal("juan@email.com", userInfo.Email);
        }

        // ==================== EQUALITY ====================

        [Fact]
        public void Equals_WithSameValues_ShouldReturnTrue()
        {
            // Arrange
            UserInfo info1 = new UserInfo("Juan Pérez", "juan@email.com");
            UserInfo info2 = new UserInfo("Juan Pérez", "juan@email.com");

            // Act & Assert
            Assert.True(info1.Equals(info2));
            Assert.True(info1 == info2);
        }

        [Fact]
        public void Equals_WithDifferentUserName_ShouldReturnFalse()
        {
            // Arrange
            UserInfo info1 = new UserInfo("Juan Pérez", "juan@email.com");
            UserInfo info2 = new UserInfo("María García", "juan@email.com");

            // Act & Assert
            Assert.False(info1.Equals(info2));
            Assert.False(info1 == info2);
        }

        [Fact]
        public void Equals_WithDifferentEmail_ShouldReturnFalse()
        {
            // Arrange
            UserInfo info1 = new UserInfo("Juan Pérez", "juan@email.com");
            UserInfo info2 = new UserInfo("Juan Pérez", "maria@email.com");

            // Act & Assert
            Assert.False(info1.Equals(info2));
            Assert.False(info1 == info2);
        }

        // ==================== TO STRING ====================

        [Fact]
        public void ToString_ShouldReturnFormattedString()
        {
            // Arrange
            UserInfo userInfo = new UserInfo("Juan Pérez", "juan@email.com");

            // Act
            string result = userInfo.ToString();

            // Assert
            Assert.Equal("Juan Pérez <juan@email.com>", result);
        }
    }
}