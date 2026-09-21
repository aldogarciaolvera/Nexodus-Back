using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Nexodus_Back.Application.DTOs.TodoList;

namespace Nexodus_Back.Application.Interfaces;

public interface ITodoListService
{
    Task<TodoListDto> CreateAsync(Guid userId, CreateTodoListRequest request);
    Task<IEnumerable<TodoListDto>> GetAllByUserIdAsync(Guid userId);
    Task<TodoListDto> GetByIdAsync(Guid userId, Guid id);
    Task<TodoListDto> UpdateAsync(Guid userId, Guid id, UpdateTodoListRequest request);
    Task DeleteAsync(Guid userId, Guid id);
    Task<TodoListDto> MarkAsCompletedAsync(Guid userId, Guid id);
    Task<TodoListDto> MarkAsUncompletedAsync(Guid userId, Guid id);
}
