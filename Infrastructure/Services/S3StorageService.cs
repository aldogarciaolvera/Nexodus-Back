using Amazon.S3;
using Amazon.S3.Model;
using Microsoft.Extensions.Configuration;
using Nexodus_Back.Core.Interfaces;

namespace Nexodus_Back.Infrastructure.Services;

public class S3StorageService : IStorageService
{
    private readonly string _bucketName;
    private readonly AmazonS3Client _s3Client;

    public S3StorageService()
    {
        string accessKey = Environment.GetEnvironmentVariable("S3_ACCESS_KEY") ?? throw new ArgumentNullException("S3_ACCESS_KEY is missing in environment variables");
        string secretKey = Environment.GetEnvironmentVariable("S3_SECRET_KEY") ?? throw new ArgumentNullException("S3_SECRET_KEY is missing in environment variables");
        string serviceUrl = Environment.GetEnvironmentVariable("S3_ENDPOINT") ?? throw new ArgumentNullException("S3_ENDPOINT is missing in environment variables");
        
        // Default bucket name to nexodus if not provided
        _bucketName = Environment.GetEnvironmentVariable("S3_BUCKET_NAME") ?? "nexodus";

        var config = new AmazonS3Config
        {
            ServiceURL = serviceUrl.StartsWith("http") ? serviceUrl : $"https://{serviceUrl}",
            ForcePathStyle = true // Requerido normalmente para S3 customizados (como MinIO o Cloudflare R2) para que use s3.url.com/bucket/key en vez de bucket.s3.url.com/key
        };

        _s3Client = new AmazonS3Client(accessKey, secretKey, config);
    }

    public Task<string> GetFileUrlAsync(string key)
    {
        if (string.IsNullOrEmpty(key))
            return Task.FromResult(string.Empty);

        var request = new GetPreSignedUrlRequest
        {
            BucketName = _bucketName,
            Key = key,
            Expires = DateTime.UtcNow.AddHours(2) // La URL expirará en 2 horas
        };

        string url = _s3Client.GetPreSignedURL(request);
        return Task.FromResult(url);
    }
}
