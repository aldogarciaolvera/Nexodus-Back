using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Mvc;
using Nexodus_Back.Application.DTOs.Category;
using Nexodus_Back.Application.DTOs.Common;
using Nexodus_Back.Application.Interfaces;
using System.Security.Claims;
using System;

namespace Nexodus_Back.API.Endpoints;

public static class CategoryEndpoints
{
    public static void MapCategoryEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/categories")
            .WithTags("Categories")
            .RequireAuthorization();

        group.MapPost("/", async (
            [FromBody] CreateCategoryRequest request,
            ICategoryService categoryService,
            ClaimsPrincipal user) =>
        {
            var userIdStr = user.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!Guid.TryParse(userIdStr, out var userId))
                return Results.Unauthorized();

            var category = await categoryService.CreateAsync(userId, request);
            return Results.Created($"/api/categories/{category.Id}", ApiResponse<CategoryDto>.Success(category, 201));
        })
        .WithName("CreateCategory")
        .Produces<CategoryDto>(StatusCodes.Status201Created)
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .ProducesProblem(StatusCodes.Status409Conflict);

        group.MapGet("/", async (
            ICategoryService categoryService,
            ClaimsPrincipal user) =>
        {
            var userIdStr = user.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!Guid.TryParse(userIdStr, out var userId))
                return Results.Unauthorized();

            var categories = await categoryService.GetAllByUserIdAsync(userId);
            return Results.Ok(ApiResponse<IEnumerable<CategoryDto>>.Success(categories));
        })
        .WithName("GetAllCategories")
        .Produces<IEnumerable<CategoryDto>>(StatusCodes.Status200OK);

        group.MapGet("/{id:guid}", async (
            Guid id,
            ICategoryService categoryService,
            ClaimsPrincipal user) =>
        {
            var userIdStr = user.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!Guid.TryParse(userIdStr, out var userId))
                return Results.Unauthorized();

            var category = await categoryService.GetByIdAsync(userId, id);
            return Results.Ok(ApiResponse<CategoryDto>.Success(category));
        })
        .WithName("GetCategoryById")
        .Produces<CategoryDto>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status404NotFound);

        group.MapPut("/{id:guid}", async (
            Guid id,
            [FromBody] UpdateCategoryRequest request,
            ICategoryService categoryService,
            ClaimsPrincipal user) =>
        {
            var userIdStr = user.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!Guid.TryParse(userIdStr, out var userId))
                return Results.Unauthorized();

            var category = await categoryService.UpdateAsync(userId, id, request);
            return Results.Ok(ApiResponse<CategoryDto>.Success(category));
        })
        .WithName("UpdateCategory")
        .Produces<CategoryDto>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status404NotFound)
        .ProducesProblem(StatusCodes.Status409Conflict);

        group.MapDelete("/{id:guid}", async (
            Guid id,
            ICategoryService categoryService,
            ClaimsPrincipal user) =>
        {
            var userIdStr = user.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!Guid.TryParse(userIdStr, out var userId))
                return Results.Unauthorized();

            await categoryService.DeleteAsync(userId, id);
            return Results.Ok(ApiResponse<object>.Success(null));
        })
        .WithName("DeleteCategory")
        .Produces<ApiResponse<object>>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status404NotFound);
    }
}
