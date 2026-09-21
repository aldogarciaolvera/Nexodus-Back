using System;

namespace Nexodus_Back.Core.Entities;

public class TodoList
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string Task { get; set; } = string.Empty;
    public string? Subtitle { get; set; }
    public string? Tag { get; set; }
    public bool Urgent { get; set; }
    public bool IsCompleted { get; set; }
    public DateTime? DueDate { get; set; }
    public bool IsHabit { get; set; }
    public string? Frequency { get; set; }
    public string? CustomDays { get; set; }
    public int CurrentStreak { get; set; }
    public int HighestStreak { get; set; }
    public DateTime? LastCompletedAt { get; set; }
    public DateTime? PreviousCompletedAt { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public User? User { get; set; }
}
