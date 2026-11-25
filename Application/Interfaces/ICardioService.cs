using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Nexodus_Back.Application.DTOs;

namespace Nexodus_Back.Application.Interfaces;

public interface ICardioService
{
    Task<CardioSessionDto> SaveSessionAsync(Guid userId, SaveCardioSessionDto dto, CancellationToken cancellationToken = default);
    Task<List<CardioSessionDto>> GetUserSessionsAsync(Guid userId, int limit = 20, CancellationToken cancellationToken = default);
    Task DeleteSessionAsync(Guid id, Guid userId, CancellationToken cancellationToken = default);
}

public interface ICardioAiService
{
    Task<GeneratedAiCardioPlanDto> GenerateCardioPlanAsync(Guid userId, GenerateAiCardioRequestDto request, CancellationToken cancellationToken = default);
}
