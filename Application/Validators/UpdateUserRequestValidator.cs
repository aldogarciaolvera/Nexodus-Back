using FluentValidation;
using Nexodus_Back.Application.DTOs.User;

namespace Nexodus_Back.Application.Validators;

public class UpdateUserRequestValidator : AbstractValidator<UpdateUserRequest>
{
    public UpdateUserRequestValidator()
    {
        RuleFor(x => x.Username)
            .NotEmpty().WithMessage("El nombre de usuario es obligatorio.")
            .MinimumLength(3).WithMessage("El nombre de usuario debe tener al menos 3 caracteres.");

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("El correo es obligatorio.")
            .EmailAddress().WithMessage("El correo es inválido.");

        RuleFor(x => x.Weight)
            .GreaterThanOrEqualTo(0).WithMessage("El peso debe ser mayor o igual a 0.");

        RuleFor(x => x.Height)
            .GreaterThanOrEqualTo(0).WithMessage("La altura debe ser mayor o igual a 0.");
    }
}
