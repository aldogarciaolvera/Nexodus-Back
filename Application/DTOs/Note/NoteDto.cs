namespace Nexodus_Back.Application.DTOs.Note;

public class NoteDto
{
    public Guid Id { get; set; }
    public string Type { get; set; } = string.Empty;
    public string? Title { get; set; }
    public string? Content { get; set; }
    public List<ChecklistItemDto> Checklist { get; set; } = new();
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
