using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Nexodus_Back.Application.DTOs;

namespace Nexodus_Back.Application.Interfaces;

/// <summary>
/// Defines workout log tracking operations.
/// </summary>
public interface IWorkoutLogService
{
    Task<IEnumerable<WorkoutLogDto>> GetAllByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<WorkoutLogDto> GetByIdAsync(Guid id, Guid userId, CancellationToken cancellationToken = default);
    Task<WorkoutLogDto> CreateAsync(CreateWorkoutLogDto dto, Guid userId, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid id, Guid userId, CancellationToken cancellationToken = default);
}
