namespace Nexodus_Back.Application.DTOs;

public record ExerciseDto(
    Guid Id,
    string ExternalId,
    string Name,
    string? BodyPart,
    string? Muscle,
    string? Equipment,
    string? Category,
    string? Instructions,
    string? GifUrl,
    string? ThumbUrl
);
