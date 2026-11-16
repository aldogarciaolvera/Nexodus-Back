using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Nexodus_Back.API.Extensions;
using Nexodus_Back.Application.DTOs.Habit;
using Nexodus_Back.Application.Interfaces;

namespace Nexodus_Back.API.Endpoints;

public static class HabitEndpoints
{
    public static void MapHabitEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/habits")
            .WithTags("Habits")
            .RequireAuthorization();

        group.MapPost("/", async (CreateHabitDto dto, IHabitService habitService, ClaimsPrincipal user, CancellationToken ct) =>
        {
            var userId = user.GetUserId();
            var result = await habitService.CreateHabitAsync(userId, dto, ct);
            return Results.Created($"/api/habits/{result.Id}", result);
        })
        .WithName("CreateHabit")
        .WithSummary("Crea un nuevo hábito (Build o Quit, con tipo de registro y programación)")
        .Produces<HabitDto>(StatusCodes.Status201Created)
        .ProducesProblem(StatusCodes.Status400BadRequest);

        group.MapGet("/", async (bool? includeArchived, int? recentDays, IHabitService habitService, ClaimsPrincipal user, CancellationToken ct) =>
        {
            var userId = user.GetUserId();
            var result = await habitService.GetHabitsAsync(userId, includeArchived ?? false, recentDays ?? 60, ct);
            return Results.Ok(result);
        })
        .WithName("GetAllHabits")
        .WithSummary("Obtiene los hábitos del usuario con su cuadrícula reciente y estado de hoy")
        .Produces<IEnumerable<HabitDto>>(StatusCodes.Status200OK);

        group.MapGet("/{id:guid}", async (Guid id, int? recentDays, IHabitService habitService, ClaimsPrincipal user, CancellationToken ct) =>
        {
            var userId = user.GetUserId();
            var result = await habitService.GetHabitByIdAsync(userId, id, recentDays ?? 60, ct);
            return result != null ? Results.Ok(result) : Results.NotFound();
        })
        .WithName("GetHabitById")
        .WithSummary("Obtiene el detalle y estado de un hábito por su Id")
        .Produces<HabitDto>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status404NotFound);

        group.MapPut("/{id:guid}", async (Guid id, UpdateHabitDto dto, IHabitService habitService, ClaimsPrincipal user, CancellationToken ct) =>
        {
            var userId = user.GetUserId();
            var result = await habitService.UpdateHabitAsync(userId, id, dto, ct);
            return Results.Ok(result);
        })
        .WithName("UpdateHabit")
        .WithSummary("Actualiza la configuración o estado de un hábito")
        .Produces<HabitDto>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status404NotFound);

        group.MapDelete("/{id:guid}", async (Guid id, IHabitService habitService, ClaimsPrincipal user, CancellationToken ct) =>
        {
            var userId = user.GetUserId();
            await habitService.DeleteHabitAsync(userId, id, ct);
            return Results.NoContent();
        })
        .WithName("DeleteHabit")
        .WithSummary("Elimina un hábito permanentemente")
        .Produces(StatusCodes.Status204NoContent)
        .ProducesProblem(StatusCodes.Status404NotFound);

        group.MapPost("/{id:guid}/check-in", async (Guid id, HabitCheckInDto dto, IHabitService habitService, ClaimsPrincipal user, CancellationToken ct) =>
        {
            var userId = user.GetUserId();
            var result = await habitService.CheckInAsync(userId, id, dto, ct);
            return Results.Ok(result);
        })
        .WithName("CheckInHabit")
        .WithSummary("Realiza un registro táctil rápido (+1, cantidad exacta, o toggle de cumplimiento/desliz)")
        .Produces<HabitDto>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status404NotFound);

        group.MapGet("/{id:guid}/history", async (Guid id, int? days, IHabitService habitService, ClaimsPrincipal user, CancellationToken ct) =>
        {
            var userId = user.GetUserId();
            var result = await habitService.GetHabitHistoryAsync(userId, id, days ?? 90, ct);
            return Results.Ok(result);
        })
        .WithName("GetHabitHistory")
        .WithSummary("Obtiene el listado cronológico de azulejos para renderizar la cuadrícula Heatmap")
        .Produces<List<HabitDayCellDto>>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status404NotFound);

        group.MapGet("/{id:guid}/stats", async (Guid id, IHabitService habitService, ClaimsPrincipal user, CancellationToken ct) =>
        {
            var userId = user.GetUserId();
            var result = await habitService.GetHabitStatsAsync(userId, id, ct);
            return Results.Ok(result);
        })
        .WithName("GetHabitStats")
        .WithSummary("Obtiene las estadísticas avanzadas, tendencias semanales y el mapa de 365 días del hábito")
        .Produces<HabitStatsDto>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status404NotFound);

        group.MapPost("/reorder", async (ReorderHabitsDto dto, IHabitService habitService, ClaimsPrincipal user, CancellationToken ct) =>
        {
            var userId = user.GetUserId();
            await habitService.ReorderHabitsAsync(userId, dto, ct);
            return Results.NoContent();
        })
        .WithName("ReorderHabits")
        .WithSummary("Reordena los hábitos del usuario en la pantalla principal")
        .Produces(StatusCodes.Status204NoContent);
    }
}
