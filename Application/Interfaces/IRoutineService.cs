using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Nexodus_Back.Application.DTOs;

namespace Nexodus_Back.Application.Interfaces;

/// <summary>
/// Service providing routine creation, exercise arrangement, and routine lifecycle management.
/// </summary>
public interface IRoutineService
{
    /// <summary>
    /// Retrieves all workout routines created by the specified user.
    /// </summary>
    Task<IEnumerable<RoutineDto>> GetAllRoutinesAsync(Guid userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves a single workout routine by ID and user ID.
    /// </summary>
    Task<RoutineDto> GetRoutineByIdAsync(Guid id, Guid userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Creates a new workout routine with configured exercises and target sets.
    /// </summary>
    Task<RoutineDto> CreateRoutineAsync(Guid userId, CreateRoutineDto dto, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates an existing workout routine.
    /// </summary>
    Task<RoutineDto> UpdateRoutineAsync(Guid id, Guid userId, CreateRoutineDto dto, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes a workout routine.
    /// </summary>
    Task DeleteRoutineAsync(Guid id, Guid userId, CancellationToken cancellationToken = default);
}
