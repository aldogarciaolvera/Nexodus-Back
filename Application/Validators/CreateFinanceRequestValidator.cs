using FluentValidation;
using Nexodus_Back.Application.DTOs.Finance;

namespace Nexodus_Back.Application.Validators;

public class CreateFinanceRequestValidator : AbstractValidator<CreateFinanceRequest>
{
    public CreateFinanceRequestValidator()
    {
        RuleFor(x => x.Amount)
            .GreaterThan(0).WithMessage("El monto debe ser mayor a cero.");

        RuleFor(x => x.TransactionType)
            .NotEmpty().WithMessage("El tipo de transacción es obligatorio.")
            .Must(t => t == "Ingreso" || t == "Gasto")
            .WithMessage("El tipo de transacción debe ser 'Ingreso' o 'Gasto'.");
    }
}
