using Application.DTOs.Request;
using FluentValidation;

namespace Application.Validators
{
    public class CreateCategoryRequestValidator : AbstractValidator<CreateCategoryRequestDTO>
    {
        public CreateCategoryRequestValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Category name is required")
                .MinimumLength(2).WithMessage("Category name must have at least 2 characters")
                .MaximumLength(100).WithMessage("Category name cannot exceed 100 characters");

            RuleFor(x => x.Description)
                .MaximumLength(500).WithMessage("Description cannot exceed 500 characters")
                .When(x => x.Description != null);
        }
    }
}