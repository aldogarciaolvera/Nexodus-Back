using System;

namespace Nexodus_Back.Core.Entities;

public class RecipeIngredient
{
    public Guid Id { get; set; }
    public Guid RecipeId { get; set; }
    public Guid? FoodId { get; set; }
    public string CustomFoodName { get; set; } = string.Empty;
    public string CookingState { get; set; } = "Raw"; // Raw, Boiled, Grilled, Fried, Baked, Steamed
    public double Quantity { get; set; }
    public string Unit { get; set; } = "g"; // g, ml, pieza, cda, taza
    public double Calories { get; set; }
    public double Protein { get; set; }
    public double Carbs { get; set; }
    public double Fat { get; set; }
    public bool IsOptional { get; set; } = false;

    public Recipe? Recipe { get; set; }
    public Food? Food { get; set; }
}

