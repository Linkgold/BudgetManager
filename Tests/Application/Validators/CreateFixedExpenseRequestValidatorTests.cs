using Application.Validators;
using FluentAssertions;
using FluentValidation.Results;
using Shared.DTOs.Request;

namespace Tests.Application.Validators
{
    public class CreateFixedExpenseRequestValidatorTests
    {
        private readonly CreateFixedExpenseRequestValidator _validator = new CreateFixedExpenseRequestValidator();

        private static CreateFixedExpenseRequestDTO CreateValidRequest()
        {
            return new CreateFixedExpenseRequestDTO
            {
                CategoryId = 1,
                Name = "Netflix",
                Description = "Suscripción mensual",
                Amount = 15.90m,
                Currency = "EUR",
                Month = 1,
                Year = 2024
            };
        }

        [Fact]
        public void Validate_ShouldBeValid_WhenRequestIsValid()
        {
            // Arrange
            CreateFixedExpenseRequestDTO request = CreateValidRequest();

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
            CreateFixedExpenseRequestDTO request = CreateValidRequest();
            request.Description = string.Empty;

            // Act
            ValidationResult result = _validator.Validate(request);

            // Assert
            result.IsValid.Should().BeTrue();
        }

        [Fact]
        public void Validate_ShouldBeInvalid_WhenCategoryIdIsZero()
        {
            // Arrange
            CreateFixedExpenseRequestDTO request = CreateValidRequest();
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
            CreateFixedExpenseRequestDTO request = CreateValidRequest();
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
            CreateFixedExpenseRequestDTO request = CreateValidRequest();
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
            CreateFixedExpenseRequestDTO request = CreateValidRequest();
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
            CreateFixedExpenseRequestDTO request = CreateValidRequest();
            request.Description = new string('A', 201);

            // Act
            ValidationResult result = _validator.Validate(request);

            // Assert
            result.IsValid.Should().BeFalse();
            result.Errors.Should().ContainSingle(error => error.PropertyName == nameof(request.Description));
        }

        [Fact]
        public void Validate_ShouldBeInvalid_WhenAmountIsZero()
        {
            // Arrange
            CreateFixedExpenseRequestDTO request = CreateValidRequest();
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
            CreateFixedExpenseRequestDTO request = CreateValidRequest();
            request.Amount = -10;

            // Act
            ValidationResult result = _validator.Validate(request);

            // Assert
            result.IsValid.Should().BeFalse();
            result.Errors.Should().ContainSingle(error => error.PropertyName == nameof(request.Amount) && error.ErrorMessage == "Amount must be greater than zero");
        }

        [Fact]
        public void Validate_ShouldBeInvalid_WhenYearIsNotGreaterThan1900()
        {
            // Arrange
            CreateFixedExpenseRequestDTO request = CreateValidRequest();
            request.Year = 1900;

            // Act
            ValidationResult result = _validator.Validate(request);

            // Assert
            result.IsValid.Should().BeFalse();
            result.Errors.Should().ContainSingle(error => error.PropertyName == nameof(request.Year) && error.ErrorMessage == "Year must be greater than 1900");
        }

        [Fact]
        public void Validate_ShouldBeInvalid_WhenYearIsNotLessThan2100()
        {
            // Arrange
            CreateFixedExpenseRequestDTO request = CreateValidRequest();
            request.Year = 2100;

            // Act
            ValidationResult result = _validator.Validate(request);

            // Assert
            result.IsValid.Should().BeFalse();
            result.Errors.Should().ContainSingle(error => error.PropertyName == nameof(request.Year) && error.ErrorMessage == "Year must be less than 2100");
        }

        [Fact]
        public void Validate_ShouldBeInvalid_WhenMonthIsBelowOne()
        {
            // Arrange
            CreateFixedExpenseRequestDTO request = CreateValidRequest();
            request.Month = 0;

            // Act
            ValidationResult result = _validator.Validate(request);

            // Assert
            result.IsValid.Should().BeFalse();
            result.Errors.Should().ContainSingle(error => error.PropertyName == nameof(request.Month) && error.ErrorMessage == "Month must be between 1 and 12");
        }

        [Fact]
        public void Validate_ShouldBeInvalid_WhenMonthIsAboveTwelve()
        {
            // Arrange
            CreateFixedExpenseRequestDTO request = CreateValidRequest();
            request.Month = 13;

            // Act
            ValidationResult result = _validator.Validate(request);

            // Assert
            result.IsValid.Should().BeFalse();
            result.Errors.Should().ContainSingle(error => error.PropertyName == nameof(request.Month) && error.ErrorMessage == "Month must be between 1 and 12");
        }
    }
}
