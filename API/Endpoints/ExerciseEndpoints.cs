using System;
using System.Collections.Generic;
using System.Threading;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Nexodus_Back.Application.DTOs;
using Nexodus_Back.Application.Interfaces;

namespace Nexodus_Back.API.Endpoints;

public static class ExerciseEndpoints
{
    public static void MapExerciseEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/api/exercises")
            .WithTags("Exercises")
            .RequireAuthorization();

        group.MapGet("/", async (IExerciseService exerciseService, int? page, int? pageSize, CancellationToken cancellationToken) =>
        {
            var result = await exerciseService.GetAllExercisesAsync(page ?? 1, pageSize ?? 50, cancellationToken);
            return Results.Ok(result);
        })
        .WithName("GetAllExercises")
        .WithSummary("Obtiene el catálogo paginado de ejercicios con URLs pre-firmadas")
        .Produces<IEnumerable<ExerciseDto>>(StatusCodes.Status200OK);

        group.MapGet("/{id:guid}", async (Guid id, IExerciseService exerciseService, CancellationToken cancellationToken) =>
        {
            var result = await exerciseService.GetExerciseByIdAsync(id, cancellationToken);
            return Results.Ok(result);
        })
        .WithName("GetExerciseById")
        .WithSummary("Obtiene el detalle de un ejercicio por su identificador único")
        .Produces<ExerciseDto>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status404NotFound);
    }
}
