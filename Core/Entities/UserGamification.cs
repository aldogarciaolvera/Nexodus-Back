using System;

namespace Nexodus_Back.Core.Entities;

public class UserGamification
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public User? User { get; set; }

    public int CurrentStreak { get; set; } = 0;
    public int HighestStreak { get; set; } = 0;
    public DateTime? LastActiveDate { get; set; }
    public DateTime? StreakGracePeriodExpiresAt { get; set; }
    public string ScheduledRestDays { get; set; } = "Saturday,Sunday";
    public int StreakFreezesAvailable { get; set; } = 2;

    public int NxpTotal { get; set; } = 0;
    public string Tier { get; set; } = "Initiate";
    public int Cyphers { get; set; } = 100;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}

