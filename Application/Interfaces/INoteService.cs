using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Nexodus_Back.Application.DTOs.Note;

namespace Nexodus_Back.Application.Interfaces;

/// <summary>
/// Service providing business operations for user notes and checklists.
/// </summary>
public interface INoteService
{
    /// <summary>
    /// Retrieves all notes for the given user with optional pagination.
    /// </summary>
    Task<IEnumerable<NoteDto>> GetAllNotesAsync(Guid userId, int? page = null, int? pageSize = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves a single note by ID and user ID.
    /// </summary>
    Task<NoteDto?> GetNoteByIdAsync(Guid id, Guid userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Creates a new note with optional checklist items.
    /// </summary>
    Task<NoteDto> CreateNoteAsync(Guid userId, CreateNoteDto dto, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates an existing note and reconciles its checklist items.
    /// </summary>
    Task<NoteDto> UpdateNoteAsync(Guid id, Guid userId, UpdateNoteDto dto, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes a note and all its checklist items.
    /// </summary>
    Task DeleteNoteAsync(Guid id, Guid userId, CancellationToken cancellationToken = default);
}
