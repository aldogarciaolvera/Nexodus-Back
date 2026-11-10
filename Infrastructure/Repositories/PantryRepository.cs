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
/// Entity Framework implementation of user pantry repository.
/// </summary>
public class PantryRepository(NexodusDbContext context) : IPantryRepository
{
    private readonly NexodusDbContext _context = context ?? throw new ArgumentNullException(nameof(context));

    public async Task<List<PantryItem>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await _context.PantryItems
            .AsNoTracking()
            .Include(p => p.Food)
            .Where(p => p.UserId == userId)
            .OrderBy(p => p.Category)
            .ThenBy(p => p.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<PantryItem?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.PantryItems
            .Include(p => p.Food)
            .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);
    }

    public async Task<PantryItem> CreateAsync(PantryItem item, CancellationToken cancellationToken = default)
    {
        item.CreatedAt = DateTime.UtcNow;
        item.UpdatedAt = DateTime.UtcNow;
        await _context.PantryItems.AddAsync(item, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
        return item;
    }

    public async Task<PantryItem> UpdateAsync(PantryItem item, CancellationToken cancellationToken = default)
    {
        item.UpdatedAt = DateTime.UtcNow;
        _context.PantryItems.Update(item);
        await _context.SaveChangesAsync(cancellationToken);
        return item;
    }

    public async Task DeleteAsync(PantryItem item, CancellationToken cancellationToken = default)
    {
        _context.PantryItems.Remove(item);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteRangeAsync(IEnumerable<PantryItem> items, CancellationToken cancellationToken = default)
    {
        _context.PantryItems.RemoveRange(items);
        await _context.SaveChangesAsync(cancellationToken);
    }
}
