using System;
using System.Security.Claims;
using Nexodus_Back.Core.Exceptions;

namespace Nexodus_Back.API.Extensions;

/// <summary>
/// Extension methods for ClaimsPrincipal to securely retrieve identity information.
/// </summary>
public static class ClaimsPrincipalExtensions
{
    /// <summary>
    /// Extracts the authenticated user's unique Guid identifier from the claims.
    /// </summary>
    /// <param name="user">The ClaimsPrincipal from the current request.</param>
    /// <returns>The parsed Guid identifier.</returns>
    /// <exception cref="UnauthorizedException">Thrown when the identifier claim is missing or invalid.</exception>
    public static Guid GetUserId(this ClaimsPrincipal user)
    {
        var claimValue = user.FindFirst(ClaimTypes.NameIdentifier)?.Value 
                      ?? user.FindFirst("sub")?.Value;

        if (string.IsNullOrWhiteSpace(claimValue) || !Guid.TryParse(claimValue, out var userId))
        {
            throw new UnauthorizedException("Identificador de usuario inválido o no encontrado en el token.");
        }

        return userId;
    }
}
