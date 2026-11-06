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

public class WorkoutSessionRepository(NexodusDbContext context) : IWorkoutSessionRepository
{
    private readonly NexodusDbContext _context = context ?? throw new ArgumentNullException(nameof(context));

    public async Task<WorkoutSession?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.WorkoutSessions
            .Include(ws => ws.Routine)
            .Include(ws => ws.Sets)
                .ThenInclude(s => s.Exercise)
            .FirstOrDefaultAsync(ws => ws.Id == id, cancellationToken);
    }

    public async Task<WorkoutSession?> GetActiveByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await _context.WorkoutSessions
            .Include(ws => ws.Routine)
            .Include(ws => ws.Sets)
                .ThenInclude(s => s.Exercise)
            .FirstOrDefaultAsync(ws => ws.UserId == userId && ws.IsActive, cancellationToken);
    }

    public async Task<IEnumerable<WorkoutSession>> GetAllByUserIdAsync(Guid userId, int limit = 50, CancellationToken cancellationToken = default)
    {
        return await _context.WorkoutSessions
            .Include(ws => ws.Routine)
            .Include(ws => ws.Sets)
                .ThenInclude(s => s.Exercise)
            .Where(ws => ws.UserId == userId && !ws.IsActive)
            .OrderByDescending(ws => ws.StartTime)
            .Take(limit)
            .ToListAsync(cancellationToken);
    }

    public async Task AddAsync(WorkoutSession session, CancellationToken cancellationToken = default)
    {
        await _context.WorkoutSessions.AddAsync(session, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(WorkoutSession session, CancellationToken cancellationToken = default)
    {
        _context.WorkoutSessions.Update(session);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(WorkoutSession session, CancellationToken cancellationToken = default)
    {
        _context.WorkoutSessions.Remove(session);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<WorkoutSet?> GetSetByIdAsync(Guid setId, CancellationToken cancellationToken = default)
    {
        return await _context.WorkoutSets
            .Include(s => s.Exercise)
            .FirstOrDefaultAsync(s => s.Id == setId, cancellationToken);
    }

    public async Task AddSetAsync(WorkoutSet set, CancellationToken cancellationToken = default)
    {
        await _context.WorkoutSets.AddAsync(set, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateSetAsync(WorkoutSet set, CancellationToken cancellationToken = default)
    {
        _context.WorkoutSets.Update(set);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteSetAsync(WorkoutSet set, CancellationToken cancellationToken = default)
    {
        _context.WorkoutSets.Remove(set);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<double> GetExercisePersonalBest1RMAsync(Guid userId, Guid exerciseId, CancellationToken cancellationToken = default)
    {
        var max1Rm = await _context.WorkoutSets
            .Where(s => s.WorkoutSession.UserId == userId && s.ExerciseId == exerciseId && s.IsCompleted)
            .MaxAsync(s => (double?)s.CalculatedOneRepMax, cancellationToken);

        return max1Rm ?? 0.0;
    }

    public async Task<IEnumerable<WorkoutSet>> GetExerciseHistoryAsync(Guid userId, Guid exerciseId, int limit = 20, CancellationToken cancellationToken = default)
    {
        return await _context.WorkoutSets
            .Include(s => s.WorkoutSession)
            .Include(s => s.Exercise)
            .Where(s => s.WorkoutSession.UserId == userId && s.ExerciseId == exerciseId && s.IsCompleted)
            .OrderByDescending(s => s.CompletedAt ?? s.WorkoutSession.StartTime)
            .Take(limit)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<WorkoutSession>> GetRecentCompletedSessionsAsync(Guid userId, DateTime since, CancellationToken cancellationToken = default)
    {
        return await _context.WorkoutSessions
            .Include(ws => ws.Sets)
                .ThenInclude(s => s.Exercise)
            .Where(ws => ws.UserId == userId && !ws.IsActive && ws.StartTime >= since)
            .ToListAsync(cancellationToken);
    }
}

