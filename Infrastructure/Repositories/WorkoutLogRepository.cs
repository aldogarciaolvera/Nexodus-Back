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
/// Entity Framework implementation of workout log repository.
/// </summary>
public class WorkoutLogRepository(NexodusDbContext context) : IWorkoutLogRepository
{
    private readonly NexodusDbContext _context = context ?? throw new ArgumentNullException(nameof(context));

    public async Task<WorkoutLog?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.WorkoutLogs
            .AsNoTracking()
            .FirstOrDefaultAsync(w => w.Id == id, cancellationToken);
    }

    public async Task<IEnumerable<WorkoutLog>> GetAllByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await _context.WorkoutLogs
            .AsNoTracking()
            .Where(w => w.UserId == userId)
            .OrderByDescending(w => w.DateCompleted)
            .ToListAsync(cancellationToken);
    }

    public async Task AddAsync(WorkoutLog workoutLog, CancellationToken cancellationToken = default)
    {
        await _context.WorkoutLogs.AddAsync(workoutLog, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(WorkoutLog workoutLog, CancellationToken cancellationToken = default)
    {
        _context.WorkoutLogs.Remove(workoutLog);
        await _context.SaveChangesAsync(cancellationToken);
    }
}
