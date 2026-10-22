using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Nexodus_Back.Application.DTOs;

namespace Nexodus_Back.Application.Interfaces;

/// <summary>
/// Defines exercise catalog operations.
/// </summary>
public interface IExerciseService
{
    Task<IEnumerable<ExerciseDto>> GetAllExercisesAsync(int page = 1, int pageSize = 50, CancellationToken cancellationToken = default);
    Task<ExerciseDto> GetExerciseByIdAsync(Guid id, CancellationToken cancellationToken = default);
}
