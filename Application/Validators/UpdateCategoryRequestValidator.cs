using FluentValidation;
using Nexodus_Back.Application.DTOs.Category;

namespace Nexodus_Back.Application.Validators;

public class UpdateCategoryRequestValidator : AbstractValidator<UpdateCategoryRequest>
{
    public UpdateCategoryRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("El nombre de la categoría es obligatorio.");

        RuleFor(x => x.MonthlyLimit)
            .GreaterThanOrEqualTo(0).When(x => x.MonthlyLimit.HasValue)
            .WithMessage("El límite mensual debe ser mayor o igual a cero.");
    }
}
