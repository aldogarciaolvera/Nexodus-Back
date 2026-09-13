using System;

namespace Nexodus_Back.Core.Entities;

public class TodoList
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string Task { get; set; } = string.Empty;
    public bool IsCompleted { get; set; }
    public DateTime? DueDate { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public User? User { get; set; }
}
