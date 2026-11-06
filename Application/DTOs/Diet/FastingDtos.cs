using System;

namespace Nexodus_Back.Application.DTOs.Diet;

public class FastingStatusDto
{
    public bool IsFasting { get; set; }
    public DateTime? StartTime { get; set; }
    public DateTime? EndTime { get; set; }
    public double TargetHours { get; set; }
    public double ElapsedHours { get; set; }
    public double RemainingHours { get; set; }
    public double ProgressPercentage { get; set; }
    public DateTime? EstimatedEndTime { get; set; }
    public string FastingProtocol { get; set; } = "16:8";
}

public class StartFastingDto
{
    public double TargetHours { get; set; } = 16;
    public string FastingProtocol { get; set; } = "16:8";
    public DateTime? StartTime { get; set; }
}
