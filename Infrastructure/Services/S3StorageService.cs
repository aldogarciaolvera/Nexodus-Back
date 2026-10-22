using System;
using System.Threading.Tasks;
using Amazon.S3;
using Amazon.S3.Model;
using Microsoft.Extensions.Options;
using Nexodus_Back.Core.Interfaces;
using Nexodus_Back.Infrastructure.Configuration;

namespace Nexodus_Back.Infrastructure.Services;

/// <summary>
/// Provides S3 pre-signed URL generation and object storage interaction using an injected IAmazonS3 client.
/// </summary>
public class S3StorageService(IAmazonS3 s3Client, IOptions<S3Options> options) : IStorageService
{
    private readonly IAmazonS3 _s3Client = s3Client ?? throw new ArgumentNullException(nameof(s3Client));
    private readonly S3Options _options = options?.Value ?? throw new ArgumentNullException(nameof(options));

    public Task<string> GetFileUrlAsync(string key)
    {
        if (string.IsNullOrEmpty(key))
            return Task.FromResult(string.Empty);

        var request = new GetPreSignedUrlRequest
        {
            BucketName = _options.BucketName,
            Key = key,
            Expires = DateTime.UtcNow.AddHours(2)
        };

        string url = _s3Client.GetPreSignedURL(request);
        return Task.FromResult(url);
    }
}
