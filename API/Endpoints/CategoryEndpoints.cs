using System;
using System.Collections.Generic;
using System.Security.Claims;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Nexodus_Back.API.Extensions;
using Nexodus_Back.API.Filters;
using Nexodus_Back.Application.DTOs.Category;
using Nexodus_Back.Application.DTOs.Common;
using Nexodus_Back.Application.Interfaces;

namespace Nexodus_Back.API.Endpoints;

public static class CategoryEndpoints
{
    public static void MapCategoryEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/categories")
            .WithTags("Categories")
            .RequireAuthorization();

        group.MapPost("/", async (CreateCategoryRequest request, ICategoryService categoryService, ClaimsPrincipal user) =>
        {
            var userId = user.GetUserId();
            var category = await categoryService.CreateAsync(userId, request);
            return Results.Created($"/api/categories/{category.Id}", ApiResponse<CategoryDto>.Success(category, 201));
        })
        .AddEndpointFilter<ValidationFilter<CreateCategoryRequest>>()
        .WithName("CreateCategory")
        .WithSummary("Crea una nueva categoría financiera personalizada")
        .Produces<ApiResponse<CategoryDto>>(StatusCodes.Status201Created)
        .ProducesValidationProblem(StatusCodes.Status400BadRequest)
        .ProducesProblem(StatusCodes.Status409Conflict);

        group.MapGet("/", async (ICategoryService categoryService, ClaimsPrincipal user) =>
        {
            var userId = user.GetUserId();
            var categories = await categoryService.GetAllByUserIdAsync(userId);
            return Results.Ok(ApiResponse<IEnumerable<CategoryDto>>.Success(categories));
        })
        .WithName("GetAllCategories")
        .WithSummary("Obtiene todas las categorías del usuario")
        .Produces<ApiResponse<IEnumerable<CategoryDto>>>(StatusCodes.Status200OK);

        group.MapGet("/{id:guid}", async (Guid id, ICategoryService categoryService, ClaimsPrincipal user) =>
        {
            var userId = user.GetUserId();
            var category = await categoryService.GetByIdAsync(userId, id);
            return Results.Ok(ApiResponse<CategoryDto>.Success(category));
        })
        .WithName("GetCategoryById")
        .WithSummary("Obtiene una categoría por su ID")
        .Produces<ApiResponse<CategoryDto>>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status404NotFound);

        group.MapPut("/{id:guid}", async (Guid id, UpdateCategoryRequest request, ICategoryService categoryService, ClaimsPrincipal user) =>
        {
            var userId = user.GetUserId();
            var category = await categoryService.UpdateAsync(userId, id, request);
            return Results.Ok(ApiResponse<CategoryDto>.Success(category));
        })
        .AddEndpointFilter<ValidationFilter<UpdateCategoryRequest>>()
        .WithName("UpdateCategory")
        .WithSummary("Actualiza los datos de una categoría")
        .Produces<ApiResponse<CategoryDto>>(StatusCodes.Status200OK)
        .ProducesValidationProblem(StatusCodes.Status400BadRequest)
        .ProducesProblem(StatusCodes.Status404NotFound)
        .ProducesProblem(StatusCodes.Status409Conflict);

        group.MapDelete("/{id:guid}", async (Guid id, ICategoryService categoryService, ClaimsPrincipal user) =>
        {
            var userId = user.GetUserId();
            await categoryService.DeleteAsync(userId, id);
            return Results.Ok(ApiResponse<string>.Success("Registro eliminado"));
        })
        .WithName("DeleteCategory")
        .WithSummary("Elimina una categoría")
        .Produces<ApiResponse<string>>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status404NotFound);
    }
}
