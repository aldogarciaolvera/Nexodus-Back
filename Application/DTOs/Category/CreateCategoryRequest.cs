using System;

namespace Nexodus_Back.Application.DTOs.Category;

public class CreateCategoryRequest
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal? MonthlyLimit { get; set; }
}
