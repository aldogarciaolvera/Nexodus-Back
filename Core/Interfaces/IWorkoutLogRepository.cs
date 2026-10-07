using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Nexodus_Back.Core.Entities;

namespace Nexodus_Back.Core.Interfaces
{
    public interface IWorkoutLogRepository
    {
        Task<WorkoutLog?> GetByIdAsync(Guid id);
        Task<IEnumerable<WorkoutLog>> GetAllByUserIdAsync(Guid userId);
        Task AddAsync(WorkoutLog workoutLog);
        Task DeleteAsync(WorkoutLog workoutLog);
    }
}
