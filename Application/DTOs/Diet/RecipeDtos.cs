using System;
using System.Collections.Generic;

namespace Nexodus_Back.Application.DTOs.Diet;

public class RecipeIngredientDto
{
    public Guid Id { get; set; }
    public Guid RecipeId { get; set; }
    public Guid? FoodId { get; set; }
    public string CustomFoodName { get; set; } = string.Empty;
    public string CookingState { get; set; } = "Raw";
    public double Quantity { get; set; }
    public string Unit { get; set; } = "g";
    public double Calories { get; set; }
    public double Protein { get; set; }
    public double Carbs { get; set; }
    public double Fat { get; set; }
    public bool IsOptional { get; set; }
}

public class RecipeStepDto
{
    public Guid Id { get; set; }
    public Guid RecipeId { get; set; }
    public int StepNumber { get; set; }
    public string Instruction { get; set; } = string.Empty;
}

public class RecipeDto
{
    public Guid Id { get; set; }
    public Guid? UserId { get; set; }
    public string? AuthorName { get; set; }
    public Guid? ParentRecipeId { get; set; }
    public string? ParentRecipeName { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string MealType { get; set; } = "Any";
    public string DietType { get; set; } = "Standard";
    public int PrepTimeMinutes { get; set; }
    public int CookTimeMinutes { get; set; }
    public int Servings { get; set; } = 1;
    public double TotalCalories { get; set; }
    public double TotalProtein { get; set; }
    public double TotalCarbs { get; set; }
    public double TotalFat { get; set; }
    public string Difficulty { get; set; } = "Easy";
    public bool IsCommunity { get; set; }
    public List<string> Tags { get; set; } = [];
    public string? ImageUrl { get; set; }
    public int VariantsCount { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public List<RecipeIngredientDto> Ingredients { get; set; } = [];
    public List<RecipeStepDto> Steps { get; set; } = [];
}

public class CreateRecipeIngredientDto
{
    public Guid? FoodId { get; set; }
    public string CustomFoodName { get; set; } = string.Empty;
    public string CookingState { get; set; } = "Raw";
    public double Quantity { get; set; }
    public string Unit { get; set; } = "g";
    public double Calories { get; set; }
    public double Protein { get; set; }
    public double Carbs { get; set; }
    public double Fat { get; set; }
    public bool IsOptional { get; set; } = false;
}

public class CreateRecipeStepDto
{
    public int StepNumber { get; set; }
    public string Instruction { get; set; } = string.Empty;
}

public class CreateRecipeDto
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string MealType { get; set; } = "Any";
    public string DietType { get; set; } = "Standard";
    public int PrepTimeMinutes { get; set; }
    public int CookTimeMinutes { get; set; }
    public int Servings { get; set; } = 1;
    public string Difficulty { get; set; } = "Easy";
    public bool IsCommunity { get; set; } = false;
    public List<string> Tags { get; set; } = [];
    public string? ImageUrl { get; set; }
    public List<CreateRecipeIngredientDto> Ingredients { get; set; } = [];
    public List<CreateRecipeStepDto> Steps { get; set; } = [];
}

public class UpdateRecipeDto
{
    public string? Name { get; set; }
    public string? Description { get; set; }
    public string? MealType { get; set; }
    public string? DietType { get; set; }
    public int? PrepTimeMinutes { get; set; }
    public int? CookTimeMinutes { get; set; }
    public int? Servings { get; set; }
    public string? Difficulty { get; set; }
    public bool? IsCommunity { get; set; }
    public List<string>? Tags { get; set; }
    public string? ImageUrl { get; set; }
    public List<CreateRecipeIngredientDto>? Ingredients { get; set; }
    public List<CreateRecipeStepDto>? Steps { get; set; }
}

public class LogRecipeAsMealDto
{
    public Guid RecipeId { get; set; }
    public string Date { get; set; } = string.Empty; // YYYY-MM-DD
    public string MealType { get; set; } = "Lunch"; // Breakfast, Lunch, Dinner, Snack
    public double Servings { get; set; } = 1.0;
}

