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

public static class WorkoutLogEndpoints
{
    public static void MapWorkoutLogEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/workouts")
            .WithTags("Workouts")
            .RequireAuthorization();

        group.MapGet("/", async (IWorkoutLogService service, ClaimsPrincipal user, CancellationToken cancellationToken) =>
        {
            var userId = user.GetUserId();
            var logs = await service.GetAllByUserIdAsync(userId, cancellationToken);
            return Results.Ok(logs);
        })
        .WithName("GetAllWorkoutLogs")
        .WithSummary("Obtiene todos los registros de entrenamientos completados por el usuario")
        .Produces<IEnumerable<WorkoutLogDto>>(StatusCodes.Status200OK);

        group.MapGet("/{id:guid}", async (Guid id, IWorkoutLogService service, ClaimsPrincipal user, CancellationToken cancellationToken) =>
        {
            var userId = user.GetUserId();
            var log = await service.GetByIdAsync(id, userId, cancellationToken);
            return Results.Ok(log);
        })
        .WithName("GetWorkoutLogById")
        .WithSummary("Obtiene un registro de entrenamiento por su ID")
        .Produces<WorkoutLogDto>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status404NotFound);

        group.MapPost("/", async (CreateWorkoutLogDto dto, IWorkoutLogService service, ClaimsPrincipal user, CancellationToken cancellationToken) =>
        {
            var userId = user.GetUserId();
            var result = await service.CreateAsync(dto, userId, cancellationToken);
            return Results.Created($"/api/workouts/{result.Id}", result);
        })
        .AddEndpointFilter<ValidationFilter<CreateWorkoutLogDto>>()
        .WithName("CreateWorkoutLog")
        .WithSummary("Registra un nuevo entrenamiento completado")
        .Produces<WorkoutLogDto>(StatusCodes.Status201Created)
        .ProducesValidationProblem(StatusCodes.Status400BadRequest)
        .ProducesProblem(StatusCodes.Status404NotFound);

        group.MapDelete("/{id:guid}", async (Guid id, IWorkoutLogService service, ClaimsPrincipal user, CancellationToken cancellationToken) =>
        {
            var userId = user.GetUserId();
            await service.DeleteAsync(id, userId, cancellationToken);
            return Results.NoContent();
        })
        .WithName("DeleteWorkoutLog")
        .WithSummary("Elimina un registro de entrenamiento completado")
        .Produces(StatusCodes.Status204NoContent)
        .ProducesProblem(StatusCodes.Status404NotFound);
    }
}
