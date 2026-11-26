using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Nexodus_Back.Core.Entities;

namespace Nexodus_Back.Core.Interfaces;

/// <summary>
/// Repository interface for user to-do tasks and habits.
/// </summary>
public interface ITodoListRepository
{
    /// <summary>
    /// Persists a new to-do task.
    /// </summary>
    Task<TodoList> CreateAsync(TodoList todo, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves all to-do tasks belonging to the user.
    /// </summary>
    Task<IEnumerable<TodoList>> GetAllByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves a to-do task by its user ID and task ID.
    /// </summary>
    Task<TodoList?> GetByIdAsync(Guid userId, Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates an existing to-do task.
    /// </summary>
    Task<TodoList> UpdateAsync(TodoList todo, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes a to-do task.
    /// </summary>
    Task DeleteAsync(TodoList todo, CancellationToken cancellationToken = default);
}
