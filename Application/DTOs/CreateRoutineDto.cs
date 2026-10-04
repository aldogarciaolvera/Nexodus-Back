namespace Nexodus_Back.Application.DTOs;

public record CreateRoutineDto(
    string Name,
    string? Description,
    string? DifficultyLevel,
    IEnumerable<CreateRoutineExerciseDto> Exercises
);

public record CreateRoutineExerciseDto(
    Guid ExerciseId,
    int Sets,
    int Reps,
    int RestTimeInSeconds
);
