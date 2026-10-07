using FluentValidation;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Nexodus_Back.Application.DTOs;
using Nexodus_Back.Application.Services;
using System.Security.Claims;

namespace Nexodus_Back.API.Endpoints
{
    public static class WorkoutLogEndpoints
    {
        public static void MapWorkoutLogEndpoints(this IEndpointRouteBuilder app)
        {
            var group = app.MapGroup("/api/workouts").RequireAuthorization();

            group.MapGet("/", async (WorkoutLogService service, ClaimsPrincipal user) =>
            {
                var userId = Guid.Parse(user.FindFirstValue(ClaimTypes.NameIdentifier)!);
                var logs = await service.GetAllByUserIdAsync(userId);
                return Results.Ok(logs);
            });

            group.MapGet("/{id:guid}", async (Guid id, WorkoutLogService service, ClaimsPrincipal user) =>
            {
                var userId = Guid.Parse(user.FindFirstValue(ClaimTypes.NameIdentifier)!);
                var log = await service.GetByIdAsync(id, userId);
                return Results.Ok(log);
            });

            group.MapPost("/", async (CreateWorkoutLogDto dto, WorkoutLogService service, IValidator<CreateWorkoutLogDto> validator, ClaimsPrincipal user) =>
            {
                var validationResult = await validator.ValidateAsync(dto);
                if (!validationResult.IsValid)
                {
                    return Results.ValidationProblem(validationResult.ToDictionary());
                }

                var userId = Guid.Parse(user.FindFirstValue(ClaimTypes.NameIdentifier)!);
                var result = await service.CreateAsync(dto, userId);

                return Results.Created($"/api/workouts/{result.Id}", result);
            });

            group.MapDelete("/{id:guid}", async (Guid id, WorkoutLogService service, ClaimsPrincipal user) =>
            {
                var userId = Guid.Parse(user.FindFirstValue(ClaimTypes.NameIdentifier)!);
                await service.DeleteAsync(id, userId);
                return Results.NoContent();
            });
        }
    }
}
