namespace Nexodus_Back.Application.DTOs.User;

public class UserResponse
{
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? PhoneNumber { get; set; }
}
