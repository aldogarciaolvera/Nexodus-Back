using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Nexodus_Back.Core.Entities;

namespace Nexodus_Back.Core.Interfaces;

public interface IRecipeRepository
{
    Task<Recipe?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<(List<Recipe> Items, int TotalCount)> SearchAsync(
        string? query,
        string? mealType,
        string? dietType,
        Guid? userId,
        bool communityOnly,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default);
    Task<List<Recipe>> GetVariantsAsync(Guid parentRecipeId, CancellationToken cancellationToken = default);
    Task<Recipe> CreateAsync(Recipe recipe, CancellationToken cancellationToken = default);
    Task<Recipe> UpdateAsync(Recipe recipe, CancellationToken cancellationToken = default);
    Task DeleteAsync(Recipe recipe, CancellationToken cancellationToken = default);
}
