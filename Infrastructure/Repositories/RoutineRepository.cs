using Microsoft.EntityFrameworkCore;
using Nexodus_Back.Core.Entities;
using Nexodus_Back.Core.Interfaces;
using Nexodus_Back.Infrastructure.Data;

namespace Nexodus_Back.Infrastructure.Repositories;

public class RoutineRepository : IRoutineRepository
{
    private readonly NexodusDbContext _context;

    public RoutineRepository(NexodusDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Routine>> GetAllByUserIdAsync(Guid userId)
    {
        return await _context.Routines
            .Include(r => r.RoutineExercises)
                .ThenInclude(re => re.Exercise)
            .Where(r => r.UserId == userId)
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync();
    }

    public async Task<Routine?> GetByIdAsync(Guid id, Guid userId)
    {
        return await _context.Routines
            .Include(r => r.RoutineExercises)
                .ThenInclude(re => re.Exercise)
            .FirstOrDefaultAsync(r => r.Id == id && r.UserId == userId);
    }

    public async Task<Routine> AddAsync(Routine routine)
    {
        await _context.Routines.AddAsync(routine);
        await _context.SaveChangesAsync();
        return routine;
    }

    public async Task UpdateAsync(Routine routine)
    {
        _context.Routines.Update(routine);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(Routine routine)
    {
        _context.Routines.Remove(routine);
        await _context.SaveChangesAsync();
    }
}
