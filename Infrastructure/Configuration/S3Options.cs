using System.ComponentModel.DataAnnotations;

namespace Nexodus_Back.Infrastructure.Configuration;

/// <summary>
/// Strongly-typed configuration options for Amazon S3 / MinIO storage.
/// </summary>
public class S3Options
{
    public const string SectionName = "S3";

    [Required(ErrorMessage = "S3 AccessKey is required.")]
    public string AccessKey { get; set; } = string.Empty;

    [Required(ErrorMessage = "S3 SecretKey is required.")]
    public string SecretKey { get; set; } = string.Empty;

    [Required(ErrorMessage = "S3 ServiceUrl / Endpoint is required.")]
    public string ServiceUrl { get; set; } = string.Empty;

    public string BucketName { get; set; } = "nexodus";
}
