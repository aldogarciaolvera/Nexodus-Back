using System;
using System.Collections.Generic;
using System.Security.Claims;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Nexodus_Back.API.Extensions;
using Nexodus_Back.API.Filters;
using Nexodus_Back.Application.DTOs.Note;
using Nexodus_Back.Application.Interfaces;

namespace Nexodus_Back.API.Endpoints;

public static class NoteEndpoints
{
    public static void MapNoteEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/notes")
            .WithTags("Notes")
            .RequireAuthorization();

        group.MapGet("/", async (int? page, int? pageSize, INoteService noteService, ClaimsPrincipal user, CancellationToken ct) =>
        {
            var userId = user.GetUserId();
            var notes = await noteService.GetAllNotesAsync(userId, page, pageSize, ct);
            return Results.Ok(notes);
        })
        .WithName("GetAllNotes")
        .WithSummary("Obtiene las notas del usuario con soporte opcional de paginación")
        .Produces<IEnumerable<NoteDto>>(StatusCodes.Status200OK);

        group.MapGet("/{id:guid}", async (Guid id, INoteService noteService, ClaimsPrincipal user, CancellationToken ct) =>
        {
            var userId = user.GetUserId();
            var note = await noteService.GetNoteByIdAsync(id, userId, ct);
            return Results.Ok(note);
        })
        .WithName("GetNoteById")
        .WithSummary("Obtiene una nota específica por su ID")
        .Produces<NoteDto>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status404NotFound);

        group.MapPost("/", async (CreateNoteDto dto, INoteService noteService, ClaimsPrincipal user, CancellationToken ct) =>
        {
            var userId = user.GetUserId();
            var note = await noteService.CreateNoteAsync(userId, dto, ct);
            return Results.Created($"/api/notes/{note.Id}", note);
        })
        .AddEndpointFilter<ValidationFilter<CreateNoteDto>>()
        .WithName("CreateNote")
        .WithSummary("Crea una nueva nota o checklist")
        .Produces<NoteDto>(StatusCodes.Status201Created)
        .ProducesValidationProblem(StatusCodes.Status400BadRequest);

        group.MapPatch("/{id:guid}", async (Guid id, UpdateNoteDto dto, INoteService noteService, ClaimsPrincipal user, CancellationToken ct) =>
        {
            var userId = user.GetUserId();
            var note = await noteService.UpdateNoteAsync(id, userId, dto, ct);
            return Results.Ok(note);
        })
        .AddEndpointFilter<ValidationFilter<UpdateNoteDto>>()
        .WithName("UpdateNote")
        .WithSummary("Actualiza los detalles o checklist de una nota existente")
        .Produces<NoteDto>(StatusCodes.Status200OK)
        .ProducesValidationProblem(StatusCodes.Status400BadRequest)
        .ProducesProblem(StatusCodes.Status404NotFound);

        group.MapDelete("/{id:guid}", async (Guid id, INoteService noteService, ClaimsPrincipal user, CancellationToken ct) =>
        {
            var userId = user.GetUserId();
            await noteService.DeleteNoteAsync(id, userId, ct);
            return Results.NoContent();
        })
        .WithName("DeleteNote")
        .WithSummary("Elimina una nota")
        .Produces(StatusCodes.Status204NoContent)
        .ProducesProblem(StatusCodes.Status404NotFound);
    }
}
