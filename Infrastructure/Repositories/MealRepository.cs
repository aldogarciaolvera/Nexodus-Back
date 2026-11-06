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
/// Entity Framework implementation of meal repository.
/// </summary>
public class MealRepository(NexodusDbContext context) : IMealRepository
{
    private readonly NexodusDbContext _context = context ?? throw new ArgumentNullException(nameof(context));

    public async Task<List<Meal>> GetByDateAsync(Guid userId, string date, CancellationToken cancellationToken = default)
    {
        return await _context.Meals
            .AsNoTracking()
            .Include(m => m.Items)
            .Where(m => m.UserId == userId && m.Date == date)
            .OrderBy(m => m.Time)
            .ToListAsync(cancellationToken);
    }

    public async Task<Meal?> GetByIdAsync(Guid id, Guid userId, CancellationToken cancellationToken = default)
    {
        return await _context.Meals
            .Include(m => m.Items)
            .FirstOrDefaultAsync(m => m.Id == id && m.UserId == userId, cancellationToken);
    }

    public async Task<Meal> CreateAsync(Meal meal, CancellationToken cancellationToken = default)
    {
        meal.CreatedAt = DateTime.UtcNow;
        meal.UpdatedAt = DateTime.UtcNow;
        await _context.Meals.AddAsync(meal, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
        return meal;
    }

    public async Task<Meal> UpdateAsync(Meal meal, CancellationToken cancellationToken = default)
    {
        meal.UpdatedAt = DateTime.UtcNow;
        if (_context.Entry(meal).State == EntityState.Detached)
        {
            _context.Meals.Update(meal);
        }
        await _context.SaveChangesAsync(cancellationToken);
        return meal;
    }

    public async Task DeleteAsync(Meal meal, CancellationToken cancellationToken = default)
    {
        _context.Meals.Remove(meal);
        await _context.SaveChangesAsync(cancellationToken);
    }
}
