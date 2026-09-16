using System.Linq;
using System.Security.Claims;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Nexodus_Back.Application.DTOs.Common;
using Nexodus_Back.Application.DTOs.User;
using Nexodus_Back.Application.Interfaces;
using Nexodus_Back.Core.Exceptions;

namespace Nexodus_Back.API.Endpoints;

public static class UserEndpoints
{
    public static void MapUserEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/user").WithTags("User").RequireAuthorization();

        group.MapGet("/me", async (IUserService userService, ClaimsPrincipal user) =>
        {
            var userId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            
            if (string.IsNullOrEmpty(userId))
            {
                throw new UnauthorizedException("ID de usuario no encontrado en el token.");
            }

            var response = await userService.GetCurrentUserAsync(userId);
            return Results.Ok(ApiResponse<UserResponse>.Success(response));
        });

        group.MapPut("/me", async (UpdateUserRequest request, IUserService userService, FluentValidation.IValidator<UpdateUserRequest> validator, ClaimsPrincipal user) =>
        {
            var validationResult = await validator.ValidateAsync(request);
            if (!validationResult.IsValid)
            {
                var errors = validationResult.Errors
                    .GroupBy(x => x.PropertyName)
                    .ToDictionary(g => g.Key, g => g.Select(x => x.ErrorMessage).ToArray());
                throw new ValidationException(errors);
            }

            var userId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            
            if (string.IsNullOrEmpty(userId))
            {
                throw new UnauthorizedException("ID de usuario no encontrado en el token.");
            }

            var response = await userService.UpdateUserAsync(userId, request);
            return Results.Ok(ApiResponse<UserResponse>.Success(response));
        });
    }
}
