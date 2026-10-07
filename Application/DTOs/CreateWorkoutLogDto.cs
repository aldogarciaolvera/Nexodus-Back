using System;

namespace Nexodus_Back.Application.DTOs
{
    public record CreateWorkoutLogDto(
        Guid RoutineId,
        int DurationInSeconds,
        int CompletedExercisesCount,
        DateTime DateCompleted
    );
}
