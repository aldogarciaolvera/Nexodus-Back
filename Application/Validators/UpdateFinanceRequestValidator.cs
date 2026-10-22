using System;
using FluentValidation;
using Nexodus_Back.Application.DTOs.Finance;

namespace Nexodus_Back.Application.Validators;

public class UpdateFinanceRequestValidator : AbstractValidator<UpdateFinanceRequest>
{
    private static readonly string[] AllowedTransactionTypes = ["Ingreso", "Gasto", "Income", "Expense"];
    private static readonly string[] AllowedPaymentMethods = ["Tarjeta", "Efectivo", "Card", "Cash"];

    public UpdateFinanceRequestValidator()
    {
        RuleFor(x => x.Amount)
            .GreaterThan(0).WithMessage("El monto debe ser mayor a cero.");

        RuleFor(x => x.TransactionType)
            .NotEmpty().WithMessage("El tipo de transacción es obligatorio.")
            .Must(t => AllowedTransactionTypes.Contains(t, StringComparer.OrdinalIgnoreCase))
            .WithMessage("El tipo de transacción debe ser 'Ingreso'/'Income' o 'Gasto'/'Expense'.");

        RuleFor(x => x.PaymentMethod)
            .NotEmpty().WithMessage("El método de pago es obligatorio.")
            .Must(p => AllowedPaymentMethods.Contains(p, StringComparer.OrdinalIgnoreCase))
            .WithMessage("El método de pago debe ser 'Tarjeta'/'Card' o 'Efectivo'/'Cash'.");
    }
}
