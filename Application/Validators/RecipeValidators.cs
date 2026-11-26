using FluentValidation;
using Nexodus_Back.Application.DTOs.Diet;

namespace Nexodus_Back.Application.Validators;

public class CreateRecipeDtoValidator : AbstractValidator<CreateRecipeDto>
{
    public CreateRecipeDtoValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("El nombre de la receta es obligatorio.")
            .MaximumLength(150).WithMessage("El nombre no puede exceder 150 caracteres.");

        RuleFor(x => x.Servings)
            .GreaterThan(0).WithMessage("Las porciones deben ser al menos 1.");

        RuleFor(x => x.PrepTimeMinutes)
            .GreaterThanOrEqualTo(0).WithMessage("El tiempo de preparación no puede ser negativo.");

        RuleFor(x => x.CookTimeMinutes)
            .GreaterThanOrEqualTo(0).WithMessage("El tiempo de cocción no puede ser negativo.");
    }
}

public class UpdateRecipeDtoValidator : AbstractValidator<UpdateRecipeDto>
{
    public UpdateRecipeDtoValidator()
    {
        RuleFor(x => x.Name)
            .MaximumLength(150).WithMessage("El nombre no puede exceder 150 caracteres.")
            .When(x => !string.IsNullOrEmpty(x.Name));

        RuleFor(x => x.Servings)
            .GreaterThan(0).WithMessage("Las porciones deben ser al menos 1.")
            .When(x => x.Servings.HasValue);

        RuleFor(x => x.PrepTimeMinutes)
            .GreaterThanOrEqualTo(0).WithMessage("El tiempo de preparación no puede ser negativo.")
            .When(x => x.PrepTimeMinutes.HasValue);

        RuleFor(x => x.CookTimeMinutes)
            .GreaterThanOrEqualTo(0).WithMessage("El tiempo de cocción no puede ser negativo.")
            .When(x => x.CookTimeMinutes.HasValue);
    }
}

public class LogRecipeAsMealDtoValidator : AbstractValidator<LogRecipeAsMealDto>
{
    public LogRecipeAsMealDtoValidator()
    {
        RuleFor(x => x.RecipeId)
            .NotEmpty().WithMessage("El ID de la receta es obligatorio.");

        RuleFor(x => x.Date)
            .NotEmpty().WithMessage("La fecha del registro es obligatoria.")
            .Matches(@"^\d{4}-\d{2}-\d{2}$").WithMessage("El formato de fecha debe ser YYYY-MM-DD.");

        RuleFor(x => x.Servings)
            .GreaterThan(0).WithMessage("Las porciones deben ser mayores a 0.");
    }
}

public class MatchMacrosRecipeRequestDtoValidator : AbstractValidator<MatchMacrosRecipeRequestDto>
{
    public MatchMacrosRecipeRequestDtoValidator()
    {
        RuleFor(x => x.RemainingCalories)
            .GreaterThanOrEqualTo(0).WithMessage("Las calorías restantes no pueden ser negativas.");

        RuleFor(x => x.RemainingProtein)
            .GreaterThanOrEqualTo(0).WithMessage("Las proteínas restantes no pueden ser negativas.");

        RuleFor(x => x.RemainingCarbs)
            .GreaterThanOrEqualTo(0).WithMessage("Los carbohidratos restantes no pueden ser negativos.");

        RuleFor(x => x.RemainingFat)
            .GreaterThanOrEqualTo(0).WithMessage("Las grasas restantes no pueden ser negativas.");

        RuleFor(x => x.MaxPrepTimeMinutes)
            .GreaterThan(0).WithMessage("El tiempo de preparación debe ser mayor a 0 minutos.")
            .LessThanOrEqualTo(180).WithMessage("El tiempo de preparación no puede exceder 180 minutos.");
    }
}

public class PantryCookRequestDtoValidator : AbstractValidator<PantryCookRequestDto>
{
    public PantryCookRequestDtoValidator()
    {
        RuleFor(x => x.MaxPrepTimeMinutes)
            .GreaterThan(0).WithMessage("El tiempo de preparación debe ser mayor a 0 minutos.")
            .LessThanOrEqualTo(180).WithMessage("El tiempo de preparación no puede exceder 180 minutos.");
    }
}

