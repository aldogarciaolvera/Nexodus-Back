using FluentValidation;
using Nexodus_Back.Application.DTOs.Auth;

namespace Nexodus_Back.Application.Validators;

public class RevokeTokenRequestValidator : AbstractValidator<RevokeTokenRequest>
{
    public RevokeTokenRequestValidator()
    {
        RuleFor(x => x.RefreshToken)
            .NotEmpty().WithMessage("El Refresh Token es obligatorio para revocar la sesión.");
    }
}

