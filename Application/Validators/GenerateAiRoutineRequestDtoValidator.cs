using FluentValidation;
using Nexodus_Back.Application.DTOs;

namespace Nexodus_Back.Application.Validators;

public class GenerateAiRoutineRequestDtoValidator : AbstractValidator<GenerateAiRoutineRequestDto>
{
    public GenerateAiRoutineRequestDtoValidator()
    {
        RuleFor(x => x)
            .Must(x => !string.IsNullOrWhiteSpace(x.Prompt) || !string.IsNullOrWhiteSpace(x.RoutineName))
            .WithMessage("Debes indicar qué deseas entrenar o proporcionar un nombre para la rutina.");

        RuleFor(x => x.Prompt)
            .MaximumLength(500)
            .When(x => !string.IsNullOrWhiteSpace(x.Prompt))
            .WithMessage("La descripción de lo que quieres entrenar no puede superar los 500 caracteres.");

        RuleFor(x => x.RoutineName)
            .MaximumLength(100)
            .When(x => !string.IsNullOrWhiteSpace(x.RoutineName))
            .WithMessage("El nombre de la rutina no puede superar los 100 caracteres.");

        RuleFor(x => x.DurationMinutes)
            .InclusiveBetween(15, 180)
            .WithMessage("La duración objetivo debe estar entre 15 y 180 minutos.");
    }
}
