namespace Drashyam.API.Services;

public class FileStorageService : IFileStorageService
{
    private readonly ILogger<FileStorageService> _logger;

    public FileStorageService(ILogger<FileStorageService> logger)
    {
        _logger = logger;
    }

    public async Task<string> UploadVideoAsync(IFormFile videoFile)
    {
        var fileName = $"{Guid.NewGuid()}_{videoFile.FileName}";
        _logger.LogInformation("Uploading video {FileName}", fileName);
        return $"https://storage.blob.core.windows.net/videos/{fileName}";
    }

    public async Task<string> UploadThumbnailAsync(IFormFile thumbnailFile)
    {
        var fileName = $"{Guid.NewGuid()}_{thumbnailFile.FileName}";
        _logger.LogInformation("Uploading thumbnail {FileName}", fileName);
        return $"https://storage.blob.core.windows.net/thumbnails/{fileName}";
    }

    public async Task<string> UploadProfilePictureAsync(IFormFile profilePicture)
    {
        var fileName = $"{Guid.NewGuid()}_{profilePicture.FileName}";
        _logger.LogInformation("Uploading profile picture {FileName}", fileName);
        return $"https://storage.blob.core.windows.net/profile-pictures/{fileName}";
    }

    public async Task<string> UploadBannerAsync(IFormFile bannerFile)
    {
        var fileName = $"{Guid.NewGuid()}_{bannerFile.FileName}";
        _logger.LogInformation("Uploading banner {FileName}", fileName);
        return $"https://storage.blob.core.windows.net/banners/{fileName}";
    }

    public async Task<bool> DeleteFileAsync(string fileUrl)
    {
        _logger.LogInformation("Deleting file {FileUrl}", fileUrl);
        return true;
    }

    public async Task<string> GetFileUrlAsync(string fileName)
    {
        return $"https://storage.blob.core.windows.net/files/{fileName}";
    }

    public async Task<Stream> DownloadFileAsync(string fileName)
    {
        return new MemoryStream();
    }

    public async Task<bool> FileExistsAsync(string fileName)
    {
        return true;
    }

    public async Task<long> GetFileSizeAsync(string fileName)
    {
        return 1024;
    }

    public async Task<string> GenerateSignedUrlAsync(string fileName, TimeSpan expiration)
    {
        return $"https://storage.blob.core.windows.net/files/{fileName}?sas_token";
    }
}
