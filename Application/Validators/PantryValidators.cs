using FluentValidation;
using Nexodus_Back.Application.DTOs.Diet;

namespace Nexodus_Back.Application.Validators;

public class CreatePantryItemDtoValidator : AbstractValidator<CreatePantryItemDto>
{
    public CreatePantryItemDtoValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("El nombre del ingrediente es obligatorio.")
            .MaximumLength(150).WithMessage("El nombre no puede exceder 150 caracteres.");

        RuleFor(x => x.Quantity)
            .GreaterThan(0).WithMessage("La cantidad debe ser mayor a 0.");

        RuleFor(x => x.Unit)
            .NotEmpty().WithMessage("La unidad de medida es obligatoria.")
            .MaximumLength(20).WithMessage("La unidad no puede exceder 20 caracteres.");
    }
}

public class UpdatePantryItemDtoValidator : AbstractValidator<UpdatePantryItemDto>
{
    public UpdatePantryItemDtoValidator()
    {
        RuleFor(x => x.Name)
            .MaximumLength(150).WithMessage("El nombre no puede exceder 150 caracteres.")
            .When(x => !string.IsNullOrEmpty(x.Name));

        RuleFor(x => x.Quantity)
            .GreaterThan(0).WithMessage("La cantidad debe ser mayor a 0.")
            .When(x => x.Quantity.HasValue);

        RuleFor(x => x.Unit)
            .MaximumLength(20).WithMessage("La unidad no puede exceder 20 caracteres.")
            .When(x => !string.IsNullOrEmpty(x.Unit));
    }
}

