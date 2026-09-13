using System;

namespace Nexodus_Back.Core.Entities;

public class Diet
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string MealType { get; set; } = string.Empty;
    public int? Calories { get; set; }
    public DateTime MealDate { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public User? User { get; set; }
}
