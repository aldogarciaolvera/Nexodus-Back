using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Nexodus_Back.Application.DTOs.TodoList;

namespace Nexodus_Back.Application.Interfaces;

/// <summary>
/// Service providing business operations for user to-do tasks and habits.
/// </summary>
public interface ITodoListService
{
    /// <summary>
    /// Creates a new to-do task for the user.
    /// </summary>
    Task<TodoListDto> CreateAsync(Guid userId, CreateTodoListRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves all to-do tasks for the user.
    /// </summary>
    Task<IEnumerable<TodoListDto>> GetAllByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves a single to-do task by user ID and task ID.
    /// </summary>
    Task<TodoListDto> GetByIdAsync(Guid userId, Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates details of an existing to-do task.
    /// </summary>
    Task<TodoListDto> UpdateAsync(Guid userId, Guid id, UpdateTodoListRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes a to-do task.
    /// </summary>
    Task DeleteAsync(Guid userId, Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Marks a to-do task as completed and tracks gamification progress.
    /// </summary>
    Task<TodoListDto> MarkAsCompletedAsync(Guid userId, Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Marks a to-do task as uncompleted.
    /// </summary>
    Task<TodoListDto> MarkAsUncompletedAsync(Guid userId, Guid id, CancellationToken cancellationToken = default);
}
