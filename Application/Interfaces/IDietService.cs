using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Nexodus_Back.Application.DTOs.Diet;

namespace Nexodus_Back.Application.Interfaces;

/// <summary>
/// Service providing comprehensive nutrition, diet targets, meal tracking, water logs, food search, and fasting operations.
/// </summary>
public interface IDietService
{
    // Targets
    /// <summary>
    /// Retrieves user dietary macronutrient targets.
    /// </summary>
    Task<DietTargetDto> GetTargetAsync(Guid userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates user dietary macronutrient targets.
    /// </summary>
    Task<DietTargetDto> UpdateTargetAsync(Guid userId, DietTargetDto dto, CancellationToken cancellationToken = default);

    /// <summary>
    /// Calculates personalized calorie and macro targets using scientific formulas based on user stats.
    /// </summary>
    Task<CalculatedTargetResponseDto> CalculateTargetAsync(CalculateTargetRequestDto request, CancellationToken cancellationToken = default);

    // Meals & Daily Summary
    /// <summary>
    /// Retrieves daily nutritional intake and remaining targets for the given date.
    /// </summary>
    Task<DailyDietSummaryDto> GetDailySummaryAsync(Guid userId, string date, CancellationToken cancellationToken = default);

    /// <summary>
    /// Creates a meal record with atomic food items for the given date and meal type.
    /// </summary>
    Task<MealDto> CreateMealAsync(Guid userId, CreateMealDto dto, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates meal items, quantities, and nutritional totals.
    /// </summary>
    Task<MealDto> UpdateMealAsync(Guid id, Guid userId, UpdateMealDto dto, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes a logged meal.
    /// </summary>
    Task DeleteMealAsync(Guid id, Guid userId, CancellationToken cancellationToken = default);

    // Foods
    /// <summary>
    /// Retrieves food categorization list.
    /// </summary>
    Task<List<Nexodus_Back.Application.DTOs.Category.CategoryDto>> GetFoodCategoriesAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Searches food database across verified foods and user custom entries.
    /// </summary>
    Task<FoodSearchResponseDto> SearchFoodsAsync(string query, Guid userId, int page, int pageSize, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves food information by barcode, checking local DB first and OpenFoodFacts fallback.
    /// </summary>
    Task<FoodItemDto> GetFoodByBarcodeAsync(string barcode, Guid? userId = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Creates a custom user food entry.
    /// </summary>
    Task<FoodItemDto> CreateCustomFoodAsync(Guid userId, CreateFoodItemDto dto, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates a user custom food entry.
    /// </summary>
    Task<FoodItemDto> UpdateFoodAsync(Guid id, Guid userId, CreateFoodItemDto dto, CancellationToken cancellationToken = default);

    // Water
    /// <summary>
    /// Retrieves water hydration log for the specified date.
    /// </summary>
    Task<WaterLogDto> GetWaterAsync(Guid userId, string date, CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds water intake (in ml) to the daily hydration log.
    /// </summary>
    Task<WaterLogDto> AddWaterAsync(Guid userId, AddWaterDto dto, CancellationToken cancellationToken = default);

    /// <summary>
    /// Sets total water intake (in ml) for the daily hydration log.
    /// </summary>
    Task<WaterLogDto> SetWaterAsync(Guid userId, SetWaterDto dto, CancellationToken cancellationToken = default);

    // Fasting
    /// <summary>
    /// Retrieves current intermittent fasting timer status and history.
    /// </summary>
    Task<FastingStatusDto> GetFastingStatusAsync(Guid userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Starts an intermittent fasting session.
    /// </summary>
    Task<FastingStatusDto> StartFastingAsync(Guid userId, StartFastingDto dto, CancellationToken cancellationToken = default);

    /// <summary>
    /// Concludes the active intermittent fasting session.
    /// </summary>
    Task<FastingStatusDto> StopFastingAsync(Guid userId, CancellationToken cancellationToken = default);
}
