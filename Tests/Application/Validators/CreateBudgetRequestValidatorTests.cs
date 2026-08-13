using Application.Validators;
using FluentAssertions;
using FluentValidation.Results;
using Shared.DTOs.Request;

namespace Tests.Application.Validators
{
    public class CreateBudgetRequestValidatorTests
    {
        private readonly CreateBudgetRequestValidator _validator = new CreateBudgetRequestValidator();

        private static CreateBudgetRequestDTO CreateValidRequest()
        {
            return new CreateBudgetRequestDTO
            {
                CategoryId = 1,
                Amount = 500.00m,
                Currency = "EUR",
                Month = 1,
                Year = 2024
            };
        }

        [Fact]
        public void Validate_ShouldBeValid_WhenRequestIsValid()
        {
            // Arrange
            CreateBudgetRequestDTO request = CreateValidRequest();

            // Act
            ValidationResult result = _validator.Validate(request);

            // Assert
            result.IsValid.Should().BeTrue();
            result.Errors.Should().BeEmpty();
        }

        [Fact]
        public void Validate_ShouldBeInvalid_WhenCategoryIdIsZero()
        {
            // Arrange
            CreateBudgetRequestDTO request = CreateValidRequest();
            request.CategoryId = 0;

            // Act
            ValidationResult result = _validator.Validate(request);

            // Assert
            result.IsValid.Should().BeFalse();
            result.Errors.Should().ContainSingle(error => error.PropertyName == nameof(request.CategoryId) && error.ErrorMessage == "Category ID must be greater than zero");
        }

        [Fact]
        public void Validate_ShouldBeInvalid_WhenYearIsNotGreaterThan1900()
        {
            // Arrange
            CreateBudgetRequestDTO request = CreateValidRequest();
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
            CreateBudgetRequestDTO request = CreateValidRequest();
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
            CreateBudgetRequestDTO request = CreateValidRequest();
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
            CreateBudgetRequestDTO request = CreateValidRequest();
            request.Month = 13;

            // Act
            ValidationResult result = _validator.Validate(request);

            // Assert
            result.IsValid.Should().BeFalse();
            result.Errors.Should().ContainSingle(error => error.PropertyName == nameof(request.Month) && error.ErrorMessage == "Month must be between 1 and 12");
        }
    }
}
