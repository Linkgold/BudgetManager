using Application.DTOs.Request;
using FluentValidation;

namespace Application.Validators
{
    public class CreateFixedExpenseRequestValidator : AbstractValidator<CreateFixedExpenseRequestDTO>
    {
        public CreateFixedExpenseRequestValidator()
        {
            RuleFor(x => x.CategoryId)
                .GreaterThan(0).WithMessage("Category ID must be greater than zero");

            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Name is required")
                .MinimumLength(2).WithMessage("Name must have at least 2 characters")
                .MaximumLength(50).WithMessage("Name cannot exceed 50 characters");

            RuleFor(x => x.Description)
                .MaximumLength(200)
                    .When(x => !string.IsNullOrEmpty(x.Description));

            RuleFor(x => x.Amount)
                .GreaterThan(0).WithMessage("Amount must be greater than zero");

            RuleFor(x => x.Year)
                .GreaterThan(1900).WithMessage("Year must be greater than 1900")
                .LessThan(2100).WithMessage("Year must be less than 2100");

            RuleFor(x => x.Month)
                .InclusiveBetween(1, 12).WithMessage("Month must be between 1 and 12");
        }
    }
}