using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Nexodus_Back.Core.Entities;
using Nexodus_Back.Core.Interfaces;
using Nexodus_Back.Infrastructure.Data;

namespace Nexodus_Back.Infrastructure.Repositories;

/// <summary>
/// Entity Framework implementation of user dietary target repository.
/// </summary>
public class DietTargetRepository(NexodusDbContext context) : IDietTargetRepository
{
    private readonly NexodusDbContext _context = context ?? throw new ArgumentNullException(nameof(context));

    public async Task<DietTarget?> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await _context.DietTargets
            .AsNoTracking()
            .FirstOrDefaultAsync(dt => dt.UserId == userId, cancellationToken);
    }

    public async Task<DietTarget> UpsertAsync(DietTarget target, CancellationToken cancellationToken = default)
    {
        var existing = await _context.DietTargets
            .FirstOrDefaultAsync(dt => dt.UserId == target.UserId, cancellationToken);

        if (existing == null)
        {
            target.Id = Guid.NewGuid();
            target.CreatedAt = DateTime.UtcNow;
            target.UpdatedAt = DateTime.UtcNow;
            await _context.DietTargets.AddAsync(target, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);
            return target;
        }

        existing.TargetCalories = target.TargetCalories;
        existing.ProteinGrams = target.ProteinGrams;
        existing.CarbsGrams = target.CarbsGrams;
        existing.FatGrams = target.FatGrams;
        existing.WaterTargetMl = target.WaterTargetMl;
        existing.FastingProtocol = target.FastingProtocol;
        existing.EatingWindowStart = target.EatingWindowStart;
        existing.EatingWindowEnd = target.EatingWindowEnd;
        existing.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);
        return existing;
    }
}
