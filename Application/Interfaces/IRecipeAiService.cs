using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Nexodus_Back.Application.DTOs.Diet;

namespace Nexodus_Back.Application.Interfaces;

/// <summary>
/// Service providing AI-driven recipe generation matching remaining daily macros and pantry inventory.
/// </summary>
public interface IRecipeAiService
{
    Task<RecipeDto> GenerateRecipeForRemainingMacrosAsync(
        Guid userId,
        MatchMacrosRecipeRequestDto request,
        CancellationToken cancellationToken = default);

    Task<List<PantryCookSuggestionDto>> SuggestRecipesFromPantryAsync(
        Guid userId,
        PantryCookRequestDto request,
        CancellationToken cancellationToken = default);
}

