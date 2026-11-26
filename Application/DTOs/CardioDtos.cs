using System;
using System.Collections.Generic;

namespace Nexodus_Back.Application.DTOs;

public record CardioSegmentDto(
    int SegmentOrder,
    string State,
    int DurationSeconds,
    DateTimeOffset StartedAt,
    DateTimeOffset EndedAt
);

public record SaveCardioSessionDto(
    string Name,
    string Environment,
    string ActivityType,
    string TrackingMode,
    DateTimeOffset StartTime,
    DateTimeOffset EndTime,
    int TotalDurationSeconds,
    double? DistanceKm,
    double? EstimatedCalories,
    bool IsAiGenerated,
    string? Notes,
    List<CardioSegmentDto> Segments
);

public record CardioSessionDto(
    Guid Id,
    string Name,
    string Environment,
    string ActivityType,
    string TrackingMode,
    DateTimeOffset StartTime,
    DateTimeOffset? EndTime,
    int TotalDurationSeconds,
    double? DistanceKm,
    double EstimatedCalories,
    bool IsAiGenerated,
    string? Notes,
    List<CardioSegmentDto> Segments
);

public record GenerateAiCardioRequestDto(
    string Environment = "Outdoor", // "Outdoor" | "Gym"
    string Modality = "Mixed",      // "Walking" | "Jogging" | "Running" | "Mixed"
    int DurationMinutes = 30,
    string DifficultyLevel = "Intermedio",
    string? Prompt = null,
    double? UserWeightKg = null
);

public record AiCardioBlockDto(
    int Order,
    string State, // "Warmup" | "Walking" | "Jogging" | "Running" | "Sprint" | "Resting" | "Cooldown"
    int DurationSeconds,
    string Label,
    string? Cue = null
);

public record GeneratedAiCardioPlanDto(
    string Name,
    string Description,
    string Environment,
    string Modality,
    int EstimatedDurationMinutes,
    double EstimatedCalories,
    string Rationale,
    List<AiCardioBlockDto> Blocks
);
