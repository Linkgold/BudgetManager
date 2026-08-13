using Application.Validators;
using Contracts.Enums;
using FluentAssertions;
using FluentValidation.Results;
using Shared.DTOs.Request;

namespace Tests.Application.Validators
{
    public class UpdateCategoryRequestValidatorTests
    {
        private readonly UpdateCategoryRequestValidator _validator = new UpdateCategoryRequestValidator();

        private static UpdateCategoryRequestDTO CreateValidRequest()
        {
            return new UpdateCategoryRequestDTO
            {
                Name = "Alimentación",
                Description = "Gastos de comida",
                Nature = CategoryNatureEnum.Expense
            };
        }

        [Fact]
        public void Validate_ShouldBeValid_WhenRequestIsValid()
        {
            // Arrange
            UpdateCategoryRequestDTO request = CreateValidRequest();

            // Act
            ValidationResult result = _validator.Validate(request);

            // Assert
            result.IsValid.Should().BeTrue();
            result.Errors.Should().BeEmpty();
        }

        [Fact]
        public void Validate_ShouldBeValid_WhenDescriptionIsNull()
        {
            // Arrange
            UpdateCategoryRequestDTO request = CreateValidRequest();
            request.Description = null!;

            // Act
            ValidationResult result = _validator.Validate(request);

            // Assert
            result.IsValid.Should().BeTrue();
        }

        [Fact]
        public void Validate_ShouldBeValid_WhenNatureIsNull()
        {
            // Arrange
            UpdateCategoryRequestDTO request = CreateValidRequest();
            request.Nature = null;

            // Act
            ValidationResult result = _validator.Validate(request);

            // Assert
            result.IsValid.Should().BeTrue();
        }

        [Fact]
        public void Validate_ShouldBeInvalid_WhenNameIsEmpty()
        {
            // Arrange
            UpdateCategoryRequestDTO request = CreateValidRequest();
            request.Name = string.Empty;

            // Act
            ValidationResult result = _validator.Validate(request);

            // Assert
            result.IsValid.Should().BeFalse();
            result.Errors.Should().ContainSingle(error => error.PropertyName == nameof(request.Name) && error.ErrorMessage == "Category name is required");
        }

        [Fact]
        public void Validate_ShouldBeInvalid_WhenNameIsTooShort()
        {
            // Arrange
            UpdateCategoryRequestDTO request = CreateValidRequest();
            request.Name = "A";

            // Act
            ValidationResult result = _validator.Validate(request);

            // Assert
            result.IsValid.Should().BeFalse();
            result.Errors.Should().ContainSingle(error => error.PropertyName == nameof(request.Name) && error.ErrorMessage == "Category name must have at least 2 characters");
        }

        [Fact]
        public void Validate_ShouldBeInvalid_WhenNameIsTooLong()
        {
            // Arrange
            UpdateCategoryRequestDTO request = CreateValidRequest();
            request.Name = new string('A', 101);

            // Act
            ValidationResult result = _validator.Validate(request);

            // Assert
            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain(error => error.PropertyName == nameof(request.Name) && error.ErrorMessage == "Category name cannot exceed 100 characters");
        }

        [Fact]
        public void Validate_ShouldBeInvalid_WhenDescriptionIsTooLong()
        {
            // Arrange
            UpdateCategoryRequestDTO request = CreateValidRequest();
            request.Description = new string('A', 501);

            // Act
            ValidationResult result = _validator.Validate(request);

            // Assert
            result.IsValid.Should().BeFalse();
            result.Errors.Should().ContainSingle(error => error.PropertyName == nameof(request.Description) && error.ErrorMessage == "Description cannot exceed 500 characters");
        }

        [Fact]
        public void Validate_ShouldBeInvalid_WhenNatureIsNotDefinedInEnum()
        {
            // Arrange
            UpdateCategoryRequestDTO request = CreateValidRequest();
            request.Nature = (CategoryNatureEnum)999;

            // Act
            ValidationResult result = _validator.Validate(request);

            // Assert
            result.IsValid.Should().BeFalse();
            result.Errors.Should().ContainSingle(error => error.PropertyName == nameof(request.Nature) && error.ErrorMessage == "Invalid category nature");
        }
    }
}
