using Nexodus_Back.Core.Entities;

namespace Nexodus_Back.Core.Interfaces;

public interface IExerciseRepository
{
    Task<IEnumerable<Exercise>> GetAllAsync();
    Task<Exercise?> GetByIdAsync(Guid id);
}
