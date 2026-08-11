using Application.Validators;
using Contracts.Enums;
using FluentAssertions;
using FluentValidation.Results;
using Shared.DTOs.Request;

namespace Tests.Application.Validators
{
    public class CreateTransactionRequestValidatorTests
    {
        private readonly CreateTransactionRequestValidator _validator = new CreateTransactionRequestValidator();

        private static CreateTransactionRequestDTO CreateValidRequest()
        {
            return new CreateTransactionRequestDTO
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
            CreateTransactionRequestDTO request = CreateValidRequest();

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
            CreateTransactionRequestDTO request = CreateValidRequest();
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
            CreateTransactionRequestDTO request = CreateValidRequest();
            request.Currency = string.Empty;

            // Act
            ValidationResult result = _validator.Validate(request);

            // Assert
            result.IsValid.Should().BeTrue();
        }

        [Fact]
        public void Validate_ShouldBeInvalid_WhenCategoryIdIsZero()
        {
            // Arrange
            CreateTransactionRequestDTO request = CreateValidRequest();
            request.CategoryId = 0;

            // Act
            ValidationResult result = _validator.Validate(request);

            // Assert
            result.IsValid.Should().BeFalse();
            result.Errors.Should().ContainSingle(error => error.PropertyName == nameof(request.CategoryId) && error.ErrorMessage == "Category ID must be greater than zero");
        }

        [Fact]
        public void Validate_ShouldBeInvalid_WhenNameIsEmpty()
        {
            // Arrange
            CreateTransactionRequestDTO request = CreateValidRequest();
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
            CreateTransactionRequestDTO request = CreateValidRequest();
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
            CreateTransactionRequestDTO request = CreateValidRequest();
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
            CreateTransactionRequestDTO request = CreateValidRequest();
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
            CreateTransactionRequestDTO request = CreateValidRequest();
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
            CreateTransactionRequestDTO request = CreateValidRequest();
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
            CreateTransactionRequestDTO request = CreateValidRequest();
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
            CreateTransactionRequestDTO request = CreateValidRequest();
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
            CreateTransactionRequestDTO request = CreateValidRequest();
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
            CreateTransactionRequestDTO request = CreateValidRequest();
            request.TransactionType = (TransactionTypeEnum)999;

            // Act
            ValidationResult result = _validator.Validate(request);

            // Assert
            result.IsValid.Should().BeFalse();
            result.Errors.Should().ContainSingle(error => error.PropertyName == nameof(request.TransactionType) && error.ErrorMessage == "Invalid transaction type");
        }
    }
}
