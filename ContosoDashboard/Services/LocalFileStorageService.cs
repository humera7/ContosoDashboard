using System.IO;

namespace ContosoDashboard.Services;

public class LocalFileStorageService : IFileStorageService
{
    private readonly string _rootPath;

    public LocalFileStorageService()
    {
        var appRoot = AppContext.BaseDirectory;
        _rootPath = Path.Combine(appRoot, "AppData", "uploads");
        Directory.CreateDirectory(_rootPath);
    }

    public async Task<string> UploadAsync(Stream fileStream, string fileName, string contentType, string relativePath)
    {
        var absolutePath = Path.Combine(_rootPath, relativePath.Replace('/', Path.DirectorySeparatorChar));
        var directory = Path.GetDirectoryName(absolutePath);
        if (!string.IsNullOrWhiteSpace(directory))
        {
            Directory.CreateDirectory(directory);
        }

        await using var target = File.Create(absolutePath);
        if (fileStream.CanSeek)
        {
            fileStream.Position = 0;
        }
        await fileStream.CopyToAsync(target);

        return relativePath;
    }

    public Task DeleteAsync(string relativePath)
    {
        var absolutePath = Path.Combine(_rootPath, relativePath.Replace('/', Path.DirectorySeparatorChar));
        if (File.Exists(absolutePath))
        {
            File.Delete(absolutePath);
        }

        return Task.CompletedTask;
    }

    public async Task<Stream> DownloadAsync(string relativePath)
    {
        var absolutePath = Path.Combine(_rootPath, relativePath.Replace('/', Path.DirectorySeparatorChar));
        if (!File.Exists(absolutePath))
        {
            throw new FileNotFoundException("Document not found.", absolutePath);
        }

        return await Task.FromResult<Stream>(File.OpenRead(absolutePath));
    }

    public Task<string> GetUrlAsync(string relativePath, TimeSpan expiration)
    {
        var relative = relativePath.Replace('\\', '/');
        return Task.FromResult($"/documents/download/{relative}");
    }
}
