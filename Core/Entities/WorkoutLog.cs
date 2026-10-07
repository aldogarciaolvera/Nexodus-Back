using System;

namespace Nexodus_Back.Core.Entities
{
    public class WorkoutLog
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid UserId { get; set; }
        public Guid RoutineId { get; set; }
        public int DurationInSeconds { get; set; }
        public int CompletedExercisesCount { get; set; }
        public DateTime DateCompleted { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Propiedades de navegación
        public User User { get; set; } = null!;
        public Routine Routine { get; set; } = null!;
    }
}
