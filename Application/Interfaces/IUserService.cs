using System.Threading;
using System.Threading.Tasks;
using Nexodus_Back.Application.DTOs.User;

namespace Nexodus_Back.Application.Interfaces;

/// <summary>
/// Service providing user profile retrieval and management.
/// </summary>
public interface IUserService
{
    /// <summary>
    /// Retrieves current user profile details.
    /// </summary>
    Task<UserResponse> GetCurrentUserAsync(string userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates current user profile details.
    /// </summary>
    Task<UserResponse> UpdateUserAsync(string userId, UpdateUserRequest request, CancellationToken cancellationToken = default);
}
