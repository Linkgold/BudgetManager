using Shared.DTOs.Request;
using FluentValidation;

namespace Application.Validators
{
    /// <summary>
    /// Validador para actualizar presupuestos
    /// </summary>
    public class UpdateBudgetRequestValidator : AbstractValidator<UpdateBudgetRequestDTO>
    {
        public UpdateBudgetRequestValidator()
        {
            RuleFor(x => x.Amount)
                .GreaterThan(0).WithMessage("Amount must be greater than zero");
        }
    }
}