using FluentValidation;
using Nexodus_Back.Application.DTOs.Finance;

namespace Nexodus_Back.Application.Validators;

public class UpdateFinanceRequestValidator : AbstractValidator<UpdateFinanceRequest>
{
    public UpdateFinanceRequestValidator()
    {
        RuleFor(x => x.Amount)
            .GreaterThan(0).WithMessage("El monto debe ser mayor a cero.");

        RuleFor(x => x.TransactionType)
            .NotEmpty().WithMessage("El tipo de transacción es obligatorio.")
            .Must(t => t == "Ingreso" || t == "Gasto")
            .WithMessage("El tipo de transacción debe ser 'Ingreso' o 'Gasto'.");

        RuleFor(x => x.PaymentMethod)
            .NotEmpty().WithMessage("El método de pago es obligatorio.")
            .Must(p => p == "Tarjeta" || p == "Efectivo")
            .WithMessage("El método de pago debe ser 'Tarjeta' o 'Efectivo'.");
    }
}
