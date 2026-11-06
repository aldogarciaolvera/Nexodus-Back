using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Nexodus_Back.Application.DTOs;

namespace Nexodus_Back.Application.Interfaces;

public interface IWorkoutSessionService
{
    Task<WorkoutSessionDto> StartSessionAsync(StartWorkoutSessionDto dto, Guid userId, CancellationToken cancellationToken = default);
    Task<WorkoutSessionDto?> GetActiveSessionAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<WorkoutSessionDto> GetSessionByIdAsync(Guid id, Guid userId, CancellationToken cancellationToken = default);
    Task<IEnumerable<WorkoutSessionDto>> GetUserSessionsAsync(Guid userId, int limit = 30, CancellationToken cancellationToken = default);
    Task<WorkoutSetDto> LogSetAsync(Guid sessionId, LogWorkoutSetDto dto, Guid userId, CancellationToken cancellationToken = default);
    Task DeleteSetAsync(Guid sessionId, Guid setId, Guid userId, CancellationToken cancellationToken = default);
    Task<WorkoutSessionDto> FinishSessionAsync(Guid sessionId, FinishWorkoutSessionDto dto, Guid userId, CancellationToken cancellationToken = default);
    Task CancelSessionAsync(Guid sessionId, Guid userId, CancellationToken cancellationToken = default);
    Task<IEnumerable<MuscleRecoveryDto>> GetMuscleRecoveryStatusAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<ExerciseRecordDto> GetExerciseRecordAsync(Guid exerciseId, Guid userId, CancellationToken cancellationToken = default);
}

