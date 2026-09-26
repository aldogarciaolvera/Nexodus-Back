using Nexodus_Back.Application.DTOs.Note;

namespace Nexodus_Back.Application.Interfaces;

public interface INoteService
{
    Task<IEnumerable<NoteDto>> GetAllNotesAsync(Guid userId);
    Task<NoteDto?> GetNoteByIdAsync(Guid id, Guid userId);
    Task<NoteDto> CreateNoteAsync(Guid userId, CreateNoteDto dto);
    Task<NoteDto> UpdateNoteAsync(Guid id, Guid userId, UpdateNoteDto dto);
    Task DeleteNoteAsync(Guid id, Guid userId);
}
