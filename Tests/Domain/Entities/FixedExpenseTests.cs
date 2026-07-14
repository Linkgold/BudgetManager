using Domain.Entities;
using Domain.ValueObjects;
using Tests.Helpers;

namespace Tests.Domain.Entities
{
    public class FixedExpenseTests
    {
        // ==================== CONSTRUCTOR ====================

        [Fact]
        public void Constructor_WithValidValues_ShouldCreateFixedExpense()
        {
            // Arrange
            Category category = TestDataFactory.CreateCategory();

            // Act
            FixedExpense fixedExpense = TestDataFactory.CreateFixedExpenseWithoutId(category: category);

            // Assert
            Assert.NotNull(fixedExpense);
            Assert.Equal(category.Id, fixedExpense.CategoryId);
            Assert.Equal(TestDataFactory.DEFAULT_ENTITY_INFO_NAME, fixedExpense.Info.Name);
            Assert.Equal(TestDataFactory.DEFAULT_ENTITY_INFO_DESCRIPTION, fixedExpense.Info.Description);
            Assert.Equal(TestDataFactory.DEFAULT_MONEY_AMOUNT, fixedExpense.Amount.Value);
            Assert.Equal(TestDataFactory.DEFAULT_MONTHLY_MONTH, fixedExpense.ChargePeriod.Month);
            Assert.Equal(TestDataFactory.DEFAULT_YEAR, fixedExpense.ChargePeriod.Year);
            Assert.True(fixedExpense.IsActive);
            Assert.NotEqual(default, fixedExpense.CreatedAt);
            Assert.Null(fixedExpense.UpdatedAt);
        }

        [Fact]
        public void Constructor_WithNullUser_ShouldThrowArgumentNullException()
        {
            // Arrange
            Category category = TestDataFactory.CreateCategory();
            EntityInfo info = TestDataFactory.CreateEntityInfo();
            Money amount = TestDataFactory.CreateMoney();
            MonthlyPeriod chargePeriod = TestDataFactory.CreateMonthlyPeriod();

            // Act & Assert
            ArgumentNullException exception = Assert.Throws<ArgumentNullException>(() => TestDataFactory.CreateFixedExpenseWithoutAutoCreation(null, category, info, amount, chargePeriod));

            Assert.Equal("user", exception.ParamName);
        }

        [Fact]
        public void Constructor_WithNullCategory_ShouldThrowArgumentNullException()
        {
            // Arrange
            User user = TestDataFactory.CreateUser();
            EntityInfo info = TestDataFactory.CreateEntityInfo();
            Money amount = TestDataFactory.CreateMoney();
            MonthlyPeriod chargePeriod = TestDataFactory.CreateMonthlyPeriod();

            // Act & Assert
            ArgumentNullException exception = Assert.Throws<ArgumentNullException>(() => TestDataFactory.CreateFixedExpenseWithoutAutoCreation(user, null, info, amount, chargePeriod));

            Assert.Equal("category", exception.ParamName);
        }

        [Fact]
        public void Constructor_WithNullInfo_ShouldThrowArgumentNullException()
        {
            // Arrange
            User user = TestDataFactory.CreateUser();
            Category category = TestDataFactory.CreateCategory();
            Money amount = TestDataFactory.CreateMoney();
            MonthlyPeriod chargePeriod = TestDataFactory.CreateMonthlyPeriod();

            // Act & Assert
            ArgumentNullException exception = Assert.Throws<ArgumentNullException>(() => TestDataFactory.CreateFixedExpenseWithoutAutoCreation(user, category, null, amount, chargePeriod));

            Assert.Equal("info", exception.ParamName);
        }

        [Fact]
        public void Constructor_WithNullAmount_ShouldThrowArgumentNullException()
        {
            // Arrange
            User user = TestDataFactory.CreateUser();
            Category category = TestDataFactory.CreateCategory();
            EntityInfo info = TestDataFactory.CreateEntityInfo();
            MonthlyPeriod chargePeriod = TestDataFactory.CreateMonthlyPeriod();

            // Act & Assert
            ArgumentNullException exception = Assert.Throws<ArgumentNullException>(() => TestDataFactory.CreateFixedExpenseWithoutAutoCreation(user, category, info, null, chargePeriod));

            Assert.Equal("amount", exception.ParamName);
        }

