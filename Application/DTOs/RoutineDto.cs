namespace Nexodus_Back.Application.DTOs;

public record RoutineDto(
    Guid Id,
    string Name,
    string? Description,
    string? DifficultyLevel,
    int? TargetDay,
    IEnumerable<RoutineExerciseDto> Exercises,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt
);

public record RoutineExerciseDto(
    Guid ExerciseId,
    int Sets,
    int Reps,
    int RestTimeInSeconds,
    double Weight,
    ExerciseDto? ExerciseDetails
);
