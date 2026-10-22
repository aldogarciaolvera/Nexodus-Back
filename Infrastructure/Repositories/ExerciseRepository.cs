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
/// Entity Framework implementation of exercise catalog repository.
/// </summary>
public class ExerciseRepository(NexodusDbContext context) : IExerciseRepository
{
    private readonly NexodusDbContext _context = context ?? throw new ArgumentNullException(nameof(context));

    public async Task<IEnumerable<Exercise>> GetAllAsync(int page = 1, int pageSize = 50, CancellationToken cancellationToken = default)
    {
        var sanitizedPage = page < 1 ? 1 : page;
        var sanitizedPageSize = pageSize is < 1 or > 200 ? 50 : pageSize;

        return await _context.Exercises
            .AsNoTracking()
            .OrderBy(e => e.Name)
            .Skip((sanitizedPage - 1) * sanitizedPageSize)
            .Take(sanitizedPageSize)
            .ToListAsync(cancellationToken);
    }

    public async Task<Exercise?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Exercises
            .AsNoTracking()
            .FirstOrDefaultAsync(e => e.Id == id, cancellationToken);
    }
}