        [Fact]
        public void Constructor_WithNullChargePeriod_ShouldThrowArgumentNullException()
        {
            // Arrange
            User user = TestDataFactory.CreateUser();
            Category category = TestDataFactory.CreateCategory();
            EntityInfo info = TestDataFactory.CreateEntityInfo();
            Money amount = TestDataFactory.CreateMoney();

            // Act & Assert
            ArgumentNullException exception = Assert.Throws<ArgumentNullException>(() => TestDataFactory.CreateFixedExpenseWithoutAutoCreation(user, category, info, amount, null));

            Assert.Equal("chargePeriod", exception.ParamName);
        }

        // ==================== ACTIVAR / DESACTIVAR ====================

        [Fact]
        public void Activate_ShouldActivateFixedExpense()
        {
            // Arrange
            FixedExpense fixedExpense = TestDataFactory.CreateFixedExpenseWithoutId();

            // Act
            fixedExpense.Deactivate();
            fixedExpense.Activate();

            // Assert
            Assert.True(fixedExpense.IsActive);
            Assert.NotNull(fixedExpense.UpdatedAt);
        }

        [Fact]
        public void Deactivate_ShouldDeactivateFixedExpense()
        {
            // Arrange
            FixedExpense fixedExpense = TestDataFactory.CreateFixedExpenseWithoutId();

            // Act
            fixedExpense.Deactivate();

            // Assert
            Assert.False(fixedExpense.IsActive);
            Assert.NotNull(fixedExpense.UpdatedAt);
        }

        // ==================== UPDATE ====================

        [Fact]
        public void Update_WithValidValues_ShouldUpdateFixedExpense()
        {
            // Arrange
            string updatedName = "Netflix Premium";
            string updatedDescription = "Suscripción mensual Premium";
            decimal updatedAmount = 17.99m;
            int updatedmonth = 2;

            FixedExpense fixedExpense = TestDataFactory.CreateFixedExpenseWithoutId();

            // Act
            fixedExpense.Update
            (
                TestDataFactory.CreateEntityInfo(updatedName, updatedDescription), 
                TestDataFactory.CreateMoney(updatedAmount), 
                TestDataFactory.CreateMonthlyPeriod(updatedmonth)
            );

            // Assert
            Assert.Equal(updatedName, fixedExpense.Info.Name);
            Assert.Equal(updatedDescription, fixedExpense.Info.Description);
            Assert.Equal(updatedAmount, fixedExpense.Amount.Value);
            Assert.Equal(updatedmonth, fixedExpense.ChargePeriod.Month);
            Assert.Equal(TestDataFactory.DEFAULT_YEAR, fixedExpense.ChargePeriod.Year);
            Assert.NotNull(fixedExpense.UpdatedAt);
        }

        [Fact]
        public void Update_WithNullInfo_ShouldThrowArgumentNullException()
        {
            // Arrange
            FixedExpense fixedExpense = TestDataFactory.CreateFixedExpenseWithoutId();

            // Act & Assert
            ArgumentNullException exception = Assert.Throws<ArgumentNullException>(() => fixedExpense.Update(null, TestDataFactory.CreateMoney(), TestDataFactory.CreateMonthlyPeriod()));

            Assert.Equal("info", exception.ParamName);
        }

        [Fact]
        public void Update_WithNullAmount_ShouldThrowArgumentNullException()
        {
            // Arrange
            FixedExpense fixedExpense = TestDataFactory.CreateFixedExpenseWithoutId();

            // Act & Assert
            ArgumentNullException exception = Assert.Throws<ArgumentNullException>(() => fixedExpense.Update(TestDataFactory.CreateEntityInfo(), null, TestDataFactory.CreateMonthlyPeriod()));

            Assert.Equal("amount", exception.ParamName);
        }

