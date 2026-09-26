namespace Nexodus_Back.Application.DTOs.Note;

public class CreateNoteDto
{
    public string Type { get; set; } = string.Empty;
    public string? Title { get; set; }
    public string? Content { get; set; }
    public List<ChecklistItemDto>? Checklist { get; set; }
}
