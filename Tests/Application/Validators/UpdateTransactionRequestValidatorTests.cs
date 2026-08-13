using Application.Validators;
using Contracts.Enums;
using FluentAssertions;
using FluentValidation.Results;
using Shared.DTOs.Request;

namespace Tests.Application.Validators
{
    public class UpdateTransactionRequestValidatorTests
    {
        private readonly UpdateTransactionRequestValidator _validator = new UpdateTransactionRequestValidator();

        private static UpdateTransactionRequestDTO CreateValidRequest()
        {
            return new UpdateTransactionRequestDTO
            {
                CategoryId = 1,
                Name = "Compra supermercado",
                Description = "Carrefour",
                Amount = 45.75m,
                Currency = "EUR",
                Date = new DateTime(2024, 6, 15),
                TransactionType = TransactionTypeEnum.Expense
            };
        }

        [Fact]
        public void Validate_ShouldBeValid_WhenRequestIsValid()
        {
            // Arrange
            UpdateTransactionRequestDTO request = CreateValidRequest();

            // Act
            ValidationResult result = _validator.Validate(request);

            // Assert
            result.IsValid.Should().BeTrue();
            result.Errors.Should().BeEmpty();
        }

        [Fact]
        public void Validate_ShouldBeValid_WhenDescriptionIsEmpty()
        {
            // Arrange
            UpdateTransactionRequestDTO request = CreateValidRequest();
            request.Description = string.Empty;

            // Act
            ValidationResult result = _validator.Validate(request);

            // Assert
            result.IsValid.Should().BeTrue();
        }

        [Fact]
        public void Validate_ShouldBeValid_WhenCurrencyIsEmpty()
        {
            // Arrange
            UpdateTransactionRequestDTO request = CreateValidRequest();
            request.Currency = string.Empty;

            // Act
            ValidationResult result = _validator.Validate(request);

            // Assert
            result.IsValid.Should().BeTrue();
        }

        [Fact]
        public void Validate_ShouldBeInvalid_WhenNameIsEmpty()
        {
            // Arrange
            UpdateTransactionRequestDTO request = CreateValidRequest();
            request.Name = string.Empty;

            // Act
            ValidationResult result = _validator.Validate(request);

            // Assert
            result.IsValid.Should().BeFalse();
            result.Errors.Should().ContainSingle(error => error.PropertyName == nameof(request.Name) && error.ErrorMessage == "Name is required");
        }

        [Fact]
        public void Validate_ShouldBeInvalid_WhenNameIsTooShort()
        {
            // Arrange
            UpdateTransactionRequestDTO request = CreateValidRequest();
            request.Name = "A";

            // Act
            ValidationResult result = _validator.Validate(request);

            // Assert
            result.IsValid.Should().BeFalse();
            result.Errors.Should().ContainSingle(error => error.PropertyName == nameof(request.Name) && error.ErrorMessage == "Name must have at least 2 characters");
        }

        [Fact]
        public void Validate_ShouldBeInvalid_WhenNameIsTooLong()
        {
            // Arrange
            UpdateTransactionRequestDTO request = CreateValidRequest();
            request.Name = new string('A', 51);

            // Act
            ValidationResult result = _validator.Validate(request);

            // Assert
            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain(error => error.PropertyName == nameof(request.Name) && error.ErrorMessage == "Name cannot exceed 50 characters");
        }

        [Fact]
        public void Validate_ShouldBeInvalid_WhenDescriptionIsTooLong()
        {
            // Arrange
            UpdateTransactionRequestDTO request = CreateValidRequest();
            request.Description = new string('A', 201);

            // Act
            ValidationResult result = _validator.Validate(request);

            // Assert
            result.IsValid.Should().BeFalse();
            result.Errors.Should().ContainSingle(error => error.PropertyName == nameof(request.Description) && error.ErrorMessage == "Description cannot exceed 200 characters");
        }

        [Fact]
        public void Validate_ShouldBeInvalid_WhenAmountIsZero()
        {
            // Arrange
            UpdateTransactionRequestDTO request = CreateValidRequest();
            request.Amount = 0;

            // Act
            ValidationResult result = _validator.Validate(request);

            // Assert
            result.IsValid.Should().BeFalse();
            result.Errors.Should().ContainSingle(error => error.PropertyName == nameof(request.Amount) && error.ErrorMessage == "Amount must be greater than zero");
        }

        [Fact]
        public void Validate_ShouldBeInvalid_WhenAmountIsNegative()
        {
            // Arrange
            UpdateTransactionRequestDTO request = CreateValidRequest();
            request.Amount = -10;

            // Act
            ValidationResult result = _validator.Validate(request);

            // Assert
            result.IsValid.Should().BeFalse();
            result.Errors.Should().ContainSingle(error => error.PropertyName == nameof(request.Amount) && error.ErrorMessage == "Amount must be greater than zero");
        }

        [Fact]
        public void Validate_ShouldBeInvalid_WhenCurrencyIsTooLong()
        {
            // Arrange
            UpdateTransactionRequestDTO request = CreateValidRequest();
            request.Currency = "EURO";

            // Act
            ValidationResult result = _validator.Validate(request);

            // Assert
            result.IsValid.Should().BeFalse();
            result.Errors.Should().ContainSingle(error => error.PropertyName == nameof(request.Currency) && error.ErrorMessage == "Currency must be a 3-letter ISO code");
        }

        [Fact]
        public void Validate_ShouldBeInvalid_WhenDateIsDefault()
        {
            // Arrange
            UpdateTransactionRequestDTO request = CreateValidRequest();
            request.Date = default;

            // Act
            ValidationResult result = _validator.Validate(request);

            // Assert
            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain(error => error.PropertyName == nameof(request.Date));
        }

        [Fact]
        public void Validate_ShouldBeInvalid_WhenDateYearIsOutOfRange()
        {
            // Arrange
            UpdateTransactionRequestDTO request = CreateValidRequest();
            request.Date = new DateTime(1899, 6, 15);

            // Act
            ValidationResult result = _validator.Validate(request);

            // Assert
            result.IsValid.Should().BeFalse();
            result.Errors.Should().ContainSingle(error => error.PropertyName == nameof(request.Date) && error.ErrorMessage == "Year must be between 1900 and 2100");
        }

        [Fact]
        public void Validate_ShouldBeInvalid_WhenTransactionTypeIsNotDefinedInEnum()
        {
            // Arrange
            UpdateTransactionRequestDTO request = CreateValidRequest();
            request.TransactionType = (TransactionTypeEnum)999;

            // Act
            ValidationResult result = _validator.Validate(request);

            // Assert
            result.IsValid.Should().BeFalse();
            result.Errors.Should().ContainSingle(error => error.PropertyName == nameof(request.TransactionType) && error.ErrorMessage == "Invalid transaction type");
        }
    }
}
