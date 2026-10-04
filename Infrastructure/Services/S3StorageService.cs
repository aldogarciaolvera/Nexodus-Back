using Nexodus_Back.Core.Interfaces;

namespace Nexodus_Back.Infrastructure.Services;

public class S3StorageService : IStorageService
{
    public Task<string> GetFileUrlAsync(string key)
    {
        // For now, return a placeholder URL or a fake S3 URL
        // In the future, this will use AWS SDK to generate a pre-signed URL or construct a public S3 URL
        string baseUrl = "https://your-s3-bucket-url.com/";
        return Task.FromResult(baseUrl + key);
    }
}
