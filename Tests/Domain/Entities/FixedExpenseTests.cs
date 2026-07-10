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
            User user = TestDataFactory.CreateUser();
            Category category = TestDataFactory.CreateCategory();
            EntityInfo info = new EntityInfo("Netflix", "Suscripción mensual");
            Money amount = new Money(15.99m, "EUR");
            MonthlyPeriod chargePeriod = new MonthlyPeriod(1, 2024);

            // Act
            FixedExpense fixedExpense = new FixedExpense(user, category, info, amount, chargePeriod);

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
        public void Constructor_WithNullUser_ShouldThrowArgumentNullException()
        {
            // Arrange
            Category category = TestDataFactory.CreateCategory();
            EntityInfo info = new EntityInfo("Netflix", "Suscripción mensual");
            Money amount = new Money(15.99m, "EUR");
            MonthlyPeriod chargePeriod = new MonthlyPeriod(1, 2024);

            // Act & Assert
            ArgumentNullException exception = Assert.Throws<ArgumentNullException>(() => new FixedExpense(null, category, info, amount, chargePeriod));

            Assert.Equal("category", exception.ParamName);
        }

        [Fact]
        public void Constructor_WithNullCategory_ShouldThrowArgumentNullException()
        {
            // Arrange
            User user = TestDataFactory.CreateUser();
            EntityInfo info = new EntityInfo("Netflix", "Suscripción mensual");
            Money amount = new Money(15.99m, "EUR");
            MonthlyPeriod chargePeriod = new MonthlyPeriod(1, 2024);

            // Act & Assert
            ArgumentNullException exception = Assert.Throws<ArgumentNullException>(() => new FixedExpense(user, null, info, amount, chargePeriod));

            Assert.Equal("category", exception.ParamName);
        }

        [Fact]
        public void Constructor_WithNullInfo_ShouldThrowArgumentNullException()
        {
            // Arrange
            User user = TestDataFactory.CreateUser();
            Category category = TestDataFactory.CreateCategory();
            Money amount = new Money(15.99m, "EUR");
            MonthlyPeriod chargePeriod = new MonthlyPeriod(1, 2024);

            // Act & Assert
            ArgumentNullException exception = Assert.Throws<ArgumentNullException>(() => new FixedExpense(user, category, null, amount, chargePeriod));

            Assert.Equal("info", exception.ParamName);
        }

        [Fact]
        public void Constructor_WithNullAmount_ShouldThrowArgumentNullException()
        {
            // Arrange
            User user = TestDataFactory.CreateUser();
            Category category = TestDataFactory.CreateCategory();
            EntityInfo info = new EntityInfo("Netflix", "Suscripción mensual");
            MonthlyPeriod chargePeriod = new MonthlyPeriod(1, 2024);

            // Act & Assert
            ArgumentNullException exception = Assert.Throws<ArgumentNullException>(() => new FixedExpense(user, category, info, null, chargePeriod));

            Assert.Equal("amount", exception.ParamName);
        }

        [Fact]
        public void Constructor_WithNullChargePeriod_ShouldThrowArgumentNullException()
        {
            // Arrange
            User user = TestDataFactory.CreateUser();
            Category category = TestDataFactory.CreateCategory();
            EntityInfo info = new EntityInfo("Netflix", "Suscripción mensual");
            Money amount = new Money(15.99m, "EUR");

            // Act & Assert
            ArgumentNullException exception = Assert.Throws<ArgumentNullException>(() => new FixedExpense(user, category, info, amount, null));

            Assert.Equal("chargePeriod", exception.ParamName);
        }

        // ==================== ACTIVAR / DESACTIVAR ====================

        [Fact]
        public void Activate_ShouldActivateFixedExpense()
        {
            // Arrange
            User user = TestDataFactory.CreateUser();
            Category category = TestDataFactory.CreateCategory();
            EntityInfo info = new EntityInfo("Netflix", "Suscripción mensual");
            Money amount = new Money(15.99m, "EUR");
            MonthlyPeriod chargePeriod = new MonthlyPeriod(1, 2024);
            FixedExpense fixedExpense = new FixedExpense(user, category, info, amount, chargePeriod);

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
            User user = TestDataFactory.CreateUser();
            Category category = TestDataFactory.CreateCategory();
            EntityInfo info = new EntityInfo("Netflix", "Suscripción mensual");
            Money amount = new Money(15.99m, "EUR");
            MonthlyPeriod chargePeriod = new MonthlyPeriod(1, 2024);
            FixedExpense fixedExpense = new FixedExpense(user, category, info, amount, chargePeriod);

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
            User user = TestDataFactory.CreateUser();
            Category category = TestDataFactory.CreateCategory();
            EntityInfo info = new EntityInfo("Netflix", "Suscripción mensual");
            Money amount = new Money(15.99m, "EUR");
            MonthlyPeriod chargePeriod = new MonthlyPeriod(1, 2024);
            FixedExpense fixedExpense = new FixedExpense(user, category, info, amount, chargePeriod);

            // Act
            EntityInfo newInfo = new EntityInfo("Netflix Premium", "Suscripción mensual Premium");
            Money newAmount = new Money(17.99m, "EUR");
            MonthlyPeriod newChargePeriod = new MonthlyPeriod(2, 2024);
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
            User user = TestDataFactory.CreateUser();
            Category category = TestDataFactory.CreateCategory();
            EntityInfo info = new EntityInfo("Netflix", "Suscripción mensual");
            Money amount = new Money(15.99m, "EUR");
            MonthlyPeriod chargePeriod = new MonthlyPeriod(1, 2024);
            FixedExpense fixedExpense = new FixedExpense(user, category, info, amount, chargePeriod);

            // Act & Assert
            ArgumentNullException exception = Assert.Throws<ArgumentNullException>(() => fixedExpense.Update(null, new Money(17.99m, "EUR"), new MonthlyPeriod(2, 2024)));

            Assert.Equal("info", exception.ParamName);
        }

        [Fact]
        public void Update_WithNullAmount_ShouldThrowArgumentNullException()
        {
            // Arrange
            User user = TestDataFactory.CreateUser();
            Category category = TestDataFactory.CreateCategory();
            EntityInfo info = new EntityInfo("Netflix", "Suscripción mensual");
            Money amount = new Money(15.99m, "EUR");
            MonthlyPeriod chargePeriod = new MonthlyPeriod(1, 2024);
            FixedExpense fixedExpense = new FixedExpense(user, category, info, amount, chargePeriod);

            // Act & Assert
            ArgumentNullException exception = Assert.Throws<ArgumentNullException>(() => fixedExpense.Update(new EntityInfo("Netflix Premium", "Suscripción mensual Premium"), null, new MonthlyPeriod(2, 2024)));

            Assert.Equal("amount", exception.ParamName);
        }

        [Fact]
        public void Update_WithNullChargePeriod_ShouldThrowArgumentNullException()
        {
            // Arrange
            User user = TestDataFactory.CreateUser();
            Category category = TestDataFactory.CreateCategory();
            EntityInfo info = new EntityInfo("Netflix", "Suscripción mensual");
            Money amount = new Money(15.99m, "EUR");
            MonthlyPeriod chargePeriod = new MonthlyPeriod(1, 2024);
            FixedExpense fixedExpense = new FixedExpense(user, category, info, amount, chargePeriod);

            // Act & Assert
            ArgumentNullException exception = Assert.Throws<ArgumentNullException>(() => fixedExpense.Update(new EntityInfo("Netflix Premium", "Suscripción mensual Premium"), new Money(17.99m, "EUR"), null));

            Assert.Equal("chargePeriod", exception.ParamName);
        }

        // ==================== UPDATE AMOUNT ====================

        [Fact]
        public void UpdateAmount_WithValidValue_ShouldUpdateAmount()
        {
            // Arrange
            User user = TestDataFactory.CreateUser();
            Category category = TestDataFactory.CreateCategory();
            EntityInfo info = new EntityInfo("Netflix", "Suscripción mensual");
            Money amount = new Money(15.99m, "EUR");
            MonthlyPeriod chargePeriod = new MonthlyPeriod(1, 2024);
            FixedExpense fixedExpense = new FixedExpense(user, category, info, amount, chargePeriod);

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
            User user = TestDataFactory.CreateUser();
            Category category = TestDataFactory.CreateCategory();
            EntityInfo info = new EntityInfo("Netflix", "Suscripción mensual");
            Money amount = new Money(15.99m, "EUR");
            MonthlyPeriod chargePeriod = new MonthlyPeriod(1, 2024);
            FixedExpense fixedExpense = new FixedExpense(user, category, info, amount, chargePeriod);

            // Act & Assert
            ArgumentNullException exception = Assert.Throws<ArgumentNullException>(() => fixedExpense.UpdateAmount(null));

            Assert.Equal("newAmount", exception.ParamName);
        }

        // ==================== IS ACTIVE FOR PERIOD ====================

        [Fact]
        public void IsActiveForPeriod_WithActiveAndSamePeriod_ShouldReturnTrue()
        {
            // Arrange
            User user = TestDataFactory.CreateUser();
            Category category = TestDataFactory.CreateCategory();
            EntityInfo info = new EntityInfo("Netflix", "Suscripción mensual");
            Money amount = new Money(15.99m, "EUR");
            MonthlyPeriod chargePeriod = new MonthlyPeriod(1, 2024);
            FixedExpense fixedExpense = new FixedExpense(user, category, info, amount, chargePeriod);

            MonthlyPeriod period = new MonthlyPeriod(1, 2024);

            // Act
            bool result = fixedExpense.IsActiveForPeriod(period);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void IsActiveForPeriod_WithActiveAndLaterPeriod_ShouldReturnTrue()
        {
            // Arrange
            User user = TestDataFactory.CreateUser();
            Category category = TestDataFactory.CreateCategory();
            EntityInfo info = new EntityInfo("Netflix", "Suscripción mensual");
            Money amount = new Money(15.99m, "EUR");
            MonthlyPeriod chargePeriod = new MonthlyPeriod(1, 2024);
            FixedExpense fixedExpense = new FixedExpense(user, category, info, amount, chargePeriod);

            MonthlyPeriod period = new MonthlyPeriod(2, 2024);

            // Act
            bool result = fixedExpense.IsActiveForPeriod(period);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void IsActiveForPeriod_WithActiveAndEarlierPeriod_ShouldReturnFalse()
        {
            // Arrange
            User user = TestDataFactory.CreateUser();
            Category category = TestDataFactory.CreateCategory();
            EntityInfo info = new EntityInfo("Netflix", "Suscripción mensual");
            Money amount = new Money(15.99m, "EUR");
            MonthlyPeriod chargePeriod = new MonthlyPeriod(2, 2024);
            FixedExpense fixedExpense = new FixedExpense(user, category, info, amount, chargePeriod);

            MonthlyPeriod period = new MonthlyPeriod(1, 2024);

            // Act
            bool result = fixedExpense.IsActiveForPeriod(period);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void IsActiveForPeriod_WithInactiveFixedExpense_ShouldReturnFalse()
        {
            // Arrange
            User user = TestDataFactory.CreateUser();
            Category category = TestDataFactory.CreateCategory();
            EntityInfo info = new EntityInfo("Netflix", "Suscripción mensual");
            Money amount = new Money(15.99m, "EUR");
            MonthlyPeriod chargePeriod = new MonthlyPeriod(1, 2024);
            FixedExpense fixedExpense = new FixedExpense(user, category, info, amount, chargePeriod);
            fixedExpense.Deactivate();

            MonthlyPeriod period = new MonthlyPeriod(1, 2024);

            // Act
            bool result = fixedExpense.IsActiveForPeriod(period);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void IsActiveForPeriod_WithNullPeriod_ShouldThrowArgumentNullException()
        {
            // Arrange
            User user = TestDataFactory.CreateUser();
            Category category = TestDataFactory.CreateCategory();
            EntityInfo info = new EntityInfo("Netflix", "Suscripción mensual");
            Money amount = new Money(15.99m, "EUR");
            MonthlyPeriod chargePeriod = new MonthlyPeriod(1, 2024);
            FixedExpense fixedExpense = new FixedExpense(user, category, info, amount, chargePeriod);

            // Act & Assert
            ArgumentNullException exception = Assert.Throws<ArgumentNullException>(() => fixedExpense.IsActiveForPeriod(null));

            Assert.Equal("period", exception.ParamName);
        }

        // ==================== TO STRING ====================

        [Fact]
        public void ToString_ShouldReturnFormattedString()
        {
            // Arrange
            User user = TestDataFactory.CreateUser();
            Category category = TestDataFactory.CreateCategory();
            EntityInfo info = new EntityInfo("Netflix", "Suscripción mensual");
            Money amount = new Money(15.99m, "EUR");
            MonthlyPeriod chargePeriod = new MonthlyPeriod(1, 2024);
            FixedExpense fixedExpense = new FixedExpense(user, category, info, amount, chargePeriod);

            // Act
            string result = fixedExpense.ToString();

            // Assert
            Assert.Contains("Netflix", result);
            Assert.Contains("15,99 EUR", result);
            Assert.Contains("01-2024", result);
        }
    }
}