using Nexodus_Back.Core.Entities;

namespace Nexodus_Back.Core.Interfaces;

public interface IJwtProvider
{
    string GenerateToken(User user);
    RefreshToken GenerateRefreshToken(User user);
}
