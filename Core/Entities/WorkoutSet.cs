using System;
using Nexodus_Back.Core.Enums;

namespace Nexodus_Back.Core.Entities;

public class WorkoutSet
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid WorkoutSessionId { get; set; }
    public Guid ExerciseId { get; set; }

    public int SetOrder { get; set; } = 1;
    public SetType Type { get; set; } = SetType.Normal;
    public double WeightKg { get; set; } = 0;
    public int Reps { get; set; } = 0;
    public double? Rpe { get; set; }
    public int? DurationSeconds { get; set; }
    public double CalculatedOneRepMax { get; set; } = 0;
    public bool IsCompleted { get; set; } = false;
    public DateTime? CompletedAt { get; set; }

    // Navigation properties
    public WorkoutSession WorkoutSession { get; set; } = null!;
    public Exercise Exercise { get; set; } = null!;
}

