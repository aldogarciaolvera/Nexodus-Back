using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Nexodus_Back.Application.DTOs.Diet;
using Nexodus_Back.Application.Interfaces;
using Nexodus_Back.Core.Entities;
using Nexodus_Back.Core.Exceptions;
using Nexodus_Back.Core.Interfaces;

namespace Nexodus_Back.Application.Services;

/// <summary>
/// Service managing healthy recipes, community sharing, variant derivation, and meal diary logging.
/// </summary>
public class RecipeService(
    IRecipeRepository recipeRepo,
    IMealRepository mealRepo,
    IUserRepository userRepo,
    IFoodRepository foodRepo) : IRecipeService
{
    private readonly IRecipeRepository _recipeRepo = recipeRepo ?? throw new ArgumentNullException(nameof(recipeRepo));
    private readonly IMealRepository _mealRepo = mealRepo ?? throw new ArgumentNullException(nameof(mealRepo));
    private readonly IUserRepository _userRepo = userRepo ?? throw new ArgumentNullException(nameof(userRepo));
    private readonly IFoodRepository _foodRepo = foodRepo ?? throw new ArgumentNullException(nameof(foodRepo));

    public async Task<RecipeDto> GetByIdAsync(Guid id, Guid? userId = null, CancellationToken cancellationToken = default)
    {
        var recipe = await _recipeRepo.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException($"Receta con ID '{id}' no encontrada.");

        var variants = await _recipeRepo.GetVariantsAsync(id, cancellationToken);
        return MapToDto(recipe, variants.Count);
    }

    public async Task<(List<RecipeDto> Items, int TotalCount)> SearchAsync(
        string? query,
        string? mealType,
        string? dietType,
        Guid? userId,
        bool communityOnly,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        var pageNumber = page < 1 ? 1 : page;
        var limit = pageSize < 1 ? 20 : Math.Min(pageSize, 50);

        var (items, totalCount) = await _recipeRepo.SearchAsync(
            query,
            mealType,
            dietType,
            userId,
            communityOnly,
            pageNumber,
            limit,
            cancellationToken);

        var dtos = items.Select(r => MapToDto(r)).ToList();
        return (dtos, totalCount);
    }

    public async Task<List<RecipeDto>> GetVariantsAsync(Guid parentRecipeId, CancellationToken cancellationToken = default)
    {
        var variants = await _recipeRepo.GetVariantsAsync(parentRecipeId, cancellationToken);
        return variants.Select(v => MapToDto(v)).ToList();
    }

    public async Task<RecipeDto> CreateAsync(Guid userId, CreateRecipeDto dto, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(dto.Name))
            throw new ValidationException("El nombre de la receta es obligatorio.");

        var user = await _userRepo.GetByIdAsync(userId)
            ?? throw new NotFoundException("Usuario no encontrado.");

        var recipeId = Guid.NewGuid();
        var ingredients = MapIngredientsFromDto(dto.Ingredients, recipeId);
        var steps = MapStepsFromDto(dto.Steps, recipeId);

        // Calculate totals if not provided or to ensure precision
        var totalCalories = ingredients.Sum(i => i.Calories);
        var totalProtein = ingredients.Sum(i => i.Protein);
        var totalCarbs = ingredients.Sum(i => i.Carbs);
        var totalFat = ingredients.Sum(i => i.Fat);

        var recipe = new Recipe
        {
            Id = recipeId,
            UserId = userId,
            Name = dto.Name.Trim(),
            Description = dto.Description?.Trim() ?? string.Empty,
            MealType = string.IsNullOrWhiteSpace(dto.MealType) ? "Any" : dto.MealType,
            DietType = string.IsNullOrWhiteSpace(dto.DietType) ? "Standard" : dto.DietType,
            PrepTimeMinutes = dto.PrepTimeMinutes,
            CookTimeMinutes = dto.CookTimeMinutes,
            Servings = Math.Max(1, dto.Servings),
            TotalCalories = Math.Round(totalCalories, 1),
            TotalProtein = Math.Round(totalProtein, 1),
            TotalCarbs = Math.Round(totalCarbs, 1),
            TotalFat = Math.Round(totalFat, 1),
            Difficulty = string.IsNullOrWhiteSpace(dto.Difficulty) ? "Easy" : dto.Difficulty,
            IsCommunity = dto.IsCommunity,
            Tags = JsonSerializer.Serialize(dto.Tags ?? []),
            ImageUrl = dto.ImageUrl,
            Ingredients = ingredients,
            Steps = steps
        };

        var created = await _recipeRepo.CreateAsync(recipe, cancellationToken);
        created.User = user;
        return MapToDto(created);
    }

    public async Task<RecipeDto> CreateVariantAsync(Guid parentRecipeId, Guid userId, CreateRecipeDto dto, CancellationToken cancellationToken = default)
    {
        var parent = await _recipeRepo.GetByIdAsync(parentRecipeId, cancellationToken)
            ?? throw new NotFoundException($"Receta original '{parentRecipeId}' no encontrada.");

        var user = await _userRepo.GetByIdAsync(userId)
            ?? throw new NotFoundException("Usuario no encontrado.");

        var recipeId = Guid.NewGuid();
        var ingredients = MapIngredientsFromDto(dto.Ingredients, recipeId);
        var steps = MapStepsFromDto(dto.Steps, recipeId);

        var totalCalories = ingredients.Sum(i => i.Calories);
        var totalProtein = ingredients.Sum(i => i.Protein);
        var totalCarbs = ingredients.Sum(i => i.Carbs);
        var totalFat = ingredients.Sum(i => i.Fat);

        var variantName = string.IsNullOrWhiteSpace(dto.Name)
            ? $"{parent.Name} (Variante de @{user.Username})"
            : dto.Name.Trim();

        var recipe = new Recipe
        {
            Id = recipeId,
            UserId = userId,
            ParentRecipeId = parentRecipeId,
            Name = variantName,
            Description = string.IsNullOrWhiteSpace(dto.Description) ? parent.Description : dto.Description.Trim(),
            MealType = string.IsNullOrWhiteSpace(dto.MealType) ? parent.MealType : dto.MealType,
            DietType = string.IsNullOrWhiteSpace(dto.DietType) ? parent.DietType : dto.DietType,
            PrepTimeMinutes = dto.PrepTimeMinutes > 0 ? dto.PrepTimeMinutes : parent.PrepTimeMinutes,
            CookTimeMinutes = dto.CookTimeMinutes > 0 ? dto.CookTimeMinutes : parent.CookTimeMinutes,
            Servings = Math.Max(1, dto.Servings),
            TotalCalories = Math.Round(totalCalories, 1),
            TotalProtein = Math.Round(totalProtein, 1),
            TotalCarbs = Math.Round(totalCarbs, 1),
            TotalFat = Math.Round(totalFat, 1),
            Difficulty = string.IsNullOrWhiteSpace(dto.Difficulty) ? parent.Difficulty : dto.Difficulty,
            IsCommunity = dto.IsCommunity,
            Tags = JsonSerializer.Serialize(dto.Tags ?? []),
            ImageUrl = dto.ImageUrl ?? parent.ImageUrl,
            Ingredients = ingredients,
            Steps = steps
        };

        var created = await _recipeRepo.CreateAsync(recipe, cancellationToken);
        created.User = user;
        created.ParentRecipe = parent;
        return MapToDto(created);
    }

    public async Task<RecipeDto> UpdateAsync(Guid id, Guid userId, UpdateRecipeDto dto, CancellationToken cancellationToken = default)
    {
        var recipe = await _recipeRepo.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException($"Receta con ID '{id}' no encontrada.");

        if (recipe.UserId != userId)
            throw new UnauthorizedException("No tienes permisos para modificar esta receta.");

        if (dto.Name != null) recipe.Name = dto.Name.Trim();
        if (dto.Description != null) recipe.Description = dto.Description.Trim();
        if (dto.MealType != null) recipe.MealType = dto.MealType;
        if (dto.DietType != null) recipe.DietType = dto.DietType;
        if (dto.PrepTimeMinutes.HasValue) recipe.PrepTimeMinutes = dto.PrepTimeMinutes.Value;
        if (dto.CookTimeMinutes.HasValue) recipe.CookTimeMinutes = dto.CookTimeMinutes.Value;
        if (dto.Servings.HasValue) recipe.Servings = Math.Max(1, dto.Servings.Value);
        if (dto.Difficulty != null) recipe.Difficulty = dto.Difficulty;
        if (dto.IsCommunity.HasValue) recipe.IsCommunity = dto.IsCommunity.Value;
        if (dto.Tags != null) recipe.Tags = JsonSerializer.Serialize(dto.Tags);
        if (dto.ImageUrl != null) recipe.ImageUrl = dto.ImageUrl;

        if (dto.Ingredients != null)
        {
            recipe.Ingredients.Clear();
            foreach (var ing in MapIngredientsFromDto(dto.Ingredients, recipe.Id))
            {
                recipe.Ingredients.Add(ing);
            }
            recipe.TotalCalories = Math.Round(recipe.Ingredients.Sum(i => i.Calories), 1);
            recipe.TotalProtein = Math.Round(recipe.Ingredients.Sum(i => i.Protein), 1);
            recipe.TotalCarbs = Math.Round(recipe.Ingredients.Sum(i => i.Carbs), 1);
            recipe.TotalFat = Math.Round(recipe.Ingredients.Sum(i => i.Fat), 1);
        }

        if (dto.Steps != null)
        {
            recipe.Steps.Clear();
            foreach (var step in MapStepsFromDto(dto.Steps, recipe.Id))
            {
                recipe.Steps.Add(step);
            }
        }

        var updated = await _recipeRepo.UpdateAsync(recipe, cancellationToken);
        return MapToDto(updated);
    }

    public async Task DeleteAsync(Guid id, Guid userId, CancellationToken cancellationToken = default)
    {
        var recipe = await _recipeRepo.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException($"Receta con ID '{id}' no encontrada.");

        if (recipe.UserId != userId)
            throw new UnauthorizedException("No tienes permisos para eliminar esta receta.");

        await _recipeRepo.DeleteAsync(recipe, cancellationToken);
    }

    public async Task<MealDto> LogRecipeAsMealAsync(Guid userId, LogRecipeAsMealDto dto, CancellationToken cancellationToken = default)
    {
        var recipe = await _recipeRepo.GetByIdAsync(dto.RecipeId, cancellationToken)
            ?? throw new NotFoundException($"Receta con ID '{dto.RecipeId}' no encontrada.");

        var servings = dto.Servings <= 0 ? 1.0 : dto.Servings;
        var scale = servings / Math.Max(1.0, recipe.Servings);

        var mealId = Guid.NewGuid();
        var mealItems = recipe.Ingredients.Select(i =>
        {
            var displayFoodName = !string.IsNullOrWhiteSpace(i.CookingState) && !i.CookingState.Equals("Raw", StringComparison.OrdinalIgnoreCase)
                ? $"{i.CustomFoodName} ({i.CookingState})"
                : i.CustomFoodName;

            return new MealItem
            {
                Id = Guid.NewGuid(),
                MealId = mealId,
                FoodId = i.FoodId,
                FoodName = displayFoodName,
                Quantity = Math.Round(i.Quantity * scale, 1),
                Unit = i.Unit,
                Calories = Math.Round(i.Calories * scale, 1),
                Protein = Math.Round(i.Protein * scale, 1),
                Carbs = Math.Round(i.Carbs * scale, 1),
                Fat = Math.Round(i.Fat * scale, 1)
            };
        }).ToList();

        var mealName = servings != 1.0
            ? $"{recipe.Name} ({servings:0.##} porciones)"
            : recipe.Name;

        var meal = new Meal
        {
            Id = mealId,
            UserId = userId,
            Date = dto.Date,
            MealType = string.IsNullOrWhiteSpace(dto.MealType) ? "Lunch" : dto.MealType,
            Name = mealName,
            Time = DateTime.UtcNow.ToString("HH:mm"),
            IsCompleted = true,
            Calories = Math.Round(recipe.TotalCalories * scale, 1),
            Protein = Math.Round(recipe.TotalProtein * scale, 1),
            Carbs = Math.Round(recipe.TotalCarbs * scale, 1),
            Fat = Math.Round(recipe.TotalFat * scale, 1),
            Items = mealItems
        };

        var createdMeal = await _mealRepo.CreateAsync(meal, cancellationToken);

        return new MealDto
        {
            Id = createdMeal.Id,
            Date = createdMeal.Date,
            MealType = createdMeal.MealType,
            Name = createdMeal.Name,
            Time = createdMeal.Time,
            IsCompleted = createdMeal.IsCompleted,
            Calories = createdMeal.Calories,
            Protein = createdMeal.Protein,
            Carbs = createdMeal.Carbs,
            Fat = createdMeal.Fat,
            Items = createdMeal.Items.Select(item => new MealItemDto
            {
                Id = item.Id,
                FoodId = item.FoodId,
                FoodName = item.FoodName,
                Quantity = item.Quantity,
                Unit = item.Unit,
                Calories = item.Calories,
                Protein = item.Protein,
                Carbs = item.Carbs,
                Fat = item.Fat
            }).ToList()
        };
    }

    #region Helpers & Mapping

    private static List<RecipeIngredient> MapIngredientsFromDto(List<CreateRecipeIngredientDto> dtos, Guid recipeId)
    {
        return dtos.Select(d => new RecipeIngredient
        {
            Id = Guid.NewGuid(),
            RecipeId = recipeId,
            FoodId = d.FoodId,
            CustomFoodName = d.CustomFoodName.Trim(),
            CookingState = string.IsNullOrWhiteSpace(d.CookingState) ? "Raw" : d.CookingState,
            Quantity = d.Quantity,
            Unit = string.IsNullOrWhiteSpace(d.Unit) ? "g" : d.Unit,
            Calories = d.Calories,
            Protein = d.Protein,
            Carbs = d.Carbs,
            Fat = d.Fat,
            IsOptional = d.IsOptional
        }).ToList();
    }

    private static List<RecipeStep> MapStepsFromDto(List<CreateRecipeStepDto> dtos, Guid recipeId)
    {
        return dtos
            .OrderBy(d => d.StepNumber)
            .Select((d, index) => new RecipeStep
            {
                Id = Guid.NewGuid(),
                RecipeId = recipeId,
                StepNumber = index + 1,
                Instruction = d.Instruction.Trim()
            }).ToList();
    }

    private static RecipeDto MapToDto(Recipe entity, int variantsCount = 0)
    {
        List<string> tags = [];
        try
        {
            if (!string.IsNullOrWhiteSpace(entity.Tags))
                tags = JsonSerializer.Deserialize<List<string>>(entity.Tags) ?? [];
        }
        catch { }

        return new RecipeDto
        {
            Id = entity.Id,
            UserId = entity.UserId,
            AuthorName = entity.User?.Username,
            ParentRecipeId = entity.ParentRecipeId,
            ParentRecipeName = entity.ParentRecipe?.Name,
            Name = entity.Name,
            Description = entity.Description,
            MealType = entity.MealType,
            DietType = entity.DietType,
            PrepTimeMinutes = entity.PrepTimeMinutes,
            CookTimeMinutes = entity.CookTimeMinutes,
            Servings = entity.Servings,
            TotalCalories = entity.TotalCalories,
            TotalProtein = entity.TotalProtein,
            TotalCarbs = entity.TotalCarbs,
            TotalFat = entity.TotalFat,
            Difficulty = entity.Difficulty,
            IsCommunity = entity.IsCommunity,
            Tags = tags,
            ImageUrl = entity.ImageUrl,
            VariantsCount = variantsCount,
            CreatedAt = entity.CreatedAt,
            UpdatedAt = entity.UpdatedAt,
            Ingredients = entity.Ingredients.Select(i => new RecipeIngredientDto
            {
                Id = i.Id,
                RecipeId = i.RecipeId,
                FoodId = i.FoodId,
                CustomFoodName = i.CustomFoodName,
                CookingState = i.CookingState,
                Quantity = i.Quantity,
                Unit = i.Unit,
                Calories = i.Calories,
                Protein = i.Protein,
                Carbs = i.Carbs,
                Fat = i.Fat,
                IsOptional = i.IsOptional
            }).ToList(),
            Steps = entity.Steps.OrderBy(s => s.StepNumber).Select(s => new RecipeStepDto
            {
                Id = s.Id,
                RecipeId = s.RecipeId,
                StepNumber = s.StepNumber,
                Instruction = s.Instruction
            }).ToList()
        };
    }

    #endregion
}

