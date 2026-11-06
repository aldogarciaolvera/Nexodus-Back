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
/// Entity Framework implementation of food catalog repository.
/// </summary>
public class FoodRepository(NexodusDbContext context) : IFoodRepository
{
    private readonly NexodusDbContext _context = context ?? throw new ArgumentNullException(nameof(context));

    public async Task<(List<Food> Items, int TotalCount)> SearchAsync(string query, Guid? userId, int page, int pageSize, CancellationToken cancellationToken = default)
    {
        var baseQuery = _context.Foods
            .AsNoTracking()
            .Where(f => !f.IsCustom || f.UserId == null || (userId.HasValue && f.UserId == userId.Value));

        if (!string.IsNullOrWhiteSpace(query))
        {
            var lowerQuery = query.Trim().ToLower();
            baseQuery = baseQuery.Where(f =>
                f.Name.ToLower().Contains(lowerQuery) ||
                (f.Brand != null && f.Brand.ToLower().Contains(lowerQuery)));
        }

        var totalCount = await baseQuery.CountAsync(cancellationToken);

        var items = await baseQuery
            .OrderBy(f => f.Name)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (items, totalCount);
    }

    public async Task<Food?> GetByBarcodeAsync(string barcode, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(barcode))
            return null;

        return await _context.Foods
            .AsNoTracking()
            .FirstOrDefaultAsync(f => f.Barcode == barcode, cancellationToken);
    }

    public async Task<Food?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Foods
            .AsNoTracking()
            .FirstOrDefaultAsync(f => f.Id == id, cancellationToken);
    }

    public async Task<Food> CreateAsync(Food food, CancellationToken cancellationToken = default)
    {
        food.Id = Guid.NewGuid();
        food.CreatedAt = DateTime.UtcNow;
        await _context.Foods.AddAsync(food, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
        return food;
    }
}
