using System.Collections.Generic;

namespace Nexodus_Back.Application.DTOs.Diet;

public class MatchMacrosRecipeRequestDto
{
    public double RemainingCalories { get; set; }
    public double RemainingProtein { get; set; }
    public double RemainingCarbs { get; set; }
    public double RemainingFat { get; set; }
    public string MealType { get; set; } = "Dinner"; // Breakfast, Lunch, Dinner, Snack
    public string DietType { get; set; } = "HighProtein"; // Standard, HighProtein, Keto, Vegan, LowCarb
    public string? Preferences { get; set; } // e.g. "sin lácteos", "con pollo", "preparación rápida"
    public int MaxPrepTimeMinutes { get; set; } = 30;
}

public class PantryCookRequestDto
{
    public List<string>? Ingredients { get; set; }
    public string? FreeTextIngredients { get; set; } // ej. "tengo pechuga, 2 papas y tomate"
    public string MealType { get; set; } = "Any";
    public string DietType { get; set; } = "Standard";
    public bool UseUserPantry { get; set; } = true;
    public int MaxPrepTimeMinutes { get; set; } = 40;
}

public class PantryCookSuggestionDto
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string MealType { get; set; } = "Lunch";
    public string DietType { get; set; } = "Standard";
    public int PrepTimeMinutes { get; set; }
    public int CookTimeMinutes { get; set; }
    public int Servings { get; set; } = 1;
    public double TotalCalories { get; set; }
    public double TotalProtein { get; set; }
    public double TotalCarbs { get; set; }
    public double TotalFat { get; set; }
    public string Difficulty { get; set; } = "Easy";
    public double MatchPercentage { get; set; } // ej. 85.0 (%)
    public List<string> UsedIngredients { get; set; } = [];
    public List<string> MissingIngredients { get; set; } = []; // para agregar a lista de compras
    public List<CreateRecipeIngredientDto> Ingredients { get; set; } = [];
    public List<CreateRecipeStepDto> Steps { get; set; } = [];
}
