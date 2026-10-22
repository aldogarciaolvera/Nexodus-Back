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
        var existingUser = await _userRepository.GetByEmailAsync(request.Email) ?? await _userRepository.GetByUsernameAsync(request.Username);
        
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

        var createdUser = await _userRepository.AddAsync(user);

        return await GenerateAuthResponseAsync(createdUser);
    }

    public async Task<AuthResponse> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default)
    {
        request.Email = request.Email?.ToLowerInvariant() ?? string.Empty;
        var user = await _userRepository.GetByEmailAsync(request.Email);
        
        if (user == null || !_passwordHasher.Verify(request.Password, user.PasswordHash))
        {
            throw new UnauthorizedException("Correo o contraseña inválidos.");
        }

        return await GenerateAuthResponseAsync(user);
    }

    public async Task<AuthResponse> RefreshTokenAsync(RefreshTokenRequest request, CancellationToken cancellationToken = default)
    {
        var refreshToken = await _userRepository.GetRefreshTokenAsync(request.RefreshToken);

        if (refreshToken == null || refreshToken.Invalidated || refreshToken.Used || refreshToken.ExpiryDate < DateTime.UtcNow)
        {
            throw new UnauthorizedException("Refresh token inválido.");
        }

        refreshToken.Used = true;
        await _userRepository.UpdateRefreshTokenAsync(refreshToken);

        if (refreshToken.User == null)
        {
             throw new UnauthorizedException("Usuario no encontrado.");
        }

        return await GenerateAuthResponseAsync(refreshToken.User);
    }

    private async Task<AuthResponse> GenerateAuthResponseAsync(User user)
    {
        var token = _jwtProvider.GenerateToken(user);
        var refreshToken = _jwtProvider.GenerateRefreshToken(user);

        await _userRepository.AddRefreshTokenAsync(refreshToken);

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
