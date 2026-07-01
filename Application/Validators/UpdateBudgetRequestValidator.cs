using Application.DTOs.Request;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.Text;

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
