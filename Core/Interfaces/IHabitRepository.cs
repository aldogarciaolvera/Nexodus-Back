using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Nexodus_Back.Core.Entities;

namespace Nexodus_Back.Core.Interfaces;

public interface IHabitRepository
{
    Task<Habit> CreateAsync(Habit habit, CancellationToken cancellationToken = default);
    Task<IEnumerable<Habit>> GetAllByUserIdAsync(Guid userId, bool includeArchived = false, CancellationToken cancellationToken = default);
    Task<Habit?> GetByIdAsync(Guid userId, Guid id, CancellationToken cancellationToken = default);
    Task<Habit> UpdateAsync(Habit habit, CancellationToken cancellationToken = default);
    Task DeleteAsync(Habit habit, CancellationToken cancellationToken = default);

    Task<HabitCompletion?> GetCompletionByDateAsync(Guid habitId, string date, CancellationToken cancellationToken = default);
    Task<IEnumerable<HabitCompletion>> GetCompletionsRangeAsync(Guid habitId, string startDate, string endDate, CancellationToken cancellationToken = default);
    Task<IEnumerable<HabitCompletion>> GetAllCompletionsAsync(Guid habitId, CancellationToken cancellationToken = default);
    Task<HabitCompletion> AddOrUpdateCompletionAsync(HabitCompletion completion, CancellationToken cancellationToken = default);
    Task DeleteCompletionAsync(HabitCompletion completion, CancellationToken cancellationToken = default);

    Task<IEnumerable<Habit>> GetHabitsByAutoSourceAsync(Guid userId, string autoSource, CancellationToken cancellationToken = default);
}
