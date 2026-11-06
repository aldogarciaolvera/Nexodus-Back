using System;

namespace Nexodus_Back.Core.Entities;

public class Food
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Brand { get; set; }
    public string? Barcode { get; set; }
    public double ServingSize { get; set; } = 100;
    public string ServingUnit { get; set; } = "g"; // "g", "ml", "unit"
    public double Calories { get; set; }
    public double Protein { get; set; }
    public double Carbs { get; set; }
    public double Fat { get; set; }
    public double? Fiber { get; set; }
    public double? SodiumMg { get; set; }
    public double? PotassiumMg { get; set; }
    public bool IsCustom { get; set; } = false;
    public Guid? UserId { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public User? User { get; set; }
}
