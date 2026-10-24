using System;

namespace Nexodus_Back.Core.Entities;

public class FastingLog
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public DateTime StartTime { get; set; } = DateTime.UtcNow;
    public DateTime? EndTime { get; set; }
    public double TargetHours { get; set; } = 16;
    public string FastingProtocol { get; set; } = "16:8";
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public User? User { get; set; }
}
