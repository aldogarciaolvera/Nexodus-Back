using System;
using System.Collections.Generic;
using System.Linq;
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

    public async Task<IEnumerable<Note>> GetAllByUserIdAsync(Guid userId)
    {
        return await _context.Notes
            .AsNoTracking()
            .Include(n => n.Checklist)
            .Where(n => n.UserId == userId)
            .OrderByDescending(n => n.CreatedAt)
            .ToListAsync();
    }

    public async Task<Note?> GetByIdAndUserIdAsync(Guid id, Guid userId)
    {
        return await _context.Notes
            .Include(n => n.Checklist)
            .FirstOrDefaultAsync(n => n.Id == id && n.UserId == userId);
    }

    public async Task<Note> AddAsync(Note note)
    {
        _context.Notes.Add(note);
        await _context.SaveChangesAsync();
        return note;
    }

    public async Task UpdateAsync(Note note)
    {
        await _context.SaveChangesAsync();
    }

    public async Task DeleteChecklistItemsAsync(IEnumerable<ChecklistItem> items)
    {
        _context.ChecklistItems.RemoveRange(items);
        await Task.CompletedTask; 
    }

    public async Task DeleteAsync(Note note)
    {
        _context.Notes.Remove(note);
        await _context.SaveChangesAsync();
    }
}
