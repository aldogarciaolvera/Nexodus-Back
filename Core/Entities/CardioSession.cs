using System;
using System.Collections.Generic;

namespace Nexodus_Back.Core.Entities;

public class CardioSession
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid UserId { get; set; }
    public string Name { get; set; } = "Sesión de Cardio";
    public string Environment { get; set; } = "Outdoor"; // "Outdoor" | "Gym"
    public string ActivityType { get; set; } = "Mixed"; // "Walking" | "Jogging" | "Running" | "Mixed" | "Treadmill" | "Cycling" | "Elliptical" | "Rowing"
    public string TrackingMode { get; set; } = "Manual"; // "Manual" | "Gps"
    public DateTimeOffset StartTime { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset? EndTime { get; set; }
    public int TotalDurationSeconds { get; set; }
    public double? DistanceKm { get; set; }
    public double EstimatedCalories { get; set; }
    public bool IsAiGenerated { get; set; } = false;
    public string? Notes { get; set; }

    public ICollection<CardioSegment> Segments { get; set; } = new List<CardioSegment>();
}

public class CardioSegment
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid CardioSessionId { get; set; }
    public int SegmentOrder { get; set; }
    public string State { get; set; } = "Jogging"; // "Warmup" | "Walking" | "Jogging" | "Running" | "Sprint" | "Resting" | "Cooldown"
    public int DurationSeconds { get; set; }
    public DateTimeOffset StartedAt { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset EndedAt { get; set; } = DateTimeOffset.UtcNow;

    public CardioSession? CardioSession { get; set; }
}
