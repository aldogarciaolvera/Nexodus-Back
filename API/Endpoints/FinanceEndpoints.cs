using System.Security.Claims;
using FluentValidation;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Nexodus_Back.Application.DTOs.Finance;
using Nexodus_Back.Application.Services;

namespace Nexodus_Back.API.Endpoints;

public static class FinanceEndpoints
{
    public static void MapFinanceEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/finances")
            .RequireAuthorization()
            .WithTags("Finances");

        group.MapPost("/", async (
            CreateFinanceRequest request, 
            IValidator<CreateFinanceRequest> validator, 
            IFinanceService financeService, 
            ClaimsPrincipal user) =>
        {
            var validationResult = await validator.ValidateAsync(request);
            if (!validationResult.IsValid)
            {
                var errors = validationResult.Errors
                    .GroupBy(x => x.PropertyName)
                    .ToDictionary(g => g.Key, g => g.Select(x => x.ErrorMessage).ToArray());
                throw new Core.Exceptions.ValidationException(errors);
            }

            var userId = Guid.Parse(user.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            var result = await financeService.CreateAsync(userId, request);
            
            return Results.Created($"/api/finances/{result.Id}", result);
        });

        group.MapGet("/", async (IFinanceService financeService, ClaimsPrincipal user) =>
        {
            var userId = Guid.Parse(user.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            var finances = await financeService.GetAllByUserIdAsync(userId);
            return Results.Ok(finances);
        });

        group.MapGet("/summary", async (IFinanceService financeService, ClaimsPrincipal user) =>
        {
            var userId = Guid.Parse(user.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            var summary = await financeService.GetSummaryAsync(userId);
            return Results.Ok(summary);
        });

        group.MapGet("/{id}", async (Guid id, IFinanceService financeService, ClaimsPrincipal user) =>
        {
            var userId = Guid.Parse(user.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            var finance = await financeService.GetByIdAsync(userId, id);
            return Results.Ok(finance);
        });

        group.MapPut("/{id}", async (
            Guid id, 
            UpdateFinanceRequest request, 
            IValidator<UpdateFinanceRequest> validator, 
            IFinanceService financeService, 
            ClaimsPrincipal user) =>
        {
            var validationResult = await validator.ValidateAsync(request);
            if (!validationResult.IsValid)
            {
                var errors = validationResult.Errors
                    .GroupBy(x => x.PropertyName)
                    .ToDictionary(g => g.Key, g => g.Select(x => x.ErrorMessage).ToArray());
                throw new Core.Exceptions.ValidationException(errors);
            }

            var userId = Guid.Parse(user.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            var result = await financeService.UpdateAsync(userId, id, request);
            
            return Results.Ok(result);
        });

        group.MapDelete("/{id}", async (Guid id, IFinanceService financeService, ClaimsPrincipal user) =>
        {
            var userId = Guid.Parse(user.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            await financeService.DeleteAsync(userId, id);
            
            return Results.NoContent();
        });
    }
}
