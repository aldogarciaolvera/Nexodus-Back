using System;

namespace Nexodus_Back.Application.DTOs;

public record StartWorkoutSessionDto(
    string? Name,
    Guid? RoutineId
);

