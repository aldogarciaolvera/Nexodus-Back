using System;
using System.Threading;
using System.Threading.Tasks;
using Nexodus_Back.Core.Entities;

namespace Nexodus_Back.Core.Interfaces;

public interface IWaterLogRepository
{
    Task<WaterLog?> GetByDateAsync(Guid userId, string date, CancellationToken cancellationToken = default);
    Task<WaterLog> AddWaterAsync(Guid userId, string date, double amountMl, CancellationToken cancellationToken = default);
    Task<WaterLog> SetWaterAsync(Guid userId, string date, double totalMl, CancellationToken cancellationToken = default);
}
