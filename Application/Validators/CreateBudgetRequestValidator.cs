using Shared.DTOs.Request;
using FluentValidation;

namespace Application.Validators
{
    /// <summary>
    /// Validador para crear presupuestos
    /// </summary>
    public class CreateBudgetRequestValidator : AbstractValidator<CreateBudgetRequestDTO>
    {
        public CreateBudgetRequestValidator()
        {
            RuleFor(x => x.CategoryId)
                .GreaterThan(0).WithMessage("Category ID must be greater than zero");

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
