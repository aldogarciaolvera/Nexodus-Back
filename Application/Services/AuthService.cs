using System;
using System.Threading.Tasks;
using Nexodus_Back.Application.DTOs.Auth;
using Nexodus_Back.Core.Entities;
using Nexodus_Back.Core.Interfaces;
using Nexodus_Back.Core.Exceptions;

namespace Nexodus_Back.Application.Services;

public class AuthService : IAuthService
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtProvider _jwtProvider;

    public AuthService(IUserRepository userRepository, IPasswordHasher passwordHasher, IJwtProvider jwtProvider)
    {
        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
        _jwtProvider = jwtProvider;
    }

    public async Task<AuthResponse> RegisterAsync(RegisterRequest request)
    {
        var existingUser = await _userRepository.GetByEmailAsync(request.Email) ?? await _userRepository.GetByUsernameAsync(request.Username);
        
        if (existingUser != null)
        {
            throw new ConflictException("User already exists.");
        }

        var user = new User
        {
            Username = request.Username,
            Email = request.Email,
            PasswordHash = _passwordHasher.Hash(request.Password),
            PhoneNumber = request.PhoneNumber,
            Role = "User", // Role por defecto
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        var createdUser = await _userRepository.AddAsync(user);

        return await GenerateAuthResponse(createdUser);
    }

    public async Task<AuthResponse> LoginAsync(LoginRequest request)
    {
        var user = await _userRepository.GetByEmailAsync(request.Email);
        
        if (user == null || !_passwordHasher.Verify(request.Password, user.PasswordHash))
        {
            throw new UnauthorizedException("Invalid email or password.");
        }

        return await GenerateAuthResponse(user);
    }

    public async Task<AuthResponse> RefreshTokenAsync(RefreshTokenRequest request)
    {
        var refreshToken = await _userRepository.GetRefreshTokenAsync(request.RefreshToken);

        if (refreshToken == null || refreshToken.Invalidated || refreshToken.Used || refreshToken.ExpiryDate < DateTime.UtcNow)
        {
            throw new UnauthorizedException("Invalid refresh token.");
        }

        refreshToken.Used = true;
        await _userRepository.UpdateRefreshTokenAsync(refreshToken);

        if (refreshToken.User == null)
        {
             throw new UnauthorizedException("User not found.");
        }

        return await GenerateAuthResponse(refreshToken.User);
    }

    private async Task<AuthResponse> GenerateAuthResponse(User user)
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
