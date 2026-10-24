using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Nexodus_Back.Core.Entities;

namespace Nexodus_Back.Core.Interfaces;

public interface IFastingRepository
{
    Task<FastingLog?> GetActiveFastingAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<FastingLog> StartFastingAsync(Guid userId, double targetHours, string protocol, DateTime? startTime = null, CancellationToken cancellationToken = default);
    Task<FastingLog> StopFastingAsync(Guid userId, DateTime? endTime = null, CancellationToken cancellationToken = default);
    Task<List<FastingLog>> GetRecentFastingLogsAsync(Guid userId, int count = 10, CancellationToken cancellationToken = default);
}
