using Domain.Entities;
using Domain.ValueObjects;
using FluentAssertions;

namespace Tests.Domain.Entities
{
    public class CategoryTests
    {
        // ==================== HELPER ====================

        private EntityInfo CreateEntityInfo(string name = "Alimentación", string description = "Gastos de comida")
        {
            return new EntityInfo(name, description);
        }

        private static Category CreateForTest(int id, EntityInfo info)
        {
            Category category = new Category(info);
            typeof(Category).GetProperty("Id")?.SetValue(category, id);
            return category;
        }

        // ==================== CONSTRUCTOR ====================

        [Fact]
        public void Constructor_WithValidValues_ShouldCreateCategory()
        {
            // Arrange
            EntityInfo info = CreateEntityInfo();

            // Act
            Category category = new Category(info);

            // Assert
            Assert.NotNull(category);
            Assert.Equal("Alimentación", category.Info.Name);
            Assert.Equal("Gastos de comida", category.Info.Description);
            Assert.True(category.IsActive);
            Assert.NotEqual(default(DateTime), category.CreatedAt);
            Assert.Null(category.UpdatedAt);
        }

        [Fact]
        public void Constructor_WithNullInfo_ShouldThrowArgumentNullException()
        {
            // Act & Assert
            ArgumentNullException exception = Assert.Throws<ArgumentNullException>(                () => new Category(null));

            Assert.Equal("info", exception.ParamName);
        }

        // ==================== UPDATE ====================

        [Fact]
        public void Update_WithValidValues_ShouldUpdateCategory()
        {
            // Arrange
            EntityInfo info = CreateEntityInfo();
            Category category = new Category(info);

            // Act
            EntityInfo newInfo = new EntityInfo("Comida", "Gastos de supermercado");
            category.Update(newInfo);

            // Assert
            Assert.Equal("Comida", category.Info.Name);
            Assert.Equal("Gastos de supermercado", category.Info.Description);
            Assert.NotNull(category.UpdatedAt);
        }

        [Fact]
        public void Update_WithNullInfo_ShouldThrowArgumentNullException()
        {
            // Arrange
            EntityInfo info = CreateEntityInfo();
            Category category = new Category(info);

            // Act & Assert
            ArgumentNullException exception = Assert.Throws<ArgumentNullException>(
                () => category.Update(null));

            Assert.Equal("newInfo", exception.ParamName);
        }

        // ==================== ACTIVAR / DESACTIVAR ====================

        [Fact]
        public void Activate_ShouldActivateCategory()
        {
            // Arrange
            EntityInfo info = CreateEntityInfo();
            Category category = new Category(info);
            category.Deactivate();

            // Act
            category.Activate();

            // Assert
            Assert.True(category.IsActive);
            Assert.NotNull(category.UpdatedAt);
        }

        [Fact]
        public void Deactivate_ShouldDeactivateCategory()
        {
            // Arrange
            EntityInfo info = CreateEntityInfo();
            Category category = new Category(info);

            // Act
            category.Deactivate();

            // Assert
            Assert.False(category.IsActive);
            Assert.NotNull(category.UpdatedAt);
        }

        // ==================== CAN BE DELETED ====================

        [Fact]
        public void CanBeDeleted_WhenNoExpensesAndNoBudgets_ShouldReturnTrue()
        {
            // Arrange
            EntityInfo info = CreateEntityInfo();
            Category category = new Category(info);

            // Act
            bool result = category.CanBeDeleted();

            // Assert
            Assert.True(result);
        }

        // ==================== TO STRING ====================

        [Fact]
        public void ToString_ShouldReturnFormattedString()
        {
            // Arrange
            EntityInfo info = CreateEntityInfo();
            Category category = new Category(info);

            // Act
            string result = category.ToString();

            // Assert
            Assert.Contains("Alimentación", result);
            Assert.Contains("ID", result);
        }

        // ==================== EQUALITY (opcional) ====================

        [Fact]
        public void TwoCategoriesWithDifferentIds_ShouldBeDifferent()
        {
            // Arrange
            EntityInfo info = new EntityInfo("Alimentación", "Gastos de comida");
            Category category1 = CreateForTest(1, info);
            Category category2 = CreateForTest(2, info);

            // Act & Assert
            Assert.NotEqual(category1.Id, category2.Id);
            Assert.NotSame(category1, category2);
        }

        [Fact]
        public void TwoCategoriesWithSameId_ShouldHaveSameId()
        {
            // Arrange
            EntityInfo info = new EntityInfo("Alimentación", "Gastos de comida");
            Category category1 = CreateForTest(1, info);
            Category category2 = CreateForTest(1, info);

            // Act & Assert
            Assert.Equal(category1.Id, category2.Id);
            Assert.NotSame(category1, category2);
        }

        [Fact]
        public void Category_WithSameId_ShouldBeEqualById()
        {
            // Arrange
            EntityInfo info = new EntityInfo("Alimentación", "Gastos de comida");
            Category category1 = CreateForTest(1, info);
            Category category2 = CreateForTest(1, info);

            // Act
            bool idsAreEqual = category1.Id == category2.Id;

            // Assert
            Assert.True(idsAreEqual);
        }
    }
}