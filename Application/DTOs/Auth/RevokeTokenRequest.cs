namespace Nexodus_Back.Application.DTOs.Auth;

public class RevokeTokenRequest
{
    public string RefreshToken { get; set; } = string.Empty;
}

