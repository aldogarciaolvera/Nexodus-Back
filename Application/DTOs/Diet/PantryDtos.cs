using System;

namespace Nexodus_Back.Application.DTOs.Diet;

public class PantryItemDto
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public Guid? FoodId { get; set; }
    public string Name { get; set; } = string.Empty;
    public double Quantity { get; set; }
    public string Unit { get; set; } = "g";
    public string Category { get; set; } = "Other";
    public DateTime? ExpirationDate { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    // Info nutricional del alimento vinculado (si existe)
    public double? CaloriesPer100g { get; set; }
    public double? ProteinPer100g { get; set; }
    public double? CarbsPer100g { get; set; }
    public double? FatPer100g { get; set; }
}

public class CreatePantryItemDto
{
    public Guid? FoodId { get; set; }
    public string Name { get; set; } = string.Empty;
    public double Quantity { get; set; }
    public string Unit { get; set; } = "g";
    public string Category { get; set; } = "Other"; // Protein, Carb, Fat, Vegetable, Dairy, Spice, Other
    public DateTime? ExpirationDate { get; set; }
}

public class UpdatePantryItemDto
{
    public string? Name { get; set; }
    public double? Quantity { get; set; }
    public string? Unit { get; set; }
    public string? Category { get; set; }
    public DateTime? ExpirationDate { get; set; }
}
