using System;
using System.Threading.Tasks;
using Nexodus_Back.Application.DTOs.User;
using Nexodus_Back.Application.Interfaces;
using Nexodus_Back.Core.Exceptions;
using Nexodus_Back.Core.Interfaces;

namespace Nexodus_Back.Application.Services;

/// <summary>
/// Provides user profile retrieval and modification operations.
/// </summary>
public class UserService(IUserRepository userRepository) : IUserService
{
    private readonly IUserRepository _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));

    public async Task<UserResponse> GetCurrentUserAsync(string userId)
    {
        if (!Guid.TryParse(userId, out var parsedId))
        {
            throw new UnauthorizedException("ID de usuario inválido.");
        }

        var user = await _userRepository.GetByIdAsync(parsedId);
        if (user == null)
        {
            throw new NotFoundException("Usuario no encontrado.");
        }

        return new UserResponse
        {
            Username = user.Username,
            Email = user.Email,
            PhoneNumber = user.PhoneNumber,
            Weight = user.Weight,
            Height = user.Height,
            Dieta = user.Dieta
        };
    }

    public async Task<UserResponse> UpdateUserAsync(string userId, UpdateUserRequest request)
    {
        if (!Guid.TryParse(userId, out var parsedId))
        {
            throw new UnauthorizedException("ID de usuario inválido.");
        }

        var user = await _userRepository.GetByIdAsync(parsedId);
        if (user == null)
        {
            throw new NotFoundException("Usuario no encontrado.");
        }

        if (user.Email != request.Email)
        {
            var existingEmail = await _userRepository.GetByEmailAsync(request.Email);
            if (existingEmail != null)
            {
                throw new ConflictException("El correo ya está registrado.");
            }
        }

        if (user.Username != request.Username)
        {
            var existingUsername = await _userRepository.GetByUsernameAsync(request.Username);
            if (existingUsername != null)
            {
                throw new ConflictException("El nombre de usuario ya está en uso.");
            }
        }

        user.Username = request.Username;
        user.Email = request.Email;
        user.PhoneNumber = request.PhoneNumber;
        user.Weight = request.Weight;
        user.Height = request.Height;
        if (request.Dieta.HasValue)
        {
            user.Dieta = request.Dieta.Value;
        }
        user.UpdatedAt = DateTime.UtcNow;

        await _userRepository.UpdateAsync(user);

        return new UserResponse
        {
            Username = user.Username,
            Email = user.Email,
            PhoneNumber = user.PhoneNumber,
            Weight = user.Weight,
            Height = user.Height,
            Dieta = user.Dieta
        };
    }
}
