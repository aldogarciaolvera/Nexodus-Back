using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Nexodus_Back.Application.DTOs;

namespace Nexodus_Back.Application.Interfaces;

/// <summary>
/// Defines routine management operations.
/// </summary>
public interface IRoutineService
{
    Task<IEnumerable<RoutineDto>> GetAllRoutinesAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<RoutineDto> GetRoutineByIdAsync(Guid id, Guid userId, CancellationToken cancellationToken = default);
    Task<RoutineDto> CreateRoutineAsync(Guid userId, CreateRoutineDto dto, CancellationToken cancellationToken = default);
    Task<RoutineDto> UpdateRoutineAsync(Guid id, Guid userId, CreateRoutineDto dto, CancellationToken cancellationToken = default);
    Task DeleteRoutineAsync(Guid id, Guid userId, CancellationToken cancellationToken = default);
}
