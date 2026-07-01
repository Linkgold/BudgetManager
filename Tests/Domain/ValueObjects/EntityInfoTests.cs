using Domain.ValueObjects;

namespace Tests.Domain.ValueObjects
{
    public class EntityInfoTests
    {
        [Fact]
        public void Constructor_WithValidValues_ShouldCreateEntityInfo()
        {
            // Act
            EntityInfo info = new EntityInfo("Test", "Test Description");

            // Assert
            Assert.Equal("Test", info.Name);
            Assert.Equal("Test Description", info.Description);
        }

        [Fact]
        public void Constructor_WithNullDescription_ShouldCreateEntityInfo()
        {
            // Act
            EntityInfo info = new EntityInfo("Test", null);

            // Assert
            Assert.Equal("Test", info.Name);
            Assert.Null(info.Description);
        }

        [Fact]
        public void Constructor_WithEmptyName_ShouldThrowArgumentException()
        {
            // Act & Assert
            ArgumentException exception = Assert.Throws<ArgumentException>(() => new EntityInfo(""));

            Assert.Contains("Name cannot be empty", exception.Message);
        }

        [Fact]
        public void Constructor_WithNameTooShort_ShouldThrowArgumentException()
        {
            // Arrange
            string longName = new string("a");

            // Act & Assert
            ArgumentException exception = Assert.Throws<ArgumentException>(() => new EntityInfo(longName));

            Assert.Contains("Name must have at least 2 characters", exception.Message);
        }

        [Fact]
        public void Constructor_WithNameTooLong_ShouldThrowArgumentException()
        {
            // Arrange
            string longName = new string('a', 51);

            // Act & Assert
            ArgumentException exception = Assert.Throws<ArgumentException>(() => new EntityInfo(longName));

            Assert.Contains("Name cannot exceed 50 characters", exception.Message);
        }

        [Fact]
        public void Constructor_WithDescriptionTooLong_ShouldThrowArgumentException()
        {
            // Arrange
            string longDescription = new string('a', 201);

            // Act & Assert
            ArgumentException exception = Assert.Throws<ArgumentException>(() => new EntityInfo("Test", longDescription));

            Assert.Contains("Description cannot exceed 200 characters", exception.Message);
        }

        [Fact]
        public void Constructor_ShouldTrimNameAndDescription()
        {
            // Act
            EntityInfo info = new EntityInfo("  Test  ", "  Description  ");

            // Assert
            Assert.Equal("Test", info.Name);
            Assert.Equal("Description", info.Description);
        }

        [Fact]
        public void Equals_WithSameValues_ShouldReturnTrue()
        {
            // Arrange
            EntityInfo info1 = new EntityInfo("Test", "Description");
            EntityInfo info2 = new EntityInfo("Test", "Description");

            // Act & Assert
            Assert.True(info1.Equals(info2));
            Assert.True(info1 == info2);
        }

        [Fact]
        public void Equals_WithDifferentName_ShouldReturnFalse()
        {
            // Arrange
            EntityInfo info1 = new EntityInfo("Test1", "Description");
            EntityInfo info2 = new EntityInfo("Test2", "Description");

            // Act & Assert
            Assert.False(info1.Equals(info2));
        }

        [Fact]
        public void ToString_WithoutDescription_ShouldReturnOnlyName()
        {
            // Arrange
            EntityInfo info = new EntityInfo("Test");

            // Act
            string result = info.ToString();

            // Assert
            Assert.Equal("Test", result);
        }

        [Fact]
        public void ToString_WithDescription_ShouldReturnNameAndDescription()
        {
            // Arrange
            EntityInfo info = new EntityInfo("Test", "Description");

            // Act
            string result = info.ToString();

            // Assert
            Assert.Equal("Test: Description", result);
        }

        [Fact]
        public void ImplicitConversion_ToString_ShouldReturnName()
        {
            // Arrange
            EntityInfo info = new EntityInfo("Test");

            // Act
            string result = info;

            // Assert
            Assert.Equal("Test", result);
        }

        [Fact]
        public void ExplicitConversion_FromString_ShouldCreateEntityInfo()
        {
            // Act
            EntityInfo info = (EntityInfo)"Test";

            // Assert
            Assert.Equal("Test", info.Name);
        }
    }
}