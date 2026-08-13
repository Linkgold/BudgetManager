using Application.Validators;
using Contracts.Enums;
using FluentAssertions;
using FluentValidation.Results;
using Shared.DTOs.Request;

namespace Tests.Application.Validators
{
    public class CreateCategoryRequestValidatorTests
    {
        private readonly CreateCategoryRequestValidator _validator = new CreateCategoryRequestValidator();

        private static CreateCategoryRequestDTO CreateValidRequest()
        {
            return new CreateCategoryRequestDTO
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
            CreateCategoryRequestDTO request = CreateValidRequest();

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
            CreateCategoryRequestDTO request = CreateValidRequest();
            request.Description = null!;

            // Act
            ValidationResult result = _validator.Validate(request);

            // Assert
            result.IsValid.Should().BeTrue();
        }

        [Fact]
        public void Validate_ShouldBeInvalid_WhenNameIsEmpty()
        {
            // Arrange
            CreateCategoryRequestDTO request = CreateValidRequest();
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
            CreateCategoryRequestDTO request = CreateValidRequest();
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
            CreateCategoryRequestDTO request = CreateValidRequest();
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
            CreateCategoryRequestDTO request = CreateValidRequest();
            request.Description = new string('A', 201);

            // Act
            ValidationResult result = _validator.Validate(request);

            // Assert
            result.IsValid.Should().BeFalse();
            result.Errors.Should().ContainSingle(error => error.PropertyName == nameof(request.Description) && error.ErrorMessage == "Description cannot exceed 200 characters");
        }

        [Fact]
        public void Validate_ShouldBeInvalid_WhenNatureIsNotDefinedInEnum()
        {
            // Arrange
            CreateCategoryRequestDTO request = CreateValidRequest();
            request.Nature = (CategoryNatureEnum)999;

            // Act
            ValidationResult result = _validator.Validate(request);

            // Assert
            result.IsValid.Should().BeFalse();
            result.Errors.Should().ContainSingle(error => error.PropertyName == nameof(request.Nature) && error.ErrorMessage == "Invalid category nature");
        }
    }
}
