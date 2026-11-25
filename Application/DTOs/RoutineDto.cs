using System;
using System.Collections.Generic;

namespace Nexodus_Back.Application.DTOs;

public record WarmupExerciseDto(
    string Name,
    int DurationSeconds,
    string? Focus = null
);

public record RoutineDto(
    Guid Id,
    string Name,
    string? Description,
    string? DifficultyLevel,
    int? TargetDay,
    IEnumerable<RoutineExerciseDto> Exercises,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt,
    bool IsAiGenerated = false,
    int? WarmupMinutes = null,
    List<WarmupExerciseDto>? WarmupExercises = null,
    bool IncludeCardio = false,
    int? CardioMinutes = null,
    string? CardioType = null,
    string RoutineType = "Strength",
    bool IsTemplate = false,
    string? Purpose = null
);

public record RoutineExerciseDto(
    Guid ExerciseId,
    int Sets,
    int Reps,
    int RestTimeInSeconds,
    double Weight,
    ExerciseDto? ExerciseDetails
);
