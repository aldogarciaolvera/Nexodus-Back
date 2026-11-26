using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Nexodus_Back.Core.Entities;

namespace Nexodus_Back.Core.Interfaces;

public interface ICardioRepository
{
    Task<CardioSession> AddAsync(CardioSession session, CancellationToken cancellationToken = default);
    Task<List<CardioSession>> GetByUserIdAsync(Guid userId, int limit = 20, CancellationToken cancellationToken = default);
    Task<CardioSession?> GetByIdAsync(Guid id, Guid userId, CancellationToken cancellationToken = default);
    Task DeleteAsync(CardioSession session, CancellationToken cancellationToken = default);
}
