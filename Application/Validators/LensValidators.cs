using FluentValidation;
using Nexodus_Back.Application.DTOs.Diet;

namespace Nexodus_Back.Application.Validators;

public class AnalyzeFoodImageRequestDtoValidator : AbstractValidator<AnalyzeFoodImageRequestDto>
{
    public AnalyzeFoodImageRequestDtoValidator()
    {
        RuleFor(x => x.Base64Image)
            .NotEmpty().WithMessage("La imagen en formato base64 es obligatoria para el análisis de Nexodus Lens.");

        When(x => !string.IsNullOrEmpty(x.AdditionalContext), () =>
        {
            RuleFor(x => x.AdditionalContext)
                .MaximumLength(500).WithMessage("El contexto adicional no puede superar los 500 caracteres.");
        });
    }
}

