using Microsoft.EntityFrameworkCore;
using Nexodus_Back.Core.Entities;
using Nexodus_Back.Core.Interfaces;
using Nexodus_Back.Infrastructure.Data;

namespace Nexodus_Back.Infrastructure.Repositories;

public class ExerciseRepository : IExerciseRepository
{
    private readonly NexodusDbContext _context;

    public ExerciseRepository(NexodusDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Exercise>> GetAllAsync()
    {
        return await _context.Exercises.ToListAsync();
    }

    public async Task<Exercise?> GetByIdAsync(Guid id)
    {
        return await _context.Exercises.FirstOrDefaultAsync(e => e.Id == id);
    }
}
