using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Nexodus_Back.Application.DTOs.Category;

namespace Nexodus_Back.Application.Interfaces;

/// <summary>
/// Service providing business operations for user categories.
/// </summary>
public interface ICategoryService
{
    /// <summary>
    /// Creates a new category for the user.
    /// </summary>
    Task<CategoryDto> CreateAsync(Guid userId, CreateCategoryRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves a category by user ID and category ID.
    /// </summary>
    Task<CategoryDto> GetByIdAsync(Guid userId, Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves all categories belonging to the user.
    /// </summary>
    Task<IEnumerable<CategoryDto>> GetAllByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates details of an existing category.
    /// </summary>
    Task<CategoryDto> UpdateAsync(Guid userId, Guid id, UpdateCategoryRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes a category.
    /// </summary>
    Task DeleteAsync(Guid userId, Guid id, CancellationToken cancellationToken = default);
}
