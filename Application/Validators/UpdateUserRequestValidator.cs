using Shared.DTOs.Request;
using FluentValidation;

namespace Application.Validators
{
    public class UpdateUserRequestValidator : AbstractValidator<UpdateUserRequestDTO>
    {
        public UpdateUserRequestValidator()
        {
            RuleFor(x => x.UserName)
                .NotEmpty().WithMessage("User name is required")
                .MinimumLength(2).WithMessage("User name must have at least 2 characters")
                .MaximumLength(50).WithMessage("User name cannot exceed 50 characters");

            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("Email is required")
                .EmailAddress().WithMessage("Invalid email format")
                .MaximumLength(100).WithMessage("Email cannot exceed 100 characters");
        }
    }
}