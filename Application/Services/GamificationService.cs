using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Nexodus_Back.Application.DTOs.Gamification;
using Nexodus_Back.Application.Interfaces;
using Nexodus_Back.Core.Entities;
using Nexodus_Back.Core.Exceptions;
using Nexodus_Back.Core.Interfaces;

namespace Nexodus_Back.Application.Services;

public class GamificationService : IGamificationService
{
    private readonly IUserGamificationRepository _repository;

    private static readonly (string Tier, int Threshold)[] TierLevels = new[]
    {
        ("Initiate", 0),
        ("Vanguard", 500),
        ("Cyber", 1500),
        ("Titan", 3000),
        ("Apex", 5000),
        ("Sovereign", 7500),
        ("Obsidian", 10000)
    };

    public GamificationService(IUserGamificationRepository repository)
    {
        _repository = repository;
    }

    public async Task<GamificationStatusDto> GetStatusAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var gamification = await GetOrCreateAsync(userId, cancellationToken);
        EvaluateStreakMaintenance(gamification);
        await _repository.UpdateAsync(gamification, cancellationToken);

        return MapToDto(gamification);
    }

    public async Task<GamificationStatusDto> RecordActivityAsync(Guid userId, string activityType, int baseNxp = 10, int bonusNxp = 0, CancellationToken cancellationToken = default)
    {
        var gamification = await GetOrCreateAsync(userId, cancellationToken);
        var now = DateTime.UtcNow;
        var today = now.Date;

        // 1. Streak evaluation
        if (gamification.LastActiveDate == null)
        {
            gamification.CurrentStreak = 1;
            gamification.HighestStreak = 1;
            gamification.LastActiveDate = now;
            gamification.StreakGracePeriodExpiresAt = null;
        }
        else
        {
            var lastDate = gamification.LastActiveDate.Value.Date;
            if (lastDate != today)
            {
                var missedDays = (today - lastDate).Days - 1;
                var restDays = ParseRestDays(gamification.ScheduledRestDays);

                bool allMissedWereRest = true;
                for (int i = 1; i <= missedDays; i++)
                {
                    var checkDay = lastDate.AddDays(i).DayOfWeek;
                    if (!restDays.Contains(checkDay))
                    {
                        allMissedWereRest = false;
                        break;
                    }
                }

                if (missedDays == 0 || allMissedWereRest)
                {
                    gamification.CurrentStreak += 1;
                }
                else if (gamification.StreakGracePeriodExpiresAt.HasValue && now <= gamification.StreakGracePeriodExpiresAt.Value)
                {
                    // Restored within grace period!
                    gamification.CurrentStreak += 1;
                }
                else if (gamification.StreakFreezesAvailable > 0)
                {
                    // Consumed freeze
                    gamification.StreakFreezesAvailable -= 1;
                    gamification.CurrentStreak += 1;
                }
                else
                {
                    // Reset streak
                    gamification.CurrentStreak = 1;
                }

                if (gamification.CurrentStreak > gamification.HighestStreak)
                {
                    gamification.HighestStreak = gamification.CurrentStreak;
                }

                gamification.LastActiveDate = now;
                gamification.StreakGracePeriodExpiresAt = null;
            }
        }

        // 2. NXP & Cyphers evaluation
        int streakMultiplier = Math.Min(5, 1 + (gamification.CurrentStreak / 7));
        int earnedNxp = (baseNxp * streakMultiplier) + bonusNxp;
        
        string oldTier = gamification.Tier;
        gamification.NxpTotal += earnedNxp;
        gamification.Tier = CalculateTier(gamification.NxpTotal);

        // Tier promotion gives bonus Cyphers
        if (oldTier != gamification.Tier)
        {
            gamification.Cyphers += 50;
        }

        // Milestone streak Cyphers (every 7 days gives +20 Cyphers)
        if (gamification.CurrentStreak > 0 && gamification.CurrentStreak % 7 == 0 && gamification.LastActiveDate?.Date == today)
        {
            gamification.Cyphers += 20;
        }

        gamification.UpdatedAt = now;
        await _repository.UpdateAsync(gamification, cancellationToken);

        return MapToDto(gamification);
    }

    public async Task<GamificationStatusDto> UpdateRestDaysAsync(Guid userId, List<string> restDays, CancellationToken cancellationToken = default)
    {
        var gamification = await GetOrCreateAsync(userId, cancellationToken);
        var validDays = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "Monday", "Tuesday", "Wednesday", "Thursday", "Friday", "Saturday", "Sunday"
        };

        var filtered = restDays
            .Where(d => validDays.Contains(d))
            .Select(d => char.ToUpper(d[0]) + d.Substring(1).ToLower())
            .Distinct();

        gamification.ScheduledRestDays = string.Join(",", filtered);
        gamification.UpdatedAt = DateTime.UtcNow;

        await _repository.UpdateAsync(gamification, cancellationToken);
        return MapToDto(gamification);
    }

    public async Task<StreakRestoreResultDto> RestoreStreakAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var gamification = await GetOrCreateAsync(userId, cancellationToken);
        const int restoreCost = 50;

        if (gamification.CurrentStreak > 0 && gamification.LastActiveDate?.Date == DateTime.UtcNow.Date)
        {
            return new StreakRestoreResultDto
            {
                Success = false,
                RestoredStreak = gamification.CurrentStreak,
                CyphersRemaining = gamification.Cyphers,
                Message = "La racha ya está activa hoy."
            };
        }

        if (gamification.Cyphers < restoreCost && gamification.StreakFreezesAvailable <= 0)
        {
            throw new ValidationException(new Dictionary<string, string[]>
            {
                { "Cyphers", new[] { $"Se requieren {restoreCost} Cyphers o 1 Escudo de Racha para restaurar." } }
            });
        }

        if (gamification.StreakFreezesAvailable > 0)
        {
            gamification.StreakFreezesAvailable -= 1;
        }
        else
        {
            gamification.Cyphers -= restoreCost;
        }

        // Restore to highest or previous streak + 1
        gamification.CurrentStreak = Math.Max(1, gamification.HighestStreak);
        gamification.LastActiveDate = DateTime.UtcNow;
        gamification.StreakGracePeriodExpiresAt = null;
        gamification.UpdatedAt = DateTime.UtcNow;

        await _repository.UpdateAsync(gamification, cancellationToken);

        return new StreakRestoreResultDto
        {
            Success = true,
            RestoredStreak = gamification.CurrentStreak,
            CyphersRemaining = gamification.Cyphers,
            Message = "¡Racha restaurada con éxito mediante el Protocolo Nexodus!"
        };
    }

    public async Task<VaultPurchaseResultDto> PurchaseVaultItemAsync(Guid userId, string itemType, CancellationToken cancellationToken = default)
    {
        var gamification = await GetOrCreateAsync(userId, cancellationToken);

        if (itemType.Equals("streak_freeze", StringComparison.OrdinalIgnoreCase))
        {
            const int cost = 30;
            if (gamification.Cyphers < cost)
            {
                throw new ValidationException(new Dictionary<string, string[]>
                {
                    { "Cyphers", new[] { $"Fondos insuficientes. Se requieren {cost} Cyphers." } }
                });
            }

            gamification.Cyphers -= cost;
            gamification.StreakFreezesAvailable += 1;
            gamification.UpdatedAt = DateTime.UtcNow;

            await _repository.UpdateAsync(gamification, cancellationToken);

            return new VaultPurchaseResultDto
            {
                Success = true,
                ItemType = "streak_freeze",
                CyphersRemaining = gamification.Cyphers,
                FreezesAvailable = gamification.StreakFreezesAvailable,
                Message = "Escudo de Racha adquirido con éxito."
            };
        }

        throw new NotFoundException($"El artículo '{itemType}' no está disponible en el Cyber Vault.");
    }

    private async Task<UserGamification> GetOrCreateAsync(Guid userId, CancellationToken ct)
    {
        var existing = await _repository.GetByUserIdAsync(userId, ct);
        if (existing != null) return existing;

        var created = new UserGamification
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            CurrentStreak = 0,
            HighestStreak = 0,
            ScheduledRestDays = "Saturday,Sunday",
            StreakFreezesAvailable = 2,
            NxpTotal = 0,
            Tier = "Initiate",
            Cyphers = 100,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        return await _repository.AddAsync(created, ct);
    }

    private void EvaluateStreakMaintenance(UserGamification gamification)
    {
        if (gamification.LastActiveDate == null) return;

        var now = DateTime.UtcNow;
        var today = now.Date;
        var lastDate = gamification.LastActiveDate.Value.Date;
        var missedDays = (today - lastDate).Days - 1;

        if (missedDays <= 0) return;

        var restDays = ParseRestDays(gamification.ScheduledRestDays);
        bool allMissedWereRest = true;
        for (int i = 1; i <= missedDays; i++)
        {
            var checkDay = lastDate.AddDays(i).DayOfWeek;
            if (!restDays.Contains(checkDay))
            {
                allMissedWereRest = false;
                break;
            }
        }

        if (allMissedWereRest)
        {
            // Rest days protect the streak
            return;
        }

        // Non-rest missed day: check grace period
        if (gamification.StreakGracePeriodExpiresAt == null)
        {
            gamification.StreakGracePeriodExpiresAt = lastDate.AddDays(2).AddHours(4); // 28h window
        }

        if (now > gamification.StreakGracePeriodExpiresAt.Value)
        {
            if (gamification.StreakFreezesAvailable > 0)
            {
                gamification.StreakFreezesAvailable -= 1;
                gamification.StreakGracePeriodExpiresAt = null;
            }
            else
            {
                gamification.CurrentStreak = 0;
            }
        }
    }

    private static HashSet<DayOfWeek> ParseRestDays(string restDaysStr)
    {
        var set = new HashSet<DayOfWeek>();
        if (string.IsNullOrWhiteSpace(restDaysStr)) return set;

        foreach (var part in restDaysStr.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
        {
            if (Enum.TryParse<DayOfWeek>(part, true, out var dow))
            {
                set.Add(dow);
            }
        }
        return set;
    }

    private static string CalculateTier(int nxp)
    {
        for (int i = TierLevels.Length - 1; i >= 0; i--)
        {
            if (nxp >= TierLevels[i].Threshold)
            {
                return TierLevels[i].Tier;
            }
        }
        return "Initiate";
    }

    private static GamificationStatusDto MapToDto(UserGamification g)
    {
        var now = DateTime.UtcNow;
        var today = now.Date;
        var restDays = ParseRestDays(g.ScheduledRestDays);

        int currentTierIdx = Array.FindIndex(TierLevels, t => t.Tier.Equals(g.Tier, StringComparison.OrdinalIgnoreCase));
        if (currentTierIdx < 0) currentTierIdx = 0;

        int currentBase = TierLevels[currentTierIdx].Threshold;
        string? nextTier = currentTierIdx < TierLevels.Length - 1 ? TierLevels[currentTierIdx + 1].Tier : null;
        int nextThreshold = currentTierIdx < TierLevels.Length - 1 ? TierLevels[currentTierIdx + 1].Threshold : currentBase;

        double progress = 100.0;
        if (nextTier != null && nextThreshold > currentBase)
        {
            progress = Math.Clamp((double)(g.NxpTotal - currentBase) / (nextThreshold - currentBase) * 100.0, 0.0, 100.0);
        }

        return new GamificationStatusDto
        {
            CurrentStreak = g.CurrentStreak,
            HighestStreak = g.HighestStreak,
            LastActiveDate = g.LastActiveDate,
            IsActiveToday = g.LastActiveDate?.Date == today,
            IsRestDayToday = restDays.Contains(now.DayOfWeek),
            IsGracePeriodActive = g.StreakGracePeriodExpiresAt.HasValue && now <= g.StreakGracePeriodExpiresAt.Value,
            GracePeriodExpiresAt = g.StreakGracePeriodExpiresAt,
            ScheduledRestDays = g.ScheduledRestDays.Split(',', StringSplitOptions.RemoveEmptyEntries).ToList(),
            StreakFreezesAvailable = g.StreakFreezesAvailable,
            NxpTotal = g.NxpTotal,
            Tier = g.Tier,
            NextTier = nextTier,
            NextTierNxp = nextThreshold,
            CurrentTierBaseNxp = currentBase,
            ProgressPercent = Math.Round(progress, 1),
            Cyphers = g.Cyphers
        };
    }
}

