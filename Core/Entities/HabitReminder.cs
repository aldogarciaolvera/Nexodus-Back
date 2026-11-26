using System;

namespace Nexodus_Back.Core.Entities;

public class HabitReminder
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid HabitId { get; set; }
    public Habit? Habit { get; set; }

    /// <summary>
    /// Hora en formato "HH:mm".
    /// </summary>
    public string Time { get; set; } = "08:00";

    /// <summary>
    /// Días de la semana para el recordatorio (ej: "1,2,3,4,5"). Nulo o vacío significa todos los días programados.
    /// </summary>
    public string? DaysOfWeek { get; set; }

    public bool IsEnabled { get; set; } = true;
}
