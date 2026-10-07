using FluentValidation;
using Nexodus_Back.Application.DTOs;

namespace Nexodus_Back.Application.Validators
{
    public class CreateWorkoutLogDtoValidator : AbstractValidator<CreateWorkoutLogDto>
    {
        public CreateWorkoutLogDtoValidator()
        {
            RuleFor(x => x.RoutineId).NotEmpty().WithMessage("El ID de la rutina es requerido.");
            RuleFor(x => x.DurationInSeconds).GreaterThanOrEqualTo(0).WithMessage("La duración debe ser mayor o igual a 0.");
            RuleFor(x => x.CompletedExercisesCount).GreaterThanOrEqualTo(0).WithMessage("La cantidad de ejercicios completados debe ser mayor o igual a 0.");
            RuleFor(x => x.DateCompleted).NotEmpty().WithMessage("La fecha de compleción es requerida.");
        }
    }
}
