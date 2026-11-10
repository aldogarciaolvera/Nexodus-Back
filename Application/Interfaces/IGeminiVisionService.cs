using System.Threading;
using System.Threading.Tasks;
using Nexodus_Back.Application.DTOs.Diet;

namespace Nexodus_Back.Application.Interfaces;

/// <summary>
/// Service for AI-driven visual meal and macronutrient analysis using Google Gemini Multimodal Vision API.
/// </summary>
public interface IGeminiVisionService
{
    Task<FoodAnalysisResultDto> AnalyzeMealImageAsync(
        string base64Image,
        string? mimeType,
        string? additionalContext,
        string? hint = null,
        CancellationToken cancellationToken = default
    );
}

