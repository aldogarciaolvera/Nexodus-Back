using System;
using Nexodus_Back.Core.Enums;

namespace Nexodus_Back.Application.DTOs;

public record LogWorkoutSetDto(
    Guid? SetId,
    Guid ExerciseId,
    int SetOrder,
    SetType Type,
    double WeightKg,
    int Reps,
    double? Rpe,
    int? DurationSeconds,
    bool IsCompleted
);

