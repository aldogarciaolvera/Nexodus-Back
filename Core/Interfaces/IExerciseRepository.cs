using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Nexodus_Back.Core.Entities;

namespace Nexodus_Back.Core.Interfaces;

/// <summary>
/// Repository interface for exercise catalog data access.
/// </summary>
public interface IExerciseRepository
{
    Task<IEnumerable<Exercise>> GetAllAsync(int page = 1, int pageSize = 50, CancellationToken cancellationToken = default);
    Task<Exercise?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
}
