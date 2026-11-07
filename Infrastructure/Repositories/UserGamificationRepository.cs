using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Nexodus_Back.Core.Entities;
using Nexodus_Back.Core.Interfaces;
using Nexodus_Back.Infrastructure.Data;

namespace Nexodus_Back.Infrastructure.Repositories;

public class UserGamificationRepository : IUserGamificationRepository
{
    private readonly NexodusDbContext _context;

    public UserGamificationRepository(NexodusDbContext context)
    {
        _context = context;
    }

    public async Task<UserGamification?> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await _context.Set<UserGamification>()
            .FirstOrDefaultAsync(g => g.UserId == userId, cancellationToken);
    }

    public async Task<UserGamification> AddAsync(UserGamification gamification, CancellationToken cancellationToken = default)
    {
        await _context.Set<UserGamification>().AddAsync(gamification, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
        return gamification;
    }

    public async Task UpdateAsync(UserGamification gamification, CancellationToken cancellationToken = default)
    {
        _context.Set<UserGamification>().Update(gamification);
        await _context.SaveChangesAsync(cancellationToken);
    }
}

