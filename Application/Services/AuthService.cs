using System;
using System.Threading;
using System.Threading.Tasks;
using Nexodus_Back.Application.DTOs.Auth;
using Nexodus_Back.Application.Interfaces;
using Nexodus_Back.Core.Entities;
using Nexodus_Back.Core.Exceptions;
using Nexodus_Back.Core.Interfaces;

namespace Nexodus_Back.Application.Services;

/// <summary>
/// Provides user authentication, registration and token issuance operations.
/// </summary>
public class AuthService(
    IUserRepository userRepository,
    IPasswordHasher passwordHasher,
    IJwtProvider jwtProvider) : IAuthService
{
    private readonly IUserRepository _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
    private readonly IPasswordHasher _passwordHasher = passwordHasher ?? throw new ArgumentNullException(nameof(passwordHasher));
    private readonly IJwtProvider _jwtProvider = jwtProvider ?? throw new ArgumentNullException(nameof(jwtProvider));

    public async Task<AuthResponse> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken = default)
    {
        request.Email = request.Email?.ToLowerInvariant() ?? string.Empty;
        var existingUser = await _userRepository.GetByEmailAsync(request.Email, cancellationToken) 
            ?? await _userRepository.GetByUsernameAsync(request.Username, cancellationToken);
        
        if (existingUser != null)
        {
            throw new ConflictException("El usuario ya existe.");
        }

        var user = new User
        {
            Username = request.Username,
            Email = request.Email,
            PasswordHash = _passwordHasher.Hash(request.Password),
            PhoneNumber = request.PhoneNumber,
            Role = "User",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        var createdUser = await _userRepository.AddAsync(user, cancellationToken);

        return await GenerateAuthResponseAsync(createdUser, cancellationToken);
    }

    public async Task<AuthResponse> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default)
    {
        request.Email = request.Email?.ToLowerInvariant() ?? string.Empty;
        var user = await _userRepository.GetByEmailAsync(request.Email, cancellationToken);
        
        if (user == null || !_passwordHasher.Verify(request.Password, user.PasswordHash))
        {
            throw new UnauthorizedException("Correo o contraseña inválidos.");
        }

        return await GenerateAuthResponseAsync(user, cancellationToken);
    }

    public async Task<AuthResponse> RefreshTokenAsync(RefreshTokenRequest request, CancellationToken cancellationToken = default)
    {
        var refreshToken = await _userRepository.GetRefreshTokenAsync(request.RefreshToken, cancellationToken);

        if (refreshToken == null || refreshToken.Invalidated || refreshToken.Used || refreshToken.ExpiryDate < DateTime.UtcNow)
        {
            throw new UnauthorizedException("Refresh token inválido o revocado.");
        }

        refreshToken.Used = true;
        await _userRepository.UpdateRefreshTokenAsync(refreshToken, cancellationToken);

        if (refreshToken.User == null)
        {
             throw new UnauthorizedException("Usuario no encontrado.");
        }

        return await GenerateAuthResponseAsync(refreshToken.User, cancellationToken);
    }

    public async Task LogoutAsync(string refreshToken, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(refreshToken))
            return;

        var token = await _userRepository.GetRefreshTokenAsync(refreshToken, cancellationToken);
        if (token != null)
        {
            token.Used = true;
            token.Invalidated = true;
            await _userRepository.UpdateRefreshTokenAsync(token, cancellationToken);
        }
    }

    private async Task<AuthResponse> GenerateAuthResponseAsync(User user, CancellationToken cancellationToken)
    {
        var token = _jwtProvider.GenerateToken(user);
        var refreshToken = _jwtProvider.GenerateRefreshToken(user);

        await _userRepository.AddRefreshTokenAsync(refreshToken, cancellationToken);

        return new AuthResponse
        {
            Token = token,
            RefreshToken = refreshToken.Token,
            Username = user.Username,
            Email = user.Email,
            PhoneNumber = user.PhoneNumber,
            Role = user.Role
        };
    }
}
