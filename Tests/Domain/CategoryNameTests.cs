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
            EntityInfo categoryName = new EntityInfo(validName);

            // Assert
            categoryName.Name.Should().Be("Alimentación");
        }

        [Theory]
        [InlineData("")]
        [InlineData(" ")]
        [InlineData(null)]
        public void Constructor_WithNullOrWhiteSpace_ThrowsArgumentException(string invalidName)
        {
            // Act
            Action act = () => new EntityInfo(invalidName);

            // Assert
            act.Should().Throw<ArgumentException>().WithMessage("Name cannot be empty*");
        }

        [Fact]
        public void Constructor_WithTooShortName_ThrowsArgumentException()
        {
            // Arrange
            string shortName = "A";

            // Act
            Action act = () => new EntityInfo(shortName);

            // Assert
            act.Should().Throw<ArgumentException>().WithMessage("Name must have at least 2 characters*");
        }

        [Fact]
        public void Constructor_WithTooLongName_ThrowsArgumentException()
        {
            // Arrange
            string longName = new string('x', 51);

            // Act
            Action act = () => new EntityInfo(longName);

            // Assert
            act.Should().Throw<ArgumentException>().WithMessage("Name cannot exceed 50 characters*");
        }

        [Fact]
        public void Constructor_WithEmptySpacesName_NormalizesTrimmed()
        {
            // Arrange
            string emptySpaces = "  Alimentación  ";

            // Act
            EntityInfo categoryName = new EntityInfo(emptySpaces);

            // Assert
            categoryName.Name.Should().Be("Alimentación");
        }

        [Fact]
        public void Equals_SameValue_ReturnsTrue()
        {
            // Arrange
            EntityInfo name1 = new EntityInfo("Comida");
            EntityInfo name2 = new EntityInfo("comida"); // Diferente casing

            // Act & Assert
            name1.Equals(name2).Should().BeTrue();
            (name1 == name2).Should().BeTrue();
            (name1 != name2).Should().BeFalse();
        }

        [Fact]
        public void Equals_DifferentValue_ReturnsFalse()
        {
            // Arrange
            EntityInfo name1 = new EntityInfo("Comida");
            EntityInfo name2 = new EntityInfo("Bebida");

            // Act & Assert
            name1.Equals(name2).Should().BeFalse();
            (name1 == name2).Should().BeFalse();
            (name1 != name2).Should().BeTrue();
        }

        [Fact]
        public void ImplicitOperator_ReturnsStringValue()
        {
            // Arrange
            EntityInfo categoryName = new EntityInfo("Transporte");

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
            EntityInfo categoryName = (EntityInfo)name;

            // Assert
            categoryName.Name.Should().Be("Salud");
        }

        [Fact]
        public void ToString_ReturnsValue()
        {
            // Arrange
            EntityInfo categoryName = new EntityInfo("Entretenimiento");

            // Act
            string result = categoryName.ToString();

            // Assert
            result.Should().Be("Entretenimiento");
        }
    }
}