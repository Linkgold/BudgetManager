using Domain.Entities;
using Domain.ValueObjects;
using FluentAssertions;
using System;
using System.Collections.Generic;
using System.Text;

namespace Tests.Domain
{
    public class CategoryTests
    {
        [Fact]
        public void Constructor_WithValidNameAndDescription_CreatesCategory()
        {
            // Arrange
            string name = "Alimentación";
            string description = "Gastos de comida y supermercado";

            // Act
            Category category = new Category(new EntityInfo(name, description));

            // Assert
            category.Info.Name.Should().Be("Alimentación");
            category.Info.Description.Should().Be(description);
            category.IsActive.Should().BeTrue();
            category.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
            category.UpdatedAt.Should().BeNull();
        }

        [Fact]
        public void Constructor_WithValidNameAndNullDescription_CreatesCategory()
        {
            // Act
            Category category = new Category(new EntityInfo("Ocio", null));

            // Assert
            category.Info.Description.Should().BeNull();
        }

        [Fact]
        public void Constructor_WithInvalidName_ThrowsArgumentException()
        {
            // Act
            Action act = () => new Category(new EntityInfo("", "Descripción"));

            // Assert
            act.Should().Throw<ArgumentException>().WithMessage("Name cannot be empty*");
        }

        [Fact]
        public void Update_WithValidData_UpdatesCategory()
        {
            // Arrange
            Category category = new Category(new EntityInfo("Viejo nombre", "Vieja descripción"));

            // Act
            category.Update(new EntityInfo("Nuevo nombre", "Nueva descripción"));

            // Assert
            category.Info.Name.Should().Be("Nuevo nombre");
            category.Info.Description.Should().Be("Nueva descripción");
            category.UpdatedAt.Should().NotBeNull();
            category.UpdatedAt.Value.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        }

        [Fact]
        public void Update_WithInvalidName_ThrowsArgumentException()
        {
            // Arrange
            Category category = new Category(new EntityInfo("Válido", "Descripción"));

            // Act
            Action act = () => category.Update(new EntityInfo("", "Nueva descripción"));

            // Assert
            act.Should().Throw<ArgumentException>().WithMessage("Name cannot be empty*");
        }

        [Fact]
        public void Activate_WhenInactive_SetsActiveTrueAndUpdatesTimestamp()
        {
            // Arrange
            Category category = new Category(new EntityInfo("Inactiva", "Descripción"));
            category.Deactivate(); // Ponemos inactiva

            // Act
            category.Activate();

            // Assert
            category.IsActive.Should().BeTrue();
            category.UpdatedAt.Should().NotBeNull();
        }

        [Fact]
        public void Deactivate_WhenActive_SetsActiveFalseAndUpdatesTimestamp()
        {
            // Arrange
            Category category = new Category(new EntityInfo("Activa", "Descripción"));

            // Act
            category.Deactivate();

            // Assert
            category.IsActive.Should().BeFalse();
            category.UpdatedAt.Should().NotBeNull();
        }

        [Fact]
        public void ToString_ReturnsFormattedString()
        {
            // Arrange
            Category category = new Category(new EntityInfo("Trabajo", "Gastos de oficina"));

            // Act
            string result = category.ToString();

            // Assert
            result.Should().Be("Category: Trabajo (ID: 0)");
        }

        // Este test solo pasa si el ID se asigna (normalmente en infraestructura)
        // Para pruebas unitarias, el ID será 0 porque no se asigna en dominio.
        // Es un test opcional, solo para demostración.
        [Fact]
        public void Category_IdIsZeroByDefault()
        {
            // Arrange & Act
            Category category = new Category(new EntityInfo("Prueba", ""));

            // Assert
            category.Id.Should().Be(0);
        }
    }
}
