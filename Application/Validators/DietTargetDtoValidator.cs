using FluentValidation;
using Nexodus_Back.Application.DTOs.Diet;

namespace Nexodus_Back.Application.Validators;

public class DietTargetDtoValidator : AbstractValidator<DietTargetDto>
{
    public DietTargetDtoValidator()
    {
        RuleFor(x => x.TargetCalories).GreaterThan(0).WithMessage("Las calorías meta deben ser mayores a 0.");
        RuleFor(x => x.ProteinGrams).GreaterThanOrEqualTo(0).WithMessage("Los gramos de proteína deben ser mayores o iguales a 0.");
        RuleFor(x => x.CarbsGrams).GreaterThanOrEqualTo(0).WithMessage("Los gramos de carbohidratos deben ser mayores o iguales a 0.");
        RuleFor(x => x.FatGrams).GreaterThanOrEqualTo(0).WithMessage("Los gramos de grasas deben ser mayores o iguales a 0.");
        RuleFor(x => x.WaterTargetMl).GreaterThan(0).WithMessage("La meta de agua debe ser mayor a 0 ml.");
    }
}
