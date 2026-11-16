using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Nexodus_Back.Application.DTOs.Habit;
using Nexodus_Back.Application.Interfaces;
using Nexodus_Back.Core.Entities;
using Nexodus_Back.Core.Exceptions;
using Nexodus_Back.Core.Interfaces;

namespace Nexodus_Back.Application.Services;

public class HabitService(
    IHabitRepository habitRepository,
    IGamificationService gamificationService) : IHabitService
{
    private readonly IHabitRepository _habitRepository = habitRepository ?? throw new ArgumentNullException(nameof(habitRepository));
    private readonly IGamificationService _gamificationService = gamificationService ?? throw new ArgumentNullException(nameof(gamificationService));

    public async Task<HabitDto> CreateHabitAsync(Guid userId, CreateHabitDto dto, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(dto.Name))
        {
            throw new ValidationException("El nombre del hábito es obligatorio.");
        }

        var habit = new Habit
        {
            UserId = userId,
            Name = dto.Name.Trim(),
            Description = dto.Description?.Trim(),
            Icon = string.IsNullOrWhiteSpace(dto.Icon) ? "check" : dto.Icon.Trim(),
            Color = string.IsNullOrWhiteSpace(dto.Color) ? "#00F0FF" : dto.Color.Trim(),
            Category = string.IsNullOrWhiteSpace(dto.Category) ? "General" : dto.Category.Trim(),
            HabitType = string.IsNullOrWhiteSpace(dto.HabitType) ? "Build" : dto.HabitType.Trim(),
            TrackingType = string.IsNullOrWhiteSpace(dto.TrackingType) ? "YesNo" : dto.TrackingType.Trim(),
            TargetValue = dto.TargetValue <= 0 ? 1.0 : dto.TargetValue,
            TargetUnit = dto.TargetUnit?.Trim(),
            OnTapAction = string.IsNullOrWhiteSpace(dto.OnTapAction) ? "Increment" : dto.OnTapAction.Trim(),
            FrequencyType = string.IsNullOrWhiteSpace(dto.FrequencyType) ? "Daily" : dto.FrequencyType.Trim(),
            FrequencyDays = dto.FrequencyDays,
            FrequencyTargetCount = dto.FrequencyTargetCount <= 0 ? 1 : dto.FrequencyTargetCount,
            EveryNDaysInterval = dto.EveryNDaysInterval <= 0 ? 1 : dto.EveryNDaysInterval,
            StreakUnit = string.IsNullOrWhiteSpace(dto.StreakUnit) ? "Days" : dto.StreakUnit.Trim(),
            StreakGoal = dto.StreakGoal,
            FillMetPeriods = dto.FillMetPeriods,
            AutoCompleteSource = dto.AutoCompleteSource,
            Reminders = dto.Reminders?.Select(r => new HabitReminder
            {
                Time = r.Time,
                DaysOfWeek = r.DaysOfWeek,
                IsEnabled = r.IsEnabled
            }).ToList() ?? new List<HabitReminder>()
        };

        var created = await _habitRepository.CreateAsync(habit, cancellationToken);
        return await MapToDtoAsync(created, 60, cancellationToken);
    }

    public async Task<IEnumerable<HabitDto>> GetHabitsAsync(Guid userId, bool includeArchived = false, int recentDays = 60, CancellationToken cancellationToken = default)
    {
        var habits = await _habitRepository.GetAllByUserIdAsync(userId, includeArchived, cancellationToken);
        var dtos = new List<HabitDto>();
        foreach (var habit in habits)
        {
            dtos.Add(await MapToDtoAsync(habit, recentDays, cancellationToken));
        }
        return dtos;
    }

    public async Task<HabitDto?> GetHabitByIdAsync(Guid userId, Guid id, int recentDays = 60, CancellationToken cancellationToken = default)
    {
        var habit = await _habitRepository.GetByIdAsync(userId, id, cancellationToken);
        if (habit == null) return null;
        return await MapToDtoAsync(habit, recentDays, cancellationToken);
    }

    public async Task<HabitDto> UpdateHabitAsync(Guid userId, Guid id, UpdateHabitDto dto, CancellationToken cancellationToken = default)
    {
        var habit = await _habitRepository.GetByIdAsync(userId, id, cancellationToken);
        if (habit == null)
        {
            throw new NotFoundException($"El hábito con Id '{id}' no fue encontrado.");
        }

        if (string.IsNullOrWhiteSpace(dto.Name))
        {
            throw new ValidationException("El nombre del hábito es obligatorio.");
        }

        habit.Name = dto.Name.Trim();
        habit.Description = dto.Description?.Trim();
        habit.Icon = string.IsNullOrWhiteSpace(dto.Icon) ? habit.Icon : dto.Icon.Trim();
        habit.Color = string.IsNullOrWhiteSpace(dto.Color) ? habit.Color : dto.Color.Trim();
        habit.Category = string.IsNullOrWhiteSpace(dto.Category) ? habit.Category : dto.Category.Trim();
        habit.HabitType = string.IsNullOrWhiteSpace(dto.HabitType) ? habit.HabitType : dto.HabitType.Trim();
        habit.TrackingType = string.IsNullOrWhiteSpace(dto.TrackingType) ? habit.TrackingType : dto.TrackingType.Trim();
        habit.TargetValue = dto.TargetValue <= 0 ? 1.0 : dto.TargetValue;
        habit.TargetUnit = dto.TargetUnit?.Trim();
        habit.OnTapAction = string.IsNullOrWhiteSpace(dto.OnTapAction) ? habit.OnTapAction : dto.OnTapAction.Trim();
        habit.FrequencyType = string.IsNullOrWhiteSpace(dto.FrequencyType) ? habit.FrequencyType : dto.FrequencyType.Trim();
        habit.FrequencyDays = dto.FrequencyDays;
        habit.FrequencyTargetCount = dto.FrequencyTargetCount <= 0 ? 1 : dto.FrequencyTargetCount;
        habit.EveryNDaysInterval = dto.EveryNDaysInterval <= 0 ? 1 : dto.EveryNDaysInterval;
        habit.StreakUnit = string.IsNullOrWhiteSpace(dto.StreakUnit) ? habit.StreakUnit : dto.StreakUnit.Trim();
        habit.StreakGoal = dto.StreakGoal;
        habit.FillMetPeriods = dto.FillMetPeriods;
        habit.AutoCompleteSource = dto.AutoCompleteSource;
        habit.IsArchived = dto.IsArchived;
        habit.OrderIndex = dto.OrderIndex;

        if (dto.Reminders != null)
        {
            habit.Reminders = dto.Reminders.Select(r => new HabitReminder
            {
                HabitId = habit.Id,
                Time = r.Time,
                DaysOfWeek = r.DaysOfWeek,
                IsEnabled = r.IsEnabled
            }).ToList();
        }

        await _habitRepository.UpdateAsync(habit, cancellationToken);
        await RecalculateStreaksAsync(habit, cancellationToken);

        return await MapToDtoAsync(habit, 60, cancellationToken);
    }

    public async Task DeleteHabitAsync(Guid userId, Guid id, CancellationToken cancellationToken = default)
    {
        var habit = await _habitRepository.GetByIdAsync(userId, id, cancellationToken);
        if (habit == null)
        {
            throw new NotFoundException($"El hábito con Id '{id}' no fue encontrado.");
        }

        await _habitRepository.DeleteAsync(habit, cancellationToken);
    }

    public async Task<HabitDto> CheckInAsync(Guid userId, Guid habitId, HabitCheckInDto dto, CancellationToken cancellationToken = default)
    {
        var habit = await _habitRepository.GetByIdAsync(userId, habitId, cancellationToken);
        if (habit == null)
        {
            throw new NotFoundException($"El hábito con Id '{habitId}' no fue encontrado.");
        }

        string targetDate = string.IsNullOrWhiteSpace(dto.Date)
            ? DateTime.UtcNow.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)
            : dto.Date.Trim();

        var existingCompletion = await _habitRepository.GetCompletionByDateAsync(habitId, targetDate, cancellationToken);

        bool isQuit = habit.HabitType.Equals("Quit", StringComparison.OrdinalIgnoreCase);
        bool isYesNo = habit.TrackingType.Equals("YesNo", StringComparison.OrdinalIgnoreCase);

        if (dto.Toggle)
        {
            if (isQuit)
            {
                // In Quit habit, toggling marks a Slip or clears it back to clean
                if (existingCompletion != null && existingCompletion.Status == "Slip")
                {
                    await _habitRepository.DeleteCompletionAsync(existingCompletion, cancellationToken);
                }
                else
                {
                    var slip = new HabitCompletion
                    {
                        HabitId = habitId,
                        UserId = userId,
                        Date = targetDate,
                        Amount = 1.0,
                        Status = "Slip",
                        Note = dto.Note ?? "Desliz registrado",
                        LoggedAt = DateTime.UtcNow
                    };
                    await _habitRepository.AddOrUpdateCompletionAsync(slip, cancellationToken);
                }
            }
            else
            {
                // In Build habit, toggle checks/unchecks
                if (existingCompletion != null && existingCompletion.Status == "Completed")
                {
                    await _habitRepository.DeleteCompletionAsync(existingCompletion, cancellationToken);
                }
                else
                {
                    var comp = new HabitCompletion
                    {
                        HabitId = habitId,
                        UserId = userId,
                        Date = targetDate,
                        Amount = habit.TargetValue,
                        Status = "Completed",
                        Note = dto.Note,
                        LoggedAt = DateTime.UtcNow
                    };
                    await _habitRepository.AddOrUpdateCompletionAsync(comp, cancellationToken);
                    await _gamificationService.RecordActivityAsync(userId, "HabitCheckIn", baseNxp: 10, cancellationToken: cancellationToken);
                }
            }
        }
        else
        {
            double newAmount = dto.Amount ?? (existingCompletion?.Amount ?? 0.0);

            if (dto.Amount == null && habit.OnTapAction.Equals("Increment", StringComparison.OrdinalIgnoreCase))
            {
                newAmount = (existingCompletion?.Amount ?? 0.0) + 1.0;
            }

            string computedStatus = "Partial";
            if (isQuit)
            {
                computedStatus = (dto.Status == "Slip" || newAmount > habit.TargetValue) ? "Slip" : "Completed";
            }
            else
            {
                if (newAmount >= habit.TargetValue)
                {
                    computedStatus = "Completed";
                }
                else if (newAmount <= 0)
                {
                    computedStatus = "Missed";
                }
            }

            if (newAmount <= 0 && !isQuit && string.IsNullOrWhiteSpace(dto.Note))
            {
                if (existingCompletion != null)
                {
                    await _habitRepository.DeleteCompletionAsync(existingCompletion, cancellationToken);
                }
            }
            else
            {
                bool wasCompletedBefore = existingCompletion != null && existingCompletion.Status == "Completed";

                var comp = existingCompletion ?? new HabitCompletion
                {
                    HabitId = habitId,
                    UserId = userId,
                    Date = targetDate
                };

                comp.Amount = newAmount;
                comp.Status = dto.Status ?? computedStatus;
                comp.Note = dto.Note ?? comp.Note;
                comp.LoggedAt = DateTime.UtcNow;

                await _habitRepository.AddOrUpdateCompletionAsync(comp, cancellationToken);

                if (comp.Status == "Completed" && !wasCompletedBefore && !isQuit)
                {
                    await _gamificationService.RecordActivityAsync(userId, "HabitCheckIn", baseNxp: 10, cancellationToken: cancellationToken);
                }
            }
        }

        await RecalculateStreaksAsync(habit, cancellationToken);
        return await MapToDtoAsync(habit, 60, cancellationToken);
    }

    public async Task<HabitStatsDto> GetHabitStatsAsync(Guid userId, Guid habitId, CancellationToken cancellationToken = default)
    {
        var habit = await _habitRepository.GetByIdAsync(userId, habitId, cancellationToken);
        if (habit == null)
        {
            throw new NotFoundException($"El hábito con Id '{habitId}' no fue encontrado.");
        }

        var allCompletions = (await _habitRepository.GetAllCompletionsAsync(habitId, cancellationToken)).ToList();
        var completionsMap = allCompletions.ToDictionary(c => c.Date);

        var fullYearHistory = BuildDayCells(habit, completionsMap, 365);

        int totalCompletions = fullYearHistory.Count(c => c.Status == "Done");
        int totalCleanDays = habit.HabitType.Equals("Quit", StringComparison.OrdinalIgnoreCase)
            ? fullYearHistory.Count(c => c.Status != "Slip")
            : totalCompletions;

        int totalRequiredDays = fullYearHistory.Count(c => c.Status != "Free");
        double consistencyRate = totalRequiredDays > 0
            ? Math.Round((double)totalCompletions / totalRequiredDays * 100.0, 1)
            : 0.0;

        // Weekly trends for the last 12 weeks
        var weeklyTrends = new List<HabitWeeklyProgressDto>();
        for (int w = 11; w >= 0; w--)
        {
            var weekCells = fullYearHistory.Skip(fullYearHistory.Count - (w + 1) * 7).Take(7).ToList();
            if (weekCells.Any())
            {
                int done = weekCells.Count(c => c.Status == "Done");
                int target = weekCells.Count(c => c.Status != "Free");
                weeklyTrends.Add(new HabitWeeklyProgressDto
                {
                    WeekLabel = $"S-{w + 1}",
                    CompletedCount = done,
                    TargetCount = target,
                    Rate = target > 0 ? Math.Round((double)done / target * 100.0, 1) : 0.0
                });
            }
        }

        return new HabitStatsDto
        {
            HabitId = habit.Id,
            CurrentStreak = habit.CurrentStreak,
            BestStreak = habit.BestStreak,
            TotalCompletions = totalCompletions,
            TotalCleanDays = totalCleanDays,
            ConsistencyRate = consistencyRate,
            TotalLoggedDays = allCompletions.Count,
            FullYearHistory = fullYearHistory,
            WeeklyTrends = weeklyTrends
        };
    }

    public async Task<List<HabitDayCellDto>> GetHabitHistoryAsync(Guid userId, Guid habitId, int days = 90, CancellationToken cancellationToken = default)
    {
        var habit = await _habitRepository.GetByIdAsync(userId, habitId, cancellationToken);
        if (habit == null)
        {
            throw new NotFoundException($"El hábito con Id '{habitId}' no fue encontrado.");
        }

        string startDate = DateTime.UtcNow.AddDays(-(days - 1)).ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
        string endDate = DateTime.UtcNow.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);

        var completions = await _habitRepository.GetCompletionsRangeAsync(habitId, startDate, endDate, cancellationToken);
        var completionsMap = completions.ToDictionary(c => c.Date);

        return BuildDayCells(habit, completionsMap, days);
    }

    public async Task ReorderHabitsAsync(Guid userId, ReorderHabitsDto dto, CancellationToken cancellationToken = default)
    {
        var habits = (await _habitRepository.GetAllByUserIdAsync(userId, false, cancellationToken)).ToList();
        for (int i = 0; i < dto.HabitIdsInOrder.Count; i++)
        {
            var id = dto.HabitIdsInOrder[i];
            var habit = habits.FirstOrDefault(h => h.Id == id);
            if (habit != null)
            {
                habit.OrderIndex = i;
                await _habitRepository.UpdateAsync(habit, cancellationToken);
            }
        }
    }

    public async Task HandleAutoCompleteTriggerAsync(Guid userId, string source, double value = 1.0, CancellationToken cancellationToken = default)
    {
        var linkedHabits = await _habitRepository.GetHabitsByAutoSourceAsync(userId, source, cancellationToken);
        string today = DateTime.UtcNow.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);

        foreach (var habit in linkedHabits)
        {
            var existing = await _habitRepository.GetCompletionByDateAsync(habit.Id, today, cancellationToken);
            double newAmount = (existing?.Amount ?? 0.0) + value;

            var comp = existing ?? new HabitCompletion
            {
                HabitId = habit.Id,
                UserId = userId,
                Date = today
            };

            comp.Amount = newAmount;
            comp.Status = newAmount >= habit.TargetValue ? "Completed" : "Partial";
            comp.LoggedAt = DateTime.UtcNow;

            await _habitRepository.AddOrUpdateCompletionAsync(comp, cancellationToken);
            await RecalculateStreaksAsync(habit, cancellationToken);
        }
    }

    private async Task RecalculateStreaksAsync(Habit habit, CancellationToken cancellationToken)
    {
        var allCompletions = (await _habitRepository.GetAllCompletionsAsync(habit.Id, cancellationToken)).ToList();
        var completionsMap = allCompletions.ToDictionary(c => c.Date);

        bool isQuit = habit.HabitType.Equals("Quit", StringComparison.OrdinalIgnoreCase);
        DateTime today = DateTime.UtcNow.Date;

        if (isQuit)
        {
            // For Quit habits: streak is consecutive days without a "Slip"
            int streak = 0;
            var cur = today;

            // Look back from today
            while (cur >= habit.CreatedAt.Date)
            {
                string dateStr = cur.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
                if (completionsMap.TryGetValue(dateStr, out var comp) && (comp.Status == "Slip" || comp.Amount > habit.TargetValue))
                {
                    break;
                }
                streak++;
                cur = cur.AddDays(-1);
            }

            habit.CurrentStreak = streak;
            if (habit.CurrentStreak > habit.BestStreak)
            {
                habit.BestStreak = habit.CurrentStreak;
            }
        }
        else
        {
            // For Build habits: streak counts consecutive required days completed
            int streak = 0;
            DateTime checkDate = today;

            string todayStr = today.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
            bool isTodayDone = completionsMap.TryGetValue(todayStr, out var todayComp) && todayComp.Status == "Completed";
            bool isTodayFree = IsFreeDay(habit, today);

            if (isTodayDone || isTodayFree)
            {
                if (isTodayDone) streak++;
                checkDate = checkDate.AddDays(-1);
            }
            else
            {
                // Today not completed yet: don't break streak yet; check from yesterday
                checkDate = checkDate.AddDays(-1);
            }

            while (true)
            {
                string dateStr = checkDate.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
                bool isFree = IsFreeDay(habit, checkDate);

                if (completionsMap.TryGetValue(dateStr, out var comp) && comp.Status == "Completed")
                {
                    streak++;
                }
                else if (isFree)
                {
                    // Free day doesn't add nor break streak
                }
                else
                {
                    // Required day not completed -> streak broken
                    break;
                }

                checkDate = checkDate.AddDays(-1);
                if (checkDate < habit.CreatedAt.Date.AddDays(-365)) break; // reasonable boundary
            }

            habit.CurrentStreak = streak;
            if (habit.CurrentStreak > habit.BestStreak)
            {
                habit.BestStreak = habit.CurrentStreak;
            }

            if (isTodayDone)
            {
                habit.LastCompletedDate = DateTime.UtcNow;
            }
        }

        await _habitRepository.UpdateAsync(habit, cancellationToken);
    }

    private static bool IsFreeDay(Habit habit, DateTime date)
    {
        if (habit.FrequencyType.Equals("SpecificDays", StringComparison.OrdinalIgnoreCase) &&
            !string.IsNullOrWhiteSpace(habit.FrequencyDays))
        {
            int dayOfWeek = ((int)date.DayOfWeek == 0) ? 7 : (int)date.DayOfWeek; // 1=Mon, 7=Sun
            var activeDays = habit.FrequencyDays.Split(',').Select(s => s.Trim()).ToList();
            if (!activeDays.Contains(dayOfWeek.ToString()))
            {
                return true;
            }
        }
        return false;
    }

    private static List<HabitDayCellDto> BuildDayCells(Habit habit, Dictionary<string, HabitCompletion> completionsMap, int days)
    {
        var cells = new List<HabitDayCellDto>();
        DateTime today = DateTime.UtcNow.Date;
        bool isQuit = habit.HabitType.Equals("Quit", StringComparison.OrdinalIgnoreCase);

        for (int i = days - 1; i >= 0; i--)
        {
            DateTime date = today.AddDays(-i);
            string dateStr = date.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
            bool isToday = (i == 0);
            bool isFree = IsFreeDay(habit, date);

            completionsMap.TryGetValue(dateStr, out var comp);
            double amount = comp?.Amount ?? 0.0;
            string? note = comp?.Note;

            string status;
            double percentage = 0.0;

            if (isQuit)
            {
                if (comp != null && (comp.Status == "Slip" || comp.Amount > habit.TargetValue))
                {
                    status = "Slip";
                    percentage = 0.0;
                }
                else
                {
                    status = "Done"; // Clean day
                    percentage = 100.0;
                }
            }
            else
            {
                if (comp != null && comp.Status == "Completed")
                {
                    status = "Done";
                    percentage = 100.0;
                }
                else if (amount > 0)
                {
                    status = "Partial";
                    percentage = habit.TargetValue > 0 ? Math.Min(100.0, Math.Round(amount / habit.TargetValue * 100.0, 1)) : 50.0;
                }
                else if (isFree)
                {
                    status = "Free";
                    percentage = 0.0;
                }
                else
                {
                    status = "Missed";
                    percentage = 0.0;
                }
            }

            cells.Add(new HabitDayCellDto
            {
                Date = dateStr,
                Status = status,
                Amount = amount,
                TargetValue = habit.TargetValue,
                Percentage = percentage,
                Note = note,
                IsToday = isToday
            });
        }

        return cells;
    }

    private async Task<HabitDto> MapToDtoAsync(Habit habit, int recentDays, CancellationToken cancellationToken)
    {
        string todayStr = DateTime.UtcNow.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
        string startDateStr = DateTime.UtcNow.AddDays(-(recentDays - 1)).ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);

        var completions = await _habitRepository.GetCompletionsRangeAsync(habit.Id, startDateStr, todayStr, cancellationToken);
        var completionsMap = completions.ToDictionary(c => c.Date);

        var recentCells = BuildDayCells(habit, completionsMap, recentDays);
        var todayCell = recentCells.FirstOrDefault(c => c.IsToday);

        var todayStatus = new TodayHabitStatusDto
        {
            Date = todayStr,
            IsCompleted = todayCell != null && todayCell.Status == "Done",
            CurrentAmount = todayCell?.Amount ?? 0.0,
            TargetValue = habit.TargetValue,
            Percentage = todayCell?.Percentage ?? 0.0,
            Status = todayCell?.Status ?? "Missed",
            Note = todayCell?.Note
        };

        return new HabitDto
        {
            Id = habit.Id,
            UserId = habit.UserId,
            Name = habit.Name,
            Description = habit.Description,
            Icon = habit.Icon,
            Color = habit.Color,
            Category = habit.Category,
            HabitType = habit.HabitType,
            TrackingType = habit.TrackingType,
            TargetValue = habit.TargetValue,
            TargetUnit = habit.TargetUnit,
            OnTapAction = habit.OnTapAction,
            FrequencyType = habit.FrequencyType,
            FrequencyDays = habit.FrequencyDays,
            FrequencyTargetCount = habit.FrequencyTargetCount,
            EveryNDaysInterval = habit.EveryNDaysInterval,
            StreakUnit = habit.StreakUnit,
            StreakGoal = habit.StreakGoal,
            FillMetPeriods = habit.FillMetPeriods,
            AutoCompleteSource = habit.AutoCompleteSource,
            CurrentStreak = habit.CurrentStreak,
            BestStreak = habit.BestStreak,
            LastCompletedDate = habit.LastCompletedDate,
            IsArchived = habit.IsArchived,
            OrderIndex = habit.OrderIndex,
            CreatedAt = habit.CreatedAt,
            Reminders = habit.Reminders.Select(r => new HabitReminderDto
            {
                Id = r.Id,
                Time = r.Time,
                DaysOfWeek = r.DaysOfWeek,
                IsEnabled = r.IsEnabled
            }).ToList(),
            TodayStatus = todayStatus,
            RecentHistory = recentCells
        };
    }
}
