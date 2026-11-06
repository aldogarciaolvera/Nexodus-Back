using System;
using Nexodus_Back.Core.Enums;

namespace Nexodus_Back.Application.DTOs;

public record WorkoutSetDto(
    Guid Id,
    Guid WorkoutSessionId,
    Guid ExerciseId,
    string ExerciseName,
    int SetOrder,
    SetType Type,
    double WeightKg,
    int Reps,
    double? Rpe,
    int? DurationSeconds,
    double CalculatedOneRepMax,
    bool IsCompleted,
    DateTime? CompletedAt
);

