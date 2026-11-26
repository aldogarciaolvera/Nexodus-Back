using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Nexodus_Back.Application.DTOs.Diet;

namespace Nexodus_Back.Application.Interfaces;

/// <summary>
/// Service providing recipe management, community cookbook discovery, recipe variations and meal logging.
/// </summary>
public interface IRecipeService
{
    /// <summary>
    /// Retrieves a recipe by ID with authorization checks for private recipes.
    /// </summary>
    Task<RecipeDto> GetByIdAsync(Guid id, Guid? userId = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Searches recipes across official catalog, user private recipes and community entries.
    /// </summary>
    Task<(List<RecipeDto> Items, int TotalCount)> SearchAsync(
        string? query,
        string? mealType,
        string? dietType,
        Guid? userId,
        bool communityOnly,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves recipe variations derived from a parent recipe.
    /// </summary>
    Task<List<RecipeDto>> GetVariantsAsync(Guid parentRecipeId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Creates a new user recipe or publishes it to the community.
    /// </summary>
    Task<RecipeDto> CreateAsync(Guid userId, CreateRecipeDto dto, CancellationToken cancellationToken = default);

    /// <summary>
    /// Creates a personalized variation of an existing parent recipe.
    /// </summary>
    Task<RecipeDto> CreateVariantAsync(Guid parentRecipeId, Guid userId, CreateRecipeDto dto, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates details, ingredients or steps of an owned recipe.
    /// </summary>
    Task<RecipeDto> UpdateAsync(Guid id, Guid userId, UpdateRecipeDto dto, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes an owned recipe.
    /// </summary>
    Task DeleteAsync(Guid id, Guid userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Logs a recipe as a meal in the daily dietary log, registering each ingredient atomically.
    /// </summary>
    Task<MealDto> LogRecipeAsMealAsync(Guid userId, LogRecipeAsMealDto dto, CancellationToken cancellationToken = default);
}
