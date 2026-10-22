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
/// Provides routine management operations and exercise association.
/// </summary>
public class RoutineService(
    IRoutineRepository routineRepository,
    IExerciseRepository exerciseRepository,
    IStorageService storageService) : IRoutineService
{
    private readonly IRoutineRepository _routineRepository = routineRepository ?? throw new ArgumentNullException(nameof(routineRepository));
    private readonly IExerciseRepository _exerciseRepository = exerciseRepository ?? throw new ArgumentNullException(nameof(exerciseRepository));
    private readonly IStorageService _storageService = storageService ?? throw new ArgumentNullException(nameof(storageService));

    public async Task<IEnumerable<RoutineDto>> GetAllRoutinesAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var routines = await _routineRepository.GetAllByUserIdAsync(userId, cancellationToken);
        
        return routines.Select(r => new RoutineDto(
            r.Id,
            r.Name,
            r.Description,
            r.DifficultyLevel,
            r.TargetDay,
            [], // Sin detalle para listado general por rendimiento
            r.CreatedAt,
            r.UpdatedAt
        ));
    }

    public async Task<RoutineDto> GetRoutineByIdAsync(Guid id, Guid userId, CancellationToken cancellationToken = default)
    {
        var routine = await _routineRepository.GetByIdAsync(id, userId, cancellationToken);
        if (routine == null) throw new NotFoundException($"Routine with ID {id} not found.");

        var exerciseTasks = routine.RoutineExercises.Select(async re =>
        {
            ExerciseDto? exDto = null;
            if (re.Exercise != null)
            {
                var gifTask = string.IsNullOrEmpty(re.Exercise.GifS3Key) ? null : _storageService.GetFileUrlAsync(re.Exercise.GifS3Key);
                var thumbTask = string.IsNullOrEmpty(re.Exercise.ThumbS3Key) ? null : _storageService.GetFileUrlAsync(re.Exercise.ThumbS3Key);

                var gifUrl = gifTask != null ? await gifTask : null;
                var thumbUrl = thumbTask != null ? await thumbTask : null;

                exDto = new ExerciseDto(
                    re.Exercise.Id,
                    re.Exercise.ExternalId,
                    re.Exercise.Name,
                    re.Exercise.BodyPart,
                    re.Exercise.Muscle,
                    re.Exercise.Equipment,
                    re.Exercise.Category,
                    re.Exercise.Instructions,
                    gifUrl,
                    thumbUrl
                );
            }

            return new RoutineExerciseDto(
                re.ExerciseId,
                re.Sets,
                re.Reps,
                re.RestTimeInSeconds,
                re.Weight,
                exDto
            );
        });

        var exercisesList = (await Task.WhenAll(exerciseTasks)).ToList();

        return new RoutineDto(
            routine.Id,
            routine.Name,
            routine.Description,
            routine.DifficultyLevel,
            routine.TargetDay,
            exercisesList,
            routine.CreatedAt,
            routine.UpdatedAt
        );
    }

    public async Task<RoutineDto> CreateRoutineAsync(Guid userId, CreateRoutineDto dto, CancellationToken cancellationToken = default)
    {
        var routine = new Routine
        {
            UserId = userId,
            Name = dto.Name,
            Description = dto.Description,
            DifficultyLevel = dto.DifficultyLevel,
            TargetDay = dto.TargetDay
        };

        foreach (var ex in dto.Exercises)
        {
            var exerciseExists = await _exerciseRepository.GetByIdAsync(ex.ExerciseId, cancellationToken);
            if (exerciseExists == null) throw new ValidationException($"Exercise with ID {ex.ExerciseId} does not exist.");

            routine.RoutineExercises.Add(new RoutineExercise
            {
                ExerciseId = ex.ExerciseId,
                Sets = ex.Sets,
                Reps = ex.Reps,
                RestTimeInSeconds = ex.RestTimeInSeconds,
                Weight = ex.Weight ?? 0
            });
        }

        var created = await _routineRepository.AddAsync(routine, cancellationToken);
        
        return new RoutineDto(
            created.Id,
            created.Name,
            created.Description,
            created.DifficultyLevel,
            created.TargetDay,
            [],
            created.CreatedAt,
            created.UpdatedAt
        );
    }
    
    public async Task DeleteRoutineAsync(Guid id, Guid userId, CancellationToken cancellationToken = default)
    {
        var routine = await _routineRepository.GetByIdAsync(id, userId, cancellationToken);
        if (routine == null) throw new NotFoundException($"Routine with ID {id} not found.");

        await _routineRepository.DeleteAsync(routine, cancellationToken);
    }

    public async Task<RoutineDto> UpdateRoutineAsync(Guid id, Guid userId, CreateRoutineDto dto, CancellationToken cancellationToken = default)
    {
        var routine = await _routineRepository.GetByIdAsync(id, userId, cancellationToken);
        if (routine == null) throw new NotFoundException($"Routine with ID {id} not found.");

        routine.Name = dto.Name;
        routine.Description = dto.Description;
        routine.DifficultyLevel = dto.DifficultyLevel;
        routine.TargetDay = dto.TargetDay;

        // Limpiar ejercicios actuales
        routine.RoutineExercises.Clear();

        foreach (var ex in dto.Exercises)
        {
            var exerciseExists = await _exerciseRepository.GetByIdAsync(ex.ExerciseId, cancellationToken);
            if (exerciseExists == null) throw new ValidationException($"Exercise with ID {ex.ExerciseId} does not exist.");

            routine.RoutineExercises.Add(new RoutineExercise
            {
                ExerciseId = ex.ExerciseId,
                Sets = ex.Sets,
                Reps = ex.Reps,
                RestTimeInSeconds = ex.RestTimeInSeconds,
                Weight = ex.Weight ?? 0
            });
        }

        await _routineRepository.UpdateAsync(routine, cancellationToken);

        return await GetRoutineByIdAsync(id, userId, cancellationToken);
    }
}
