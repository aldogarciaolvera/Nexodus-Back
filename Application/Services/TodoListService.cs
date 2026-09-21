using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Nexodus_Back.Application.DTOs.TodoList;
using Nexodus_Back.Application.Interfaces;
using Nexodus_Back.Core.Entities;
using Nexodus_Back.Core.Exceptions;
using Nexodus_Back.Core.Interfaces;

namespace Nexodus_Back.Application.Services;

public class TodoListService : ITodoListService
{
    private readonly ITodoListRepository _repository;

    public TodoListService(ITodoListRepository repository)
    {
        _repository = repository;
    }

    public async Task<TodoListDto> CreateAsync(Guid userId, CreateTodoListRequest request)
    {
        var todo = new TodoList
        {
            UserId = userId,
            Task = request.Task,
            Subtitle = request.Subtitle,
            Tag = request.Tag,
            Urgent = request.Urgent,
            DueDate = request.DueDate,
            IsHabit = request.IsHabit,
            Frequency = request.Frequency,
            CustomDays = request.CustomDays,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        await _repository.CreateAsync(todo);
        return MapToDto(todo);
    }

    public async Task<IEnumerable<TodoListDto>> GetAllByUserIdAsync(Guid userId)
    {
        var todos = await _repository.GetAllByUserIdAsync(userId);
        
        // Recalcular IsCompleted para hábitos basados en LastCompletedAt
        var dtos = todos.Select(t =>
        {
            if (t.IsHabit && t.LastCompletedAt.HasValue)
            {
                // Si la última vez que se completó fue hoy, IsCompleted = true
                t.IsCompleted = t.LastCompletedAt.Value.Date == DateTime.UtcNow.Date;
            }
            return MapToDto(t);
        });

        return dtos;
    }

    public async Task<TodoListDto> GetByIdAsync(Guid userId, Guid id)
    {
        var todo = await _repository.GetByIdAsync(userId, id);
        if (todo == null)
        {
            throw new NotFoundException($"La tarea o hábito con Id '{id}' no fue encontrado.");
        }

        if (todo.IsHabit && todo.LastCompletedAt.HasValue)
        {
            todo.IsCompleted = todo.LastCompletedAt.Value.Date == DateTime.UtcNow.Date;
        }

        return MapToDto(todo);
    }

    public async Task<TodoListDto> UpdateAsync(Guid userId, Guid id, UpdateTodoListRequest request)
    {
        var todo = await _repository.GetByIdAsync(userId, id);
        if (todo == null)
        {
            throw new NotFoundException($"La tarea o hábito con Id '{id}' no fue encontrado.");
        }

        todo.Task = request.Task;
        todo.Subtitle = request.Subtitle;
        todo.Tag = request.Tag;
        todo.Urgent = request.Urgent;
        todo.IsCompleted = request.IsCompleted;
        todo.DueDate = request.DueDate;
        todo.IsHabit = request.IsHabit;
        todo.Frequency = request.Frequency;
        todo.CustomDays = request.CustomDays;

        await _repository.UpdateAsync(todo);
        return MapToDto(todo);
    }

    public async Task DeleteAsync(Guid userId, Guid id)
    {
        var todo = await _repository.GetByIdAsync(userId, id);
        if (todo == null)
        {
            throw new NotFoundException($"La tarea o hábito con Id '{id}' no fue encontrado.");
        }

        await _repository.DeleteAsync(todo);
    }

    public async Task<TodoListDto> MarkAsCompletedAsync(Guid userId, Guid id)
    {
        var todo = await _repository.GetByIdAsync(userId, id);
        if (todo == null)
        {
            throw new NotFoundException($"La tarea o hábito con Id '{id}' no fue encontrado.");
        }

        if (todo.IsHabit)
        {
            var now = DateTime.UtcNow;
            
            // Si ya se completó hoy, no hacer nada o lanzar error
            if (todo.LastCompletedAt.HasValue && todo.LastCompletedAt.Value.Date == now.Date)
            {
                throw new ConflictException("Este hábito ya fue completado hoy.");
            }

            // Lógica de racha
            if (todo.LastCompletedAt.HasValue && todo.LastCompletedAt.Value.Date == now.Date.AddDays(-1))
            {
                // Rachada continuada
                todo.CurrentStreak++;
            }
            else
            {
                // Racha rota o primera vez
                todo.CurrentStreak = 1;
            }

            if (todo.CurrentStreak > todo.HighestStreak)
            {
                todo.HighestStreak = todo.CurrentStreak;
            }

            todo.PreviousCompletedAt = todo.LastCompletedAt;
            todo.LastCompletedAt = now;
            todo.IsCompleted = true; // Reflejar en la base de datos (opcional si es dinámico)
        }
        else
        {
            // Tarea normal
            todo.IsCompleted = true;
        }

        await _repository.UpdateAsync(todo);
        return MapToDto(todo);
    }

    public async Task<TodoListDto> MarkAsUncompletedAsync(Guid userId, Guid id)
    {
        var todo = await _repository.GetByIdAsync(userId, id);
        if (todo == null)
        {
            throw new NotFoundException($"La tarea o hábito con Id '{id}' no fue encontrado.");
        }

        if (todo.IsHabit)
        {
            var now = DateTime.UtcNow;
            
            // Solo permitir desmarcar si fue completado hoy
            if (todo.LastCompletedAt.HasValue && todo.LastCompletedAt.Value.Date == now.Date)
            {
                if (todo.CurrentStreak > 0)
                {
                    todo.CurrentStreak--;
                }

                // Restaurar la fecha en que se completó previamente
                todo.LastCompletedAt = todo.PreviousCompletedAt;

                todo.IsCompleted = false;
            }
            else
            {
                throw new ConflictException("Este hábito no ha sido completado hoy.");
            }
        }
        else
        {
            // Tarea normal
            todo.IsCompleted = false;
        }

        await _repository.UpdateAsync(todo);
        return MapToDto(todo);
    }

    private static TodoListDto MapToDto(TodoList todo)
    {
        return new TodoListDto
        {
            Id = todo.Id,
            Task = todo.Task,
            Subtitle = todo.Subtitle,
            Tag = todo.Tag,
            Urgent = todo.Urgent,
            IsCompleted = todo.IsCompleted,
            DueDate = todo.DueDate,
            IsHabit = todo.IsHabit,
            Frequency = todo.Frequency,
            CustomDays = todo.CustomDays,
            CurrentStreak = todo.CurrentStreak,
            HighestStreak = todo.HighestStreak,
            LastCompletedAt = todo.LastCompletedAt,
            PreviousCompletedAt = todo.PreviousCompletedAt,
            CreatedAt = todo.CreatedAt,
            UpdatedAt = todo.UpdatedAt
        };
    }
}
