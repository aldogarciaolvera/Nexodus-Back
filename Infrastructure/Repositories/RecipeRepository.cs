using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Nexodus_Back.Core.Entities;
using Nexodus_Back.Core.Interfaces;
using Nexodus_Back.Infrastructure.Data;

namespace Nexodus_Back.Infrastructure.Repositories;

/// <summary>
/// Entity Framework implementation of recipe catalog and community repository.
/// </summary>
public class RecipeRepository(NexodusDbContext context) : IRecipeRepository
{
    private readonly NexodusDbContext _context = context ?? throw new ArgumentNullException(nameof(context));

    public async Task<Recipe?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Recipes
            .Include(r => r.Ingredients.OrderBy(i => i.Id))
                .ThenInclude(i => i.Food)
            .Include(r => r.Steps.OrderBy(s => s.StepNumber))
            .Include(r => r.User)
            .Include(r => r.ParentRecipe)
            .FirstOrDefaultAsync(r => r.Id == id, cancellationToken);
    }

    public async Task<(List<Recipe> Items, int TotalCount)> SearchAsync(
        string? query,
        string? mealType,
        string? dietType,
        Guid? userId,
        bool communityOnly,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        var baseQuery = _context.Recipes
            .AsNoTracking()
            .Include(r => r.Ingredients)
            .Include(r => r.Steps)
            .Include(r => r.User)
            .AsQueryable();

        if (communityOnly)
        {
            baseQuery = baseQuery.Where(r => r.IsCommunity);
        }
        else if (userId.HasValue)
        {
            // Ver recetas del sistema (UserId == null), recetas propias del usuario, o recetas comunitarias
            baseQuery = baseQuery.Where(r => r.UserId == null || r.UserId == userId.Value || r.IsCommunity);
        }
        else
        {
            // Sin usuario autenticado, solo recetas del sistema o públicas
            baseQuery = baseQuery.Where(r => r.UserId == null || r.IsCommunity);
        }

        if (!string.IsNullOrWhiteSpace(mealType) && !mealType.Equals("Any", StringComparison.OrdinalIgnoreCase))
        {
            baseQuery = baseQuery.Where(r => r.MealType == mealType || r.MealType == "Any");
        }

        if (!string.IsNullOrWhiteSpace(dietType) && !dietType.Equals("Standard", StringComparison.OrdinalIgnoreCase))
        {
            baseQuery = baseQuery.Where(r => r.DietType == dietType);
        }

        if (!string.IsNullOrWhiteSpace(query))
        {
            var lower = query.Trim().ToLower();
            baseQuery = baseQuery.Where(r =>
                r.Name.ToLower().Contains(lower) ||
                r.Description.ToLower().Contains(lower) ||
                r.Tags.ToLower().Contains(lower));
        }

        var totalCount = await baseQuery.CountAsync(cancellationToken);

        var items = await baseQuery
            .OrderByDescending(r => r.UserId == null) // Prioriza base oficial
            .ThenByDescending(r => r.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (items, totalCount);
    }

    public async Task<List<Recipe>> GetVariantsAsync(Guid parentRecipeId, CancellationToken cancellationToken = default)
    {
        return await _context.Recipes
            .AsNoTracking()
            .Include(r => r.Ingredients)
            .Include(r => r.Steps)
            .Include(r => r.User)
            .Where(r => r.ParentRecipeId == parentRecipeId)
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<Recipe> CreateAsync(Recipe recipe, CancellationToken cancellationToken = default)
    {
        recipe.CreatedAt = DateTime.UtcNow;
        recipe.UpdatedAt = DateTime.UtcNow;

        await _context.Recipes.AddAsync(recipe, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
        return recipe;
    }

    public async Task<Recipe> UpdateAsync(Recipe recipe, CancellationToken cancellationToken = default)
    {
        recipe.UpdatedAt = DateTime.UtcNow;
        _context.Recipes.Update(recipe);
        await _context.SaveChangesAsync(cancellationToken);
        return recipe;
    }

    public async Task DeleteAsync(Recipe recipe, CancellationToken cancellationToken = default)
    {
        _context.Recipes.Remove(recipe);
        await _context.SaveChangesAsync(cancellationToken);
    }
}
