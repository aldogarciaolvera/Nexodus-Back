namespace Nexodus_Back.Application.DTOs;

public record MuscleRecoveryDto(
    string MuscleGroup,
    double RecoveryPercentage,
    string Status,
    int HoursSinceLastTrained
);

