using System;
using System.Threading;
using System.Threading.Tasks;
using Nexodus_Back.Core.Entities;

namespace Nexodus_Back.Core.Interfaces;

/// <summary>
/// Repository interface for user accounts and authentication refresh tokens.
/// </summary>
public interface IUserRepository
{
    /// <summary>
    /// Finds a user by email address.
    /// </summary>
    Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default);

    /// <summary>
    /// Finds a user by username.
    /// </summary>
    Task<User?> GetByUsernameAsync(string username, CancellationToken cancellationToken = default);

    /// <summary>
    /// Persists a new user entity.
    /// </summary>
    Task<User> AddAsync(User user, CancellationToken cancellationToken = default);

    /// <summary>
    /// Persists a new refresh token entity.
    /// </summary>
    Task<RefreshToken> AddRefreshTokenAsync(RefreshToken token, CancellationToken cancellationToken = default);

    /// <summary>
    /// Finds a refresh token by its raw token string.
    /// </summary>
    Task<RefreshToken?> GetRefreshTokenAsync(string token, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates the state of an existing refresh token.
    /// </summary>
    Task UpdateRefreshTokenAsync(RefreshToken token, CancellationToken cancellationToken = default);

    /// <summary>
    /// Finds a user by unique identifier.
    /// </summary>
    Task<User?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates an existing user profile entity.
    /// </summary>
    Task UpdateAsync(User user, CancellationToken cancellationToken = default);
}
