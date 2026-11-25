using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Nexodus_Back.Application.DTOs;
using Nexodus_Back.Application.Interfaces;
using Nexodus_Back.Core.Entities;
using Nexodus_Back.Core.Exceptions;
using Nexodus_Back.Core.Interfaces;

namespace Nexodus_Back.Application.Services;

/// <summary>
/// Manages dedicated cardio sessions (outdoor/gym) with live state segments.
/// Cardio sessions are optional and do not alter the weight-training Gym streak.
/// </summary>
public class CardioService(ICardioRepository cardioRepository, IUserRepository userRepository) : ICardioService
{
    private readonly ICardioRepository _cardioRepository = cardioRepository ?? throw new ArgumentNullException(nameof(cardioRepository));
    private readonly IUserRepository _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));

    public async Task<CardioSessionDto> SaveSessionAsync(Guid userId, SaveCardioSessionDto dto, CancellationToken cancellationToken = default)
    {
        if (dto.TotalDurationSeconds <= 0)
        {
            throw new ValidationException("La duración de la sesión de cardio debe ser mayor a cero.");
        }

        var user = await _userRepository.GetByIdAsync(userId);
        double weightKg = (user?.Weight is double w and > 20 and < 300) ? w : 70.0;

        double estimatedCals = dto.EstimatedCalories is > 0
            ? Math.Round(dto.EstimatedCalories.Value, 1)
            : EstimateCaloriesFromSegments(dto.Segments, dto.TotalDurationSeconds, weightKg);

        var session = new CardioSession
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Name = string.IsNullOrWhiteSpace(dto.Name) ? "Sesión de Cardio" : dto.Name.Trim(),
            Environment = string.IsNullOrWhiteSpace(dto.Environment) ? "Outdoor" : dto.Environment.Trim(),
            ActivityType = string.IsNullOrWhiteSpace(dto.ActivityType) ? "Mixed" : dto.ActivityType.Trim(),
            TrackingMode = string.IsNullOrWhiteSpace(dto.TrackingMode) ? "Manual" : dto.TrackingMode.Trim(),
            StartTime = dto.StartTime,
            EndTime = dto.EndTime,
            TotalDurationSeconds = dto.TotalDurationSeconds,
            DistanceKm = dto.DistanceKm is > 0 ? Math.Round(dto.DistanceKm.Value, 2) : null,
            EstimatedCalories = estimatedCals,
            IsAiGenerated = dto.IsAiGenerated,
            Notes = string.IsNullOrWhiteSpace(dto.Notes) ? null : dto.Notes.Trim()
        };

        int order = 1;
        foreach (var seg in dto.Segments ?? [])
        {
            if (seg.DurationSeconds <= 0) continue;
            session.Segments.Add(new CardioSegment
            {
                Id = Guid.NewGuid(),
                CardioSessionId = session.Id,
                SegmentOrder = seg.SegmentOrder > 0 ? seg.SegmentOrder : order++,
                State = string.IsNullOrWhiteSpace(seg.State) ? "Jogging" : seg.State.Trim(),
                DurationSeconds = seg.DurationSeconds,
                StartedAt = seg.StartedAt,
                EndedAt = seg.EndedAt
            });
        }

        var saved = await _cardioRepository.AddAsync(session, cancellationToken);
        return MapToDto(saved);
    }

    public async Task<List<CardioSessionDto>> GetUserSessionsAsync(Guid userId, int limit = 20, CancellationToken cancellationToken = default)
    {
        var sessions = await _cardioRepository.GetByUserIdAsync(userId, limit, cancellationToken);
        return sessions.Select(MapToDto).ToList();
    }

    public async Task DeleteSessionAsync(Guid id, Guid userId, CancellationToken cancellationToken = default)
    {
        var session = await _cardioRepository.GetByIdAsync(id, userId, cancellationToken);
        if (session == null) throw new NotFoundException($"Sesión de cardio {id} no encontrada.");
        await _cardioRepository.DeleteAsync(session, cancellationToken);
    }

    private static double EstimateCaloriesFromSegments(List<CardioSegmentDto>? segments, int fallbackSeconds, double weightKg)
    {
        if (segments == null || segments.Count == 0)
        {
            return Math.Round((7.0 * 3.5 * weightKg / 200.0) * (fallbackSeconds / 60.0), 1);
        }

        double total = 0;
        foreach (var seg in segments)
        {
            double met = seg.State.ToLowerInvariant() switch
            {
                "warmup" or "cooldown" => 3.5,
                "walking" => 3.8,
                "jogging" => 7.0,
                "running" => 9.8,
                "sprint" => 12.5,
                "resting" => 1.5,
                _ => 6.0
            };
            total += (met * 3.5 * weightKg / 200.0) * (seg.DurationSeconds / 60.0);
        }

        return Math.Round(total, 1);
    }

    private static CardioSessionDto MapToDto(CardioSession s) => new(
        s.Id,
        s.Name,
        s.Environment,
        s.ActivityType,
        s.TrackingMode,
        s.StartTime,
        s.EndTime,
        s.TotalDurationSeconds,
        s.DistanceKm,
        s.EstimatedCalories,
        s.IsAiGenerated,
        s.Notes,
        s.Segments.OrderBy(seg => seg.SegmentOrder).Select(seg => new CardioSegmentDto(
            seg.SegmentOrder,
            seg.State,
            seg.DurationSeconds,
            seg.StartedAt,
            seg.EndedAt
        )).ToList()
    );
}
