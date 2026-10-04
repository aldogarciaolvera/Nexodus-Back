using Nexodus_Back.Core.Entities;

namespace Nexodus_Back.Core.Interfaces;

public interface IRoutineRepository
{
    Task<IEnumerable<Routine>> GetAllByUserIdAsync(Guid userId);
    Task<Routine?> GetByIdAsync(Guid id, Guid userId);
    Task<Routine> AddAsync(Routine routine);
    Task UpdateAsync(Routine routine);
    Task DeleteAsync(Routine routine);
}
