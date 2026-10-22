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
/// Entity Framework implementation of routines repository.
/// </summary>
public class RoutineRepository(NexodusDbContext context) : IRoutineRepository
{
    private readonly NexodusDbContext _context = context ?? throw new ArgumentNullException(nameof(context));

    public async Task<IEnumerable<Routine>> GetAllByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await _context.Routines
            .AsNoTracking()
            .Where(r => r.UserId == userId)
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<Routine?> GetByIdAsync(Guid id, Guid userId, CancellationToken cancellationToken = default)
    {
        return await _context.Routines
            .Include(r => r.RoutineExercises)
                .ThenInclude(re => re.Exercise)
            .FirstOrDefaultAsync(r => r.Id == id && r.UserId == userId, cancellationToken);
    }

    public async Task<Routine> AddAsync(Routine routine, CancellationToken cancellationToken = default)
    {
        await _context.Routines.AddAsync(routine, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
        return routine;
    }

    public async Task UpdateAsync(Routine routine, CancellationToken cancellationToken = default)
    {
        _context.Routines.Update(routine);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(Routine routine, CancellationToken cancellationToken = default)
    {
        _context.Routines.Remove(routine);
        await _context.SaveChangesAsync(cancellationToken);
    }
}
