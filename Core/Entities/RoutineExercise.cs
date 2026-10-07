namespace Nexodus_Back.Core.Entities;

public class RoutineExercise
{
    public Guid RoutineId { get; set; }
    public Routine Routine { get; set; } = null!;

    public Guid ExerciseId { get; set; }
    public Exercise Exercise { get; set; } = null!;

    public int Sets { get; set; } = 1;
    public int Reps { get; set; } = 1;
    public int RestTimeInSeconds { get; set; } = 60;
    public double Weight { get; set; } = 0;
}
