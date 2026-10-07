namespace Nexodus_Back.Application.DTOs;

public record CreateRoutineDto(
    string Name,
    string? Description,
    string? DifficultyLevel,
    int? TargetDay,
    IEnumerable<CreateRoutineExerciseDto> Exercises
);

public record CreateRoutineExerciseDto(
    Guid ExerciseId,
    int Sets,
    int Reps,
    int RestTimeInSeconds,
    double? Weight
);
