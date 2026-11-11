using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Nexodus_Back.Core.Entities;

namespace Nexodus_Back.Core.Interfaces;

/// <summary>
/// Repository interface for user and system categories.
/// </summary>
public interface ICategoryRepository
{
    /// <summary>
    /// Retrieves categories associated with the given user.
    /// </summary>
    Task<IEnumerable<Category>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves categories by type (e.g. Food, Money).
    /// </summary>
    Task<IEnumerable<Category>> GetByTypeAsync(string type, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves a category by its ID.
    /// </summary>
    Task<Category?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves a category by name for a specific user.
    /// </summary>
    Task<Category?> GetByNameAsync(Guid userId, string name, CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds a new category.
    /// </summary>
    Task<Category> AddAsync(Category category, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates an existing category.
    /// </summary>
    Task UpdateAsync(Category category, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes a category.
    /// </summary>
    Task DeleteAsync(Category category, CancellationToken cancellationToken = default);
}
