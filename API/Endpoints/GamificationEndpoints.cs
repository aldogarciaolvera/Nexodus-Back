using System.Collections.Generic;
using System.Security.Claims;
using System.Threading;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Nexodus_Back.API.Extensions;
using Nexodus_Back.API.Filters;
using Nexodus_Back.Application.DTOs.Gamification;
using Nexodus_Back.Application.Interfaces;

namespace Nexodus_Back.API.Endpoints;

public static class GamificationEndpoints
{
    public static void MapGamificationEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/gamification")
            .WithTags("Gamification & Streaks")
            .RequireAuthorization();

        group.MapGet("/status", async (IGamificationService service, ClaimsPrincipal user, CancellationToken ct) =>
        {
            var userId = user.GetUserId();
            var result = await service.GetStatusAsync(userId, ct);
            return Results.Ok(result);
        })
        .WithName("GetGamificationStatus")
        .WithSummary("Consulta el estado actual de racha, rango Obsidian, NXP y Cyphers")
        .Produces<GamificationStatusDto>(StatusCodes.Status200OK);

        group.MapPut("/rest-days", async (UpdateRestDaysDto dto, IGamificationService service, ClaimsPrincipal user, CancellationToken ct) =>
        {
            var userId = user.GetUserId();
            var result = await service.UpdateRestDaysAsync(userId, dto.RestDays, ct);
            return Results.Ok(result);
        })
        .AddEndpointFilter<ValidationFilter<UpdateRestDaysDto>>()
        .WithName("UpdateRestDays")
        .WithSummary("Configura los días de descanso programado para proteger las rachas")
        .Produces<GamificationStatusDto>(StatusCodes.Status200OK)
        .ProducesValidationProblem(StatusCodes.Status400BadRequest);

        group.MapPost("/streak/restore", async (IGamificationService service, ClaimsPrincipal user, CancellationToken ct) =>
        {
            var userId = user.GetUserId();
            var result = await service.RestoreStreakAsync(userId, ct);
            return Results.Ok(result);
        })
        .WithName("RestoreStreak")
        .WithSummary("Restaura una racha perdida usando Cyphers o un Escudo de Racha disponible")
        .Produces<StreakRestoreResultDto>(StatusCodes.Status200OK)
        .ProducesValidationProblem(StatusCodes.Status400BadRequest);

        group.MapPost("/vault/buy-freeze", async (IGamificationService service, ClaimsPrincipal user, CancellationToken ct) =>
        {
            var userId = user.GetUserId();
            var result = await service.PurchaseVaultItemAsync(userId, "streak_freeze", ct);
            return Results.Ok(result);
        })
        .WithName("BuyStreakFreeze")
        .WithSummary("Adquiere un Escudo de Racha adicional en el Cyber Vault usando Cyphers")
        .Produces<VaultPurchaseResultDto>(StatusCodes.Status200OK)
        .ProducesValidationProblem(StatusCodes.Status400BadRequest);
    }
}

