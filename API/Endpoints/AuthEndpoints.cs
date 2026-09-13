using System;
using System.Linq;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Nexodus_Back.Application.DTOs.Auth;
using Nexodus_Back.Application.Services;
using FluentValidation;
using Nexodus_Back.Core.Exceptions;

namespace Nexodus_Back.API.Endpoints;

public static class AuthEndpoints
{
    public static void MapAuthEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/auth").WithTags("Auth");

        group.MapPost("/register", async (RegisterRequest request, IAuthService authService, IValidator<RegisterRequest> validator) =>
        {
            var validationResult = await validator.ValidateAsync(request);
            if (!validationResult.IsValid)
            {
                var errors = validationResult.Errors
                    .GroupBy(x => x.PropertyName)
                    .ToDictionary(g => g.Key, g => g.Select(x => x.ErrorMessage).ToArray());
                throw new Nexodus_Back.Core.Exceptions.ValidationException(errors);
            }

            var response = await authService.RegisterAsync(request);
            return Results.Ok(response);
        });

        group.MapPost("/login", async (LoginRequest request, IAuthService authService, IValidator<LoginRequest> validator) =>
        {
            var validationResult = await validator.ValidateAsync(request);
            if (!validationResult.IsValid)
            {
                var errors = validationResult.Errors
                    .GroupBy(x => x.PropertyName)
                    .ToDictionary(g => g.Key, g => g.Select(x => x.ErrorMessage).ToArray());
                throw new Nexodus_Back.Core.Exceptions.ValidationException(errors);
            }

            var response = await authService.LoginAsync(request);
            return Results.Ok(response);
        });

        group.MapPost("/refresh", async (RefreshTokenRequest request, IAuthService authService) =>
        {
            var response = await authService.RefreshTokenAsync(request);
            return Results.Ok(response);
        });

        // Protected test endpoint
        app.MapGet("/api/protected", () => Results.Ok(new { Message = "You are authenticated!" }))
            .RequireAuthorization()
            .WithTags("Test");
            
        // Admin only test endpoint
        app.MapGet("/api/admin-only", () => Results.Ok(new { Message = "You are an admin!" }))
            .RequireAuthorization(policy => policy.RequireRole("Admin"))
            .WithTags("Test");
    }
}
