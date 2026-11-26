using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Nexodus_Back.Application.DTOs.Habit;

namespace Nexodus_Back.Application.Interfaces;

public interface IHabitService
{
    Task<HabitDto> CreateHabitAsync(Guid userId, CreateHabitDto dto, CancellationToken cancellationToken = default);
    Task<IEnumerable<HabitDto>> GetHabitsAsync(Guid userId, bool includeArchived = false, int recentDays = 60, CancellationToken cancellationToken = default);
    Task<HabitDto?> GetHabitByIdAsync(Guid userId, Guid id, int recentDays = 60, CancellationToken cancellationToken = default);
    Task<HabitDto> UpdateHabitAsync(Guid userId, Guid id, UpdateHabitDto dto, CancellationToken cancellationToken = default);
    Task DeleteHabitAsync(Guid userId, Guid id, CancellationToken cancellationToken = default);

    Task<HabitDto> CheckInAsync(Guid userId, Guid habitId, HabitCheckInDto dto, CancellationToken cancellationToken = default);
    Task<HabitStatsDto> GetHabitStatsAsync(Guid userId, Guid habitId, CancellationToken cancellationToken = default);
    Task<List<HabitDayCellDto>> GetHabitHistoryAsync(Guid userId, Guid habitId, int days = 90, CancellationToken cancellationToken = default);
    Task ReorderHabitsAsync(Guid userId, ReorderHabitsDto dto, CancellationToken cancellationToken = default);

    Task HandleAutoCompleteTriggerAsync(Guid userId, string source, double value = 1.0, CancellationToken cancellationToken = default);
}
