using System.Threading;
using System.Threading.Tasks;
using Nexodus_Back.Application.DTOs.Auth;

namespace Nexodus_Back.Application.Interfaces;

/// <summary>
/// Defines authentication and authorization operations.
/// </summary>
public interface IAuthService
{
    /// <summary>
    /// Registers a new user.
    /// </summary>
    Task<AuthResponse> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Authenticates a user and generates JWT tokens.
    /// </summary>
    Task<AuthResponse> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Refreshes an expired JWT token using a valid refresh token.
    /// </summary>
    Task<AuthResponse> RefreshTokenAsync(RefreshTokenRequest request, CancellationToken cancellationToken = default);
}
