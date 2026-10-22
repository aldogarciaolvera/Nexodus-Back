using System.Security.Claims;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Nexodus_Back.API.Extensions;
using Nexodus_Back.API.Filters;
using Nexodus_Back.Application.DTOs.Common;
using Nexodus_Back.Application.DTOs.User;
using Nexodus_Back.Application.Interfaces;

namespace Nexodus_Back.API.Endpoints;

public static class UserEndpoints
{
    public static void MapUserEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/user")
            .WithTags("User")
            .RequireAuthorization();

        group.MapGet("/me", async (IUserService userService, ClaimsPrincipal user) =>
        {
            var userId = user.GetUserId().ToString();
            var response = await userService.GetCurrentUserAsync(userId);
            return Results.Ok(ApiResponse<UserResponse>.Success(response));
        })
        .WithName("GetCurrentUser")
        .WithSummary("Obtiene los datos del usuario actualmente autenticado")
        .Produces<ApiResponse<UserResponse>>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status401Unauthorized)
        .ProducesProblem(StatusCodes.Status404NotFound);

        group.MapPut("/me", async (UpdateUserRequest request, IUserService userService, ClaimsPrincipal user) =>
        {
            var userId = user.GetUserId().ToString();
            var response = await userService.UpdateUserAsync(userId, request);
            return Results.Ok(ApiResponse<UserResponse>.Success(response));
        })
        .AddEndpointFilter<ValidationFilter<UpdateUserRequest>>()
        .WithName("UpdateCurrentUser")
        .WithSummary("Actualiza los datos del perfil del usuario autenticado")
        .Produces<ApiResponse<UserResponse>>(StatusCodes.Status200OK)
        .ProducesValidationProblem(StatusCodes.Status400BadRequest)
        .ProducesProblem(StatusCodes.Status409Conflict);
    }
}
