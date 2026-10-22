using System;
using System.Collections.Generic;
using System.Security.Claims;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Nexodus_Back.API.Extensions;
using Nexodus_Back.API.Filters;
using Nexodus_Back.Application.DTOs.TodoList;
using Nexodus_Back.Application.Interfaces;

namespace Nexodus_Back.API.Endpoints;

public static class TodoListEndpoints
{
    public static void MapTodoListEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/todos")
            .WithTags("TodoList")
            .RequireAuthorization();

        group.MapPost("/", async (CreateTodoListRequest request, ITodoListService todoListService, ClaimsPrincipal user) =>
        {
            var userId = user.GetUserId();
            var result = await todoListService.CreateAsync(userId, request);
            return Results.Created($"/api/todos/{result.Id}", result);
        })
        .AddEndpointFilter<ValidationFilter<CreateTodoListRequest>>()
        .WithName("CreateTodo")
        .WithSummary("Crea una nueva tarea o hábito")
        .Produces<TodoListDto>(StatusCodes.Status201Created)
        .ProducesValidationProblem(StatusCodes.Status400BadRequest);

        group.MapGet("/", async (ITodoListService todoListService, ClaimsPrincipal user) =>
        {
            var userId = user.GetUserId();
            var result = await todoListService.GetAllByUserIdAsync(userId);
            return Results.Ok(result);
        })
        .WithName("GetAllTodos")
        .WithSummary("Obtiene todas las tareas y hábitos del usuario")
        .Produces<IEnumerable<TodoListDto>>(StatusCodes.Status200OK);

        group.MapGet("/{id:guid}", async (Guid id, ITodoListService todoListService, ClaimsPrincipal user) =>
        {
            var userId = user.GetUserId();
            var result = await todoListService.GetByIdAsync(userId, id);
            return Results.Ok(result);
        })
        .WithName("GetTodoById")
        .WithSummary("Obtiene una tarea o hábito por ID")
        .Produces<TodoListDto>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status404NotFound);

        group.MapPut("/{id:guid}", async (Guid id, UpdateTodoListRequest request, ITodoListService todoListService, ClaimsPrincipal user) =>
        {
            var userId = user.GetUserId();
            var result = await todoListService.UpdateAsync(userId, id, request);
            return Results.Ok(result);
        })
        .AddEndpointFilter<ValidationFilter<UpdateTodoListRequest>>()
        .WithName("UpdateTodo")
        .WithSummary("Actualiza los datos de una tarea o hábito")
        .Produces<TodoListDto>(StatusCodes.Status200OK)
        .ProducesValidationProblem(StatusCodes.Status400BadRequest)
        .ProducesProblem(StatusCodes.Status404NotFound);

        group.MapDelete("/{id:guid}", async (Guid id, ITodoListService todoListService, ClaimsPrincipal user) =>
        {
            var userId = user.GetUserId();
            await todoListService.DeleteAsync(userId, id);
            return Results.NoContent();
        })
        .WithName("DeleteTodo")
        .WithSummary("Elimina una tarea o hábito")
        .Produces(StatusCodes.Status204NoContent)
        .ProducesProblem(StatusCodes.Status404NotFound);

        group.MapPost("/{id:guid}/complete", async (Guid id, ITodoListService todoListService, ClaimsPrincipal user) =>
        {
            var userId = user.GetUserId();
            var result = await todoListService.MarkAsCompletedAsync(userId, id);
            return Results.Ok(result);
        })
        .WithName("MarkTodoAsCompleted")
        .WithSummary("Marca una tarea o hábito como completado hoy")
        .Produces<TodoListDto>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status404NotFound)
        .ProducesProblem(StatusCodes.Status409Conflict);

        group.MapPost("/{id:guid}/uncompleted", async (Guid id, ITodoListService todoListService, ClaimsPrincipal user) =>
        {
            var userId = user.GetUserId();
            var result = await todoListService.MarkAsUncompletedAsync(userId, id);
            return Results.Ok(result);
        })
        .WithName("MarkTodoAsUncompleted")
        .WithSummary("Desmarca una tarea o hábito previamente completado")
        .Produces<TodoListDto>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status404NotFound)
        .ProducesProblem(StatusCodes.Status409Conflict);
    }
}
