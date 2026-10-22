using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Nexodus_Back.Core.Entities;

namespace Nexodus_Back.Core.Interfaces;

/// <summary>
/// Repository interface for workout log data access.
/// </summary>
public interface IWorkoutLogRepository
{
    Task<WorkoutLog?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IEnumerable<WorkoutLog>> GetAllByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
    Task AddAsync(WorkoutLog workoutLog, CancellationToken cancellationToken = default);
    Task DeleteAsync(WorkoutLog workoutLog, CancellationToken cancellationToken = default);
}
