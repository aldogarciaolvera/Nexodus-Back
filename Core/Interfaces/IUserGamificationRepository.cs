using System;
using System.Threading;
using System.Threading.Tasks;
using Nexodus_Back.Core.Entities;

namespace Nexodus_Back.Core.Interfaces;

public interface IUserGamificationRepository
{
    Task<UserGamification?> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<UserGamification> AddAsync(UserGamification gamification, CancellationToken cancellationToken = default);
    Task UpdateAsync(UserGamification gamification, CancellationToken cancellationToken = default);
}
