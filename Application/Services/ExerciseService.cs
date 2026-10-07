using Nexodus_Back.Application.DTOs;
using Nexodus_Back.Core.Interfaces;

namespace Nexodus_Back.Application.Services;

public class ExerciseService
{
    private readonly IExerciseRepository _exerciseRepository;
    private readonly IStorageService _storageService;

    public ExerciseService(IExerciseRepository exerciseRepository, IStorageService storageService)
    {
        _exerciseRepository = exerciseRepository;
        _storageService = storageService;
    }

    public async Task<IEnumerable<ExerciseDto>> GetAllExercisesAsync()
    {
        var exercises = await _exerciseRepository.GetAllAsync();
        var dtos = new List<ExerciseDto>();
        
        foreach (var ex in exercises)
        {
            dtos.Add(await MapToDto(ex));
        }

        return dtos;
    }

    public async Task<ExerciseDto?> GetExerciseByIdAsync(Guid id)
    {
        var ex = await _exerciseRepository.GetByIdAsync(id);
        if (ex == null) return null;

        return await MapToDto(ex);
    }

    private async Task<ExerciseDto> MapToDto(Core.Entities.Exercise ex)
    {
        var gifUrl = string.IsNullOrEmpty(ex.GifS3Key) ? null : await _storageService.GetFileUrlAsync(ex.GifS3Key);
        var thumbUrl = string.IsNullOrEmpty(ex.ThumbS3Key) ? null : await _storageService.GetFileUrlAsync(ex.ThumbS3Key);

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
