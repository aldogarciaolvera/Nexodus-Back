using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Nexodus_Back.Core.Entities;

namespace Nexodus_Back.Core.Interfaces;

/// <summary>
/// Repository interface for routines data access.
/// </summary>
public interface IRoutineRepository
{
    Task<IEnumerable<Routine>> GetAllByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<Routine?> GetByIdAsync(Guid id, Guid userId, CancellationToken cancellationToken = default);
    Task<Routine> AddAsync(Routine routine, CancellationToken cancellationToken = default);
    Task UpdateAsync(Routine routine, CancellationToken cancellationToken = default);
    Task DeleteAsync(Routine routine, CancellationToken cancellationToken = default);
}
