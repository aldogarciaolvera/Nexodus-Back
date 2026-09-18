using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Nexodus_Back.Core.Entities;
using Nexodus_Back.Core.Interfaces;
using Nexodus_Back.Infrastructure.Data;

namespace Nexodus_Back.Infrastructure.Repositories;

public class TodoListRepository : ITodoListRepository
{
    private readonly NexodusDbContext _context;

    public TodoListRepository(NexodusDbContext context)
    {
        _context = context;
    }

    public async Task<TodoList> CreateAsync(TodoList todo)
    {
        _context.TodoList.Add(todo);
        await _context.SaveChangesAsync();
        return todo;
    }

    public async Task<IEnumerable<TodoList>> GetAllByUserIdAsync(Guid userId)
    {
        return await _context.TodoList
            .Where(t => t.UserId == userId)
            .OrderByDescending(t => t.CreatedAt)
            .ToListAsync();
    }

    public async Task<TodoList?> GetByIdAsync(Guid userId, Guid id)
    {
        return await _context.TodoList
            .FirstOrDefaultAsync(t => t.Id == id && t.UserId == userId);
    }

    public async Task<TodoList> UpdateAsync(TodoList todo)
    {
        todo.UpdatedAt = DateTime.UtcNow;
        _context.TodoList.Update(todo);
        await _context.SaveChangesAsync();
        return todo;
    }

    public async Task DeleteAsync(TodoList todo)
    {
        _context.TodoList.Remove(todo);
        await _context.SaveChangesAsync();
    }
}
