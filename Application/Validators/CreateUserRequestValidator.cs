using Application.DTOs.Request;
using FluentValidation;

namespace Application.Validators
{
    public class CreateUserRequestValidator : AbstractValidator<CreateUserRequestDTO>
    {
        public CreateUserRequestValidator()
        {
            RuleFor(x => x.UserName)
                .NotEmpty().WithMessage("User name is required")
                .MinimumLength(2).WithMessage("User name must have at least 2 characters")
                .MaximumLength(50).WithMessage("User name cannot exceed 50 characters");

            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("Email is required")
                .EmailAddress().WithMessage("Invalid email format")
                .MaximumLength(100).WithMessage("Email cannot exceed 100 characters");

            RuleFor(x => x.Password)
                .NotEmpty().WithMessage("Password is required")
                .MinimumLength(6).WithMessage("Password must have at least 6 characters")
                .MaximumLength(100).WithMessage("Password cannot exceed 100 characters");

            RuleFor(x => x.ConfirmPassword)
                .NotEmpty().WithMessage("Confirm password is required")
                .Equal(x => x.Password).WithMessage("Passwords do not match");
        }
    }
}
