namespace Nexodus_Back.Core.Entities;

public class Exercise
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string ExternalId { get; set; } = null!; // Maps to "id" in JSON (e.g., "abductors/lever-seated-hip-abduction")
    public string Name { get; set; } = null!;
    public string? BodyPart { get; set; }
    public string? Muscle { get; set; }
    public string? Equipment { get; set; }
    public string? Category { get; set; }
    public string? Instructions { get; set; } // Can store as JSON string or text separated by \n
    public string? GifS3Key { get; set; } // Maps to "file" in JSON
    public string? ThumbS3Key { get; set; }
}
