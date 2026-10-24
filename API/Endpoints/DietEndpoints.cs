using System;
using System.Security.Claims;
using System.Threading;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Nexodus_Back.API.Extensions;
using Nexodus_Back.API.Filters;
using Nexodus_Back.Application.DTOs.Diet;
using Nexodus_Back.Application.Interfaces;

namespace Nexodus_Back.API.Endpoints;

public static class DietEndpoints
{
    public static void MapDietEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/diet")
            .WithTags("Diet")
            .RequireAuthorization();

        #region Target & Calculations

        group.MapGet("/target", async (IDietService service, ClaimsPrincipal user, CancellationToken ct) =>
        {
            var userId = user.GetUserId();
            var target = await service.GetTargetAsync(userId, ct);
            return Results.Ok(target);
        })
        .WithName("GetDietTarget")
        .WithSummary("Obtiene la configuración y metas nutricionales actuales del usuario")
        .Produces<DietTargetDto>(StatusCodes.Status200OK);

        group.MapPut("/target", async (DietTargetDto dto, IDietService service, ClaimsPrincipal user, CancellationToken ct) =>
        {
            var userId = user.GetUserId();
            var updated = await service.UpdateTargetAsync(userId, dto, ct);
            return Results.Ok(updated);
        })
        .AddEndpointFilter<ValidationFilter<DietTargetDto>>()
        .WithName("UpdateDietTarget")
        .WithSummary("Guarda o actualiza manualmente las metas nutricionales y macros del usuario")
        .Produces<DietTargetDto>(StatusCodes.Status200OK)
        .ProducesValidationProblem(StatusCodes.Status400BadRequest);

        group.MapPost("/calculate-target", async (CalculateTargetRequestDto dto, IDietService service, CancellationToken ct) =>
        {
            var result = await service.CalculateTargetAsync(dto, ct);
            return Results.Ok(result);
        })
        .AddEndpointFilter<ValidationFilter<CalculateTargetRequestDto>>()
        .WithName("CalculateDietTarget")
        .WithSummary("Calcula el gasto energético total (TDEE), metas calóricas y desglose de macros según biométricos")
        .Produces<CalculatedTargetResponseDto>(StatusCodes.Status200OK)
        .ProducesValidationProblem(StatusCodes.Status400BadRequest);

        #endregion

        #region Meals & Daily Summary

        group.MapGet("/daily", async (string? date, IDietService service, ClaimsPrincipal user, CancellationToken ct) =>
        {
            var userId = user.GetUserId();
            var targetDate = string.IsNullOrWhiteSpace(date) ? DateTime.UtcNow.ToString("yyyy-MM-dd") : date;
            var summary = await service.GetDailySummaryAsync(userId, targetDate, ct);
            return Results.Ok(summary);
        })
        .WithName("GetDailyDietSummary")
        .WithSummary("Obtiene el resumen diario de ingesta: totales consumidos, comidas agrupadas y metas activas")
        .Produces<DailyDietSummaryDto>(StatusCodes.Status200OK);

        group.MapPost("/meals", async (CreateMealDto dto, IDietService service, ClaimsPrincipal user, CancellationToken ct) =>
        {
            var userId = user.GetUserId();
            var result = await service.CreateMealAsync(userId, dto, ct);
            return Results.Created($"/api/diet/meals/{result.Id}", result);
        })
        .AddEndpointFilter<ValidationFilter<CreateMealDto>>()
        .WithName("CreateMeal")
        .WithSummary("Registra una nueva comida para el usuario en una fecha específica")
        .Produces<MealDto>(StatusCodes.Status201Created)
        .ProducesValidationProblem(StatusCodes.Status400BadRequest);

        group.MapPut("/meals/{id:guid}", async (Guid id, UpdateMealDto dto, IDietService service, ClaimsPrincipal user, CancellationToken ct) =>
        {
            var userId = user.GetUserId();
            var updated = await service.UpdateMealAsync(id, userId, dto, ct);
            return Results.Ok(updated);
        })
        .AddEndpointFilter<ValidationFilter<UpdateMealDto>>()
        .WithName("UpdateMeal")
        .WithSummary("Modifica una comida existente, sus alimentos o cambia su estado a completada")
        .Produces<MealDto>(StatusCodes.Status200OK)
        .ProducesValidationProblem(StatusCodes.Status400BadRequest)
        .ProducesProblem(StatusCodes.Status404NotFound);

        group.MapDelete("/meals/{id:guid}", async (Guid id, IDietService service, ClaimsPrincipal user, CancellationToken ct) =>
        {
            var userId = user.GetUserId();
            await service.DeleteMealAsync(id, userId, ct);
            return Results.NoContent();
        })
        .WithName("DeleteMeal")
        .WithSummary("Elimina una comida registrada por su ID")
        .Produces(StatusCodes.Status204NoContent)
        .ProducesProblem(StatusCodes.Status404NotFound);

        #endregion

        #region Foods

