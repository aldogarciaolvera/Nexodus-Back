using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Nexodus_Back.Core.Entities;

namespace Nexodus_Back.Core.Interfaces;

/// <summary>
/// Repository interface for active and completed workout sessions and individual sets.
/// </summary>
public interface IWorkoutSessionRepository
{
    Task<WorkoutSession?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<WorkoutSession?> GetActiveByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<IEnumerable<WorkoutSession>> GetAllByUserIdAsync(Guid userId, int limit = 50, CancellationToken cancellationToken = default);
    Task AddAsync(WorkoutSession session, CancellationToken cancellationToken = default);
    Task UpdateAsync(WorkoutSession session, CancellationToken cancellationToken = default);
    Task DeleteAsync(WorkoutSession session, CancellationToken cancellationToken = default);
    Task<WorkoutSet?> GetSetByIdAsync(Guid setId, CancellationToken cancellationToken = default);
    Task AddSetAsync(WorkoutSet set, CancellationToken cancellationToken = default);
    Task UpdateSetAsync(WorkoutSet set, CancellationToken cancellationToken = default);
    Task DeleteSetAsync(WorkoutSet set, CancellationToken cancellationToken = default);
    Task<double> GetExercisePersonalBest1RMAsync(Guid userId, Guid exerciseId, CancellationToken cancellationToken = default);
    Task<IEnumerable<WorkoutSet>> GetExerciseHistoryAsync(Guid userId, Guid exerciseId, int limit = 20, CancellationToken cancellationToken = default);
    Task<IEnumerable<WorkoutSession>> GetRecentCompletedSessionsAsync(Guid userId, DateTime since, CancellationToken cancellationToken = default);
}

