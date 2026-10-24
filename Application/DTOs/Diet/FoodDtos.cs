using System;
using System.Collections.Generic;

namespace Nexodus_Back.Application.DTOs.Diet;

public class FoodItemDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Brand { get; set; }
    public string? Barcode { get; set; }
    public double ServingSize { get; set; } // e.g. 100
    public string ServingUnit { get; set; } = "g"; // "g", "ml", "unit"
    public double Calories { get; set; }
    public double Protein { get; set; }
    public double Carbs { get; set; }
    public double Fat { get; set; }
    public double? Fiber { get; set; }
    public double? SodiumMg { get; set; }
    public double? PotassiumMg { get; set; }
    public bool IsCustom { get; set; }
}

public class CreateFoodItemDto
{
    public string Name { get; set; } = string.Empty;
    public string? Brand { get; set; }
    public string? Barcode { get; set; }
    public double ServingSize { get; set; } = 100;
    public string ServingUnit { get; set; } = "g";
    public double Calories { get; set; }
    public double Protein { get; set; }
    public double Carbs { get; set; }
    public double Fat { get; set; }
    public double? Fiber { get; set; }
    public double? SodiumMg { get; set; }
    public double? PotassiumMg { get; set; }
}

public class FoodSearchResponseDto
{
    public List<FoodItemDto> Items { get; set; } = [];
    public int TotalCount { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
}
