using System.Security.Claims;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Nexodus_Back.Application.DTOs;
using Nexodus_Back.Application.Services;

namespace Nexodus_Back.API.Endpoints;

public static class RoutineEndpoints
{
    public static void MapRoutineEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/api/routines")
            .RequireAuthorization();

        group.MapGet("/", async (HttpContext context, RoutineService routineService) =>
        {
            var userId = Guid.Parse(context.User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            var result = await routineService.GetAllRoutinesAsync(userId);
            return Results.Ok(result);
        });

        group.MapGet("/{id:guid}", async (Guid id, HttpContext context, RoutineService routineService) =>
        {
            var userId = Guid.Parse(context.User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            var result = await routineService.GetRoutineByIdAsync(id, userId);
            return Results.Ok(result);
        });

        group.MapPost("/", async (CreateRoutineDto dto, HttpContext context, RoutineService routineService) =>
        {
            var userId = Guid.Parse(context.User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            var result = await routineService.CreateRoutineAsync(userId, dto);
            return Results.Created($"/api/routines/{result.Id}", result);
        });

        group.MapPut("/{id:guid}", async (Guid id, CreateRoutineDto dto, HttpContext context, RoutineService routineService) =>
        {
            var userId = Guid.Parse(context.User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            var result = await routineService.UpdateRoutineAsync(id, userId, dto);
            return Results.Ok(result);
        });

        group.MapDelete("/{id:guid}", async (Guid id, HttpContext context, RoutineService routineService) =>
        {
            var userId = Guid.Parse(context.User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            await routineService.DeleteRoutineAsync(id, userId);
            return Results.NoContent();
        });
    }
}
