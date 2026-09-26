namespace Nexodus_Back.Application.DTOs.Note;

public class ChecklistItemDto
{
    public Guid? Id { get; set; }
    public string Text { get; set; } = string.Empty;
    public bool IsCompleted { get; set; }
}
