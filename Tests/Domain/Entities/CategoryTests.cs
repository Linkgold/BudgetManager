using Domain.Entities;
using Domain.ValueObjects;
using Tests.Helpers;

namespace Tests.Domain.Entities
{
    public class CategoryTests
    {
        // ==================== CONSTRUCTOR ====================

        [Fact]
        public void Constructor_WithValidValues_ShouldCreateCategory()
        {
            // Act
            Category category = TestDataFactory.CreateCategory();

            // Assert
            Assert.NotNull(category);
            Assert.Equal(TestDataFactory.DEFAULT_CATEGORY_NAME, category.Info.Name);
            Assert.Equal(TestDataFactory.DEFAULT_CATEGORY_DESCRIPTION, category.Info.Description);
            Assert.True(category.IsActive);
            Assert.NotEqual(default, category.CreatedAt);
            Assert.Null(category.UpdatedAt);
        }

        [Fact]
        public void Constructor_WithNullUser_ShouldThrowArgumentNullException()
        {
            // Arrange
            EntityInfo info = TestDataFactory.CreateEntityInfo();

            // Act & Assert
            ArgumentNullException exception = Assert.Throws<ArgumentNullException>(() => new Category(null, info));

            Assert.Equal("user", exception.ParamName);
        }

        [Fact]
        public void Constructor_WithNullInfo_ShouldThrowArgumentNullException()
        {
            // Arrange
            User user = TestDataFactory.CreateUser();

            // Act & Assert
            ArgumentNullException exception = Assert.Throws<ArgumentNullException>(() => new Category(user, null));

            Assert.Equal("info", exception.ParamName);
        }

        // ==================== UPDATE ====================

        [Fact]
        public void Update_WithValidValues_ShouldUpdateCategory()
        {
            // Arrange
            Category category = TestDataFactory.CreateCategory();

            // Act
            category.Update(TestDataFactory.CreateEntityInfo());

            // Assert
            Assert.Equal(TestDataFactory.DEFAULT_ENTITY_INFO_NAME, category.Info.Name);
            Assert.Equal(TestDataFactory.DEFAULT_ENTITY_INFO_DESCRIPTION, category.Info.Description);
            Assert.NotNull(category.UpdatedAt);
        }

        [Fact]
        public void Update_WithNullInfo_ShouldThrowArgumentNullException()
        {
            // Arrange
            Category category = TestDataFactory.CreateCategory();

            // Act & Assert
            ArgumentNullException exception = Assert.Throws<ArgumentNullException>(() => category.Update(null));

            Assert.Equal("info", exception.ParamName);
        }

        // ==================== ACTIVAR / DESACTIVAR ====================

        [Fact]
        public void Activate_ShouldActivateCategory()
        {
            // Arrange
            Category category = TestDataFactory.CreateCategory();
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
            Category category = TestDataFactory.CreateCategory();

            // Act
            category.Deactivate();

            // Assert
            Assert.False(category.IsActive);
            Assert.NotNull(category.UpdatedAt);
        }

        // ==================== TO STRING ====================

        [Fact]
        public void ToString_ShouldReturnFormattedString()
        {
            // Arrange
            Category category = TestDataFactory.CreateCategory();

            // Act
            string result = category.ToString();

            // Assert
            Assert.Contains(TestDataFactory.DEFAULT_CATEGORY_NAME, result);
            Assert.Contains(TestDataFactory.DEFAULT_CATEGORY_DESCRIPTION, result);
        }

        // ==================== EQUALITY (opcional) ====================

        [Fact]
        public void TwoCategoriesWithDifferentIds_ShouldBeDifferent()
        {
            // Arrange
            User user = TestDataFactory.CreateUser();
            EntityInfo info = TestDataFactory.CreateEntityInfo();
            Category category1 = TestDataFactory.CreateCategory(1, user, info);
            Category category2 = TestDataFactory.CreateCategory(2, user, info);

            // Act & Assert
            Assert.NotEqual(category1.Id, category2.Id);
            Assert.NotSame(category1, category2);
        }

        [Fact]
        public void TwoCategoriesWithSameId_ShouldHaveSameId()
        {
            // Arrange
            User user = TestDataFactory.CreateUser();
            EntityInfo info = TestDataFactory.CreateEntityInfo();
            Category category1 = TestDataFactory.CreateCategory(1, user, info);
            Category category2 = TestDataFactory.CreateCategory(1, user, info);

            // Act & Assert
            Assert.Equal(category1.Id, category2.Id);
            Assert.NotSame(category1, category2);
        }

        [Fact]
        public void Category_WithSameId_ShouldBeEqualById()
        {
            // Arrange
            User user = TestDataFactory.CreateUser();
            EntityInfo info = TestDataFactory.CreateEntityInfo();
            Category category1 = TestDataFactory.CreateCategory(1, user, info);
            Category category2 = TestDataFactory.CreateCategory(1, user, info);

            // Act
            bool idsAreEqual = category1.Id == category2.Id;

            // Assert
            Assert.True(idsAreEqual);
        }
    }
}