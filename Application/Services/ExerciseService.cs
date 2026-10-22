using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Nexodus_Back.Application.DTOs;
using Nexodus_Back.Application.Interfaces;
using Nexodus_Back.Core.Entities;
using Nexodus_Back.Core.Exceptions;
using Nexodus_Back.Core.Interfaces;

namespace Nexodus_Back.Application.Services;

/// <summary>
/// Provides exercise catalog queries and asset URL resolution.
/// </summary>
public class ExerciseService(IExerciseRepository exerciseRepository, IStorageService storageService) : IExerciseService
{
    private readonly IExerciseRepository _exerciseRepository = exerciseRepository ?? throw new ArgumentNullException(nameof(exerciseRepository));
    private readonly IStorageService _storageService = storageService ?? throw new ArgumentNullException(nameof(storageService));

    public async Task<IEnumerable<ExerciseDto>> GetAllExercisesAsync(int page = 1, int pageSize = 50, CancellationToken cancellationToken = default)
    {
        var exercises = await _exerciseRepository.GetAllAsync(page, pageSize, cancellationToken);
        var tasks = exercises.Select(ex => MapToDtoAsync(ex, cancellationToken));
        return await Task.WhenAll(tasks);
    }

    public async Task<ExerciseDto> GetExerciseByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var ex = await _exerciseRepository.GetByIdAsync(id, cancellationToken);
        if (ex == null)
        {
            throw new NotFoundException($"No se encontró un ejercicio con ID '{id}'.");
        }

        return await MapToDtoAsync(ex, cancellationToken);
    }

    private async Task<ExerciseDto> MapToDtoAsync(Exercise ex, CancellationToken cancellationToken)
    {
        var gifTask = string.IsNullOrEmpty(ex.GifS3Key) ? null : _storageService.GetFileUrlAsync(ex.GifS3Key);
        var thumbTask = string.IsNullOrEmpty(ex.ThumbS3Key) ? null : _storageService.GetFileUrlAsync(ex.ThumbS3Key);

        var gifUrl = gifTask != null ? await gifTask : null;
        var thumbUrl = thumbTask != null ? await thumbTask : null;

        return new ExerciseDto(
            ex.Id,
            ex.ExternalId,
            ex.Name,
            ex.BodyPart,
            ex.Muscle,
            ex.Equipment,
            ex.Category,
            ex.Instructions,
            gifUrl,
            thumbUrl
        );
    }
}
