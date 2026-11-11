using System;
using System.Linq;
using FluentValidation;
using Nexodus_Back.Application.DTOs.Gamification;

namespace Nexodus_Back.Application.Validators;

public class UpdateRestDaysDtoValidator : AbstractValidator<UpdateRestDaysDto>
{
    private static readonly string[] ValidDays =
    [
        "Monday", "Tuesday", "Wednesday", "Thursday", "Friday", "Saturday", "Sunday"
    ];

    public UpdateRestDaysDtoValidator()
    {
        RuleFor(x => x.RestDays)
            .NotNull().WithMessage("La lista de días de descanso no puede ser nula.")
            .Must(days => days == null || days.Count <= 7).WithMessage("No se pueden especificar más de 7 días de descanso.")
            .Must(days => days == null || days.All(d => ValidDays.Contains(d, StringComparer.OrdinalIgnoreCase)))
            .WithMessage("Los días de descanso deben ser nombres de día válidos en inglés (ej. Monday, Tuesday, ...).");
    }
}

