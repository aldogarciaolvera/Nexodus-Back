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

public static class RecipeEndpoints
{
    public static void MapRecipeEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/recipes")
            .WithTags("Recipes")
            .RequireAuthorization();

        // Buscar recetas (catálogo base, propias y comunitarias)
        group.MapGet("/", async (
            string? query,
            string? mealType,
            string? dietType,
            bool? communityOnly,
            int? page,
            int? pageSize,
            IRecipeService service,
            ClaimsPrincipal user,
            CancellationToken ct) =>
        {
            var userId = user.GetUserId();
            var (items, totalCount) = await service.SearchAsync(
                query,
                mealType,
                dietType,
                userId,
                communityOnly ?? false,
                page ?? 1,
                pageSize ?? 20,
                ct);

            return Results.Ok(new
            {
                items,
                totalCount,
                page = page ?? 1,
                pageSize = pageSize ?? 20
            });
        })
        .WithName("SearchRecipes")
        .WithSummary("Busca recetas en el catálogo oficial, recetas propias del usuario y recetas comunitarias")
        .Produces(StatusCodes.Status200OK);

        // Obtener receta por ID con ingredientes y pasos detallados
        group.MapGet("/{id:guid}", async (Guid id, IRecipeService service, ClaimsPrincipal user, CancellationToken ct) =>
        {
            var userId = user.GetUserId();
            var recipe = await service.GetByIdAsync(id, userId, ct);
            return Results.Ok(recipe);
        })
        .WithName("GetRecipeById")
        .WithSummary("Obtiene el detalle completo de una receta incluyendo ingredientes y pasos")
        .Produces<RecipeDto>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status404NotFound);

        // Obtener variantes derivadas de una receta
        group.MapGet("/{id:guid}/variants", async (Guid id, IRecipeService service, CancellationToken ct) =>
        {
            var variants = await service.GetVariantsAsync(id, ct);
            return Results.Ok(variants);
        })
        .WithName("GetRecipeVariants")
        .WithSummary("Obtiene las variantes comunitarias derivadas de una receta")
        .Produces<List<RecipeDto>>(StatusCodes.Status200OK);

        // Crear nueva receta
        group.MapPost("/", async (CreateRecipeDto dto, IRecipeService service, ClaimsPrincipal user, CancellationToken ct) =>
        {
            var userId = user.GetUserId();
            var recipe = await service.CreateAsync(userId, dto, ct);
            return Results.Created($"/api/recipes/{recipe.Id}", recipe);
        })
        .AddEndpointFilter<ValidationFilter<CreateRecipeDto>>()
        .WithName("CreateRecipe")
        .WithSummary("Crea una receta propia del usuario o la publica en la comunidad")
        .Produces<RecipeDto>(StatusCodes.Status201Created)
        .ProducesValidationProblem(StatusCodes.Status400BadRequest);

        // Crear variante de una receta existente (Opción B)
        group.MapPost("/{id:guid}/variants", async (Guid id, CreateRecipeDto dto, IRecipeService service, ClaimsPrincipal user, CancellationToken ct) =>
        {
            var userId = user.GetUserId();
            var variant = await service.CreateVariantAsync(id, userId, dto, ct);
            return Results.Created($"/api/recipes/{variant.Id}", variant);
        })
        .AddEndpointFilter<ValidationFilter<CreateRecipeDto>>()
        .WithName("CreateRecipeVariant")
        .WithSummary("Crea una variante personalizada de una receta comunitaria existente vinculada por ParentRecipeId")
        .Produces<RecipeDto>(StatusCodes.Status201Created)
        .ProducesProblem(StatusCodes.Status404NotFound)
        .ProducesValidationProblem(StatusCodes.Status400BadRequest);

        // Registrar receta completa como plato en el diario de comidas (Opción A Híbrido Fitia)
        group.MapPost("/log", async (LogRecipeAsMealDto dto, IRecipeService service, ClaimsPrincipal user, CancellationToken ct) =>
        {
            var userId = user.GetUserId();
            var meal = await service.LogRecipeAsMealAsync(userId, dto, ct);
            return Results.Ok(meal);
        })
        .AddEndpointFilter<ValidationFilter<LogRecipeAsMealDto>>()
        .WithName("LogRecipeAsMeal")
        .WithSummary("Registra una receta completa como comida en el diario, guardando cada ingrediente atómico para edición libre")
        .Produces<MealDto>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status404NotFound)
        .ProducesValidationProblem(StatusCodes.Status400BadRequest);

        // Generar receta con IA adaptada a los macros restantes (Estilo Yuki Fitia)
        group.MapPost("/ai/match-macros", async (
            MatchMacrosRecipeRequestDto dto,
            IRecipeAiService aiService,
            ClaimsPrincipal user,
            CancellationToken ct) =>
        {
            var userId = user.GetUserId();
            var recipe = await aiService.GenerateRecipeForRemainingMacrosAsync(userId, dto, ct);
            return Results.Ok(recipe);
        })
        .AddEndpointFilter<ValidationFilter<MatchMacrosRecipeRequestDto>>()
        .RequireRateLimiting("AiPolicy")
        .WithName("GenerateRecipeForRemainingMacros")
        .WithSummary("Genera una receta personalizada mediante IA para cubrir con precisión los macros restantes del día")
        .Produces<RecipeDto>(StatusCodes.Status200OK)
        .ProducesValidationProblem(StatusCodes.Status400BadRequest);

        // Modo Despensa: Sugerir recetas con lo que el usuario tiene a mano (Opción A Híbrido)
        group.MapPost("/ai/pantry-cook", async (
            PantryCookRequestDto dto,
            IRecipeAiService aiService,
            ClaimsPrincipal user,
            CancellationToken ct) =>
        {
            var userId = user.GetUserId();
            var suggestions = await aiService.SuggestRecipesFromPantryAsync(userId, dto, ct);
            return Results.Ok(suggestions);
        })
        .AddEndpointFilter<ValidationFilter<PantryCookRequestDto>>()
        .RequireRateLimiting("AiPolicy")
        .WithName("SuggestRecipesFromPantry")
        .WithSummary("Sugiere 3 recetas con ingredientes de la despensa y marca faltantes para la lista de compras")
        .Produces<List<PantryCookSuggestionDto>>(StatusCodes.Status200OK)
        .ProducesValidationProblem(StatusCodes.Status400BadRequest);

        // Actualizar receta propia
        group.MapPut("/{id:guid}", async (Guid id, UpdateRecipeDto dto, IRecipeService service, ClaimsPrincipal user, CancellationToken ct) =>
        {
            var userId = user.GetUserId();
            var updated = await service.UpdateAsync(id, userId, dto, ct);
            return Results.Ok(updated);
        })
        .AddEndpointFilter<ValidationFilter<UpdateRecipeDto>>()
        .WithName("UpdateRecipe")
        .WithSummary("Actualiza los ingredientes, pasos o metadatos de una receta propia")
        .Produces<RecipeDto>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status404NotFound)
        .ProducesValidationProblem(StatusCodes.Status400BadRequest);

        // Eliminar receta propia
        group.MapDelete("/{id:guid}", async (Guid id, IRecipeService service, ClaimsPrincipal user, CancellationToken ct) =>
        {
            var userId = user.GetUserId();
            await service.DeleteAsync(id, userId, ct);
            return Results.NoContent();
        })
        .WithName("DeleteRecipe")
        .WithSummary("Elimina una receta creada por el usuario")
        .Produces(StatusCodes.Status204NoContent)
        .ProducesProblem(StatusCodes.Status404NotFound);
    }
}
