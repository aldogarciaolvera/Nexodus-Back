namespace Nexodus_Back.Core.Interfaces;

public interface IStorageService
{
    /// <summary>
    /// Gets the public or pre-signed URL for a given object key in the storage service.
    /// </summary>
    /// <param name="key">The key/path of the object in storage (e.g., 'gifs/exercise.gif')</param>
    /// <returns>The resolved URL</returns>
    Task<string> GetFileUrlAsync(string key);
}
