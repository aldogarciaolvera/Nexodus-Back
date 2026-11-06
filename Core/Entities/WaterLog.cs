using System;

namespace Nexodus_Back.Core.Entities;

public class WaterLog
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string Date { get; set; } = string.Empty; // "yyyy-MM-dd"
    public double ConsumedMl { get; set; }
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public User? User { get; set; }
}
