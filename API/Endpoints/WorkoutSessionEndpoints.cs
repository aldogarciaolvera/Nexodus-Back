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

public static class WorkoutSessionEndpoints
{
    public static void MapWorkoutSessionEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/gym/sessions")
            .WithTags("Gym Sessions")
            .RequireAuthorization();

        group.MapPost("/start", async (StartWorkoutSessionDto dto, IWorkoutSessionService service, ClaimsPrincipal user, CancellationToken ct) =>
        {
            var userId = user.GetUserId();
            var result = await service.StartSessionAsync(dto, userId, ct);
            return Results.Created($"/api/gym/sessions/{result.Id}", result);
        })
        .AddEndpointFilter<ValidationFilter<StartWorkoutSessionDto>>()
        .WithName("StartWorkoutSession")
        .WithSummary("Inicia una nueva sesión activa de entrenamiento en tiempo real")
        .Produces<WorkoutSessionDto>(StatusCodes.Status201Created)
        .ProducesValidationProblem(StatusCodes.Status400BadRequest)
        .ProducesProblem(StatusCodes.Status409Conflict);

        group.MapGet("/active", async (IWorkoutSessionService service, ClaimsPrincipal user, CancellationToken ct) =>
        {
            var userId = user.GetUserId();
            var result = await service.GetActiveSessionAsync(userId, ct);
            return Results.Ok(result);
        })
        .WithName("GetActiveWorkoutSession")
        .WithSummary("Recupera la sesión de entrenamiento actualmente activa para el usuario")
        .Produces<WorkoutSessionDto>(StatusCodes.Status200OK);

        group.MapGet("/{id:guid}", async (Guid id, IWorkoutSessionService service, ClaimsPrincipal user, CancellationToken ct) =>
        {
            var userId = user.GetUserId();
            var result = await service.GetSessionByIdAsync(id, userId, ct);
            return Results.Ok(result);
        })
        .WithName("GetWorkoutSessionById")
        .WithSummary("Obtiene los detalles completos de una sesión por su ID")
        .Produces<WorkoutSessionDto>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status404NotFound);

        group.MapGet("/", async (int? limit, IWorkoutSessionService service, ClaimsPrincipal user, CancellationToken ct) =>
        {
            var userId = user.GetUserId();
            var results = await service.GetUserSessionsAsync(userId, limit ?? 20, ct);
            return Results.Ok(results);
        })
        .WithName("GetUserWorkoutSessions")
        .WithSummary("Obtiene el historial de sesiones de entrenamiento del usuario")
        .Produces<IEnumerable<WorkoutSessionDto>>(StatusCodes.Status200OK);

        group.MapPost("/{id:guid}/sets", async (Guid id, LogWorkoutSetDto dto, IWorkoutSessionService service, ClaimsPrincipal user, CancellationToken ct) =>
        {
            var userId = user.GetUserId();
            var result = await service.LogSetAsync(id, dto, userId, ct);
            return Results.Created($"/api/gym/sessions/{id}/sets/{result.Id}", result);
        })
        .AddEndpointFilter<ValidationFilter<LogWorkoutSetDto>>()
        .WithName("LogWorkoutSet")
        .WithSummary("Registra o actualiza una serie realizada en una sesión activa")
        .Produces<WorkoutSetDto>(StatusCodes.Status201Created)
        .ProducesValidationProblem(StatusCodes.Status400BadRequest)
        .ProducesProblem(StatusCodes.Status404NotFound);

        group.MapDelete("/{id:guid}/sets/{setId:guid}", async (Guid id, Guid setId, IWorkoutSessionService service, ClaimsPrincipal user, CancellationToken ct) =>
        {
            var userId = user.GetUserId();
            await service.DeleteSetAsync(id, setId, userId, ct);
            return Results.NoContent();
        })
        .WithName("DeleteWorkoutSet")
        .WithSummary("Elimina una serie de la sesión de entrenamiento")
        .Produces(StatusCodes.Status204NoContent)
        .ProducesProblem(StatusCodes.Status404NotFound);

        group.MapPost("/{id:guid}/finish", async (Guid id, FinishWorkoutSessionDto dto, IWorkoutSessionService service, ClaimsPrincipal user, CancellationToken ct) =>
        {
            var userId = user.GetUserId();
            var result = await service.FinishSessionAsync(id, dto, userId, ct);
            return Results.Ok(result);
        })
        .AddEndpointFilter<ValidationFilter<FinishWorkoutSessionDto>>()
        .WithName("FinishWorkoutSession")
        .WithSummary("Finaliza y consolida la sesión activa calculando volumen, repeticiones y XP ganado")
        .Produces<WorkoutSessionDto>(StatusCodes.Status200OK)
        .ProducesValidationProblem(StatusCodes.Status400BadRequest)
        .ProducesProblem(StatusCodes.Status404NotFound);

        group.MapDelete("/{id:guid}", async (Guid id, IWorkoutSessionService service, ClaimsPrincipal user, CancellationToken ct) =>
        {
            var userId = user.GetUserId();
            await service.CancelSessionAsync(id, userId, ct);
            return Results.NoContent();
        })
        .WithName("CancelWorkoutSession")
        .WithSummary("Cancela o elimina una sesión de entrenamiento")
        .Produces(StatusCodes.Status204NoContent)
        .ProducesProblem(StatusCodes.Status404NotFound);

        group.MapGet("/recovery/muscle-status", async (IWorkoutSessionService service, ClaimsPrincipal user, CancellationToken ct) =>
        {
            var userId = user.GetUserId();
            var result = await service.GetMuscleRecoveryStatusAsync(userId, ct);
            return Results.Ok(result);
        })
        .WithName("GetMuscleRecoveryStatus")
        .WithSummary("Calcula el estado y porcentaje de recuperación muscular del usuario en base a fatiga reciente")
        .Produces<IEnumerable<MuscleRecoveryDto>>(StatusCodes.Status200OK);

        group.MapGet("/exercises/{exerciseId:guid}/records", async (Guid exerciseId, IWorkoutSessionService service, ClaimsPrincipal user, CancellationToken ct) =>
        {
            var userId = user.GetUserId();
            var result = await service.GetExerciseRecordAsync(exerciseId, userId, ct);
            return Results.Ok(result);
        })
        .WithName("GetExerciseRecord")
        .WithSummary("Obtiene los récords personales (1RM estimado, volumen máximo, peso máximo) de un ejercicio")
        .Produces<ExerciseRecordDto>(StatusCodes.Status200OK);
    }
}

