using FluentValidation;
using Nexodus_Back.Application.DTOs.Diet;

namespace Nexodus_Back.Application.Validators;

public class AddWaterDtoValidator : AbstractValidator<AddWaterDto>
{
    public AddWaterDtoValidator()
    {
        RuleFor(x => x.Date).NotEmpty().WithMessage("La fecha es requerida.");
        RuleFor(x => x.AmountMl).NotEqual(0).WithMessage("La cantidad de agua debe ser diferente de 0.");
    }
}

public class SetWaterDtoValidator : AbstractValidator<SetWaterDto>
{
    public SetWaterDtoValidator()
    {
        RuleFor(x => x.Date).NotEmpty().WithMessage("La fecha es requerida.");
        RuleFor(x => x.TotalMl).GreaterThanOrEqualTo(0).WithMessage("El total de agua debe ser mayor o igual a 0.");
    }
}

public class StartFastingDtoValidator : AbstractValidator<StartFastingDto>
{
    public StartFastingDtoValidator()
    {
        RuleFor(x => x.TargetHours).GreaterThan(0).LessThanOrEqualTo(72).WithMessage("Las horas de ayuno deben ser entre 1 y 72 horas.");
    }
}
