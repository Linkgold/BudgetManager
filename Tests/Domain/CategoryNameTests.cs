using Domain.ValueObjects;
using FluentAssertions;

namespace Tests.Domain
{
    public class CategoryNameTests
    {
        [Fact]
        public void Constructor_WithValidName_CreatesCategoryName()
        {
            // Arrange
            string validName = "Alimentación";

            // Act
            CategoryName categoryName = new CategoryName(validName);

            // Assert
            categoryName.Value.Should().Be("Alimentación");
        }

        [Theory]
        [InlineData("")]
        [InlineData(" ")]
        [InlineData(null)]
        public void Constructor_WithNullOrWhiteSpace_ThrowsArgumentException(string invalidName)
        {
            // Act
            Action act = () => new CategoryName(invalidName);

            // Assert
            act.Should().Throw<ArgumentException>().WithMessage("Category name cannot be empty*");
        }

        [Fact]
        public void Constructor_WithTooShortName_ThrowsArgumentException()
        {
            // Arrange
            string shortName = "A";

            // Act
            Action act = () => new CategoryName(shortName);

            // Assert
            act.Should().Throw<ArgumentException>().WithMessage("Category name must have at least 2 characters*");
        }

        [Fact]
        public void Constructor_WithTooLongName_ThrowsArgumentException()
        {
            // Arrange
            string longName = new string('x', 51);

            // Act
            Action act = () => new CategoryName(longName);

            // Assert
            act.Should().Throw<ArgumentException>().WithMessage("Category name cannot exceed 50 characters*");
        }

        [Fact]
        public void Constructor_WithEmptySpacesName_NormalizesTrimmed()
        {
            // Arrange
            string emptySpaces = "  Alimentación  ";

            // Act
            CategoryName categoryName = new CategoryName(emptySpaces);

            // Assert
            categoryName.Value.Should().Be("Alimentación");
        }

        [Fact]
        public void Equals_SameValue_ReturnsTrue()
        {
            // Arrange
            CategoryName name1 = new CategoryName("Comida");
            CategoryName name2 = new CategoryName("comida"); // Diferente casing

            // Act & Assert
            name1.Equals(name2).Should().BeTrue();
            (name1 == name2).Should().BeTrue();
            (name1 != name2).Should().BeFalse();
        }

        [Fact]
        public void Equals_DifferentValue_ReturnsFalse()
        {
            // Arrange
            CategoryName name1 = new CategoryName("Comida");
            CategoryName name2 = new CategoryName("Bebida");

            // Act & Assert
            name1.Equals(name2).Should().BeFalse();
            (name1 == name2).Should().BeFalse();
            (name1 != name2).Should().BeTrue();
        }

        [Fact]
        public void ImplicitOperator_ReturnsStringValue()
        {
            // Arrange
            CategoryName categoryName = new CategoryName("Transporte");

            // Act
            string stringValue = categoryName; // conversión implícita

            // Assert
            stringValue.Should().Be("Transporte");
        }

        [Fact]
        public void ExplicitOperator_CreatesCategoryNameFromString()
        {
            // Arrange
            string name = "Salud";

            // Act
            CategoryName categoryName = (CategoryName)name;

            // Assert
            categoryName.Value.Should().Be("Salud");
        }

        [Fact]
        public void ToString_ReturnsValue()
        {
            // Arrange
            CategoryName categoryName = new CategoryName("Entretenimiento");

            // Act
            string result = categoryName.ToString();

            // Assert
            result.Should().Be("Entretenimiento");
        }
    }
}