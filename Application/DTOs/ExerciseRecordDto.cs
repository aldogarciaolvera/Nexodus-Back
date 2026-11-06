using System;
using System.Collections.Generic;

namespace Nexodus_Back.Application.DTOs;

public record ExerciseRecordDto(
    Guid ExerciseId,
    string ExerciseName,
    double PersonalBest1RM,
    double MaxWeightLifted,
    int MaxRepsRecord,
    IReadOnlyList<WorkoutSetDto> RecentSets
);

