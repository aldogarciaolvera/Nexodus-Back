using System;
using System.Collections.Generic;

namespace Nexodus_Back.Application.DTOs.Habit;

public class HabitDto
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string Icon { get; set; } = "check";
    public string Color { get; set; } = "#00F0FF";
    public string Category { get; set; } = "General";
    public string HabitType { get; set; } = "Build";
    public string TrackingType { get; set; } = "YesNo";
    public double TargetValue { get; set; } = 1.0;
    public string? TargetUnit { get; set; }
    public string OnTapAction { get; set; } = "Increment";
    public string FrequencyType { get; set; } = "Daily";
    public string? FrequencyDays { get; set; }
    public int FrequencyTargetCount { get; set; } = 1;
    public int EveryNDaysInterval { get; set; } = 1;
    public string StreakUnit { get; set; } = "Days";
    public int? StreakGoal { get; set; }
    public bool FillMetPeriods { get; set; } = true;
    public string? AutoCompleteSource { get; set; }
    public int CurrentStreak { get; set; }
    public int BestStreak { get; set; }
    public DateTime? LastCompletedDate { get; set; }
    public bool IsArchived { get; set; }
    public int OrderIndex { get; set; }
    public DateTime CreatedAt { get; set; }

    public List<HabitReminderDto> Reminders { get; set; } = new();
    public TodayHabitStatusDto TodayStatus { get; set; } = new();
    public List<HabitDayCellDto> RecentHistory { get; set; } = new();
}

public class TodayHabitStatusDto
{
    public string Date { get; set; } = string.Empty;
    public bool IsCompleted { get; set; }
    public double CurrentAmount { get; set; }
    public double TargetValue { get; set; }
    public double Percentage { get; set; }
    public string Status { get; set; } = "Missed";
    public string? Note { get; set; }
}

public class HabitDayCellDto
{
    public string Date { get; set; } = string.Empty;
    public string Status { get; set; } = "Missed"; // Done, Partial, Free, Missed, Slip
    public double Amount { get; set; }
    public double TargetValue { get; set; }
    public double Percentage { get; set; }
    public string? Note { get; set; }
    public bool IsToday { get; set; }
}

public class CreateHabitDto
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string Icon { get; set; } = "check";
    public string Color { get; set; } = "#00F0FF";
    public string Category { get; set; } = "General";
    public string HabitType { get; set; } = "Build";
    public string TrackingType { get; set; } = "YesNo";
    public double TargetValue { get; set; } = 1.0;
    public string? TargetUnit { get; set; }
    public string OnTapAction { get; set; } = "Increment";
    public string FrequencyType { get; set; } = "Daily";
    public string? FrequencyDays { get; set; }
    public int FrequencyTargetCount { get; set; } = 1;
    public int EveryNDaysInterval { get; set; } = 1;
    public string StreakUnit { get; set; } = "Days";
    public int? StreakGoal { get; set; }
    public bool FillMetPeriods { get; set; } = true;
    public string? AutoCompleteSource { get; set; }
    public List<CreateHabitReminderDto>? Reminders { get; set; }
}

public class UpdateHabitDto : CreateHabitDto
{
    public bool IsArchived { get; set; }
    public int OrderIndex { get; set; }
}

public class HabitReminderDto
{
    public Guid Id { get; set; }
    public string Time { get; set; } = "08:00";
    public string? DaysOfWeek { get; set; }
    public bool IsEnabled { get; set; } = true;
}

public class CreateHabitReminderDto
{
    public string Time { get; set; } = "08:00";
    public string? DaysOfWeek { get; set; }
    public bool IsEnabled { get; set; } = true;
}

public class HabitCheckInDto
{
    public string? Date { get; set; }
    public double? Amount { get; set; }
    public string? Status { get; set; }
    public string? Note { get; set; }
    public bool Toggle { get; set; } = false;
}

public class HabitStatsDto
{
    public Guid HabitId { get; set; }
    public int CurrentStreak { get; set; }
    public int BestStreak { get; set; }
    public int TotalCompletions { get; set; }
    public int TotalCleanDays { get; set; }
    public double ConsistencyRate { get; set; }
    public int TotalLoggedDays { get; set; }
    public List<HabitDayCellDto> FullYearHistory { get; set; } = new();
    public List<HabitWeeklyProgressDto> WeeklyTrends { get; set; } = new();
}

public class HabitWeeklyProgressDto
{
    public string WeekLabel { get; set; } = string.Empty;
    public int CompletedCount { get; set; }
    public int TargetCount { get; set; }
    public double Rate { get; set; }
}

public class ReorderHabitsDto
{
    public List<Guid> HabitIdsInOrder { get; set; } = new();
}
