using System;

namespace Nexodus_Back.Application.DTOs
{
    public record WorkoutLogDto
    {
        public Guid Id { get; init; }
        public Guid RoutineId { get; init; }
        public int DurationInSeconds { get; init; }
        public int CompletedExercisesCount { get; init; }
        public DateTime DateCompleted { get; init; }
        public DateTime CreatedAt { get; init; }
    }
}
