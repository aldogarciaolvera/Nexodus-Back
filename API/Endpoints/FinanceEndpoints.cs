using System;
using System.Collections.Generic;
using System.Security.Claims;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Nexodus_Back.API.Extensions;
using Nexodus_Back.API.Filters;
using Nexodus_Back.Application.DTOs.Common;
using Nexodus_Back.Application.DTOs.Finance;
using Nexodus_Back.Application.Interfaces;

namespace Nexodus_Back.API.Endpoints;

public static class FinanceEndpoints
{
    public static void MapFinanceEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/finances")
            .WithTags("Finances")
            .RequireAuthorization();

        group.MapPost("/", async (CreateFinanceRequest request, IFinanceService financeService, ClaimsPrincipal user) =>
        {
            var userId = user.GetUserId();
            var result = await financeService.CreateAsync(userId, request);
            return Results.Created($"/api/finances/{result.Id}", ApiResponse<FinanceDto>.Success(result, 201));
        })
        .AddEndpointFilter<ValidationFilter<CreateFinanceRequest>>()
        .WithName("CreateFinance")
        .WithSummary("Registra una nueva transacción financiera (Ingreso o Gasto)")
        .Produces<ApiResponse<FinanceDto>>(StatusCodes.Status201Created)
        .ProducesValidationProblem(StatusCodes.Status400BadRequest);

        group.MapGet("/", async (IFinanceService financeService, ClaimsPrincipal user) =>
        {
            var userId = user.GetUserId();
            var finances = await financeService.GetAllByUserIdAsync(userId);
            return Results.Ok(ApiResponse<IEnumerable<FinanceDto>>.Success(finances));
        })
        .WithName("GetAllFinances")
        .WithSummary("Obtiene todas las transacciones financieras del usuario")
        .Produces<ApiResponse<IEnumerable<FinanceDto>>>(StatusCodes.Status200OK);

        group.MapGet("/summary", async (IFinanceService financeService, ClaimsPrincipal user) =>
        {
            var userId = user.GetUserId();
            var summary = await financeService.GetSummaryAsync(userId);
            return Results.Ok(ApiResponse<FinanceSummaryDto>.Success(summary));
        })
        .WithName("GetFinanceSummary")
        .WithSummary("Obtiene el resumen financiero (total de ingresos, gastos y balance)")
        .Produces<ApiResponse<FinanceSummaryDto>>(StatusCodes.Status200OK);

        group.MapGet("/{id:guid}", async (Guid id, IFinanceService financeService, ClaimsPrincipal user) =>
        {
            var userId = user.GetUserId();
            var finance = await financeService.GetByIdAsync(userId, id);
            return Results.Ok(ApiResponse<FinanceDto>.Success(finance));
        })
        .WithName("GetFinanceById")
        .WithSummary("Obtiene el detalle de una transacción financiera")
        .Produces<ApiResponse<FinanceDto>>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status404NotFound);

        group.MapPut("/{id:guid}", async (Guid id, UpdateFinanceRequest request, IFinanceService financeService, ClaimsPrincipal user) =>
        {
            var userId = user.GetUserId();
            var result = await financeService.UpdateAsync(userId, id, request);
            return Results.Ok(ApiResponse<FinanceDto>.Success(result));
        })
        .AddEndpointFilter<ValidationFilter<UpdateFinanceRequest>>()
        .WithName("UpdateFinance")
        .WithSummary("Actualiza los datos de una transacción financiera existente")
        .Produces<ApiResponse<FinanceDto>>(StatusCodes.Status200OK)
        .ProducesValidationProblem(StatusCodes.Status400BadRequest)
        .ProducesProblem(StatusCodes.Status404NotFound);

        group.MapDelete("/{id:guid}", async (Guid id, IFinanceService financeService, ClaimsPrincipal user) =>
        {
            var userId = user.GetUserId();
            await financeService.DeleteAsync(userId, id);
            return Results.Ok(ApiResponse<string>.Success("Registro eliminado"));
        })
        .WithName("DeleteFinance")
        .WithSummary("Elimina una transacción financiera")
        .Produces<ApiResponse<string>>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status404NotFound);
    }
}
