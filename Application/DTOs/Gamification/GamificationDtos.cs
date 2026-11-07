using System;
using System.Collections.Generic;

namespace Nexodus_Back.Application.DTOs.Gamification;

public class GamificationStatusDto
{
    public int CurrentStreak { get; set; }
    public int HighestStreak { get; set; }
    public DateTime? LastActiveDate { get; set; }
    public bool IsActiveToday { get; set; }
    public bool IsRestDayToday { get; set; }
    public bool IsGracePeriodActive { get; set; }
    public DateTime? GracePeriodExpiresAt { get; set; }
    public List<string> ScheduledRestDays { get; set; } = new();
    public int StreakFreezesAvailable { get; set; }
    public int NxpTotal { get; set; }
    public string Tier { get; set; } = "Initiate";
    public string? NextTier { get; set; }
    public int NextTierNxp { get; set; }
    public int CurrentTierBaseNxp { get; set; }
    public double ProgressPercent { get; set; }
    public int Cyphers { get; set; }
}

public class UpdateRestDaysDto
{
    public List<string> RestDays { get; set; } = new();
}

public class StreakRestoreResultDto
{
    public bool Success { get; set; }
    public int RestoredStreak { get; set; }
    public int CyphersRemaining { get; set; }
    public string Message { get; set; } = string.Empty;
}

public class VaultPurchaseResultDto
{
    public bool Success { get; set; }
    public string ItemType { get; set; } = string.Empty;
    public int CyphersRemaining { get; set; }
    public int FreezesAvailable { get; set; }
    public string Message { get; set; } = string.Empty;
}
