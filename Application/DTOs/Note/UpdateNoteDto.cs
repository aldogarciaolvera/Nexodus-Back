namespace Nexodus_Back.Application.DTOs.Note;

public class UpdateNoteDto
{
    public string? Title { get; set; }
    public string? Content { get; set; }
    public List<ChecklistItemDto>? Checklist { get; set; }
}
