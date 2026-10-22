using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Nexodus_Back.API.Filters;
using Nexodus_Back.Application.DTOs.Auth;
using Nexodus_Back.Application.DTOs.Common;
using Nexodus_Back.Application.Interfaces;

namespace Nexodus_Back.API.Endpoints;

public static class AuthEndpoints
{
    public static void MapAuthEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/auth").WithTags("Auth");

        group.MapPost("/register", async (RegisterRequest request, IAuthService authService) =>
        {
            var response = await authService.RegisterAsync(request);
            return Results.Ok(ApiResponse<AuthResponse>.Success(response));
        })
        .AddEndpointFilter<ValidationFilter<RegisterRequest>>()
        .WithName("Register")
        .WithSummary("Registra un nuevo usuario en la plataforma")
        .Produces<ApiResponse<AuthResponse>>(StatusCodes.Status200OK)
        .ProducesValidationProblem(StatusCodes.Status400BadRequest)
        .ProducesProblem(StatusCodes.Status409Conflict);

        group.MapPost("/login", async (LoginRequest request, IAuthService authService) =>
        {
            var response = await authService.LoginAsync(request);
            return Results.Ok(ApiResponse<AuthResponse>.Success(response));
        })
        .AddEndpointFilter<ValidationFilter<LoginRequest>>()
        .WithName("Login")
        .WithSummary("Inicia sesión y genera tokens de acceso JWT y Refresh Token")
        .Produces<ApiResponse<AuthResponse>>(StatusCodes.Status200OK)
        .ProducesValidationProblem(StatusCodes.Status400BadRequest)
        .ProducesProblem(StatusCodes.Status401Unauthorized);

        group.MapPost("/refresh", async (RefreshTokenRequest request, IAuthService authService) =>
        {
            var response = await authService.RefreshTokenAsync(request);
            return Results.Ok(ApiResponse<AuthResponse>.Success(response));
        })
        .WithName("RefreshToken")
        .WithSummary("Renueva el JWT expirado utilizando un Refresh Token activo")
        .Produces<ApiResponse<AuthResponse>>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status401Unauthorized);
    }
}
