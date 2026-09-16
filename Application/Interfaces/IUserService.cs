using System.Threading.Tasks;
using Nexodus_Back.Application.DTOs.User;

namespace Nexodus_Back.Application.Interfaces;

public interface IUserService
{
    Task<UserResponse> GetCurrentUserAsync(string userId);
    Task<UserResponse> UpdateUserAsync(string userId, UpdateUserRequest request);
}
