using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Nexodus_Back.Application.DTOs.Category;

namespace Nexodus_Back.Application.Interfaces;

public interface ICategoryService
{
    Task<CategoryDto> CreateAsync(Guid userId, CreateCategoryRequest request);
    Task<CategoryDto> GetByIdAsync(Guid userId, Guid id);
    Task<IEnumerable<CategoryDto>> GetAllByUserIdAsync(Guid userId);
    Task<CategoryDto> UpdateAsync(Guid userId, Guid id, UpdateCategoryRequest request);
    Task DeleteAsync(Guid userId, Guid id);
}
