using Application.Validators;
using FluentAssertions;
using FluentValidation.Results;
using Shared.DTOs.Request;

namespace Tests.Application.Validators
{
    public class CreateUserRequestValidatorTests
    {
        private readonly CreateUserRequestValidator _validator = new CreateUserRequestValidator();

        private static CreateUserRequestDTO CreateValidRequest()
        {
            return new CreateUserRequestDTO
            {
                UserName = "Juan Perez",
                Email = "juan@email.com",
                Password = "Password123!",
                ConfirmPassword = "Password123!"
            };
        }

        [Fact]
        public void Validate_ShouldBeValid_WhenRequestIsValid()
        {
            // Arrange
            CreateUserRequestDTO request = CreateValidRequest();

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
            CreateUserRequestDTO request = CreateValidRequest();
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
            CreateUserRequestDTO request = CreateValidRequest();
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
            CreateUserRequestDTO request = CreateValidRequest();
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
            CreateUserRequestDTO request = CreateValidRequest();
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
            CreateUserRequestDTO request = CreateValidRequest();
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
            CreateUserRequestDTO request = CreateValidRequest();
            request.Email = $"{new string('a', 91)}@email.com";

            // Act
            ValidationResult result = _validator.Validate(request);

            // Assert
            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain(error => error.PropertyName == nameof(request.Email) && error.ErrorMessage == "Email cannot exceed 100 characters");
        }

        [Fact]
        public void Validate_ShouldBeInvalid_WhenPasswordIsEmpty()
        {
            // Arrange
            CreateUserRequestDTO request = CreateValidRequest();
            request.Password = string.Empty;

            // Act
            ValidationResult result = _validator.Validate(request);

            // Assert
            result.IsValid.Should().BeFalse();
            result.Errors.Should().ContainSingle(error => error.PropertyName == nameof(request.Password) && error.ErrorMessage == "Password is required");
        }

        [Fact]
        public void Validate_ShouldBeInvalid_WhenPasswordIsTooShort()
        {
            // Arrange
            CreateUserRequestDTO request = CreateValidRequest();
            request.Password = "12345";

            // Act
            ValidationResult result = _validator.Validate(request);

            // Assert
            result.IsValid.Should().BeFalse();
            result.Errors.Should().ContainSingle(error => error.PropertyName == nameof(request.Password) && error.ErrorMessage == "Password must have at least 6 characters");
        }

        [Fact]
        public void Validate_ShouldBeInvalid_WhenPasswordIsTooLong()
        {
            // Arrange
            CreateUserRequestDTO request = CreateValidRequest();
            request.Password = new string('A', 101);

            // Act
            ValidationResult result = _validator.Validate(request);

            // Assert
            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain(error => error.PropertyName == nameof(request.Password) && error.ErrorMessage == "Password cannot exceed 100 characters");
        }

        [Fact]
        public void Validate_ShouldBeInvalid_WhenConfirmPasswordIsEmpty()
        {
            // Arrange
            CreateUserRequestDTO request = CreateValidRequest();
            request.ConfirmPassword = string.Empty;

            // Act
            ValidationResult result = _validator.Validate(request);

            // Assert
            result.IsValid.Should().BeFalse();
            result.Errors.Should().ContainSingle(error => error.PropertyName == nameof(request.ConfirmPassword) && error.ErrorMessage == "Confirm password is required");
        }

        [Fact]
        public void Validate_ShouldBeInvalid_WhenPasswordsDoNotMatch()
        {
            // Arrange
            CreateUserRequestDTO request = CreateValidRequest();
            request.ConfirmPassword = "Different123!";

            // Act
            ValidationResult result = _validator.Validate(request);

            // Assert
            result.IsValid.Should().BeFalse();
            result.Errors.Should().ContainSingle(error => error.PropertyName == nameof(request.ConfirmPassword) && error.ErrorMessage == "Passwords do not match");
        }
    }
}
