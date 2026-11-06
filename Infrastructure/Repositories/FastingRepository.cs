using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Nexodus_Back.Core.Entities;
using Nexodus_Back.Core.Interfaces;
using Nexodus_Back.Infrastructure.Data;

namespace Nexodus_Back.Infrastructure.Repositories;

/// <summary>
/// Entity Framework implementation of fasting repository.
/// </summary>
public class FastingRepository(NexodusDbContext context) : IFastingRepository
{
    private readonly NexodusDbContext _context = context ?? throw new ArgumentNullException(nameof(context));

    public async Task<FastingLog?> GetActiveFastingAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await _context.FastingLogs
            .FirstOrDefaultAsync(f => f.UserId == userId && f.IsActive, cancellationToken);
    }

    public async Task<FastingLog> StartFastingAsync(Guid userId, double targetHours, string protocol, DateTime? startTime = null, CancellationToken cancellationToken = default)
    {
        // Deactivate any existing active fasting sessions for this user
        var activeSessions = await _context.FastingLogs
            .Where(f => f.UserId == userId && f.IsActive)
            .ToListAsync(cancellationToken);

        var now = DateTime.UtcNow;
        foreach (var session in activeSessions)
        {
            session.IsActive = false;
            session.EndTime = now;
        }

        var newSession = new FastingLog
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            StartTime = startTime ?? now,
            EndTime = null,
            TargetHours = targetHours > 0 ? targetHours : 16,
            FastingProtocol = string.IsNullOrWhiteSpace(protocol) ? "16:8" : protocol,
            IsActive = true,
            CreatedAt = now
        };

        await _context.FastingLogs.AddAsync(newSession, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
        return newSession;
    }

    public async Task<FastingLog> StopFastingAsync(Guid userId, DateTime? endTime = null, CancellationToken cancellationToken = default)
    {
        var activeSession = await _context.FastingLogs
            .FirstOrDefaultAsync(f => f.UserId == userId && f.IsActive, cancellationToken);

        var end = endTime ?? DateTime.UtcNow;

        if (activeSession == null)
        {
            // If none was explicitly active, create a completed entry
            var fallback = new FastingLog
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                StartTime = end.AddHours(-16),
                EndTime = end,
                TargetHours = 16,
                FastingProtocol = "16:8",
                IsActive = false,
                CreatedAt = end
            };
            await _context.FastingLogs.AddAsync(fallback, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);
            return fallback;
        }

        activeSession.IsActive = false;
        activeSession.EndTime = end;
        await _context.SaveChangesAsync(cancellationToken);
        return activeSession;
    }

    public async Task<List<FastingLog>> GetRecentFastingLogsAsync(Guid userId, int count = 10, CancellationToken cancellationToken = default)
    {
        return await _context.FastingLogs
            .AsNoTracking()
            .Where(f => f.UserId == userId)
            .OrderByDescending(f => f.StartTime)
            .Take(count)
            .ToListAsync(cancellationToken);
    }
}
