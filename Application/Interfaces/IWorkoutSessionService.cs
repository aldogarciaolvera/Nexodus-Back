using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Nexodus_Back.Application.DTOs;

namespace Nexodus_Back.Application.Interfaces;

/// <summary>
/// Service managing real-time gym workout sessions, set tracking, muscle recovery algorithms, and personal records.
/// </summary>
public interface IWorkoutSessionService
{
    /// <summary>
    /// Starts a live workout session.
    /// </summary>
    Task<WorkoutSessionDto> StartSessionAsync(StartWorkoutSessionDto dto, Guid userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves current active workout session if one is in progress.
    /// </summary>
    Task<WorkoutSessionDto?> GetActiveSessionAsync(Guid userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves a workout session by ID.
    /// </summary>
    Task<WorkoutSessionDto> GetSessionByIdAsync(Guid id, Guid userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves recent completed workout sessions for the user.
    /// </summary>
    Task<IEnumerable<WorkoutSessionDto>> GetUserSessionsAsync(Guid userId, int limit = 30, CancellationToken cancellationToken = default);

    /// <summary>
    /// Logs a completed set within an active workout session.
    /// </summary>
    Task<WorkoutSetDto> LogSetAsync(Guid sessionId, LogWorkoutSetDto dto, Guid userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Removes a logged set from the session.
    /// </summary>
    Task DeleteSetAsync(Guid sessionId, Guid setId, Guid userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Completes the session, calculating muscle fatigue and awarding gamification rewards.
    /// </summary>
    Task<WorkoutSessionDto> FinishSessionAsync(Guid sessionId, FinishWorkoutSessionDto dto, Guid userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Cancels and discards an active workout session.
    /// </summary>
    Task CancelSessionAsync(Guid sessionId, Guid userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Calculates real-time recovery status across all muscle groups based on recent sessions.
    /// </summary>
    Task<IEnumerable<MuscleRecoveryDto>> GetMuscleRecoveryStatusAsync(Guid userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves personal best records (1RM, max weight, max volume) for a given exercise.
    /// </summary>
    Task<ExerciseRecordDto> GetExerciseRecordAsync(Guid exerciseId, Guid userId, CancellationToken cancellationToken = default);
}
