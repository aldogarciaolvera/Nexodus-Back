using System;

namespace Nexodus_Back.Core.Entities;

public class PantryItem
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public Guid? FoodId { get; set; }
    public string Name { get; set; } = string.Empty;
    public double Quantity { get; set; }
    public string Unit { get; set; } = "g";
    public string Category { get; set; } = "Other"; // Protein, Carb, Fat, Vegetable, Dairy, Spice, Other
    public DateTime? ExpirationDate { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public User? User { get; set; }
    public Food? Food { get; set; }
}
