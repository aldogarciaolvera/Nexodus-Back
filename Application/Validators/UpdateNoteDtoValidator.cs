using FluentValidation;
using Nexodus_Back.Application.DTOs.Note;

namespace Nexodus_Back.Application.Validators;

public class UpdateNoteDtoValidator : AbstractValidator<UpdateNoteDto>
{
    public UpdateNoteDtoValidator()
    {
        RuleFor(x => x.Title)
            .MaximumLength(255).WithMessage("El título no puede exceder 255 caracteres.")
            .When(x => x.Title != null);
    }
}
