using System;
using System.Collections.Generic;

namespace Nexodus_Back.Application.DTOs;

public record WorkoutSessionDto(
    Guid Id,
    Guid UserId,
    Guid? RoutineId,
    string? RoutineName,
    string Name,
    DateTime StartTime,
    DateTime? EndTime,
    int DurationSeconds,
    double TotalVolumeKg,
    int TotalReps,
    int CompletedSetsCount,
    int XpEarned,
    bool IsActive,
    string? Notes,
    IReadOnlyList<WorkoutSetDto> Sets
);

