using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Nexodus_Back.Application.DTOs;
using Nexodus_Back.Application.Interfaces;
using Nexodus_Back.Core.Entities;
using Nexodus_Back.Core.Enums;
using Nexodus_Back.Core.Exceptions;
using Nexodus_Back.Core.Interfaces;

namespace Nexodus_Back.Application.Services;

/// <summary>
/// Service coordinating active workout sessions, set logging, 1RM estimations, and muscle recovery analysis.
/// </summary>
public class WorkoutSessionService(
    IWorkoutSessionRepository sessionRepository,
    IRoutineRepository routineRepository,
    IExerciseRepository exerciseRepository,
    IUserRepository userRepository,
    IGamificationService gamificationService) : IWorkoutSessionService
{
    private readonly IWorkoutSessionRepository _sessionRepository = sessionRepository ?? throw new ArgumentNullException(nameof(sessionRepository));
    private readonly IRoutineRepository _routineRepository = routineRepository ?? throw new ArgumentNullException(nameof(routineRepository));
    private readonly IExerciseRepository _exerciseRepository = exerciseRepository ?? throw new ArgumentNullException(nameof(exerciseRepository));
    private readonly IUserRepository _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
    private readonly IGamificationService _gamificationService = gamificationService ?? throw new ArgumentNullException(nameof(gamificationService));

    public async Task<WorkoutSessionDto> StartSessionAsync(StartWorkoutSessionDto dto, Guid userId, CancellationToken cancellationToken = default)
    {
        // Si el usuario ya tiene una sesión activa, la devolvemos para permitir reconexión transparente sin pérdida de progreso
        var existingActive = await _sessionRepository.GetActiveByUserIdAsync(userId, cancellationToken);
        if (existingActive != null)
        {
            return MapToDto(existingActive);
        }

        string sessionName = dto.Name?.Trim() ?? "Entrenamiento Libre";
        Routine? routine = null;

        if (dto.RoutineId.HasValue)
        {
            routine = await _routineRepository.GetByIdAsync(dto.RoutineId.Value, userId, cancellationToken);
            if (routine == null)
            {
                throw new NotFoundException($"No se encontró la rutina con ID '{dto.RoutineId.Value}'.");
            }
            if (string.IsNullOrWhiteSpace(dto.Name))
            {
                sessionName = routine.Name;
            }
        }

        var session = new WorkoutSession
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            RoutineId = dto.RoutineId,
            Name = sessionName,
            StartTime = DateTime.UtcNow,
            IsActive = true
        };

        // Si se inició desde una rutina, precargamos los ejercicios y series planificadas
        if (routine?.RoutineExercises != null && routine.RoutineExercises.Count != 0)
        {
            int globalOrder = 1;
            foreach (var re in routine.RoutineExercises)
            {
                for (int s = 1; s <= re.Sets; s++)
                {
                    session.Sets.Add(new WorkoutSet
                    {
                        Id = Guid.NewGuid(),
                        WorkoutSessionId = session.Id,
                        ExerciseId = re.ExerciseId,
                        SetOrder = globalOrder++,
                        Type = SetType.Normal,
                        WeightKg = re.Weight,
                        Reps = re.Reps,
                        CalculatedOneRepMax = CalculateOneRepMax(re.Weight, re.Reps),
                        IsCompleted = false
                    });
                }
            }
        }

        await _sessionRepository.AddAsync(session, cancellationToken);
        var created = await _sessionRepository.GetByIdAsync(session.Id, cancellationToken);
        return MapToDto(created ?? session);
    }

    public async Task<WorkoutSessionDto?> GetActiveSessionAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var session = await _sessionRepository.GetActiveByUserIdAsync(userId, cancellationToken);
        return session == null ? null : MapToDto(session);
    }

    public async Task<WorkoutSessionDto> GetSessionByIdAsync(Guid id, Guid userId, CancellationToken cancellationToken = default)
    {
        var session = await _sessionRepository.GetByIdAsync(id, cancellationToken);
        if (session == null || session.UserId != userId)
        {
            throw new NotFoundException($"No se encontró la sesión de entrenamiento con ID '{id}'.");
        }

        return MapToDto(session);
    }

    public async Task<IEnumerable<WorkoutSessionDto>> GetUserSessionsAsync(Guid userId, int limit = 30, CancellationToken cancellationToken = default)
    {
        var sessions = await _sessionRepository.GetAllByUserIdAsync(userId, limit, cancellationToken);
        return sessions.Select(MapToDto);
    }

    public async Task<WorkoutSetDto> LogSetAsync(Guid sessionId, LogWorkoutSetDto dto, Guid userId, CancellationToken cancellationToken = default)
    {
        var session = await _sessionRepository.GetByIdAsync(sessionId, cancellationToken);
        if (session == null || session.UserId != userId)
        {
            throw new NotFoundException($"No se encontró la sesión con ID '{sessionId}'.");
        }

        if (!session.IsActive)
        {
            throw new ValidationException("No se pueden registrar series en una sesión ya finalizada.");
        }

        var exercise = await _exerciseRepository.GetByIdAsync(dto.ExerciseId, cancellationToken);
        if (exercise == null)
        {
            throw new NotFoundException($"No se encontró el ejercicio con ID '{dto.ExerciseId}'.");
        }

        double calculated1Rm = CalculateOneRepMax(dto.WeightKg, dto.Reps);

        WorkoutSet? setEntity = null;

        if (dto.SetId.HasValue && dto.SetId.Value != Guid.Empty)
        {
            setEntity = session.Sets?.FirstOrDefault(s => s.Id == dto.SetId.Value) 
                ?? await _sessionRepository.GetSetByIdAsync(dto.SetId.Value, cancellationToken);
        }

        // Si no se encontró por SetId, buscar por ejercicio y orden en la sesión actual
        setEntity ??= session.Sets?.FirstOrDefault(s => s.ExerciseId == dto.ExerciseId && s.SetOrder == dto.SetOrder);

        if (setEntity != null && setEntity.WorkoutSessionId == sessionId)
        {
            setEntity.SetOrder = dto.SetOrder;
            setEntity.Type = dto.Type;
            setEntity.WeightKg = dto.WeightKg;
            setEntity.Reps = dto.Reps;
            setEntity.Rpe = dto.Rpe;
            setEntity.DurationSeconds = dto.DurationSeconds;
            setEntity.CalculatedOneRepMax = calculated1Rm;
            setEntity.IsCompleted = dto.IsCompleted;
            setEntity.CompletedAt = dto.IsCompleted ? (setEntity.CompletedAt ?? DateTime.UtcNow) : null;

            await _sessionRepository.UpdateSetAsync(setEntity, cancellationToken);
        }
        else
        {
            setEntity = new WorkoutSet
            {
                Id = Guid.NewGuid(),
                WorkoutSessionId = sessionId,
                ExerciseId = dto.ExerciseId,
                SetOrder = dto.SetOrder,
                Type = dto.Type,
                WeightKg = dto.WeightKg,
                Reps = dto.Reps,
                Rpe = dto.Rpe,
                DurationSeconds = dto.DurationSeconds,
                CalculatedOneRepMax = calculated1Rm,
                IsCompleted = dto.IsCompleted,
                CompletedAt = dto.IsCompleted ? DateTime.UtcNow : null
            };

            await _sessionRepository.AddSetAsync(setEntity, cancellationToken);
        }

        return new WorkoutSetDto(
            setEntity.Id,
            setEntity.WorkoutSessionId,
            setEntity.ExerciseId,
            exercise.Name,
            setEntity.SetOrder,
            setEntity.Type,
            setEntity.WeightKg,
            setEntity.Reps,
            setEntity.Rpe,
            setEntity.DurationSeconds,
            setEntity.CalculatedOneRepMax,
            setEntity.IsCompleted,
            setEntity.CompletedAt
        );
    }

    public async Task DeleteSetAsync(Guid sessionId, Guid setId, Guid userId, CancellationToken cancellationToken = default)
    {
        var session = await _sessionRepository.GetByIdAsync(sessionId, cancellationToken);
        if (session == null || session.UserId != userId)
        {
            throw new NotFoundException($"No se encontró la sesión con ID '{sessionId}'.");
        }

        if (!session.IsActive)
        {
            throw new ValidationException("No se pueden eliminar series de una sesión ya finalizada.");
        }

        var set = await _sessionRepository.GetSetByIdAsync(setId, cancellationToken);
        if (set == null || set.WorkoutSessionId != sessionId)
        {
            throw new NotFoundException($"No se encontró la serie con ID '{setId}'.");
        }

        await _sessionRepository.DeleteSetAsync(set, cancellationToken);
    }

    public async Task<WorkoutSessionDto> FinishSessionAsync(Guid sessionId, FinishWorkoutSessionDto dto, Guid userId, CancellationToken cancellationToken = default)
    {
        var session = await _sessionRepository.GetByIdAsync(sessionId, cancellationToken);
        if (session == null || session.UserId != userId)
        {
            throw new NotFoundException($"No se encontró la sesión con ID '{sessionId}'.");
        }

        if (!session.IsActive)
        {
            throw new ValidationException("La sesión ya fue finalizada anteriormente.");
        }

        DateTime now = DateTime.UtcNow;
        session.EndTime = now;
        session.DurationSeconds = (int)Math.Max(0, (now - session.StartTime).TotalSeconds);
        session.IsActive = false;
        if (!string.IsNullOrWhiteSpace(dto.Notes))
        {
            session.Notes = dto.Notes.Trim();
        }

        // Métricas de volumen, repeticiones y XP
        var completedSets = session.Sets.Where(s => s.IsCompleted).ToList();
        session.CompletedSetsCount = completedSets.Count;
        session.TotalReps = completedSets.Sum(s => s.Reps);

        // Volumen excluyendo series de calentamiento (Warmup)
        double totalVolume = completedSets
            .Where(s => s.Type != SetType.Warmup)
            .Sum(s => s.WeightKg * s.Reps);
        session.TotalVolumeKg = Math.Round(totalVolume, 2);

        // NXP: 50 base XP + 1 XP por cada 100 kg levantados
        session.XpEarned = 50 + (int)(session.TotalVolumeKg / 100.0);

        await _sessionRepository.UpdateAsync(session, cancellationToken);

        // Registro de actividad y progreso de racha en The Nexodus Protocol
        await _gamificationService.RecordActivityAsync(
            userId,
            "Workout",
            baseNxp: 50,
            bonusNxp: (int)(session.TotalVolumeKg / 100.0),
            cancellationToken: cancellationToken
        );

        return MapToDto(session);
    }

    public async Task CancelSessionAsync(Guid sessionId, Guid userId, CancellationToken cancellationToken = default)
    {
        var session = await _sessionRepository.GetByIdAsync(sessionId, cancellationToken);
        if (session == null || session.UserId != userId)
        {
            throw new NotFoundException($"No se encontró la sesión con ID '{sessionId}'.");
        }

        await _sessionRepository.DeleteAsync(session, cancellationToken);
    }

    public async Task<IEnumerable<MuscleRecoveryDto>> GetMuscleRecoveryStatusAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var standardMuscles = new[] { "Pecho", "Espalda", "Piernas", "Hombros", "Bíceps", "Tríceps", "Core" };
        var since = DateTime.UtcNow.AddDays(-5);
        var recentSessions = await _sessionRepository.GetRecentCompletedSessionsAsync(userId, since, cancellationToken);

        var muscleLastTrained = new Dictionary<string, DateTime>(StringComparer.OrdinalIgnoreCase);

        foreach (var sess in recentSessions)
        {
            foreach (var set in sess.Sets.Where(s => s.IsCompleted && s.Exercise != null))
            {
                var muscle = NormalizeMuscleGroup(set.Exercise.Muscle);
                if (muscle == "Otros" && !string.IsNullOrEmpty(set.Exercise.BodyPart))
                {
                    muscle = NormalizeMuscleGroup(set.Exercise.BodyPart);
                }

                if (!string.IsNullOrEmpty(muscle) && muscle != "Otros")
                {
                    var time = set.CompletedAt ?? sess.EndTime ?? sess.StartTime;
                    if (!muscleLastTrained.TryGetValue(muscle, out var lastTime) || time > lastTime)
                    {
                        muscleLastTrained[muscle] = time;
                    }
                }
            }
        }

        var results = new List<MuscleRecoveryDto>();
        DateTime now = DateTime.UtcNow;

        foreach (var group in standardMuscles)
        {
            if (muscleLastTrained.TryGetValue(group, out var lastTrained))
            {
                int hoursSince = (int)Math.Max(0, (now - lastTrained).TotalHours);
                double recoveryPct = CalculateRecoveryPercentage(hoursSince);
                string status = recoveryPct >= 95 ? "Optimal" : recoveryPct >= 65 ? "Recovering" : "Fatigued";

                results.Add(new MuscleRecoveryDto(group, recoveryPct, status, hoursSince));
            }
            else
            {
                results.Add(new MuscleRecoveryDto(group, 100.0, "Optimal", 999));
            }
        }

        return results;
    }

    public async Task<ExerciseRecordDto> GetExerciseRecordAsync(Guid exerciseId, Guid userId, CancellationToken cancellationToken = default)
    {
        var exercise = await _exerciseRepository.GetByIdAsync(exerciseId, cancellationToken);
        if (exercise == null)
        {
            throw new NotFoundException($"No se encontró el ejercicio con ID '{exerciseId}'.");
        }

        double pb1Rm = await _sessionRepository.GetExercisePersonalBest1RMAsync(userId, exerciseId, cancellationToken);
        var historySets = await _sessionRepository.GetExerciseHistoryAsync(userId, exerciseId, 20, cancellationToken);

        var historyList = historySets.ToList();
        double maxWeight = historyList.Count != 0 ? historyList.Max(s => s.WeightKg) : 0;
        int maxReps = historyList.Count != 0 ? historyList.Max(s => s.Reps) : 0;

        var setDtos = historyList.Select(s => new WorkoutSetDto(
            s.Id,
            s.WorkoutSessionId,
            s.ExerciseId,
            exercise.Name,
            s.SetOrder,
            s.Type,
            s.WeightKg,
            s.Reps,
            s.Rpe,
            s.DurationSeconds,
            s.CalculatedOneRepMax,
            s.IsCompleted,
            s.CompletedAt
        )).ToList();

        return new ExerciseRecordDto(
            exercise.Id,
            exercise.Name,
            Math.Round(pb1Rm, 1),
            Math.Round(maxWeight, 1),
            maxReps,
            setDtos
        );
    }

    private static double CalculateOneRepMax(double weightKg, int reps)
    {
        if (weightKg <= 0 || reps <= 0) return 0;
        if (reps == 1) return weightKg;
        // Fórmula combinada de Epley: Peso * (1 + Reps / 30)
        return Math.Round(weightKg * (1.0 + (reps / 30.0)), 1);
    }

    private static double CalculateRecoveryPercentage(int hoursSince)
    {
        if (hoursSince >= 72) return 100.0;
        if (hoursSince <= 0) return 20.0;
        // Curva suave de recuperación biológica muscular de 0 a 72 horas
        double progress = hoursSince / 72.0;
        double pct = 20.0 + (80.0 * Math.Pow(progress, 0.85));
        return Math.Round(Math.Min(100.0, pct), 1);
    }

    private static string NormalizeMuscleGroup(string? raw)
    {
        if (string.IsNullOrWhiteSpace(raw)) return "Otros";
        string lower = raw.Trim().ToLowerInvariant();

        // Pecho
        if (lower.Contains("chest") || lower.Contains("pecho") || lower.Contains("pectoral"))
            return "Pecho";

        // Espalda
        if (lower.Contains("back") || lower.Contains("espalda") || lower.Contains("lat") || lower.Contains("dorsal") ||
            lower.Contains("trap") || lower.Contains("spine") || lower.Contains("scapulae"))
            return "Espalda";

        // Hombros
        if (lower.Contains("shoulder") || lower.Contains("hombro") || lower.Contains("delt"))
            return "Hombros";

        // Bíceps / Brazos / Antebrazos
        if (lower.Contains("bicep") || lower.Contains("bíceps") || lower.Contains("braquial") ||
            lower.Contains("forearm") || lower.Contains("antebrazo") || lower.Contains("arm"))
            return "Bíceps";

        // Tríceps
        if (lower.Contains("tricep") || lower.Contains("tríceps"))
            return "Tríceps";

        // Piernas
        if (lower.Contains("leg") || lower.Contains("pierna") || lower.Contains("quad") || lower.Contains("cuádricep") ||
            lower.Contains("glute") || lower.Contains("glúteo") || lower.Contains("hamstring") || lower.Contains("femoral") ||
            lower.Contains("calf") || lower.Contains("calves") || lower.Contains("pantorrilla") || lower.Contains("gemelo") ||
            lower.Contains("abductor") || lower.Contains("adductor"))
            return "Piernas";

        // Core / Abdomen
        if (lower.Contains("abs") || lower.Contains("core") || lower.Contains("abdominal") ||
            lower.Contains("oblique") || lower.Contains("oblicuo") || lower.Contains("waist") ||
            lower.Contains("cintura") || lower.Contains("serratus"))
            return "Core";

        return "Otros";
    }

    private static WorkoutSessionDto MapToDto(WorkoutSession s)
    {
        var sets = s.Sets?
            .OrderBy(set => set.SetOrder)
            .Select(set => new WorkoutSetDto(
                set.Id,
                set.WorkoutSessionId,
                set.ExerciseId,
                set.Exercise?.Name ?? "Ejercicio",
                set.SetOrder,
                set.Type,
                set.WeightKg,
                set.Reps,
                set.Rpe,
                set.DurationSeconds,
                set.CalculatedOneRepMax,
                set.IsCompleted,
                set.CompletedAt
            ))
            .ToList() ?? [];

        return new WorkoutSessionDto(
            s.Id,
            s.UserId,
            s.RoutineId,
            s.Routine?.Name,
            s.Name,
            s.StartTime,
            s.EndTime,
            s.DurationSeconds,
            s.TotalVolumeKg,
            s.TotalReps,
            s.CompletedSetsCount,
            s.XpEarned,
            s.IsActive,
            s.Notes,
            sets
        );
    }
}
