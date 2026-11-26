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

public class CardioRepository(NexodusDbContext context) : ICardioRepository
{
    private readonly NexodusDbContext _context = context ?? throw new ArgumentNullException(nameof(context));

    public async Task<CardioSession> AddAsync(CardioSession session, CancellationToken cancellationToken = default)
    {
        await _context.CardioSessions.AddAsync(session, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
        return session;
    }

    public async Task<List<CardioSession>> GetByUserIdAsync(Guid userId, int limit = 20, CancellationToken cancellationToken = default)
    {
        var safeLimit = Math.Clamp(limit, 1, 100);
        return await _context.CardioSessions
            .AsNoTracking()
            .Include(s => s.Segments.OrderBy(seg => seg.SegmentOrder))
            .Where(s => s.UserId == userId)
            .OrderByDescending(s => s.StartTime)
            .Take(safeLimit)
            .ToListAsync(cancellationToken);
    }

    public async Task<CardioSession?> GetByIdAsync(Guid id, Guid userId, CancellationToken cancellationToken = default)
    {
        return await _context.CardioSessions
            .Include(s => s.Segments)
            .FirstOrDefaultAsync(s => s.Id == id && s.UserId == userId, cancellationToken);
    }

    public async Task DeleteAsync(CardioSession session, CancellationToken cancellationToken = default)
    {
        _context.CardioSessions.Remove(session);
        await _context.SaveChangesAsync(cancellationToken);
    }
}
