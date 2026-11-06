using System;

namespace Nexodus_Back.Core.Entities;

public class MealItem
{
    public Guid Id { get; set; }
    public Guid MealId { get; set; }
    public Guid? FoodId { get; set; }
    public string FoodName { get; set; } = string.Empty;
    public double Quantity { get; set; }
    public string Unit { get; set; } = "g";
    public double Calories { get; set; }
    public double Protein { get; set; }
    public double Carbs { get; set; }
    public double Fat { get; set; }

    public Meal? Meal { get; set; }
    public Food? Food { get; set; }
}
