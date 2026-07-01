using Domain.Entities;
using Domain.ValueObjects;

namespace Tests.Domain.Entities
{
    public class FixedExpenseTests
    {
        private Category CreateCategory()
        {
            EntityInfo info = new EntityInfo("Suscripciones", "Gastos de suscripciones mensuales");
            return new Category(info);
        }

        // ==================== CONSTRUCTOR ====================

        [Fact]
        public void Constructor_WithValidValues_ShouldCreateFixedExpense()
        {
            // Arrange
            Category category = CreateCategory();
            EntityInfo info = new EntityInfo("Netflix", "Suscripción mensual");
            Money amount = new Money(15.99m, "EUR");
            Period chargePeriod = new Period(1, 2024);

            // Act
            FixedExpense fixedExpense = new FixedExpense(category, info, amount, chargePeriod);

            // Assert
            Assert.NotNull(fixedExpense);
            Assert.Equal(category.Id, fixedExpense.CategoryId);
            Assert.Equal("Netflix", fixedExpense.Info.Name);
            Assert.Equal("Suscripción mensual", fixedExpense.Info.Description);
            Assert.Equal(15.99m, fixedExpense.Amount.Value);
            Assert.Equal(1, fixedExpense.ChargePeriod.Month);
            Assert.Equal(2024, fixedExpense.ChargePeriod.Year);
            Assert.True(fixedExpense.IsActive);
            Assert.NotEqual(default(DateTime), fixedExpense.CreatedAt);
            Assert.Null(fixedExpense.UpdatedAt);
        }

        [Fact]
        public void Constructor_WithNullCategory_ShouldThrowArgumentNullException()
        {
            // Arrange
            EntityInfo info = new EntityInfo("Netflix", "Suscripción mensual");
            Money amount = new Money(15.99m, "EUR");
            Period chargePeriod = new Period(1, 2024);

            // Act & Assert
            ArgumentNullException exception = Assert.Throws<ArgumentNullException>(() => new FixedExpense(null, info, amount, chargePeriod));

            Assert.Equal("category", exception.ParamName);
        }

        [Fact]
        public void Constructor_WithNullInfo_ShouldThrowArgumentNullException()
        {
            // Arrange
            Category category = CreateCategory();
            Money amount = new Money(15.99m, "EUR");
            Period chargePeriod = new Period(1, 2024);

            // Act & Assert
            ArgumentNullException exception = Assert.Throws<ArgumentNullException>(() => new FixedExpense(category, null, amount, chargePeriod));

            Assert.Equal("info", exception.ParamName);
        }

        [Fact]
        public void Constructor_WithNullAmount_ShouldThrowArgumentNullException()
        {
            // Arrange
            Category category = CreateCategory();
            EntityInfo info = new EntityInfo("Netflix", "Suscripción mensual");
            Period chargePeriod = new Period(1, 2024);

            // Act & Assert
            ArgumentNullException exception = Assert.Throws<ArgumentNullException>(() => new FixedExpense(category, info, null, chargePeriod));

            Assert.Equal("amount", exception.ParamName);
        }

        [Fact]
        public void Constructor_WithNullChargePeriod_ShouldThrowArgumentNullException()
        {
            // Arrange
            Category category = CreateCategory();
            EntityInfo info = new EntityInfo("Netflix", "Suscripción mensual");
            Money amount = new Money(15.99m, "EUR");

            // Act & Assert
            ArgumentNullException exception = Assert.Throws<ArgumentNullException>(() => new FixedExpense(category, info, amount, null));

            Assert.Equal("chargePeriod", exception.ParamName);
        }

        // ==================== ACTIVAR / DESACTIVAR ====================

