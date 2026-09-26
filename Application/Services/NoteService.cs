using Nexodus_Back.Application.DTOs.Note;
using Nexodus_Back.Application.Interfaces;
using Nexodus_Back.Core.Entities;
using Nexodus_Back.Core.Exceptions;
using Nexodus_Back.Core.Interfaces;

namespace Nexodus_Back.Application.Services;

public class NoteService : INoteService
{
    private readonly INoteRepository _repository;

    public NoteService(INoteRepository repository)
    {
        _repository = repository;
    }

    public async Task<IEnumerable<NoteDto>> GetAllNotesAsync(Guid userId)
    {
        var notes = await _repository.GetAllByUserIdAsync(userId);
        return notes.Select(MapToDto);
    }

    public async Task<NoteDto?> GetNoteByIdAsync(Guid id, Guid userId)
    {
        var note = await _repository.GetByIdAndUserIdAsync(id, userId);
        if (note == null)
            throw new NotFoundException($"Note with ID {id} not found.");

        return MapToDto(note);
    }

    public async Task<NoteDto> CreateNoteAsync(Guid userId, CreateNoteDto dto)
    {
        var note = new Note
        {
            UserId = userId,
            Type = dto.Type,
            Title = dto.Title,
            Content = dto.Content,
            Checklist = dto.Checklist?.Select(c => new ChecklistItem
            {
                Text = c.Text,
                IsCompleted = c.IsCompleted
            }).ToList() ?? new List<ChecklistItem>()
        };

        var created = await _repository.AddAsync(note);
        return MapToDto(created);
    }

    public async Task<NoteDto> UpdateNoteAsync(Guid id, Guid userId, UpdateNoteDto dto)
    {
        var note = await _repository.GetByIdAndUserIdAsync(id, userId);
        if (note == null)
            throw new NotFoundException($"Note with ID {id} not found.");

        if (dto.Title != null) note.Title = dto.Title;
        if (dto.Content != null) note.Content = dto.Content;
        note.UpdatedAt = DateTime.UtcNow;

        if (dto.Checklist != null)
        {
            note.Checklist.Clear();
            foreach (var c in dto.Checklist)
            {
                note.Checklist.Add(new ChecklistItem
                {
                    Text = c.Text,
                    IsCompleted = c.IsCompleted
                });
            }
        }

        await _repository.UpdateAsync(note);
        return MapToDto(note);
    }

    public async Task DeleteNoteAsync(Guid id, Guid userId)
    {
        var note = await _repository.GetByIdAndUserIdAsync(id, userId);
        if (note == null)
            throw new NotFoundException($"Note with ID {id} not found.");

        await _repository.DeleteAsync(note);
    }

    private static NoteDto MapToDto(Note note)
    {
        return new NoteDto
        {
            Id = note.Id,
            Type = note.Type,
            Title = note.Title,
            Content = note.Content,
            CreatedAt = note.CreatedAt,
            UpdatedAt = note.UpdatedAt,
            Checklist = note.Checklist.Select(c => new ChecklistItemDto
            {
                Id = c.Id,
                Text = c.Text,
                IsCompleted = c.IsCompleted
            }).ToList()
        };
    }
}
