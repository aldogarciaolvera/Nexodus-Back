using Microsoft.EntityFrameworkCore;
using Nexodus_Back.Core.Entities;
using Nexodus_Back.Core.Interfaces;
using Nexodus_Back.Infrastructure.Data;

namespace Nexodus_Back.Infrastructure.Repositories;

public class NoteRepository : INoteRepository
{
    private readonly NexodusDbContext _context;

    public NoteRepository(NexodusDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Note>> GetAllByUserIdAsync(Guid userId)
    {
        return await _context.Notes
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
        // NO llamamos a _context.Notes.Update(note) porque la entidad ya está siendo "Trackeada"
        // al haberla obtenido con GetByIdAndUserIdAsync. Llamar a Update() fuerza un estado "Modified"
        // incluso para los ChecklistItems nuevos con un Guid generado, causando DbUpdateConcurrencyException.
        await _context.SaveChangesAsync();
    }

    public async Task DeleteChecklistItemsAsync(IEnumerable<ChecklistItem> items)
    {
        _context.ChecklistItems.RemoveRange(items);
        // No llamamos SaveChangesAsync aquí, se llamará dentro de UpdateAsync del servicio.
        await Task.CompletedTask; 
    }

    public async Task DeleteAsync(Note note)
    {
        _context.Notes.Remove(note);
        await _context.SaveChangesAsync();
    }
}
