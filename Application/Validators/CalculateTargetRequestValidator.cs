using FluentValidation;
using Nexodus_Back.Application.DTOs.Diet;

namespace Nexodus_Back.Application.Validators;

public class CalculateTargetRequestValidator : AbstractValidator<CalculateTargetRequestDto>
{
    public CalculateTargetRequestValidator()
    {
        RuleFor(x => x.Weight).GreaterThan(20).WithMessage("El peso debe ser mayor a 20 kg.");
        RuleFor(x => x.Height).GreaterThan(50).WithMessage("La altura debe ser mayor a 50 cm.");
        RuleFor(x => x.Age).InclusiveBetween(10, 120).WithMessage("La edad debe estar entre 10 y 120 años.");
        RuleFor(x => x.BiologicalSex).NotEmpty().Must(s => s.Equals("Male", System.StringComparison.OrdinalIgnoreCase) || s.Equals("Female", System.StringComparison.OrdinalIgnoreCase))
            .WithMessage("El sexo biológico debe ser 'Male' o 'Female'.");
        RuleFor(x => x.Goal).NotEmpty().WithMessage("El objetivo nutricional es requerido.");
    }
}
