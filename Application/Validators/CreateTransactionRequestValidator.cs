using Application.DTOs.Request;
using FluentValidation;

namespace Application.Validators
{
    /// <summary>
    /// Validador para crear transacciones
    /// </summary>
    public class CreateTransactionRequestValidator : AbstractValidator<CreateTransactionRequestDTO>
    {
        public CreateTransactionRequestValidator()
        {
            RuleFor(x => x.CategoryId)
                .GreaterThan(0).WithMessage("Category ID must be greater than zero");

            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Name is required")
                .MinimumLength(2).WithMessage("Name must have at least 2 characters")
                .MaximumLength(50).WithMessage("Name cannot exceed 50 characters");

            RuleFor(x => x.Description)
                .MaximumLength(200).WithMessage("Description cannot exceed 200 characters")
                .When(x => !string.IsNullOrEmpty(x.Description));

            RuleFor(x => x.Amount)
                .GreaterThan(0).WithMessage("Amount must be greater than zero");

            RuleFor(x => x.Type)
                .IsInEnum().WithMessage("Invalid transaction type");

            RuleFor(x => x.Date)
                .NotEmpty().WithMessage("Date is required")
                .Must(date => date.Year >= 1900 && date.Year <= 2100)
                .WithMessage("Year must be between 1900 and 2100");
        }
    }
}
