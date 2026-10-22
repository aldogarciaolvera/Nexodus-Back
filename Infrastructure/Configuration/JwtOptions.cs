using System.ComponentModel.DataAnnotations;

namespace Nexodus_Back.Infrastructure.Configuration;

/// <summary>
/// Strongly-typed configuration options for JWT token generation and validation.
/// </summary>
public class JwtOptions
{
    public const string SectionName = "Jwt";

    [Required(ErrorMessage = "JWT Secret is required.")]
    public string Secret { get; set; } = string.Empty;

    public string Issuer { get; set; } = string.Empty;

    public string Audience { get; set; } = string.Empty;

    public int ExpirationMinutes { get; set; } = 60;
}
