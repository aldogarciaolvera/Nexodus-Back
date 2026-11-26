using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Nexodus_Back.Core.Entities;
using Nexodus_Back.Core.Interfaces;
using Nexodus_Back.Infrastructure.Data;

namespace Nexodus_Back.Infrastructure.Repositories;

public class HabitRepository(NexodusDbContext context) : IHabitRepository
{
    private readonly NexodusDbContext _context = context ?? throw new ArgumentNullException(nameof(context));

    public async Task<Habit> CreateAsync(Habit habit, CancellationToken cancellationToken = default)
    {
        _context.Habits.Add(habit);
        await _context.SaveChangesAsync(cancellationToken);
        return habit;
    }

    public async Task<IEnumerable<Habit>> GetAllByUserIdAsync(Guid userId, bool includeArchived = false, CancellationToken cancellationToken = default)
    {
        var query = _context.Habits
            .AsNoTracking()
            .Include(h => h.Reminders)
            .Where(h => h.UserId == userId);

        if (!includeArchived)
        {
            query = query.Where(h => !h.IsArchived);
        }

        return await query
            .OrderBy(h => h.OrderIndex)
            .ThenByDescending(h => h.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<Habit?> GetByIdAsync(Guid userId, Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Habits
            .Include(h => h.Reminders)
            .FirstOrDefaultAsync(h => h.Id == id && h.UserId == userId, cancellationToken);
    }

    public async Task<Habit> UpdateAsync(Habit habit, CancellationToken cancellationToken = default)
    {
        habit.UpdatedAt = DateTime.UtcNow;
        _context.Habits.Update(habit);
        await _context.SaveChangesAsync(cancellationToken);
        return habit;
    }

    public async Task DeleteAsync(Habit habit, CancellationToken cancellationToken = default)
    {
        _context.Habits.Remove(habit);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<HabitCompletion?> GetCompletionByDateAsync(Guid habitId, string date, CancellationToken cancellationToken = default)
    {
        return await _context.HabitCompletions
            .FirstOrDefaultAsync(c => c.HabitId == habitId && c.Date == date, cancellationToken);
    }

    public async Task<IEnumerable<HabitCompletion>> GetCompletionsRangeAsync(Guid habitId, string startDate, string endDate, CancellationToken cancellationToken = default)
    {
        return await _context.HabitCompletions
            .AsNoTracking()
            .Where(c => c.HabitId == habitId && string.Compare(c.Date, startDate) >= 0 && string.Compare(c.Date, endDate) <= 0)
            .OrderBy(c => c.Date)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<HabitCompletion>> GetAllCompletionsAsync(Guid habitId, CancellationToken cancellationToken = default)
    {
        return await _context.HabitCompletions
            .AsNoTracking()
            .Where(c => c.HabitId == habitId)
            .OrderBy(c => c.Date)
            .ToListAsync(cancellationToken);
    }

    public async Task<HabitCompletion> AddOrUpdateCompletionAsync(HabitCompletion completion, CancellationToken cancellationToken = default)
    {
        var existing = await _context.HabitCompletions
            .FirstOrDefaultAsync(c => c.HabitId == completion.HabitId && c.Date == completion.Date, cancellationToken);

        if (existing != null)
        {
            existing.Amount = completion.Amount;
            existing.Status = completion.Status;
            existing.Note = completion.Note;
            existing.LoggedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync(cancellationToken);
            return existing;
        }

        _context.HabitCompletions.Add(completion);
        await _context.SaveChangesAsync(cancellationToken);
        return completion;
    }

    public async Task DeleteCompletionAsync(HabitCompletion completion, CancellationToken cancellationToken = default)
    {
        _context.HabitCompletions.Remove(completion);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<IEnumerable<Habit>> GetHabitsByAutoSourceAsync(Guid userId, string autoSource, CancellationToken cancellationToken = default)
    {
        return await _context.Habits
            .Where(h => h.UserId == userId && !h.IsArchived && h.AutoCompleteSource == autoSource)
            .ToListAsync(cancellationToken);
    }
}
