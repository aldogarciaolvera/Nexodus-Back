using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Nexodus_Back.API.Extensions;
using Nexodus_Back.Application.DTOs;
using Nexodus_Back.Application.Interfaces;

namespace Nexodus_Back.API.Endpoints;

public static class CardioEndpoints
{
    public static void MapCardioEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/api/cardio")
            .WithTags("Cardio")
            .RequireAuthorization();

        group.MapGet("/sessions", async (int? limit, ClaimsPrincipal user, ICardioService service, CancellationToken ct) =>
        {
            var userId = user.GetUserId();
            var sessions = await service.GetUserSessionsAsync(userId, limit ?? 20, ct);
            return Results.Ok(sessions);
        })
        .WithName("GetCardioSessions")
        .WithSummary("Obtiene el historial reciente de sesiones de cardio del usuario")
        .Produces<List<CardioSessionDto>>(StatusCodes.Status200OK);

        group.MapPost("/sessions", async (SaveCardioSessionDto dto, ClaimsPrincipal user, ICardioService service, CancellationToken ct) =>
        {
            var userId = user.GetUserId();
            var saved = await service.SaveSessionAsync(userId, dto, ct);
            return Results.Created($"/api/cardio/sessions/{saved.Id}", saved);
        })
        .WithName("SaveCardioSession")
        .WithSummary("Guarda una sesión de cardio completada con sus segmentos de estado en vivo y distancia")
        .Produces<CardioSessionDto>(StatusCodes.Status201Created)
        .ProducesValidationProblem(StatusCodes.Status400BadRequest);

        group.MapDelete("/sessions/{id:guid}", async (Guid id, ClaimsPrincipal user, ICardioService service, CancellationToken ct) =>
        {
            var userId = user.GetUserId();
            await service.DeleteSessionAsync(id, userId, ct);
            return Results.NoContent();
        })
        .WithName("DeleteCardioSession")
        .WithSummary("Elimina una sesión de cardio del historial")
        .Produces(StatusCodes.Status204NoContent)
        .ProducesProblem(StatusCodes.Status404NotFound);

        group.MapPost("/ai/generate", async (GenerateAiCardioRequestDto dto, ClaimsPrincipal user, ICardioAiService aiService, CancellationToken ct) =>
        {
            var userId = user.GetUserId();
            var plan = await aiService.GenerateCardioPlanAsync(userId, dto, ct);
            return Results.Ok(plan);
        })
        .RequireRateLimiting("AiPolicy")
        .WithName("GenerateAiCardioPlan")
        .WithSummary("Genera una propuesta de cardio con IA (trotar, correr o mixto; aire libre o gym) estructurada por bloques")
        .Produces<GeneratedAiCardioPlanDto>(StatusCodes.Status200OK)
        .ProducesValidationProblem(StatusCodes.Status400BadRequest);
    }
}