        [Fact]
        public void Activate_ShouldActivateFixedExpense()
        {
            // Arrange
            Category category = CreateCategory();
            EntityInfo info = new EntityInfo("Netflix", "Suscripción mensual");
            Money amount = new Money(15.99m, "EUR");
            Period chargePeriod = new Period(1, 2024);
            FixedExpense fixedExpense = new FixedExpense(category, info, amount, chargePeriod);

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
            Category category = CreateCategory();
            EntityInfo info = new EntityInfo("Netflix", "Suscripción mensual");
            Money amount = new Money(15.99m, "EUR");
            Period chargePeriod = new Period(1, 2024);
            FixedExpense fixedExpense = new FixedExpense(category, info, amount, chargePeriod);

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
            Category category = CreateCategory();
            EntityInfo info = new EntityInfo("Netflix", "Suscripción mensual");
            Money amount = new Money(15.99m, "EUR");
            Period chargePeriod = new Period(1, 2024);
            FixedExpense fixedExpense = new FixedExpense(category, info, amount, chargePeriod);

            // Act
            EntityInfo newInfo = new EntityInfo("Netflix Premium", "Suscripción mensual Premium");
            Money newAmount = new Money(17.99m, "EUR");
            Period newChargePeriod = new Period(2, 2024);
            fixedExpense.Update(newInfo, newAmount, newChargePeriod);

            // Assert
            Assert.Equal("Netflix Premium", fixedExpense.Info.Name);
            Assert.Equal("Suscripción mensual Premium", fixedExpense.Info.Description);
            Assert.Equal(17.99m, fixedExpense.Amount.Value);
            Assert.Equal(2, fixedExpense.ChargePeriod.Month);
            Assert.Equal(2024, fixedExpense.ChargePeriod.Year);
            Assert.NotNull(fixedExpense.UpdatedAt);
        }

        [Fact]
        public void Update_WithNullInfo_ShouldThrowArgumentNullException()
        {
            // Arrange
            Category category = CreateCategory();
            EntityInfo info = new EntityInfo("Netflix", "Suscripción mensual");
            Money amount = new Money(15.99m, "EUR");
            Period chargePeriod = new Period(1, 2024);
            FixedExpense fixedExpense = new FixedExpense(category, info, amount, chargePeriod);

            // Act & Assert
            ArgumentNullException exception = Assert.Throws<ArgumentNullException>(() => fixedExpense.Update(null, new Money(17.99m, "EUR"), new Period(2, 2024)));

            Assert.Equal("info", exception.ParamName);
        }

        [Fact]
        public void Update_WithNullAmount_ShouldThrowArgumentNullException()
        {
            // Arrange
            Category category = CreateCategory();
            EntityInfo info = new EntityInfo("Netflix", "Suscripción mensual");
            Money amount = new Money(15.99m, "EUR");
            Period chargePeriod = new Period(1, 2024);
            FixedExpense fixedExpense = new FixedExpense(category, info, amount, chargePeriod);

            // Act & Assert
            ArgumentNullException exception = Assert.Throws<ArgumentNullException>(() => fixedExpense.Update(new EntityInfo("Netflix Premium", "Suscripción mensual Premium"), null, new Period(2, 2024)));

            Assert.Equal("amount", exception.ParamName);
        }

        [Fact]
        public void Update_WithNullChargePeriod_ShouldThrowArgumentNullException()
        {
            // Arrange
            Category category = CreateCategory();
            EntityInfo info = new EntityInfo("Netflix", "Suscripción mensual");
            Money amount = new Money(15.99m, "EUR");
            Period chargePeriod = new Period(1, 2024);
            FixedExpense fixedExpense = new FixedExpense(category, info, amount, chargePeriod);

            // Act & Assert
            ArgumentNullException exception = Assert.Throws<ArgumentNullException>(() => fixedExpense.Update(new EntityInfo("Netflix Premium", "Suscripción mensual Premium"), new Money(17.99m, "EUR"), null));

            Assert.Equal("chargePeriod", exception.ParamName);
        }

        // ==================== UPDATE AMOUNT ====================

        [Fact]
        public void UpdateAmount_WithValidValue_ShouldUpdateAmount()
        {
            // Arrange
            Category category = CreateCategory();
            EntityInfo info = new EntityInfo("Netflix", "Suscripción mensual");
            Money amount = new Money(15.99m, "EUR");
            Period chargePeriod = new Period(1, 2024);
            FixedExpense fixedExpense = new FixedExpense(category, info, amount, chargePeriod);

            // Act
            Money newAmount = new Money(17.99m, "EUR");
            fixedExpense.UpdateAmount(newAmount);

            // Assert
            Assert.Equal(17.99m, fixedExpense.Amount.Value);
            Assert.NotNull(fixedExpense.UpdatedAt);
        }