        group.MapGet("/foods/search", async (string? query, int? page, int? pageSize, IDietService service, ClaimsPrincipal user, CancellationToken ct) =>
        {
            var userId = user.GetUserId();
            var result = await service.SearchFoodsAsync(query ?? string.Empty, userId, page ?? 1, pageSize ?? 20, ct);
            return Results.Ok(result);
        })
        .WithName("SearchFoods")
        .WithSummary("Busca alimentos en el catálogo por nombre o marca con soporte de paginación")
        .Produces<FoodSearchResponseDto>(StatusCodes.Status200OK);

        group.MapGet("/foods/barcode/{barcode}", async (string barcode, IDietService service, CancellationToken ct) =>
        {
            var food = await service.GetFoodByBarcodeAsync(barcode, ct);
            return Results.Ok(food);
        })
        .WithName("GetFoodByBarcode")
        .WithSummary("Consulta rápida de un alimento por su código de barras (EAN/UPC)")
        .Produces<FoodItemDto>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status404NotFound);

        group.MapPost("/foods", async (CreateFoodItemDto dto, IDietService service, ClaimsPrincipal user, CancellationToken ct) =>
        {
            var userId = user.GetUserId();
            var food = await service.CreateCustomFoodAsync(userId, dto, ct);
            return Results.Created($"/api/diet/foods/{food.Id}", food);
        })
        .AddEndpointFilter<ValidationFilter<CreateFoodItemDto>>()
        .WithName("CreateCustomFood")
        .WithSummary("Permite registrar un alimento personalizado creado por el usuario")
        .Produces<FoodItemDto>(StatusCodes.Status201Created)
        .ProducesValidationProblem(StatusCodes.Status400BadRequest);

        #endregion

        #region Water

        group.MapGet("/water", async (string? date, IDietService service, ClaimsPrincipal user, CancellationToken ct) =>
        {
            var userId = user.GetUserId();
            var targetDate = string.IsNullOrWhiteSpace(date) ? DateTime.UtcNow.ToString("yyyy-MM-dd") : date;
            var water = await service.GetWaterAsync(userId, targetDate, ct);
            return Results.Ok(water);
        })
        .WithName("GetWaterIntake")
        .WithSummary("Obtiene los mililitros de agua consumidos y la meta diaria para una fecha")
        .Produces<WaterLogDto>(StatusCodes.Status200OK);

        group.MapPost("/water", async (AddWaterDto dto, IDietService service, ClaimsPrincipal user, CancellationToken ct) =>
        {
            var userId = user.GetUserId();
            var water = await service.AddWaterAsync(userId, dto, ct);
            return Results.Ok(water);
        })
        .AddEndpointFilter<ValidationFilter<AddWaterDto>>()
        .WithName("AddWaterIntake")
        .WithSummary("Suma una cantidad rápida de agua (ej. +250ml o +500ml) al día actual")
        .Produces<WaterLogDto>(StatusCodes.Status200OK)
        .ProducesValidationProblem(StatusCodes.Status400BadRequest);

        group.MapPut("/water/set", async (SetWaterDto dto, IDietService service, ClaimsPrincipal user, CancellationToken ct) =>
        {
            var userId = user.GetUserId();
            var water = await service.SetWaterAsync(userId, dto, ct);
            return Results.Ok(water);
        })
        .AddEndpointFilter<ValidationFilter<SetWaterDto>>()
        .WithName("SetWaterIntake")
        .WithSummary("Ajusta el total exacto de agua consumida para una fecha específica")
        .Produces<WaterLogDto>(StatusCodes.Status200OK)
        .ProducesValidationProblem(StatusCodes.Status400BadRequest);

        #endregion

        #region Fasting

        group.MapGet("/fasting/status", async (IDietService service, ClaimsPrincipal user, CancellationToken ct) =>
        {
            var userId = user.GetUserId();
            var status = await service.GetFastingStatusAsync(userId, ct);
            return Results.Ok(status);
        })
        .WithName("GetFastingStatus")
        .WithSummary("Obtiene el estado en vivo del ayuno intermitente, horas transcurridas y progreso")
        .Produces<FastingStatusDto>(StatusCodes.Status200OK);

        group.MapPost("/fasting/start", async (StartFastingDto dto, IDietService service, ClaimsPrincipal user, CancellationToken ct) =>
        {
            var userId = user.GetUserId();
            var status = await service.StartFastingAsync(userId, dto, ct);
            return Results.Ok(status);
        })
        .AddEndpointFilter<ValidationFilter<StartFastingDto>>()
        .WithName("StartFasting")
        .WithSummary("Inicia un nuevo bloque de ayuno intermitente")
        .Produces<FastingStatusDto>(StatusCodes.Status200OK)
        .ProducesValidationProblem(StatusCodes.Status400BadRequest);

        group.MapPost("/fasting/stop", async (IDietService service, ClaimsPrincipal user, CancellationToken ct) =>
        {
            var userId = user.GetUserId();
            var status = await service.StopFastingAsync(userId, ct);
            return Results.Ok(status);
        })
        .WithName("StopFasting")
        .WithSummary("Concluye el bloque de ayuno activo y abre la ventana de ingesta")
        .Produces<FastingStatusDto>(StatusCodes.Status200OK);

        #endregion
    }
}
