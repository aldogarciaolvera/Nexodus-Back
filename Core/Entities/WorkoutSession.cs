using System;
using System.Collections.Generic;

namespace Nexodus_Back.Core.Entities;

public class WorkoutSession
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid UserId { get; set; }
    public Guid? RoutineId { get; set; }

    public string Name { get; set; } = "Entrenamiento";
    public DateTime StartTime { get; set; } = DateTime.UtcNow;
    public DateTime? EndTime { get; set; }
    public int DurationSeconds { get; set; } = 0;
    public double TotalVolumeKg { get; set; } = 0;
    public int TotalReps { get; set; } = 0;
    public int CompletedSetsCount { get; set; } = 0;
    public int XpEarned { get; set; } = 0;
    public bool IsActive { get; set; } = true;
    public string? Notes { get; set; }

    // Navigation properties
    public User User { get; set; } = null!;
    public Routine? Routine { get; set; }
    public ICollection<WorkoutSet> Sets { get; set; } = new List<WorkoutSet>();
}

