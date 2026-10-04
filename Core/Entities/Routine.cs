namespace Nexodus_Back.Core.Entities;

public class Routine
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid UserId { get; set; }
    
    public string Name { get; set; } = null!;
    public string? Description { get; set; }
    public string? DifficultyLevel { get; set; }
    
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.UtcNow;

    public ICollection<RoutineExercise> RoutineExercises { get; set; } = new List<RoutineExercise>();
}
