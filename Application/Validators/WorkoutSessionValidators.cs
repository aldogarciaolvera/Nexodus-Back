using FluentValidation;
using Nexodus_Back.Application.DTOs;

namespace Nexodus_Back.Application.Validators;

public class StartWorkoutSessionDtoValidator : AbstractValidator<StartWorkoutSessionDto>
{
    public StartWorkoutSessionDtoValidator()
    {
        RuleFor(x => x.Name)
            .MaximumLength(100).WithMessage("El nombre de la sesión no puede exceder 100 caracteres.");

        When(x => x.RoutineId.HasValue, () =>
        {
            RuleFor(x => x.RoutineId!.Value)
                .NotEmpty().WithMessage("El ID de la rutina no puede ser un GUID vacío.");
        });
    }
}

public class LogWorkoutSetDtoValidator : AbstractValidator<LogWorkoutSetDto>
{
    public LogWorkoutSetDtoValidator()
    {
        RuleFor(x => x.ExerciseId)
            .NotEmpty().WithMessage("El ID del ejercicio es obligatorio.");

        RuleFor(x => x.SetOrder)
            .GreaterThan(0).WithMessage("El orden de la serie debe ser mayor a 0.");

        RuleFor(x => x.WeightKg)
            .GreaterThanOrEqualTo(0).WithMessage("El peso en kg no puede ser negativo.");

        RuleFor(x => x.Reps)
            .GreaterThanOrEqualTo(0).WithMessage("Las repeticiones no pueden ser negativas.");

        When(x => x.Rpe.HasValue, () =>
        {
            RuleFor(x => x.Rpe!.Value)
                .InclusiveBetween(1, 10).WithMessage("El RPE debe estar comprendido entre 1 y 10.");
        });

        When(x => x.DurationSeconds.HasValue, () =>
        {
            RuleFor(x => x.DurationSeconds!.Value)
                .GreaterThanOrEqualTo(0).WithMessage("La duración en segundos no puede ser negativa.");
        });
    }
}

public class FinishWorkoutSessionDtoValidator : AbstractValidator<FinishWorkoutSessionDto>
{
    public FinishWorkoutSessionDtoValidator()
    {
        When(x => !string.IsNullOrEmpty(x.Notes), () =>
        {
            RuleFor(x => x.Notes)
                .MaximumLength(500).WithMessage("Las notas no pueden exceder 500 caracteres.");
        });
    }
}
