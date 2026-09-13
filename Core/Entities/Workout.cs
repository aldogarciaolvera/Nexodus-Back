using System;

namespace Nexodus_Back.Core.Entities;

public class Workout
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string WorkoutType { get; set; } = string.Empty;
    public int? Duration { get; set; }
    public int? CaloriesBurned { get; set; }
    public DateTime WorkoutDate { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public User? User { get; set; }
}
