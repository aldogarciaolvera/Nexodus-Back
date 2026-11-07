using System.Collections.Generic;

namespace Nexodus_Back.Application.DTOs.Diet;

public record AnalyzeFoodImageRequestDto(
    string Base64Image,
    string? MimeType,
    string? AdditionalContext
);

public class FoodAnalysisResultDto
{
    public string DishName { get; set; } = string.Empty;
    public double TotalCalories { get; set; }
    public double TotalProtein { get; set; }
    public double TotalCarbs { get; set; }
    public double TotalFat { get; set; }
    public double Confidence { get; set; } = 1.0;
    public string? Notes { get; set; }
    public List<AnalyzedFoodItemDto> Items { get; set; } = [];
}

public class AnalyzedFoodItemDto
{
    public string FoodName { get; set; } = string.Empty;
    public double EstimatedGrams { get; set; }
    public double Calories { get; set; }
    public double Protein { get; set; }
    public double Carbs { get; set; }
    public double Fat { get; set; }
}

