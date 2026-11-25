using System;
using System.Collections.Generic;

namespace Nexodus_Back.Application.DTOs;

public record GenerateAiRoutineRequestDto(
    string? Prompt,
    string? RoutineName,
    string Goal = "Hypertrophy",
    string DifficultyLevel = "Intermedio",
    int DurationMinutes = 60,
    List<string>? AvailableEquipment = null,
    bool ConsiderMuscleRecovery = false,
    List<string>? AvoidMuscles = null,
    double? UserWeightKg = null,
    double? UserHeightM = null,
    List<Guid>? ExcludedExerciseIds = null,
    int? WarmupMinutes = null,
    bool IncludeCardio = false,
    int? CardioMinutes = null,
    string? CardioType = null
);

public record SwapAiExerciseRequestDto(
    Guid ExerciseIdToReplace,
    List<Guid> CurrentRoutineExerciseIds,
    List<Guid>? ExcludedExerciseIds = null,
    List<string>? AvailableEquipment = null,
    string Goal = "Hypertrophy",
    string DifficultyLevel = "Intermedio",
    string? RoutineName = null
);

public record GeneratedAiRoutineResponseDto(
    string Name,
    string Description,
    string DifficultyLevel,
    int EstimatedDurationMinutes,
    string Rationale,
    List<GeneratedAiRoutineExerciseDto> Exercises,
    int? WarmupMinutes = null,
    List<WarmupExerciseDto>? WarmupExercises = null,
    bool IncludeCardio = false,
    int? CardioMinutes = null,
    string? CardioType = null
);

public record GeneratedAiRoutineExerciseDto(
    Guid ExerciseId,
    string Name,
    int Sets,
    int Reps,
    int RestTimeInSeconds,
    string? Notes,
    string? Muscle,
    string? BodyPart,
    string? Equipment,
    string? ThumbUrl
);
