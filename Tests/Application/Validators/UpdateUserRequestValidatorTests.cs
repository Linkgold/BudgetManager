using Application.Validators;
using FluentAssertions;
using FluentValidation.Results;
using Shared.DTOs.Request;

namespace Tests.Application.Validators
{
    public class UpdateUserRequestValidatorTests
    {
        private readonly UpdateUserRequestValidator _validator = new UpdateUserRequestValidator();

        private static UpdateUserRequestDTO CreateValidRequest()
        {
            return new UpdateUserRequestDTO
            {
                UserName = "Juan Perez",
                Email = "juan@email.com"
            };
        }

        [Fact]
        public void Validate_ShouldBeValid_WhenRequestIsValid()
        {
            // Arrange
            UpdateUserRequestDTO request = CreateValidRequest();

            // Act
            ValidationResult result = _validator.Validate(request);

            // Assert
            result.IsValid.Should().BeTrue();
            result.Errors.Should().BeEmpty();
        }

        [Fact]
        public void Validate_ShouldBeInvalid_WhenUserNameIsEmpty()
        {
            // Arrange
            UpdateUserRequestDTO request = CreateValidRequest();
            request.UserName = string.Empty;

            // Act
            ValidationResult result = _validator.Validate(request);

            // Assert
            result.IsValid.Should().BeFalse();
            result.Errors.Should().ContainSingle(error => error.PropertyName == nameof(request.UserName) && error.ErrorMessage == "User name is required");
        }

        [Fact]
        public void Validate_ShouldBeInvalid_WhenUserNameIsTooShort()
        {
            // Arrange
            UpdateUserRequestDTO request = CreateValidRequest();
            request.UserName = "A";

            // Act
            ValidationResult result = _validator.Validate(request);

            // Assert
            result.IsValid.Should().BeFalse();
            result.Errors.Should().ContainSingle(error => error.PropertyName == nameof(request.UserName) && error.ErrorMessage == "User name must have at least 2 characters");
        }

        [Fact]
        public void Validate_ShouldBeInvalid_WhenUserNameIsTooLong()
        {
            // Arrange
            UpdateUserRequestDTO request = CreateValidRequest();
            request.UserName = new string('A', 51);

            // Act
            ValidationResult result = _validator.Validate(request);

            // Assert
            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain(error => error.PropertyName == nameof(request.UserName) && error.ErrorMessage == "User name cannot exceed 50 characters");
        }

        [Fact]
        public void Validate_ShouldBeInvalid_WhenEmailIsEmpty()
        {
            // Arrange
            UpdateUserRequestDTO request = CreateValidRequest();
            request.Email = string.Empty;

            // Act
            ValidationResult result = _validator.Validate(request);

            // Assert
            result.IsValid.Should().BeFalse();
            result.Errors.Should().ContainSingle(error => error.PropertyName == nameof(request.Email) && error.ErrorMessage == "Email is required");
        }

        [Fact]
        public void Validate_ShouldBeInvalid_WhenEmailHasInvalidFormat()
        {
            // Arrange
            UpdateUserRequestDTO request = CreateValidRequest();
            request.Email = "not-an-email";

            // Act
            ValidationResult result = _validator.Validate(request);

            // Assert
            result.IsValid.Should().BeFalse();
            result.Errors.Should().ContainSingle(error => error.PropertyName == nameof(request.Email) && error.ErrorMessage == "Invalid email format");
        }

        [Fact]
        public void Validate_ShouldBeInvalid_WhenEmailIsTooLong()
        {
            // Arrange
            UpdateUserRequestDTO request = CreateValidRequest();
            request.Email = $"{new string('a', 91)}@email.com";

            // Act
            ValidationResult result = _validator.Validate(request);

            // Assert
            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain(error => error.PropertyName == nameof(request.Email) && error.ErrorMessage == "Email cannot exceed 100 characters");
        }
    }
}