        [Fact]
        public void Update_WithNullChargePeriod_ShouldThrowArgumentNullException()
        {
            // Arrange
            FixedExpense fixedExpense = TestDataFactory.CreateFixedExpenseWithoutId();

            // Act & Assert
            ArgumentNullException exception = Assert.Throws<ArgumentNullException>(() => fixedExpense.Update(TestDataFactory.CreateEntityInfo(), TestDataFactory.CreateMoney(), null));
            Assert.Equal("chargePeriod", exception.ParamName);
        }

        // ==================== UPDATE AMOUNT ====================

        [Fact]
        public void UpdateAmount_WithValidValue_ShouldUpdateAmount()
        {
            // Arrange
            decimal updatedAmount = 17.99m;

            FixedExpense fixedExpense = TestDataFactory.CreateFixedExpenseWithoutId();

            // Act
            fixedExpense.UpdateAmount(TestDataFactory.CreateMoney(updatedAmount));

            // Assert
            Assert.Equal(updatedAmount, fixedExpense.Amount.Value);
            Assert.NotNull(fixedExpense.UpdatedAt);
        }

        [Fact]
        public void UpdateAmount_WithNullAmount_ShouldThrowArgumentNullException()
        {
            // Arrange
            FixedExpense fixedExpense = TestDataFactory.CreateFixedExpenseWithoutId();

            // Act & Assert
            ArgumentNullException exception = Assert.Throws<ArgumentNullException>(() => fixedExpense.UpdateAmount(null));
            Assert.Equal("amount", exception.ParamName);
        }

        // ==================== IS ACTIVE FOR PERIOD ====================

        [Fact]
        public void IsActiveForPeriod_WithActiveAndSamePeriod_ShouldReturnTrue()
        {
            // Arrange
            FixedExpense fixedExpense = TestDataFactory.CreateFixedExpenseWithoutId();

            // Act
            bool result = fixedExpense.IsActiveForPeriod(TestDataFactory.CreateMonthlyPeriod());

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void IsActiveForPeriod_WithActiveAndLaterPeriod_ShouldReturnTrue()
        {
            // Arrange
            FixedExpense fixedExpense = TestDataFactory.CreateFixedExpenseWithoutId();

            // Act
            bool result = fixedExpense.IsActiveForPeriod(TestDataFactory.CreateMonthlyPeriod(2));

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void IsActiveForPeriod_WithActiveAndEarlierPeriod_ShouldReturnFalse()
        {
            // Arrange
            FixedExpense fixedExpense = TestDataFactory.CreateFixedExpenseWithoutId(monthlyPeriod: TestDataFactory.CreateMonthlyPeriod(2));

            // Act
            bool result = fixedExpense.IsActiveForPeriod(TestDataFactory.CreateMonthlyPeriod());

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void IsActiveForPeriod_WithInactiveFixedExpense_ShouldReturnFalse()
        {
            // Arrange
            FixedExpense fixedExpense = TestDataFactory.CreateFixedExpenseWithoutId();
            fixedExpense.Deactivate();

            // Act
            bool result = fixedExpense.IsActiveForPeriod(TestDataFactory.CreateMonthlyPeriod());

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void IsActiveForPeriod_WithNullPeriod_ShouldThrowArgumentNullException()
        {
            // Arrange
            FixedExpense fixedExpense = TestDataFactory.CreateFixedExpenseWithoutId();

            // Act & Assert
            ArgumentNullException exception = Assert.Throws<ArgumentNullException>(() => fixedExpense.IsActiveForPeriod(null));

            Assert.Equal("period", exception.ParamName);
        }

        // ==================== TO STRING ====================

        [Fact]
        public void ToString_ShouldReturnFormattedString()
        {
            // Arrange
            FixedExpense fixedExpense = TestDataFactory.CreateFixedExpenseWithoutId();

            // Act
            string result = fixedExpense.ToString();

            // Assert
            Assert.Contains(TestDataFactory.DEFAULT_ENTITY_INFO_NAME, result);
            Assert.Contains($"{TestDataFactory.DEFAULT_MONEY_AMOUNT}", result);
            Assert.Contains($"{TestDataFactory.DEFAULT_MONTHLY_MONTH}-{TestDataFactory.DEFAULT_YEAR}", result);
        }
    }
}