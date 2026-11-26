using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
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

    private static List<WarmupExerciseDto>? DeserializeWarmup(string? json)
    {
        if (string.IsNullOrWhiteSpace(json)) return null;
        try
        {
            return JsonSerializer.Deserialize<List<WarmupExerciseDto>>(json);
        }
        catch
        {
            return null;
        }
    }

    private static string? SerializeWarmup(List<WarmupExerciseDto>? list)
    {
        if (list == null || list.Count == 0) return null;
        return JsonSerializer.Serialize(list);
    }

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
            r.UpdatedAt,
            r.IsAiGenerated,
            r.WarmupMinutes,
            DeserializeWarmup(r.WarmupExercisesJson),
            r.IncludeCardio,
            r.CardioMinutes,
            r.CardioType,
            r.RoutineType,
            r.IsTemplate,
            r.Purpose
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
            routine.UpdatedAt,
            routine.IsAiGenerated,
            routine.WarmupMinutes,
            DeserializeWarmup(routine.WarmupExercisesJson),
            routine.IncludeCardio,
            routine.CardioMinutes,
            routine.CardioType,
            routine.RoutineType,
            routine.IsTemplate,
            routine.Purpose
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
            TargetDay = dto.TargetDay,
            IsAiGenerated = dto.IsAiGenerated,
            WarmupMinutes = dto.WarmupMinutes,
            WarmupExercisesJson = SerializeWarmup(dto.WarmupExercises),
            IncludeCardio = dto.IncludeCardio,
            CardioMinutes = dto.IncludeCardio ? dto.CardioMinutes : null,
            CardioType = dto.IncludeCardio ? dto.CardioType : null,
            RoutineType = string.IsNullOrWhiteSpace(dto.RoutineType) ? "Strength" : dto.RoutineType,
            IsTemplate = false,
            Purpose = dto.Purpose
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
            created.UpdatedAt,
            created.IsAiGenerated,
            created.WarmupMinutes,
            DeserializeWarmup(created.WarmupExercisesJson),
            created.IncludeCardio,
            created.CardioMinutes,
            created.CardioType,
            created.RoutineType,
            created.IsTemplate,
            created.Purpose
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
        routine.WarmupMinutes = dto.WarmupMinutes;
        routine.WarmupExercisesJson = SerializeWarmup(dto.WarmupExercises);
        routine.IncludeCardio = dto.IncludeCardio;
        routine.CardioMinutes = dto.IncludeCardio ? dto.CardioMinutes : null;
        routine.CardioType = dto.IncludeCardio ? dto.CardioType : null;
        if (!string.IsNullOrWhiteSpace(dto.RoutineType))
        {
            routine.RoutineType = dto.RoutineType;
        }
        routine.Purpose = dto.Purpose;
        // Once a routine is marked as AI-generated, it remains flagged as AI-generated
        if (!routine.IsAiGenerated && dto.IsAiGenerated)
        {
            routine.IsAiGenerated = true;
        }

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

    private sealed record StretchingTemplateSpec(
        string Key,
        string Name,
        string Description,
        string Purpose,
        string Difficulty,
        List<(string ExternalId, int Sets, int HoldSeconds, int RestSeconds)> Items);

    private static readonly List<StretchingTemplateSpec> StretchingTemplates =
    [
        new(
            "night-relax",
            "Relajación Nocturna",
            "Secuencia suave de 10 minutos para liberar tensión acumulada y mejorar el descanso.",
            "Relajación",
            "Principiante",
            [
                ("stretching/neck-side-stretch", 2, 30, 10),
                ("stretching/cross-body-shoulder-stretch", 2, 30, 10),
                ("stretching/childs-pose", 2, 45, 15),
                ("stretching/supine-spinal-twist", 2, 40, 10),
                ("stretching/seated-butterfly", 2, 45, 10)
            ]),
        new(
            "post-leg-day",
            "Post-Entreno Pierna y Cadera",
            "Descarga profunda para cuádriceps, isquiotibiales, glúteos y gemelos tras entrenar pierna.",
            "Post-Entreno",
            "Intermedio",
            [
                ("stretching/standing-quad-stretch", 2, 35, 10),
                ("stretching/seated-hamstring-stretch", 2, 40, 10),
                ("stretching/kneeling-hip-flexor", 2, 40, 10),
                ("stretching/pigeon-pose", 2, 45, 15),
                ("stretching/figure-four-glute", 2, 40, 10),
                ("stretching/wall-calf-stretch", 2, 30, 10)
            ]),
        new(
            "office-back-relief",
            "Alivio de Espalda y Oficina",
            "Movilidad rápida para contrarrestar horas sentado: columna, pecho, cuello y muñecas.",
            "Alivio de Espalda",
            "Principiante",
            [
                ("stretching/cat-cow-mobility", 2, 45, 10),
                ("stretching/chest-doorway-opener", 2, 35, 10),
                ("stretching/childs-pose", 2, 45, 10),
                ("stretching/supine-spinal-twist", 2, 35, 10),
                ("stretching/neck-side-stretch", 2, 30, 10),
                ("stretching/wrist-flexor-stretch", 2, 30, 10)
            ]),
        new(
            "full-body-mobility",
            "Movilidad Full Body",
            "Rutina integral de flexibilidad de cuerpo completo para días de recuperación activa.",
            "Movilidad",
            "Intermedio",
            [
                ("stretching/cat-cow-mobility", 2, 45, 10),
                ("stretching/kneeling-lat-stretch", 2, 40, 10),
                ("stretching/chest-doorway-opener", 2, 35, 10),
                ("stretching/kneeling-hip-flexor", 2, 40, 10),
                ("stretching/pigeon-pose", 2, 45, 10),
                ("stretching/downward-dog", 2, 45, 15)
            ])
    ];

    public async Task<List<RoutineDto>> GetStretchingTemplatesAsync(CancellationToken cancellationToken = default)
    {
        var allExercises = await _exerciseRepository.GetAllCatalogAsync(cancellationToken);
        var byExternalId = allExercises
            .Where(e => !string.IsNullOrWhiteSpace(e.ExternalId))
            .ToDictionary(e => e.ExternalId, e => e, StringComparer.OrdinalIgnoreCase);

        var result = new List<RoutineDto>();
        foreach (var spec in StretchingTemplates)
        {
            var exercises = new List<RoutineExerciseDto>();
            foreach (var item in spec.Items)
            {
                if (!byExternalId.TryGetValue(item.ExternalId, out var ex)) continue;
                var gifUrl = string.IsNullOrEmpty(ex.GifS3Key) ? null : await _storageService.GetFileUrlAsync(ex.GifS3Key);
                var thumbUrl = string.IsNullOrEmpty(ex.ThumbS3Key) ? null : await _storageService.GetFileUrlAsync(ex.ThumbS3Key);

                exercises.Add(new RoutineExerciseDto(
                    ex.Id,
                    item.Sets,
                    item.HoldSeconds,
                    item.RestSeconds,
                    0,
                    new ExerciseDto(
                        ex.Id,
                        ex.ExternalId,
                        ex.Name,
                        ex.BodyPart,
                        ex.Muscle,
                        ex.Equipment,
                        ex.Category,
                        ex.Instructions,
                        gifUrl,
                        thumbUrl)));
            }

            // Deterministic GUID derived from template key so frontend keys stay stable
            var templateGuid = CreateDeterministicGuid(spec.Key);
            result.Add(new RoutineDto(
                Id: templateGuid,
                Name: spec.Name,
                Description: spec.Description,
                DifficultyLevel: spec.Difficulty,
                TargetDay: null,
                Exercises: exercises,
                CreatedAt: DateTimeOffset.UtcNow,
                UpdatedAt: DateTimeOffset.UtcNow,
                IsAiGenerated: false,
                RoutineType: "Stretching",
                IsTemplate: true,
                Purpose: $"{spec.Purpose}|{spec.Key}"
            ));
        }

        return result;
    }

    public async Task<RoutineDto> CloneStretchingTemplateAsync(string templateKey, Guid userId, CancellationToken cancellationToken = default)
    {
        var spec = StretchingTemplates.FirstOrDefault(t =>
            string.Equals(t.Key, templateKey, StringComparison.OrdinalIgnoreCase));
        if (spec == null)
        {
            throw new NotFoundException($"Plantilla de estiramiento '{templateKey}' no encontrada.");
        }

        var allExercises = await _exerciseRepository.GetAllCatalogAsync(cancellationToken);
        var byExternalId = allExercises
            .Where(e => !string.IsNullOrWhiteSpace(e.ExternalId))
            .ToDictionary(e => e.ExternalId, e => e, StringComparer.OrdinalIgnoreCase);

        var routine = new Routine
        {
            UserId = userId,
            Name = spec.Name,
            Description = spec.Description,
            DifficultyLevel = spec.Difficulty,
            RoutineType = "Stretching",
            IsTemplate = false,
            Purpose = spec.Purpose,
            IsAiGenerated = false
        };

        foreach (var item in spec.Items)
        {
            if (!byExternalId.TryGetValue(item.ExternalId, out var ex)) continue;
            routine.RoutineExercises.Add(new RoutineExercise
            {
                ExerciseId = ex.Id,
                Sets = item.Sets,
                Reps = item.HoldSeconds,
                RestTimeInSeconds = item.RestSeconds,
                Weight = 0
            });
        }

        var created = await _routineRepository.AddAsync(routine, cancellationToken);
        return await GetRoutineByIdAsync(created.Id, userId, cancellationToken);
    }

    private static Guid CreateDeterministicGuid(string key)
    {
        var bytes = System.Security.Cryptography.MD5.HashData(System.Text.Encoding.UTF8.GetBytes($"nexodus-stretch-{key}"));
        return new Guid(bytes);
    }
}
