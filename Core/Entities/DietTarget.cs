using System;

namespace Nexodus_Back.Core.Entities;

public class DietTarget
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public double TargetCalories { get; set; }
    public double ProteinGrams { get; set; }
    public double CarbsGrams { get; set; }
    public double FatGrams { get; set; }
    public double WaterTargetMl { get; set; }
    public string FastingProtocol { get; set; } = "None"; // "16:8", "18:6", "None"
    public string EatingWindowStart { get; set; } = "12:00";
    public string EatingWindowEnd { get; set; } = "20:00";
    public bool IsCalorieCyclingEnabled { get; set; } = true;
    public double WorkoutDayCarbMultiplier { get; set; } = 1.20; // +20% carbohidratos en días de gym
    public double RestDayCarbMultiplier { get; set; } = 0.80; // -20% carbohidratos en días de descanso
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public User? User { get; set; }
}
