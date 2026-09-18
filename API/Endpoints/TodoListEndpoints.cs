using FluentValidation;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Nexodus_Back.Application.DTOs.TodoList;
using Nexodus_Back.Application.Interfaces;
using System.Security.Claims;
using System.Threading.Tasks;
using System;
using Nexodus_Back.Core.Exceptions;
using FluentValidation.Results;
using System.Linq;

namespace Nexodus_Back.API.Endpoints;

public static class TodoListEndpoints
{
    public static void MapTodoListEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/todos").RequireAuthorization();

        group.MapPost("/", CreateTodo);
        group.MapGet("/", GetAllTodos);
        group.MapGet("/{id:guid}", GetTodoById);
        group.MapPut("/{id:guid}", UpdateTodo);
        group.MapDelete("/{id:guid}", DeleteTodo);
        group.MapPost("/{id:guid}/complete", MarkAsCompleted);
    }

    private static async Task<IResult> CreateTodo(
        [FromBody] CreateTodoListRequest request,
        [FromServices] ITodoListService todoListService,
        [FromServices] IValidator<CreateTodoListRequest> validator,
        ClaimsPrincipal user)
    {
        var validationResult = await validator.ValidateAsync(request);
        if (!validationResult.IsValid)
        {
            var errors = string.Join("; ", validationResult.Errors.Select(e => e.ErrorMessage));
            throw new Nexodus_Back.Core.Exceptions.ValidationException(errors);
        }

        var userIdClaim = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
        {
            throw new UnauthorizedException("Usuario no autenticado.");
        }

        var result = await todoListService.CreateAsync(userId, request);
        return Results.Created($"/api/todos/{result.Id}", result);
    }

    private static async Task<IResult> GetAllTodos(
        [FromServices] ITodoListService todoListService,
        ClaimsPrincipal user)
    {
        var userIdClaim = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
        {
            throw new UnauthorizedException("Usuario no autenticado.");
        }

        var result = await todoListService.GetAllByUserIdAsync(userId);
        return Results.Ok(result);
    }

    private static async Task<IResult> GetTodoById(
        Guid id,
        [FromServices] ITodoListService todoListService,
        ClaimsPrincipal user)
    {
        var userIdClaim = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
        {
            throw new UnauthorizedException("Usuario no autenticado.");
        }

        var result = await todoListService.GetByIdAsync(userId, id);
        return Results.Ok(result);
    }

    private static async Task<IResult> UpdateTodo(
        Guid id,
        [FromBody] UpdateTodoListRequest request,
        [FromServices] ITodoListService todoListService,
        [FromServices] IValidator<UpdateTodoListRequest> validator,
        ClaimsPrincipal user)
    {
        var validationResult = await validator.ValidateAsync(request);
        if (!validationResult.IsValid)
        {
            var errors = string.Join("; ", validationResult.Errors.Select(e => e.ErrorMessage));
            throw new Nexodus_Back.Core.Exceptions.ValidationException(errors);
        }

        var userIdClaim = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
        {
            throw new UnauthorizedException("Usuario no autenticado.");
        }

        var result = await todoListService.UpdateAsync(userId, id, request);
        return Results.Ok(result);
    }

    private static async Task<IResult> DeleteTodo(
        Guid id,
        [FromServices] ITodoListService todoListService,
        ClaimsPrincipal user)
    {
        var userIdClaim = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
        {
            throw new UnauthorizedException("Usuario no autenticado.");
        }

        await todoListService.DeleteAsync(userId, id);
        return Results.NoContent();
    }

    private static async Task<IResult> MarkAsCompleted(
        Guid id,
        [FromServices] ITodoListService todoListService,
        ClaimsPrincipal user)
    {
        var userIdClaim = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
        {
            throw new UnauthorizedException("Usuario no autenticado.");
        }

        var result = await todoListService.MarkAsCompletedAsync(userId, id);
        return Results.Ok(result);
    }
}
