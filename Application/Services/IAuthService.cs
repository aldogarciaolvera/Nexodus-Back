using System.Threading.Tasks;
using Nexodus_Back.Application.DTOs.Auth;

namespace Nexodus_Back.Application.Services;

public interface IAuthService
{
    Task<AuthResponse> RegisterAsync(RegisterRequest request);
    Task<AuthResponse> LoginAsync(LoginRequest request);
    Task<AuthResponse> RefreshTokenAsync(RefreshTokenRequest request);
}
