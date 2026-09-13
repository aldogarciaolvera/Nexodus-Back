using FluentValidation;
using Nexodus_Back.Application.DTOs.Finance;

namespace Nexodus_Back.Application.Validators;

public class UpdateFinanceRequestValidator : AbstractValidator<UpdateFinanceRequest>
{
    public UpdateFinanceRequestValidator()
    {
        RuleFor(x => x.Amount)
            .GreaterThan(0).WithMessage("Amount must be greater than zero.");

        RuleFor(x => x.TransactionType)
            .NotEmpty().WithMessage("TransactionType is required.")
            .Must(t => t == "Ingreso" || t == "Gasto")
            .WithMessage("TransactionType must be either 'Ingreso' or 'Gasto'.");
    }
}
