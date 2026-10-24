using System;
using System.Threading;
using System.Threading.Tasks;
using Nexodus_Back.Core.Entities;

namespace Nexodus_Back.Core.Interfaces;

public interface IDietTargetRepository
{
    Task<DietTarget?> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<DietTarget> UpsertAsync(DietTarget target, CancellationToken cancellationToken = default);
}
