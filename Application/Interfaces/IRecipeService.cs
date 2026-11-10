using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Nexodus_Back.Application.DTOs.Diet;

namespace Nexodus_Back.Application.Interfaces;

public interface IRecipeService
{
    Task<RecipeDto> GetByIdAsync(Guid id, Guid? userId = null, CancellationToken cancellationToken = default);
    Task<(List<RecipeDto> Items, int TotalCount)> SearchAsync(
        string? query,
        string? mealType,
        string? dietType,
        Guid? userId,
        bool communityOnly,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default);
    Task<List<RecipeDto>> GetVariantsAsync(Guid parentRecipeId, CancellationToken cancellationToken = default);
    Task<RecipeDto> CreateAsync(Guid userId, CreateRecipeDto dto, CancellationToken cancellationToken = default);
    Task<RecipeDto> CreateVariantAsync(Guid parentRecipeId, Guid userId, CreateRecipeDto dto, CancellationToken cancellationToken = default);
    Task<RecipeDto> UpdateAsync(Guid id, Guid userId, UpdateRecipeDto dto, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid id, Guid userId, CancellationToken cancellationToken = default);
    Task<MealDto> LogRecipeAsMealAsync(Guid userId, LogRecipeAsMealDto dto, CancellationToken cancellationToken = default);
}
