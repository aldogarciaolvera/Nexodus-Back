using System.Security.Claims;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Nexodus_Back.Application.Services;

namespace Nexodus_Back.API.Endpoints;

public static class ExerciseEndpoints
{
    public static void MapExerciseEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/api/exercises")
            .RequireAuthorization();

        group.MapGet("/", async (ExerciseService exerciseService) =>
        {
            var result = await exerciseService.GetAllExercisesAsync();
            return Results.Ok(result);
        });

        group.MapGet("/{id:guid}", async (Guid id, ExerciseService exerciseService) =>
        {
            var result = await exerciseService.GetExerciseByIdAsync(id);
            return result != null ? Results.Ok(result) : Results.NotFound();
        });
    }
}
