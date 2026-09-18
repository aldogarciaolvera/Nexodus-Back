using FluentValidation;
using Nexodus_Back.Application.DTOs.TodoList;

namespace Nexodus_Back.Application.Validators;

public class CreateTodoListRequestValidator : AbstractValidator<CreateTodoListRequest>
{
    public CreateTodoListRequestValidator()
    {
        RuleFor(x => x.Task)
            .NotEmpty().WithMessage("La tarea es obligatoria.");

        RuleFor(x => x.Frequency)
            .Must(f => string.IsNullOrEmpty(f) || f == "Daily" || f == "Weekly" || f == "Monthly" || f == "Custom")
            .WithMessage("La frecuencia debe ser 'Daily', 'Weekly', 'Monthly' o 'Custom'.");
    }
}
