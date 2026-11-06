using System;
using System.Collections.Generic;

namespace Nexodus_Back.Core.Entities;

public class Meal
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string Date { get; set; } = string.Empty; // "yyyy-MM-dd"
    public string MealType { get; set; } = string.Empty; // "Breakfast", "Lunch", "Snack", "Dinner"
    public string Name { get; set; } = string.Empty;
    public string Time { get; set; } = string.Empty; // "HH:mm"
    public bool IsCompleted { get; set; } = false;
    public double Calories { get; set; }
    public double Protein { get; set; }
    public double Carbs { get; set; }
    public double Fat { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public User? User { get; set; }
    public ICollection<MealItem> Items { get; set; } = [];
}
