using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Nexodus_Back.Application.DTOs;
using Nexodus_Back.Application.Interfaces;
using Nexodus_Back.Core.Entities;
using Nexodus_Back.Core.Exceptions;
using Nexodus_Back.Core.Interfaces;

namespace Nexodus_Back.Application.Services;

/// <summary>
/// Provides workout log tracking and routine validation operations.
/// </summary>
public class WorkoutLogService(
    IWorkoutLogRepository workoutLogRepository,
    IRoutineRepository routineRepository) : IWorkoutLogService
{
    private readonly IWorkoutLogRepository _workoutLogRepository = workoutLogRepository ?? throw new ArgumentNullException(nameof(workoutLogRepository));
    private readonly IRoutineRepository _routineRepository = routineRepository ?? throw new ArgumentNullException(nameof(routineRepository));

    public async Task<IEnumerable<WorkoutLogDto>> GetAllByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var logs = await _workoutLogRepository.GetAllByUserIdAsync(userId, cancellationToken);
        return logs.Select(MapToDto);
    }

    public async Task<WorkoutLogDto> GetByIdAsync(Guid id, Guid userId, CancellationToken cancellationToken = default)
    {
        var log = await _workoutLogRepository.GetByIdAsync(id, cancellationToken);

        if (log == null || log.UserId != userId)
        {
            throw new NotFoundException($"No se encontró un registro de entrenamiento con ID {id}.");
        }

        return MapToDto(log);
    }

    public async Task<WorkoutLogDto> CreateAsync(CreateWorkoutLogDto dto, Guid userId, CancellationToken cancellationToken = default)
    {
        var routine = await _routineRepository.GetByIdAsync(dto.RoutineId, userId, cancellationToken);
        if (routine == null)
        {
            throw new NotFoundException($"No se encontró una rutina con ID {dto.RoutineId} para asociar al registro.");
        }

        var log = new WorkoutLog
        {
            UserId = userId,
            RoutineId = dto.RoutineId,
            DurationInSeconds = dto.DurationInSeconds,
            CompletedExercisesCount = dto.CompletedExercisesCount,
            DateCompleted = dto.DateCompleted,
            CreatedAt = DateTime.UtcNow
        };

        await _workoutLogRepository.AddAsync(log, cancellationToken);

        return MapToDto(log);
    }

    public async Task DeleteAsync(Guid id, Guid userId, CancellationToken cancellationToken = default)
    {
        var log = await _workoutLogRepository.GetByIdAsync(id, cancellationToken);

        if (log == null || log.UserId != userId)
        {
            throw new NotFoundException($"No se encontró un registro de entrenamiento con ID {id}.");
        }

        await _workoutLogRepository.DeleteAsync(log, cancellationToken);
    }

    private static WorkoutLogDto MapToDto(WorkoutLog log)
    {
        return new WorkoutLogDto
        {
            Id = log.Id,
            RoutineId = log.RoutineId,
            DurationInSeconds = log.DurationInSeconds,
            CompletedExercisesCount = log.CompletedExercisesCount,
            DateCompleted = log.DateCompleted,
            CreatedAt = log.CreatedAt
        };
    }
}
