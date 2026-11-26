using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Nexodus_Back.Core.Entities;

namespace Nexodus_Back.Core.Interfaces;

/// <summary>
/// Repository interface for user notes and checklists.
/// </summary>
public interface INoteRepository
{
    /// <summary>
    /// Retrieves all notes belonging to the specified user.
    /// </summary>
    Task<IEnumerable<Note>> GetAllByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves a single note by its ID and user ID.
    /// </summary>
    Task<Note?> GetByIdAndUserIdAsync(Guid id, Guid userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Persists a new note entity.
    /// </summary>
    Task<Note> AddAsync(Note note, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates an existing note and its tracked state.
    /// </summary>
    Task UpdateAsync(Note note, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes a note entity.
    /// </summary>
    Task DeleteAsync(Note note, CancellationToken cancellationToken = default);

    /// <summary>
    /// Removes checklist items in bulk.
    /// </summary>
    Task DeleteChecklistItemsAsync(IEnumerable<ChecklistItem> items, CancellationToken cancellationToken = default);
}
