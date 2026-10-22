using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Nexodus_Back.API.Extensions;
using Nexodus_Back.API.Filters;
using Nexodus_Back.Application.DTOs;
using Nexodus_Back.Application.Interfaces;

namespace Nexodus_Back.API.Endpoints;

public static class RoutineEndpoints
{
    public static void MapRoutineEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/api/routines")
            .WithTags("Routines")
            .RequireAuthorization();

        group.MapGet("/", async (ClaimsPrincipal user, IRoutineService routineService, CancellationToken cancellationToken) =>
        {
            var userId = user.GetUserId();
            var result = await routineService.GetAllRoutinesAsync(userId, cancellationToken);
            return Results.Ok(result);
        })
        .WithName("GetAllRoutines")
        .WithSummary("Obtiene todas las rutinas del usuario autenticado")
        .Produces<IEnumerable<RoutineDto>>(StatusCodes.Status200OK);

        group.MapGet("/{id:guid}", async (Guid id, ClaimsPrincipal user, IRoutineService routineService, CancellationToken cancellationToken) =>
        {
            var userId = user.GetUserId();
            var result = await routineService.GetRoutineByIdAsync(id, userId, cancellationToken);
            return Results.Ok(result);
        })
        .WithName("GetRoutineById")
        .WithSummary("Obtiene una rutina específica con todos sus ejercicios y assets multimedia")
        .Produces<RoutineDto>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status404NotFound);

        group.MapPost("/", async (CreateRoutineDto dto, ClaimsPrincipal user, IRoutineService routineService, CancellationToken cancellationToken) =>
        {
            var userId = user.GetUserId();
            var result = await routineService.CreateRoutineAsync(userId, dto, cancellationToken);
            return Results.Created($"/api/routines/{result.Id}", result);
        })
        .AddEndpointFilter<ValidationFilter<CreateRoutineDto>>()
        .WithName("CreateRoutine")
        .WithSummary("Crea una nueva rutina con sus ejercicios asociados")
        .Produces<RoutineDto>(StatusCodes.Status201Created)
        .ProducesValidationProblem(StatusCodes.Status400BadRequest);

        group.MapPut("/{id:guid}", async (Guid id, CreateRoutineDto dto, ClaimsPrincipal user, IRoutineService routineService, CancellationToken cancellationToken) =>
        {
            var userId = user.GetUserId();
            var result = await routineService.UpdateRoutineAsync(id, userId, dto, cancellationToken);
            return Results.Ok(result);
        })
        .AddEndpointFilter<ValidationFilter<CreateRoutineDto>>()
        .WithName("UpdateRoutine")
        .WithSummary("Actualiza los datos y ejercicios de una rutina existente")
        .Produces<RoutineDto>(StatusCodes.Status200OK)
        .ProducesValidationProblem(StatusCodes.Status400BadRequest)
        .ProducesProblem(StatusCodes.Status404NotFound);

        group.MapDelete("/{id:guid}", async (Guid id, ClaimsPrincipal user, IRoutineService routineService, CancellationToken cancellationToken) =>
        {
            var userId = user.GetUserId();
            await routineService.DeleteRoutineAsync(id, userId, cancellationToken);
            return Results.NoContent();
        })
        .WithName("DeleteRoutine")
        .WithSummary("Elimina una rutina del usuario")
        .Produces(StatusCodes.Status204NoContent)
        .ProducesProblem(StatusCodes.Status404NotFound);
    }
}
