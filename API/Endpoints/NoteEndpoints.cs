using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Nexodus_Back.Application.DTOs.Note;
using Nexodus_Back.Application.Interfaces;
using System.Security.Claims;

namespace Nexodus_Back.API.Endpoints;

public static class NoteEndpoints
{
    public static void MapNoteEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/notes").RequireAuthorization().WithTags("Notes");

        group.MapGet("/", async (INoteService noteService, ClaimsPrincipal user) =>
        {
            var userId = Guid.Parse(user.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var notes = await noteService.GetAllNotesAsync(userId);
            return Results.Ok(notes);
        });

        group.MapGet("/{id:guid}", async (Guid id, INoteService noteService, ClaimsPrincipal user) =>
        {
            var userId = Guid.Parse(user.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var note = await noteService.GetNoteByIdAsync(id, userId);
            return Results.Ok(note);
        });

        group.MapPost("/", async (
            CreateNoteDto dto, 
            INoteService noteService, 
            IValidator<CreateNoteDto> validator, 
            ClaimsPrincipal user) =>
        {
            var userId = Guid.Parse(user.FindFirstValue(ClaimTypes.NameIdentifier)!);
            
            var validationResult = await validator.ValidateAsync(dto);
            if (!validationResult.IsValid)
            {
                return Results.ValidationProblem(validationResult.ToDictionary());
            }

            var note = await noteService.CreateNoteAsync(userId, dto);
            return Results.Created($"/api/notes/{note.Id}", note);
        });

        group.MapPatch("/{id:guid}", async (
            Guid id, 
            UpdateNoteDto dto, 
            INoteService noteService, 
            IValidator<UpdateNoteDto> validator,
            ClaimsPrincipal user) =>
        {
            var userId = Guid.Parse(user.FindFirstValue(ClaimTypes.NameIdentifier)!);
            
            var validationResult = await validator.ValidateAsync(dto);
            if (!validationResult.IsValid)
            {
                return Results.ValidationProblem(validationResult.ToDictionary());
            }

            var note = await noteService.UpdateNoteAsync(id, userId, dto);
            return Results.Ok(note);
        });

        group.MapDelete("/{id:guid}", async (Guid id, INoteService noteService, ClaimsPrincipal user) =>
        {
            var userId = Guid.Parse(user.FindFirstValue(ClaimTypes.NameIdentifier)!);
            await noteService.DeleteNoteAsync(id, userId);
            return Results.NoContent();
        });
    }
}