        [Fact]
        public void UpdateAmount_WithNullAmount_ShouldThrowArgumentNullException()
        {
            // Arrange
            Category category = CreateCategory();
            EntityInfo info = new EntityInfo("Netflix", "Suscripción mensual");
            Money amount = new Money(15.99m, "EUR");
            Period chargePeriod = new Period(1, 2024);
            FixedExpense fixedExpense = new FixedExpense(category, info, amount, chargePeriod);

            // Act & Assert
            ArgumentNullException exception = Assert.Throws<ArgumentNullException>(() => fixedExpense.UpdateAmount(null));

            Assert.Equal("newAmount", exception.ParamName);
        }

        // ==================== IS ACTIVE FOR PERIOD ====================

        [Fact]
        public void IsActiveForPeriod_WithActiveAndSamePeriod_ShouldReturnTrue()
        {
            // Arrange
            Category category = CreateCategory();
            EntityInfo info = new EntityInfo("Netflix", "Suscripción mensual");
            Money amount = new Money(15.99m, "EUR");
            Period chargePeriod = new Period(1, 2024);
            FixedExpense fixedExpense = new FixedExpense(category, info, amount, chargePeriod);

            Period period = new Period(1, 2024);

            // Act
            bool result = fixedExpense.IsActiveForPeriod(period);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void IsActiveForPeriod_WithActiveAndLaterPeriod_ShouldReturnTrue()
        {
            // Arrange
            Category category = CreateCategory();
            EntityInfo info = new EntityInfo("Netflix", "Suscripción mensual");
            Money amount = new Money(15.99m, "EUR");
            Period chargePeriod = new Period(1, 2024);
            FixedExpense fixedExpense = new FixedExpense(category, info, amount, chargePeriod);

            Period period = new Period(2, 2024);

            // Act
            bool result = fixedExpense.IsActiveForPeriod(period);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void IsActiveForPeriod_WithActiveAndEarlierPeriod_ShouldReturnFalse()
        {
            // Arrange
            Category category = CreateCategory();
            EntityInfo info = new EntityInfo("Netflix", "Suscripción mensual");
            Money amount = new Money(15.99m, "EUR");
            Period chargePeriod = new Period(2, 2024);
            FixedExpense fixedExpense = new FixedExpense(category, info, amount, chargePeriod);

            Period period = new Period(1, 2024);

            // Act
            bool result = fixedExpense.IsActiveForPeriod(period);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void IsActiveForPeriod_WithInactiveFixedExpense_ShouldReturnFalse()
        {
            // Arrange
            Category category = CreateCategory();
            EntityInfo info = new EntityInfo("Netflix", "Suscripción mensual");
            Money amount = new Money(15.99m, "EUR");
            Period chargePeriod = new Period(1, 2024);
            FixedExpense fixedExpense = new FixedExpense(category, info, amount, chargePeriod);
            fixedExpense.Deactivate();

            Period period = new Period(1, 2024);

            // Act
            bool result = fixedExpense.IsActiveForPeriod(period);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void IsActiveForPeriod_WithNullPeriod_ShouldThrowArgumentNullException()
        {
            // Arrange
            Category category = CreateCategory();
            EntityInfo info = new EntityInfo("Netflix", "Suscripción mensual");
            Money amount = new Money(15.99m, "EUR");
            Period chargePeriod = new Period(1, 2024);
            FixedExpense fixedExpense = new FixedExpense(category, info, amount, chargePeriod);

            // Act & Assert
            ArgumentNullException exception = Assert.Throws<ArgumentNullException>(() => fixedExpense.IsActiveForPeriod(null));

            Assert.Equal("period", exception.ParamName);
        }

        // ==================== TO STRING ====================

        [Fact]
        public void ToString_ShouldReturnFormattedString()
        {
            // Arrange
            Category category = CreateCategory();
            EntityInfo info = new EntityInfo("Netflix", "Suscripción mensual");
            Money amount = new Money(15.99m, "EUR");
            Period chargePeriod = new Period(1, 2024);
            FixedExpense fixedExpense = new FixedExpense(category, info, amount, chargePeriod);

            // Act
            string result = fixedExpense.ToString();

            // Assert
            Assert.Contains("Netflix", result);
            Assert.Contains("15,99 EUR", result);
            Assert.Contains("01-2024", result);
        }
    }
}