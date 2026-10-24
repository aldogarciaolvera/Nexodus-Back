using FluentValidation;
using Nexodus_Back.Application.DTOs.Diet;

namespace Nexodus_Back.Application.Validators;

public class CreateFoodItemDtoValidator : AbstractValidator<CreateFoodItemDto>
{
    public CreateFoodItemDtoValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(150).WithMessage("El nombre del alimento es requerido.");
        RuleFor(x => x.ServingSize).GreaterThan(0).WithMessage("La porción debe ser mayor a 0.");
        RuleFor(x => x.ServingUnit).NotEmpty().WithMessage("La unidad de medida es requerida.");
        RuleFor(x => x.Calories).GreaterThanOrEqualTo(0).WithMessage("Las calorías no pueden ser negativas.");
        RuleFor(x => x.Protein).GreaterThanOrEqualTo(0).WithMessage("La proteína no puede ser negativa.");
        RuleFor(x => x.Carbs).GreaterThanOrEqualTo(0).WithMessage("Los carbohidratos no pueden ser negativos.");
        RuleFor(x => x.Fat).GreaterThanOrEqualTo(0).WithMessage("Las grasas no pueden ser negativas.");
    }
}
