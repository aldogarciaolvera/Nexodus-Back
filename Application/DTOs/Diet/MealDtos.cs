using System;
using System.Collections.Generic;

namespace Nexodus_Back.Application.DTOs.Diet;

public class MealDto
{
    public Guid Id { get; set; }
    public string Date { get; set; } = string.Empty; // "2026-10-23"
    public string MealType { get; set; } = string.Empty; // "Breakfast", "Lunch", "Snack", "Dinner"
    public string Name { get; set; } = string.Empty; // e.g. "Overnight Oats & Whey"
    public string Time { get; set; } = string.Empty; // "08:15"
    public bool IsCompleted { get; set; }
    public double Calories { get; set; }
    public double Protein { get; set; }
    public double Carbs { get; set; }
    public double Fat { get; set; }
    public List<MealItemDto> Items { get; set; } = [];
}

public class MealItemDto
{
    public Guid Id { get; set; }
    public Guid? FoodId { get; set; }
    public string FoodName { get; set; } = string.Empty;
    public double Quantity { get; set; } // e.g. 150
    public string Unit { get; set; } = "g"; // "g", "ml", "unit"
    public double Calories { get; set; }
    public double Protein { get; set; }
    public double Carbs { get; set; }
    public double Fat { get; set; }
}

public class CreateMealDto
{
    public string Date { get; set; } = string.Empty; // "yyyy-MM-dd"
    public string MealType { get; set; } = string.Empty; // "Breakfast", "Lunch", "Snack", "Dinner"
    public string Name { get; set; } = string.Empty;
    public string Time { get; set; } = string.Empty; // "HH:mm"
    public bool IsCompleted { get; set; }
    public double Calories { get; set; }
    public double Protein { get; set; }
    public double Carbs { get; set; }
    public double Fat { get; set; }
    public List<CreateMealItemDto> Items { get; set; } = [];
}

public class CreateMealItemDto
{
    public Guid? FoodId { get; set; }
    public string FoodName { get; set; } = string.Empty;
    public double Quantity { get; set; }
    public string Unit { get; set; } = "g";
    public double Calories { get; set; }
    public double Protein { get; set; }
    public double Carbs { get; set; }
    public double Fat { get; set; }
}

public class UpdateMealDto
{
    public string? MealType { get; set; }
    public string? Name { get; set; }
    public string? Time { get; set; }
    public bool? IsCompleted { get; set; }
    public double? Calories { get; set; }
    public double? Protein { get; set; }
    public double? Carbs { get; set; }
    public double? Fat { get; set; }
    public List<CreateMealItemDto>? Items { get; set; }
}

public class DailyDietSummaryDto
{
    public string Date { get; set; } = string.Empty;
    public double ConsumedCalories { get; set; }
    public double ConsumedProtein { get; set; }
    public double ConsumedCarbs { get; set; }
    public double ConsumedFat { get; set; }
    public double ConsumedWaterMl { get; set; }
    public DietTargetDto? Target { get; set; }
    public Dictionary<string, List<MealDto>> MealsByType { get; set; } = new();
    public List<MealDto> Meals { get; set; } = [];
}
