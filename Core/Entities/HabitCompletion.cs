using System;

namespace Nexodus_Back.Core.Entities;

public class HabitCompletion
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid HabitId { get; set; }
    public Habit? Habit { get; set; }

    public Guid UserId { get; set; }

    /// <summary>
    /// Fecha en formato "yyyy-MM-dd" para representación independiente de huso horario.
    /// </summary>
    public string Date { get; set; } = string.Empty;

    public double Amount { get; set; } = 0.0;

    /// <summary>
    /// "Completed", "Partial", "Slip", "Skipped".
    /// </summary>
    public string Status { get; set; } = "Completed";

    public string? Note { get; set; }

    public DateTime LoggedAt { get; set; } = DateTime.UtcNow;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
