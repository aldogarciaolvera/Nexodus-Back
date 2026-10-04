namespace Nexodus_Back.Application.DTOs;

public record RoutineDto(
    Guid Id,
    string Name,
    string? Description,
    string? DifficultyLevel,
    IEnumerable<RoutineExerciseDto> Exercises,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt
);

public record RoutineExerciseDto(
    Guid ExerciseId,
    int Sets,
    int Reps,
    int RestTimeInSeconds,
    ExerciseDto? ExerciseDetails
);
