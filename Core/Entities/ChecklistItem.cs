namespace Nexodus_Back.Core.Entities;

public class ChecklistItem
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid NoteId { get; set; }
    public Note Note { get; set; } = null!;
    
    public string Text { get; set; } = string.Empty;
    public bool IsCompleted { get; set; } = false;
}
