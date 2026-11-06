using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Nexodus_Back.Core.Entities;
using Nexodus_Back.Core.Interfaces;
using Nexodus_Back.Infrastructure.Data;

namespace Nexodus_Back.Infrastructure.Repositories;

/// <summary>
/// Entity Framework implementation of water intake repository.
/// </summary>
public class WaterLogRepository(NexodusDbContext context) : IWaterLogRepository
{
    private readonly NexodusDbContext _context = context ?? throw new ArgumentNullException(nameof(context));

    public async Task<WaterLog?> GetByDateAsync(Guid userId, string date, CancellationToken cancellationToken = default)
    {
        return await _context.WaterLogs
            .AsNoTracking()
            .FirstOrDefaultAsync(w => w.UserId == userId && w.Date == date, cancellationToken);
    }

    public async Task<WaterLog> AddWaterAsync(Guid userId, string date, double amountMl, CancellationToken cancellationToken = default)
    {
        var existing = await _context.WaterLogs
            .FirstOrDefaultAsync(w => w.UserId == userId && w.Date == date, cancellationToken);

        if (existing == null)
        {
            var log = new WaterLog
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                Date = date,
                ConsumedMl = Math.Max(0, amountMl),
                UpdatedAt = DateTime.UtcNow
            };
            await _context.WaterLogs.AddAsync(log, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);
            return log;
        }

        existing.ConsumedMl = Math.Max(0, existing.ConsumedMl + amountMl);
        existing.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync(cancellationToken);
        return existing;
    }

    public async Task<WaterLog> SetWaterAsync(Guid userId, string date, double totalMl, CancellationToken cancellationToken = default)
    {
        var existing = await _context.WaterLogs
            .FirstOrDefaultAsync(w => w.UserId == userId && w.Date == date, cancellationToken);

        if (existing == null)
        {
            var log = new WaterLog
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                Date = date,
                ConsumedMl = Math.Max(0, totalMl),
                UpdatedAt = DateTime.UtcNow
            };
            await _context.WaterLogs.AddAsync(log, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);
            return log;
        }

        existing.ConsumedMl = Math.Max(0, totalMl);
        existing.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync(cancellationToken);
        return existing;
    }
}
