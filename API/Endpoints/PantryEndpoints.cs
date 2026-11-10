using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Nexodus_Back.API.Extensions;
using Nexodus_Back.Application.DTOs.Diet;
using Nexodus_Back.Application.Interfaces;

namespace Nexodus_Back.API.Endpoints;

public static class PantryEndpoints
{
    public static void MapPantryEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/pantry")
            .WithTags("Pantry")
            .RequireAuthorization();

        // Obtener despensa del usuario
        group.MapGet("/", async (IPantryService service, ClaimsPrincipal user, CancellationToken ct) =>
        {
            var userId = user.GetUserId();
            var items = await service.GetUserPantryAsync(userId, ct);
            return Results.Ok(items);
        })
        .WithName("GetUserPantry")
        .WithSummary("Obtiene los ingredientes disponibles en la despensa del usuario")
        .Produces<List<PantryItemDto>>(StatusCodes.Status200OK);

        // Agregar ingrediente a la despensa
        group.MapPost("/", async (CreatePantryItemDto dto, IPantryService service, ClaimsPrincipal user, CancellationToken ct) =>
        {
            var userId = user.GetUserId();
            var item = await service.AddItemAsync(userId, dto, ct);
            return Results.Created($"/api/pantry/{item.Id}", item);
        })
        .WithName("AddPantryItem")
        .WithSummary("Agrega un ingrediente a la despensa del usuario")
        .Produces<PantryItemDto>(StatusCodes.Status201Created)
        .ProducesValidationProblem(StatusCodes.Status400BadRequest);

        // Agregar múltiples ingredientes en lote a la despensa
        group.MapPost("/bulk", async (List<CreatePantryItemDto> dtos, IPantryService service, ClaimsPrincipal user, CancellationToken ct) =>
        {
            var userId = user.GetUserId();
            var items = await service.BulkAddItemsAsync(userId, dtos, ct);
            return Results.Ok(items);
        })
        .WithName("BulkAddPantryItems")
        .WithSummary("Agrega múltiples ingredientes en bloque a la despensa")
        .Produces<List<PantryItemDto>>(StatusCodes.Status200OK);

        // Actualizar cantidad o categoría de un ingrediente en la despensa
        group.MapPut("/{id:guid}", async (Guid id, UpdatePantryItemDto dto, IPantryService service, ClaimsPrincipal user, CancellationToken ct) =>
        {
            var userId = user.GetUserId();
            var updated = await service.UpdateItemAsync(id, userId, dto, ct);
            return Results.Ok(updated);
        })
        .WithName("UpdatePantryItem")
        .WithSummary("Actualiza los detalles o cantidad de un ingrediente en la despensa")
        .Produces<PantryItemDto>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status404NotFound);

        // Eliminar ingrediente de la despensa
        group.MapDelete("/{id:guid}", async (Guid id, IPantryService service, ClaimsPrincipal user, CancellationToken ct) =>
        {
            var userId = user.GetUserId();
            await service.DeleteItemAsync(id, userId, ct);
            return Results.NoContent();
        })
        .WithName("DeletePantryItem")
        .WithSummary("Elimina un ingrediente de la despensa del usuario")
        .Produces(StatusCodes.Status204NoContent)
        .ProducesProblem(StatusCodes.Status404NotFound);
    }
}
