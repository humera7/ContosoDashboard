namespace ContosoDashboard.Services;

public interface IFileStorageService
{
    Task<string> UploadAsync(Stream fileStream, string fileName, string contentType, string relativePath);
    Task DeleteAsync(string relativePath);
    Task<Stream> DownloadAsync(string relativePath);
    Task<string> GetUrlAsync(string relativePath, TimeSpan expiration);
}
