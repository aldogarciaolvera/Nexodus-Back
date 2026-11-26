using FluentValidation;
using Nexodus_Back.Application.DTOs.Auth;

namespace Nexodus_Back.Application.Validators;

public class RefreshTokenRequestValidator : AbstractValidator<RefreshTokenRequest>
{
    public RefreshTokenRequestValidator()
    {
        RuleFor(x => x.RefreshToken)
            .NotEmpty().WithMessage("El Refresh Token es obligatorio.");
    }
}

