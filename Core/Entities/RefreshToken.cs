using System;

namespace Nexodus_Back.Core.Entities;

public class RefreshToken
{
    public long Id { get; set; }
    public string Token { get; set; } = string.Empty;
    public string JwtId { get; set; } = string.Empty;
    public DateTime CreationDate { get; set; }
    public DateTime ExpiryDate { get; set; }
    public bool Used { get; set; }
    public bool Invalidated { get; set; }
    
    public long UserId { get; set; }
    public User? User { get; set; }
}
