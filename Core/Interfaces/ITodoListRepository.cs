using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Nexodus_Back.Core.Entities;

namespace Nexodus_Back.Core.Interfaces;

public interface ITodoListRepository
{
    Task<TodoList> CreateAsync(TodoList todo);
    Task<IEnumerable<TodoList>> GetAllByUserIdAsync(Guid userId);
    Task<TodoList?> GetByIdAsync(Guid userId, Guid id);
    Task<TodoList> UpdateAsync(TodoList todo);
    Task DeleteAsync(TodoList todo);
}
