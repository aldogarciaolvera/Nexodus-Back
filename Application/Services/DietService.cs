using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Nexodus_Back.Application.DTOs.Diet;
using Nexodus_Back.Application.Interfaces;
using Nexodus_Back.Core.Entities;
using Nexodus_Back.Core.Exceptions;
using Nexodus_Back.Core.Interfaces;

namespace Nexodus_Back.Application.Services;

/// <summary>
/// Service providing business logic for the Diet module, including targets, meals, foods, water, and fasting.
/// </summary>
public class DietService(
    IDietTargetRepository dietTargetRepo,
    IMealRepository mealRepo,
    IFoodRepository foodRepo,
    IWaterLogRepository waterRepo,
    IFastingRepository fastingRepo,
    IUserRepository userRepo) : IDietService
{
    private readonly IDietTargetRepository _dietTargetRepo = dietTargetRepo ?? throw new ArgumentNullException(nameof(dietTargetRepo));
    private readonly IMealRepository _mealRepo = mealRepo ?? throw new ArgumentNullException(nameof(mealRepo));
    private readonly IFoodRepository _foodRepo = foodRepo ?? throw new ArgumentNullException(nameof(foodRepo));
    private readonly IWaterLogRepository _waterRepo = waterRepo ?? throw new ArgumentNullException(nameof(waterRepo));
    private readonly IFastingRepository _fastingRepo = fastingRepo ?? throw new ArgumentNullException(nameof(fastingRepo));
    private readonly IUserRepository _userRepo = userRepo ?? throw new ArgumentNullException(nameof(userRepo));

    #region Targets & Calculations

    public async Task<DietTargetDto> GetTargetAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var target = await _dietTargetRepo.GetByUserIdAsync(userId, cancellationToken);
        if (target == null)
        {
            return new DietTargetDto
            {
                TargetCalories = 2000,
                ProteinGrams = 150,
                CarbsGrams = 200,
                FatGrams = 65,
                WaterTargetMl = 2500,
                FastingProtocol = "16:8",
                EatingWindowStart = "12:00",
                EatingWindowEnd = "20:00"
            };
        }

        return MapToTargetDto(target);
    }

    public async Task<DietTargetDto> UpdateTargetAsync(Guid userId, DietTargetDto dto, CancellationToken cancellationToken = default)
    {
        var entity = new DietTarget
        {
            UserId = userId,
            TargetCalories = dto.TargetCalories,
            ProteinGrams = dto.ProteinGrams,
            CarbsGrams = dto.CarbsGrams,
            FatGrams = dto.FatGrams,
            WaterTargetMl = dto.WaterTargetMl,
            FastingProtocol = dto.FastingProtocol ?? "None",
            EatingWindowStart = dto.EatingWindowStart ?? "12:00",
            EatingWindowEnd = dto.EatingWindowEnd ?? "20:00"
        };

        var saved = await _dietTargetRepo.UpsertAsync(entity, cancellationToken);

        // Marcar que el usuario ya cuenta con la información de su dieta configurada
        var user = await _userRepo.GetByIdAsync(userId);
        if (user != null && !user.Dieta)
        {
            user.Dieta = true;
            user.UpdatedAt = DateTime.UtcNow;
            await _userRepo.UpdateAsync(user);
        }

        return MapToTargetDto(saved);
    }

    public Task<CalculatedTargetResponseDto> CalculateTargetAsync(CalculateTargetRequestDto request, CancellationToken cancellationToken = default)
    {
        // Mifflin-St Jeor Formula
        double bmr;
        var isFemale = string.Equals(request.BiologicalSex, "Female", StringComparison.OrdinalIgnoreCase);

        if (isFemale)
        {
            bmr = (10 * request.Weight) + (6.25 * request.Height) - (5 * request.Age) - 161;
        }
        else
        {
            bmr = (10 * request.Weight) + (6.25 * request.Height) - (5 * request.Age) + 5;
        }

        var activity = request.ActivityLevel?.Trim().ToLowerInvariant() ?? "moderate";
        double activityMultiplier = activity switch
        {
            "sedentary" or "sedentario" => 1.2,
            "light" or "ligero" => 1.375,
            "active" or "activo" => 1.725,
            "veryactive" or "very_active" or "muy_activo" or "muy activo" => 1.9,
            _ => 1.55 // "moderate" or "moderado"
        };

        double tdee = bmr * activityMultiplier;

        var goal = request.Goal?.Trim().ToLowerInvariant() ?? "maintenance";
        double targetCalories = goal switch
        {
            "cut" or "deficit" or "perder" or "definicion" or "definición" => Math.Max(1200, tdee - 450),
            "bulk" or "surplus" or "ganar" or "volumen" => tdee + 350,
            _ => tdee // "maintenance" or "mantenimiento"
        };

        // Macronutrient Breakdown
        // Protein: ~2.0g per kg of bodyweight
        double proteinGrams = Math.Round(request.Weight * 2.0);
        double proteinCalories = proteinGrams * 4;

        // Fat: ~25% of total calories (9 kcal/g)
        double fatGrams = Math.Round((targetCalories * 0.25) / 9);
        double fatCalories = fatGrams * 9;

        // Carbs: Remaining calories (4 kcal/g)
        double carbsCalories = Math.Max(0, targetCalories - proteinCalories - fatCalories);
        double carbsGrams = Math.Round(carbsCalories / 4);

        // Water: ~35 ml per kg
        double waterTargetMl = Math.Round(request.Weight * 35);

        var result = new CalculatedTargetResponseDto
        {
            Bmr = Math.Round(bmr, 0),
            Tdee = Math.Round(tdee, 0),
            TargetCalories = Math.Round(targetCalories, 0),
            ProteinGrams = proteinGrams,
            CarbsGrams = carbsGrams,
            FatGrams = fatGrams,
            WaterTargetMl = Math.Max(2000, waterTargetMl),
            FastingProtocol = "16:8",
            EatingWindowStart = "12:00",
            EatingWindowEnd = "20:00"
        };

        return Task.FromResult(result);
    }

    #endregion

    #region Meals & Daily Summary

    public async Task<DailyDietSummaryDto> GetDailySummaryAsync(Guid userId, string date, CancellationToken cancellationToken = default)
    {
        var meals = await _mealRepo.GetByDateAsync(userId, date, cancellationToken);
        var targetDto = await GetTargetAsync(userId, cancellationToken);
        var waterLog = await _waterRepo.GetByDateAsync(userId, date, cancellationToken);

        var mealDtos = meals.Select(MapToMealDto).ToList();

        var totalCalories = mealDtos.Sum(m => m.Calories);
        var totalProtein = mealDtos.Sum(m => m.Protein);
        var totalCarbs = mealDtos.Sum(m => m.Carbs);
        var totalFat = mealDtos.Sum(m => m.Fat);

        var mealsByType = new Dictionary<string, List<MealDto>>
        {
            ["Breakfast"] = mealDtos.Where(m => m.MealType.Equals("Breakfast", StringComparison.OrdinalIgnoreCase)).ToList(),
            ["Lunch"] = mealDtos.Where(m => m.MealType.Equals("Lunch", StringComparison.OrdinalIgnoreCase)).ToList(),
            ["Snack"] = mealDtos.Where(m => m.MealType.Equals("Snack", StringComparison.OrdinalIgnoreCase)).ToList(),
            ["Dinner"] = mealDtos.Where(m => m.MealType.Equals("Dinner", StringComparison.OrdinalIgnoreCase)).ToList()
        };

        return new DailyDietSummaryDto
        {
            Date = date,
            ConsumedCalories = Math.Round(totalCalories, 1),
            ConsumedProtein = Math.Round(totalProtein, 1),
            ConsumedCarbs = Math.Round(totalCarbs, 1),
            ConsumedFat = Math.Round(totalFat, 1),
            ConsumedWaterMl = waterLog?.ConsumedMl ?? 0,
            Target = targetDto,
            MealsByType = mealsByType,
            Meals = mealDtos
        };
    }

    public async Task<MealDto> CreateMealAsync(Guid userId, CreateMealDto dto, CancellationToken cancellationToken = default)
    {
        var mealId = Guid.NewGuid();
        var items = dto.Items.Select(i => new MealItem
        {
            Id = Guid.NewGuid(),
            MealId = mealId,
            FoodId = i.FoodId,
            FoodName = i.FoodName,
            Quantity = i.Quantity,
            Unit = i.Unit,
            Calories = i.Calories,
            Protein = i.Protein,
            Carbs = i.Carbs,
            Fat = i.Fat
        }).ToList();

        var hasItems = items.Count > 0;
        var meal = new Meal
        {
            Id = mealId,
            UserId = userId,
            Date = dto.Date,
            MealType = dto.MealType,
            Name = dto.Name,
            Time = dto.Time,
            IsCompleted = dto.IsCompleted,
            Calories = hasItems ? items.Sum(i => i.Calories) : dto.Calories,
            Protein = hasItems ? items.Sum(i => i.Protein) : dto.Protein,
            Carbs = hasItems ? items.Sum(i => i.Carbs) : dto.Carbs,
            Fat = hasItems ? items.Sum(i => i.Fat) : dto.Fat,
            Items = items
        };

        var created = await _mealRepo.CreateAsync(meal, cancellationToken);
        return MapToMealDto(created);
    }

    public async Task<MealDto> UpdateMealAsync(Guid id, Guid userId, UpdateMealDto dto, CancellationToken cancellationToken = default)
    {
        var meal = await _mealRepo.GetByIdAsync(id, userId, cancellationToken);
        if (meal == null)
            throw new NotFoundException($"Comida con ID '{id}' no encontrada.");

        if (!string.IsNullOrWhiteSpace(dto.MealType)) meal.MealType = dto.MealType;
        if (!string.IsNullOrWhiteSpace(dto.Name)) meal.Name = dto.Name;
        if (!string.IsNullOrWhiteSpace(dto.Time)) meal.Time = dto.Time;
        if (dto.IsCompleted.HasValue) meal.IsCompleted = dto.IsCompleted.Value;

        if (dto.Items != null)
        {
            meal.Items.Clear();
            foreach (var item in dto.Items)
            {
                meal.Items.Add(new MealItem
                {
                    Id = Guid.NewGuid(),
                    MealId = meal.Id,
                    FoodId = item.FoodId,
                    FoodName = item.FoodName,
                    Quantity = item.Quantity,
                    Unit = item.Unit,
                    Calories = item.Calories,
                    Protein = item.Protein,
                    Carbs = item.Carbs,
                    Fat = item.Fat
                });
            }

            if (meal.Items.Count > 0)
            {
                meal.Calories = meal.Items.Sum(i => i.Calories);
                meal.Protein = meal.Items.Sum(i => i.Protein);
                meal.Carbs = meal.Items.Sum(i => i.Carbs);
                meal.Fat = meal.Items.Sum(i => i.Fat);
            }
            else
            {
                if (dto.Calories.HasValue) meal.Calories = dto.Calories.Value;
                if (dto.Protein.HasValue) meal.Protein = dto.Protein.Value;
                if (dto.Carbs.HasValue) meal.Carbs = dto.Carbs.Value;
                if (dto.Fat.HasValue) meal.Fat = dto.Fat.Value;
            }
        }
        else
        {
            if (dto.Calories.HasValue) meal.Calories = dto.Calories.Value;
            if (dto.Protein.HasValue) meal.Protein = dto.Protein.Value;
            if (dto.Carbs.HasValue) meal.Carbs = dto.Carbs.Value;
            if (dto.Fat.HasValue) meal.Fat = dto.Fat.Value;
        }

        var updated = await _mealRepo.UpdateAsync(meal, cancellationToken);
        return MapToMealDto(updated);
    }

    public async Task DeleteMealAsync(Guid id, Guid userId, CancellationToken cancellationToken = default)
    {
        var meal = await _mealRepo.GetByIdAsync(id, userId, cancellationToken);
        if (meal == null)
            throw new NotFoundException($"Comida con ID '{id}' no encontrada.");

        await _mealRepo.DeleteAsync(meal, cancellationToken);
    }

    #endregion

    #region Foods

    public async Task<FoodSearchResponseDto> SearchFoodsAsync(string query, Guid userId, int page, int pageSize, CancellationToken cancellationToken = default)
    {
        var safePage = page > 0 ? page : 1;
        var safePageSize = pageSize is > 0 and <= 100 ? pageSize : 20;

        var (items, totalCount) = await _foodRepo.SearchAsync(query, userId, safePage, safePageSize, cancellationToken);

        return new FoodSearchResponseDto
        {
            Items = items.Select(MapToFoodDto).ToList(),
            TotalCount = totalCount,
            Page = safePage,
            PageSize = safePageSize
        };
    }

    public async Task<FoodItemDto> GetFoodByBarcodeAsync(string barcode, CancellationToken cancellationToken = default)
    {
        var food = await _foodRepo.GetByBarcodeAsync(barcode, cancellationToken);
        if (food == null)
            throw new NotFoundException($"Alimento con código de barras '{barcode}' no encontrado.");

        return MapToFoodDto(food);
    }

    public async Task<FoodItemDto> CreateCustomFoodAsync(Guid userId, CreateFoodItemDto dto, CancellationToken cancellationToken = default)
    {
        var food = new Food
        {
            Name = dto.Name,
            Brand = dto.Brand,
            Barcode = dto.Barcode,
            ServingSize = dto.ServingSize,
            ServingUnit = dto.ServingUnit,
            Calories = dto.Calories,
            Protein = dto.Protein,
            Carbs = dto.Carbs,
            Fat = dto.Fat,
            Fiber = dto.Fiber,
            SodiumMg = dto.SodiumMg,
            PotassiumMg = dto.PotassiumMg,
            IsCustom = true,
            UserId = userId
        };

        var created = await _foodRepo.CreateAsync(food, cancellationToken);
        return MapToFoodDto(created);
    }

    #endregion

    #region Water

    public async Task<WaterLogDto> GetWaterAsync(Guid userId, string date, CancellationToken cancellationToken = default)
    {
        var log = await _waterRepo.GetByDateAsync(userId, date, cancellationToken);
        var target = await GetTargetAsync(userId, cancellationToken);

        var consumed = log?.ConsumedMl ?? 0;
        var targetMl = target.WaterTargetMl > 0 ? target.WaterTargetMl : 2500;
        var percentage = Math.Round((consumed / targetMl) * 100, 1);

        return new WaterLogDto
        {
            Date = date,
            ConsumedMl = consumed,
            TargetMl = targetMl,
            Percentage = percentage
        };
    }

    public async Task<WaterLogDto> AddWaterAsync(Guid userId, AddWaterDto dto, CancellationToken cancellationToken = default)
    {
        var log = await _waterRepo.AddWaterAsync(userId, dto.Date, dto.AmountMl, cancellationToken);
        var target = await GetTargetAsync(userId, cancellationToken);

        var targetMl = target.WaterTargetMl > 0 ? target.WaterTargetMl : 2500;
        var percentage = Math.Round((log.ConsumedMl / targetMl) * 100, 1);

        return new WaterLogDto
        {
            Date = dto.Date,
            ConsumedMl = log.ConsumedMl,
            TargetMl = targetMl,
            Percentage = percentage
        };
    }

    public async Task<WaterLogDto> SetWaterAsync(Guid userId, SetWaterDto dto, CancellationToken cancellationToken = default)
    {
        var log = await _waterRepo.SetWaterAsync(userId, dto.Date, dto.TotalMl, cancellationToken);
        var target = await GetTargetAsync(userId, cancellationToken);

        var targetMl = target.WaterTargetMl > 0 ? target.WaterTargetMl : 2500;
        var percentage = Math.Round((log.ConsumedMl / targetMl) * 100, 1);

        return new WaterLogDto
        {
            Date = dto.Date,
            ConsumedMl = log.ConsumedMl,
            TargetMl = targetMl,
            Percentage = percentage
        };
    }

    #endregion

    #region Fasting

    public async Task<FastingStatusDto> GetFastingStatusAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var active = await _fastingRepo.GetActiveFastingAsync(userId, cancellationToken);
        if (active == null)
        {
            return new FastingStatusDto
            {
                IsFasting = false,
                StartTime = null,
                EndTime = null,
                TargetHours = 16,
                ElapsedHours = 0,
                RemainingHours = 0,
                ProgressPercentage = 0,
                EstimatedEndTime = null,
                FastingProtocol = "16:8"
            };
        }

        var now = DateTime.UtcNow;
        var elapsed = (now - active.StartTime).TotalHours;
        var remaining = Math.Max(0, active.TargetHours - elapsed);
        var progress = active.TargetHours > 0 ? Math.Min(100, Math.Round((elapsed / active.TargetHours) * 100, 1)) : 0;
        var estimatedEnd = active.StartTime.AddHours(active.TargetHours);

        return new FastingStatusDto
        {
            IsFasting = true,
            StartTime = active.StartTime,
            EndTime = null,
            TargetHours = active.TargetHours,
            ElapsedHours = Math.Round(elapsed, 2),
            RemainingHours = Math.Round(remaining, 2),
            ProgressPercentage = progress,
            EstimatedEndTime = estimatedEnd,
            FastingProtocol = active.FastingProtocol
        };
    }

    public async Task<FastingStatusDto> StartFastingAsync(Guid userId, StartFastingDto dto, CancellationToken cancellationToken = default)
    {
        var targetHours = dto.TargetHours > 0 ? dto.TargetHours : 16;
        var protocol = string.IsNullOrWhiteSpace(dto.FastingProtocol) ? "16:8" : dto.FastingProtocol;
        var startTime = dto.StartTime ?? DateTime.UtcNow;

        var active = await _fastingRepo.StartFastingAsync(userId, targetHours, protocol, startTime, cancellationToken);

        var now = DateTime.UtcNow;
        var elapsed = (now - active.StartTime).TotalHours;
        var remaining = Math.Max(0, active.TargetHours - elapsed);
        var progress = active.TargetHours > 0 ? Math.Min(100, Math.Round((elapsed / active.TargetHours) * 100, 1)) : 0;
        var estimatedEnd = active.StartTime.AddHours(active.TargetHours);

        return new FastingStatusDto
        {
            IsFasting = true,
            StartTime = active.StartTime,
            EndTime = null,
            TargetHours = active.TargetHours,
            ElapsedHours = Math.Round(elapsed, 2),
            RemainingHours = Math.Round(remaining, 2),
            ProgressPercentage = progress,
            EstimatedEndTime = estimatedEnd,
            FastingProtocol = active.FastingProtocol
        };
    }

    public async Task<FastingStatusDto> StopFastingAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var stopped = await _fastingRepo.StopFastingAsync(userId, DateTime.UtcNow, cancellationToken);

        return new FastingStatusDto
        {
            IsFasting = false,
            StartTime = stopped.StartTime,
            EndTime = stopped.EndTime,
            TargetHours = stopped.TargetHours,
            ElapsedHours = stopped.EndTime.HasValue ? Math.Round((stopped.EndTime.Value - stopped.StartTime).TotalHours, 2) : 0,
            RemainingHours = 0,
            ProgressPercentage = 100,
            EstimatedEndTime = stopped.EndTime,
            FastingProtocol = stopped.FastingProtocol
        };
    }

    #endregion

    #region Mapping Helpers

    private static DietTargetDto MapToTargetDto(DietTarget entity) => new()
    {
        TargetCalories = entity.TargetCalories,
        ProteinGrams = entity.ProteinGrams,
        CarbsGrams = entity.CarbsGrams,
        FatGrams = entity.FatGrams,
        WaterTargetMl = entity.WaterTargetMl,
        FastingProtocol = entity.FastingProtocol,
        EatingWindowStart = entity.EatingWindowStart,
        EatingWindowEnd = entity.EatingWindowEnd
    };

    private static MealDto MapToMealDto(Meal entity) => new()
    {
        Id = entity.Id,
        Date = entity.Date,
        MealType = entity.MealType,
        Name = entity.Name,
        Time = entity.Time,
        IsCompleted = entity.IsCompleted,
        Calories = entity.Calories,
        Protein = entity.Protein,
        Carbs = entity.Carbs,
        Fat = entity.Fat,
        Items = entity.Items.Select(i => new MealItemDto
        {
            Id = i.Id,
            FoodId = i.FoodId,
            FoodName = i.FoodName,
            Quantity = i.Quantity,
            Unit = i.Unit,
            Calories = i.Calories,
            Protein = i.Protein,
            Carbs = i.Carbs,
            Fat = i.Fat
        }).ToList()
    };

    private static FoodItemDto MapToFoodDto(Food entity) => new()
    {
        Id = entity.Id,
        Name = entity.Name,
        Brand = entity.Brand,
        Barcode = entity.Barcode,
        ServingSize = entity.ServingSize,
        ServingUnit = entity.ServingUnit,
        Calories = entity.Calories,
        Protein = entity.Protein,
        Carbs = entity.Carbs,
        Fat = entity.Fat,
        Fiber = entity.Fiber,
        SodiumMg = entity.SodiumMg,
        PotassiumMg = entity.PotassiumMg,
        IsCustom = entity.IsCustom
    };

    #endregion
}
