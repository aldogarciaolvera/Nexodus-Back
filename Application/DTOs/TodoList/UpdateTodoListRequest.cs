using System;

namespace Nexodus_Back.Application.DTOs.TodoList;

public class UpdateTodoListRequest
{
    public string Task { get; set; } = string.Empty;
    public string? Subtitle { get; set; }
    public string? Tag { get; set; }
    public bool Urgent { get; set; }
    public bool NotificationsEnabled { get; set; }
    public bool IsCompleted { get; set; }
    public DateTime? DueDate { get; set; }
    
    public bool IsHabit { get; set; }
    public string? Frequency { get; set; }
    public string? CustomDays { get; set; }
}
