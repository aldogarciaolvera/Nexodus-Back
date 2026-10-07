using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Nexodus_Back.Core.Entities;
using Nexodus_Back.Core.Interfaces;
using Nexodus_Back.Infrastructure.Data;

namespace Nexodus_Back.Infrastructure.Repositories
{
    public class WorkoutLogRepository : IWorkoutLogRepository
    {
        private readonly NexodusDbContext _context;

        public WorkoutLogRepository(NexodusDbContext context)
        {
            _context = context;
        }

        public async Task<WorkoutLog?> GetByIdAsync(Guid id)
        {
            return await _context.WorkoutLogs
                .Include(w => w.Routine)
                .FirstOrDefaultAsync(w => w.Id == id);
        }

        public async Task<IEnumerable<WorkoutLog>> GetAllByUserIdAsync(Guid userId)
        {
            return await _context.WorkoutLogs
                .Include(w => w.Routine)
                .Where(w => w.UserId == userId)
                .OrderByDescending(w => w.DateCompleted)
                .ToListAsync();
        }

        public async Task AddAsync(WorkoutLog workoutLog)
        {
            await _context.WorkoutLogs.AddAsync(workoutLog);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(WorkoutLog workoutLog)
        {
            _context.WorkoutLogs.Remove(workoutLog);
            await _context.SaveChangesAsync();
        }
    }
}
