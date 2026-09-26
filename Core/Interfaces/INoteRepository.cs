using Nexodus_Back.Core.Entities;

namespace Nexodus_Back.Core.Interfaces;

public interface INoteRepository
{
    Task<IEnumerable<Note>> GetAllByUserIdAsync(Guid userId);
    Task<Note?> GetByIdAndUserIdAsync(Guid id, Guid userId);
    Task<Note> AddAsync(Note note);
    Task UpdateAsync(Note note);
    Task DeleteAsync(Note note);
}
