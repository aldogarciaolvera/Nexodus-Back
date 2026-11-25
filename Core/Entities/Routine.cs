namespace Nexodus_Back.Core.Entities;

public class Routine
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid UserId { get; set; }
    
    public string Name { get; set; } = null!;
    public string? Description { get; set; }
    public string? DifficultyLevel { get; set; }
    public int? TargetDay { get; set; }
    public bool IsAiGenerated { get; set; } = false;
    public int? WarmupMinutes { get; set; }
    public string? WarmupExercisesJson { get; set; }
    public bool IncludeCardio { get; set; } = false;
    public int? CardioMinutes { get; set; }
    public string? CardioType { get; set; }
    public string RoutineType { get; set; } = "Strength"; // "Strength" | "Stretching"
    public bool IsTemplate { get; set; } = false;
    public string? Purpose { get; set; }
    
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.UtcNow;

    public ICollection<RoutineExercise> RoutineExercises { get; set; } = new List<RoutineExercise>();
}
