using System;
using System.Collections.Generic;

namespace Nexodus_Back.Core.Entities;

public class Habit
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid UserId { get; set; }
    public User? User { get; set; }

    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string Icon { get; set; } = "check";
    public string Color { get; set; } = "#00F0FF";
    public string Category { get; set; } = "General";

    /// <summary>
    /// "Build" (hábito positivo a incorporar) o "Quit" (hábito negativo a romper/dejar).
    /// </summary>
    public string HabitType { get; set; } = "Build";

    /// <summary>
    /// "YesNo" (binario), "Count" (numérico discreto), "Measure" (continuo con unidad).
    /// </summary>
    public string TrackingType { get; set; } = "YesNo";

    public double TargetValue { get; set; } = 1.0;
    public string? TargetUnit { get; set; }

    /// <summary>
    /// "Increment" (suma +1 con cada toque rápido) o "Manual" (abre selector/teclado para cantidad exacta).
    /// </summary>
    public string OnTapAction { get; set; } = "Increment";

    /// <summary>
    /// "Daily", "SpecificDays", "TimesPerWeek", "TimesPerMonth", "EveryNDays".
    /// </summary>
    public string FrequencyType { get; set; } = "Daily";

    /// <summary>
    /// Días de la semana para SpecificDays (1=Lun, 2=Mar, ... 7=Dom). Ej: "1,2,3,4,5".
    /// </summary>
    public string? FrequencyDays { get; set; }

    public int FrequencyTargetCount { get; set; } = 1;
    public int EveryNDaysInterval { get; set; } = 1;

    /// <summary>
    /// "Days", "Weeks", "Months".
    /// </summary>
    public string StreakUnit { get; set; } = "Days";

    public int? StreakGoal { get; set; }
    public bool FillMetPeriods { get; set; } = true;

    /// <summary>
    /// Si no es nulo, se puede autocompletar desde eventos del sistema: "WorkoutSession", "WaterLog", "DietTarget".
    /// </summary>
    public string? AutoCompleteSource { get; set; }

    public int CurrentStreak { get; set; } = 0;
    public int BestStreak { get; set; } = 0;
    public DateTime? LastCompletedDate { get; set; }

    public bool IsArchived { get; set; } = false;
    public int OrderIndex { get; set; } = 0;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<HabitCompletion> Completions { get; set; } = new List<HabitCompletion>();
    public ICollection<HabitReminder> Reminders { get; set; } = new List<HabitReminder>();
}
