using System;

namespace Nexodus_Back.Application.DTOs.Category;

public class CategoryDto
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal? MonthlyLimit { get; set; }
    public DateTime CreatedAt { get; set; }
}
