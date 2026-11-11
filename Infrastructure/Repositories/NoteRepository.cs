using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Nexodus_Back.Core.Entities;
using Nexodus_Back.Core.Interfaces;
using Nexodus_Back.Infrastructure.Data;

namespace Nexodus_Back.Infrastructure.Repositories;

/// <summary>
/// Entity Framework implementation of user notes and checklists repository.
/// </summary>
public class NoteRepository(NexodusDbContext context) : INoteRepository
{
    private readonly NexodusDbContext _context = context ?? throw new ArgumentNullException(nameof(context));

    public async Task<IEnumerable<Note>> GetAllByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await _context.Notes
            .AsNoTracking()
            .Include(n => n.Checklist)
            .Where(n => n.UserId == userId)
            .OrderByDescending(n => n.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<Note?> GetByIdAndUserIdAsync(Guid id, Guid userId, CancellationToken cancellationToken = default)
    {
        return await _context.Notes
            .Include(n => n.Checklist)
            .FirstOrDefaultAsync(n => n.Id == id && n.UserId == userId, cancellationToken);
    }

    public async Task<Note> AddAsync(Note note, CancellationToken cancellationToken = default)
    {
        _context.Notes.Add(note);
        await _context.SaveChangesAsync(cancellationToken);
        return note;
    }

    public async Task UpdateAsync(Note note, CancellationToken cancellationToken = default)
    {
        await _context.SaveChangesAsync(cancellationToken);
    }

    public Task DeleteChecklistItemsAsync(IEnumerable<ChecklistItem> items, CancellationToken cancellationToken = default)
    {
        _context.ChecklistItems.RemoveRange(items);
        return Task.CompletedTask;
    }

    public async Task DeleteAsync(Note note, CancellationToken cancellationToken = default)
    {
        _context.Notes.Remove(note);
        await _context.SaveChangesAsync(cancellationToken);
    }
}
