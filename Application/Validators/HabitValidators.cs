using System.Linq;
using FluentValidation;
using Nexodus_Back.Application.DTOs.Habit;

namespace Nexodus_Back.Application.Validators;

public class CreateHabitDtoValidator : AbstractValidator<CreateHabitDto>
{
    private static readonly string[] ValidHabitTypes = ["Build", "Quit"];
    private static readonly string[] ValidTrackingTypes = ["YesNo", "Numeric", "Timer"];
    private static readonly string[] ValidFrequencyTypes = ["Daily", "SpecificDays", "TimesPerPeriod", "RepeatInterval"];

    public CreateHabitDtoValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("El nombre del hábito es obligatorio.")
            .MaximumLength(150).WithMessage("El nombre no puede exceder 150 caracteres.");

        RuleFor(x => x.HabitType)
            .Must(t => ValidHabitTypes.Contains(t))
            .WithMessage("HabitType debe ser 'Build' o 'Quit'.");

        RuleFor(x => x.TrackingType)
            .Must(t => ValidTrackingTypes.Contains(t))
            .WithMessage("TrackingType debe ser 'YesNo', 'Numeric' o 'Timer'.");

        RuleFor(x => x.TargetValue)
            .GreaterThan(0).WithMessage("El valor objetivo (TargetValue) debe ser mayor a 0.");

        RuleFor(x => x.FrequencyType)
            .Must(f => ValidFrequencyTypes.Contains(f))
            .WithMessage("FrequencyType debe ser 'Daily', 'SpecificDays', 'TimesPerPeriod' o 'RepeatInterval'.");
    }
}

public class UpdateHabitDtoValidator : AbstractValidator<UpdateHabitDto>
{
    private static readonly string[] ValidHabitTypes = ["Build", "Quit"];
    private static readonly string[] ValidTrackingTypes = ["YesNo", "Numeric", "Timer"];
    private static readonly string[] ValidFrequencyTypes = ["Daily", "SpecificDays", "TimesPerPeriod", "RepeatInterval"];

    public UpdateHabitDtoValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("El nombre del hábito es obligatorio.")
            .MaximumLength(150).WithMessage("El nombre no puede exceder 150 caracteres.");

        RuleFor(x => x.HabitType)
            .Must(t => ValidHabitTypes.Contains(t))
            .WithMessage("HabitType debe ser 'Build' o 'Quit'.");

        RuleFor(x => x.TrackingType)
            .Must(t => ValidTrackingTypes.Contains(t))
            .WithMessage("TrackingType debe ser 'YesNo', 'Numeric' o 'Timer'.");

        RuleFor(x => x.TargetValue)
            .GreaterThan(0).WithMessage("El valor objetivo (TargetValue) debe ser mayor a 0.");

        RuleFor(x => x.FrequencyType)
            .Must(f => ValidFrequencyTypes.Contains(f))
            .WithMessage("FrequencyType debe ser 'Daily', 'SpecificDays', 'TimesPerPeriod' o 'RepeatInterval'.");
    }
}

public class HabitCheckInDtoValidator : AbstractValidator<HabitCheckInDto>
{
    public HabitCheckInDtoValidator()
    {
        RuleFor(x => x.Date)
            .Matches(@"^\d{4}-\d{2}-\d{2}$")
            .When(x => !string.IsNullOrEmpty(x.Date))
            .WithMessage("El formato de fecha debe ser YYYY-MM-DD.");

        RuleFor(x => x.Amount)
            .GreaterThanOrEqualTo(0)
            .When(x => x.Amount.HasValue)
            .WithMessage("La cantidad registrada no puede ser negativa.");
    }
}

public class ReorderHabitsDtoValidator : AbstractValidator<ReorderHabitsDto>
{
    public ReorderHabitsDtoValidator()
    {
        RuleFor(x => x.HabitIdsInOrder)
            .NotNull().WithMessage("La lista de IDs ordenados es obligatoria.");
    }
}
