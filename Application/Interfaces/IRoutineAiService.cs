using System;
using System.Threading;
using System.Threading.Tasks;
using Nexodus_Back.Application.DTOs;

namespace Nexodus_Back.Application.Interfaces;

/// <summary>
/// Service providing AI-driven workout routine generation and single-exercise swaps
/// strictly grounded in the database exercise catalog.
/// </summary>
public interface IRoutineAiService
{
    Task<GeneratedAiRoutineResponseDto> GenerateRoutineAsync(
        Guid userId,
        GenerateAiRoutineRequestDto request,
        CancellationToken cancellationToken = default);

    Task<GeneratedAiRoutineExerciseDto> SwapExerciseAsync(
        Guid userId,
        SwapAiExerciseRequestDto request,
        CancellationToken cancellationToken = default);
}
