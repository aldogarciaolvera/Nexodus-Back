using System;
using System.Threading;
using System.Threading.Tasks;
using Nexodus_Back.Application.DTOs.Diet;

namespace Nexodus_Back.Application.Interfaces;

public interface IDietService
{
    // Targets
    Task<DietTargetDto> GetTargetAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<DietTargetDto> UpdateTargetAsync(Guid userId, DietTargetDto dto, CancellationToken cancellationToken = default);
    Task<CalculatedTargetResponseDto> CalculateTargetAsync(CalculateTargetRequestDto request, CancellationToken cancellationToken = default);

    // Meals & Daily Summary
    Task<DailyDietSummaryDto> GetDailySummaryAsync(Guid userId, string date, CancellationToken cancellationToken = default);
    Task<MealDto> CreateMealAsync(Guid userId, CreateMealDto dto, CancellationToken cancellationToken = default);
    Task<MealDto> UpdateMealAsync(Guid id, Guid userId, UpdateMealDto dto, CancellationToken cancellationToken = default);
    Task DeleteMealAsync(Guid id, Guid userId, CancellationToken cancellationToken = default);

    // Foods
    Task<FoodSearchResponseDto> SearchFoodsAsync(string query, Guid userId, int page, int pageSize, CancellationToken cancellationToken = default);
    Task<FoodItemDto> GetFoodByBarcodeAsync(string barcode, CancellationToken cancellationToken = default);
    Task<FoodItemDto> CreateCustomFoodAsync(Guid userId, CreateFoodItemDto dto, CancellationToken cancellationToken = default);

    // Water
    Task<WaterLogDto> GetWaterAsync(Guid userId, string date, CancellationToken cancellationToken = default);
    Task<WaterLogDto> AddWaterAsync(Guid userId, AddWaterDto dto, CancellationToken cancellationToken = default);
    Task<WaterLogDto> SetWaterAsync(Guid userId, SetWaterDto dto, CancellationToken cancellationToken = default);

    // Fasting
    Task<FastingStatusDto> GetFastingStatusAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<FastingStatusDto> StartFastingAsync(Guid userId, StartFastingDto dto, CancellationToken cancellationToken = default);
    Task<FastingStatusDto> StopFastingAsync(Guid userId, CancellationToken cancellationToken = default);
}
