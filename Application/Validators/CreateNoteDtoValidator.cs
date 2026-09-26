using FluentValidation;
using Nexodus_Back.Application.DTOs.Note;

namespace Nexodus_Back.Application.Validators;

public class CreateNoteDtoValidator : AbstractValidator<CreateNoteDto>
{
    public CreateNoteDtoValidator()
    {
        RuleFor(x => x.Type)
            .NotEmpty().WithMessage("El tipo de nota es obligatorio.")
            .Must(type => type == "idea" || type == "diario")
            .WithMessage("El tipo debe ser 'idea' o 'diario'.");

        RuleFor(x => x.Title)
            .MaximumLength(255).WithMessage("El título no puede exceder 255 caracteres.");
    }
}
