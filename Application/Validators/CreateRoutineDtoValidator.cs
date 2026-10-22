using FluentValidation;
using Nexodus_Back.Application.DTOs;

namespace Nexodus_Back.Application.Validators;

public class CreateRoutineDtoValidator : AbstractValidator<CreateRoutineDto>
{
    public CreateRoutineDtoValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("El nombre de la rutina es obligatorio.")
            .MaximumLength(100).WithMessage("El nombre de la rutina no puede superar los 100 caracteres.");

        RuleFor(x => x.TargetDay)
            .InclusiveBetween(1, 7).When(x => x.TargetDay.HasValue)
            .WithMessage("El día objetivo debe estar entre 1 (Lunes) y 7 (Domingo).");

        RuleForEach(x => x.Exercises).SetValidator(new CreateRoutineExerciseDtoValidator());
    }
}

public class CreateRoutineExerciseDtoValidator : AbstractValidator<CreateRoutineExerciseDto>
{
    public CreateRoutineExerciseDtoValidator()
    {
        RuleFor(x => x.ExerciseId)
            .NotEmpty().WithMessage("El ID del ejercicio es obligatorio.");

        RuleFor(x => x.Sets)
            .GreaterThan(0).WithMessage("Las series deben ser mayores a cero.");

        RuleFor(x => x.Reps)
            .GreaterThan(0).WithMessage("Las repeticiones deben ser mayores a cero.");

        RuleFor(x => x.RestTimeInSeconds)
            .GreaterThanOrEqualTo(0).WithMessage("El tiempo de descanso no puede ser negativo.");

        RuleFor(x => x.Weight)
            .GreaterThanOrEqualTo(0).When(x => x.Weight.HasValue)
            .WithMessage("El peso no puede ser negativo.");
    }
}
