using System;
using System.Collections.Generic;

namespace Nexodus_Back.Core.Entities;

public class Recipe
{
    public Guid Id { get; set; }
    public Guid? UserId { get; set; } // null = receta oficial del sistema, valor = creada por usuario
    public Guid? ParentRecipeId { get; set; } // para variantes derivadas
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string MealType { get; set; } = "Any"; // Breakfast, Lunch, Dinner, Snack, Any
    public string DietType { get; set; } = "Standard"; // Standard, HighProtein, Keto, Vegan, Vegetarian, LowCarb
    public int PrepTimeMinutes { get; set; }
    public int CookTimeMinutes { get; set; }
    public int Servings { get; set; } = 1;
    public double TotalCalories { get; set; }
    public double TotalProtein { get; set; }
    public double TotalCarbs { get; set; }
    public double TotalFat { get; set; }
    public string Difficulty { get; set; } = "Easy"; // Easy, Medium, Hard
    public bool IsCommunity { get; set; } = false; // visible en catálogo público comunitario
    public string Tags { get; set; } = "[]"; // JSON array de tags ej. ["Keto", "PostWorkout", "Rápido"]
    public string? ImageUrl { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public User? User { get; set; }
    public Recipe? ParentRecipe { get; set; }
    public ICollection<Recipe> Variants { get; set; } = [];
    public ICollection<RecipeIngredient> Ingredients { get; set; } = [];
    public ICollection<RecipeStep> Steps { get; set; } = [];
}
