using Nexodus_Back.Application.DTOs;
using Nexodus_Back.Core.Entities;
using Nexodus_Back.Core.Exceptions;
using Nexodus_Back.Core.Interfaces;

namespace Nexodus_Back.Application.Services;

public class RoutineService
{
    private readonly IRoutineRepository _routineRepository;
    private readonly IExerciseRepository _exerciseRepository;
    private readonly IStorageService _storageService;

    public RoutineService(IRoutineRepository routineRepository, IExerciseRepository exerciseRepository, IStorageService storageService)
    {
        _routineRepository = routineRepository;
        _exerciseRepository = exerciseRepository;
        _storageService = storageService;
    }

    public async Task<IEnumerable<RoutineDto>> GetAllRoutinesAsync(Guid userId)
    {
        var routines = await _routineRepository.GetAllByUserIdAsync(userId);
        
        return routines.Select(r => new RoutineDto(
            r.Id,
            r.Name,
            r.Description,
            r.DifficultyLevel,
            r.TargetDay,
            new List<RoutineExerciseDto>(), // Sin detalle para listado general por rendimiento
            r.CreatedAt,
            r.UpdatedAt
        ));
    }

    public async Task<RoutineDto> GetRoutineByIdAsync(Guid id, Guid userId)
    {
        var routine = await _routineRepository.GetByIdAsync(id, userId);
        if (routine == null) throw new NotFoundException($"Routine with ID {id} not found.");

        var exercisesList = new List<RoutineExerciseDto>();
        foreach (var re in routine.RoutineExercises)
        {
            string? gifUrl = null, thumbUrl = null;
            if (re.Exercise != null)
            {
                gifUrl = string.IsNullOrEmpty(re.Exercise.GifS3Key) ? null : await _storageService.GetFileUrlAsync(re.Exercise.GifS3Key);
                thumbUrl = string.IsNullOrEmpty(re.Exercise.ThumbS3Key) ? null : await _storageService.GetFileUrlAsync(re.Exercise.ThumbS3Key);
            }

            var exDto = re.Exercise == null ? null : new ExerciseDto(
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

            exercisesList.Add(new RoutineExerciseDto(
                re.ExerciseId,
                re.Sets,
                re.Reps,
                re.RestTimeInSeconds,
                re.Weight,
                exDto
            ));
        }

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

    public async Task<RoutineDto> CreateRoutineAsync(Guid userId, CreateRoutineDto dto)
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
            var exerciseExists = await _exerciseRepository.GetByIdAsync(ex.ExerciseId);
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

        var created = await _routineRepository.AddAsync(routine);
        
        return new RoutineDto(
            created.Id,
            created.Name,
            created.Description,
            created.DifficultyLevel,
            created.TargetDay,
            new List<RoutineExerciseDto>(),
            created.CreatedAt,
            created.UpdatedAt
        );
    }
    
    public async Task DeleteRoutineAsync(Guid id, Guid userId)
    {
        var routine = await _routineRepository.GetByIdAsync(id, userId);
        if (routine == null) throw new NotFoundException($"Routine with ID {id} not found.");

        await _routineRepository.DeleteAsync(routine);
    }

    public async Task<RoutineDto> UpdateRoutineAsync(Guid id, Guid userId, CreateRoutineDto dto)
    {
        var routine = await _routineRepository.GetByIdAsync(id, userId);
        if (routine == null) throw new NotFoundException($"Routine with ID {id} not found.");

        routine.Name = dto.Name;
        routine.Description = dto.Description;
        routine.DifficultyLevel = dto.DifficultyLevel;
        routine.TargetDay = dto.TargetDay;

        // Limpiar ejercicios actuales
        routine.RoutineExercises.Clear();

        foreach (var ex in dto.Exercises)
        {
            var exerciseExists = await _exerciseRepository.GetByIdAsync(ex.ExerciseId);
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

        await _routineRepository.UpdateAsync(routine);

        return await GetRoutineByIdAsync(id, userId);
    }
}
