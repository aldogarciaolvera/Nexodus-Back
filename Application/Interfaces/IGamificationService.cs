using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Nexodus_Back.Application.DTOs.Gamification;

namespace Nexodus_Back.Application.Interfaces;

public interface IGamificationService
{
    Task<GamificationStatusDto> GetStatusAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<GamificationStatusDto> RecordActivityAsync(Guid userId, string activityType, int baseNxp = 10, int bonusNxp = 0, CancellationToken cancellationToken = default);
    Task<GamificationStatusDto> UpdateRestDaysAsync(Guid userId, List<string> restDays, CancellationToken cancellationToken = default);
    Task<StreakRestoreResultDto> RestoreStreakAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<VaultPurchaseResultDto> PurchaseVaultItemAsync(Guid userId, string itemType, CancellationToken cancellationToken = default);
}
