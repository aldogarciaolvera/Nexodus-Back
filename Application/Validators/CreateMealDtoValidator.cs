using FluentValidation;
using Nexodus_Back.Application.DTOs.Diet;

namespace Nexodus_Back.Application.Validators;

public class CreateMealDtoValidator : AbstractValidator<CreateMealDto>
{
    public CreateMealDtoValidator()
    {
        RuleFor(x => x.Date).NotEmpty().WithMessage("La fecha es requerida.");
        RuleFor(x => x.MealType).NotEmpty().WithMessage("El tipo de comida es requerido.");
        RuleFor(x => x.Name).NotEmpty().MaximumLength(100).WithMessage("El nombre de la comida es requerido y no debe exceder 100 caracteres.");
    }
}

public class UpdateMealDtoValidator : AbstractValidator<UpdateMealDto>
{
    public UpdateMealDtoValidator()
    {
        RuleFor(x => x.Name).MaximumLength(100).When(x => !string.IsNullOrEmpty(x.Name))
            .WithMessage("El nombre de la comida no debe exceder 100 caracteres.");
        RuleFor(x => x.MealType).NotEmpty().When(x => x.MealType != null)
            .WithMessage("El tipo de comida no puede estar vacío.");
    }
}
