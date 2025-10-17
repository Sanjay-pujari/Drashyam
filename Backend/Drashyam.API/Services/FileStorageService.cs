using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Sas;
using Drashyam.API.Configuration;
using Microsoft.Extensions.Options;

namespace Drashyam.API.Services;

public class FileStorageService : IFileStorageService
{
    private readonly BlobServiceClient _blobServiceClient;
    private readonly AzureStorageSettings _storageSettings;
    private readonly ILogger<FileStorageService> _logger;

    public FileStorageService(
        BlobServiceClient blobServiceClient,
        IOptions<AzureStorageSettings> storageSettings,
        ILogger<FileStorageService> logger)
    {
        _blobServiceClient = blobServiceClient;
        _storageSettings = storageSettings.Value;
        _logger = logger;
    }

    public async Task<string> UploadVideoAsync(IFormFile videoFile)
    {
        var fileName = $"videos/{Guid.NewGuid()}_{SanitizeFileName(videoFile.FileName)}";
        return await UploadFileAsync(videoFile, fileName, _storageSettings.VideoContainerName);
    }

    public async Task<string> UploadThumbnailAsync(IFormFile thumbnailFile)
    {
        var fileName = $"thumbnails/{Guid.NewGuid()}_{SanitizeFileName(thumbnailFile.FileName)}";
        return await UploadFileAsync(thumbnailFile, fileName, _storageSettings.ImageContainerName);
    }

    public async Task<string> UploadProfilePictureAsync(IFormFile profilePicture)
    {
        var fileName = $"profile-pictures/{Guid.NewGuid()}_{SanitizeFileName(profilePicture.FileName)}";
        return await UploadFileAsync(profilePicture, fileName, _storageSettings.ImageContainerName);
    }

    public async Task<string> UploadBannerAsync(IFormFile bannerFile)
    {
        var fileName = $"banners/{Guid.NewGuid()}_{SanitizeFileName(bannerFile.FileName)}";
        return await UploadFileAsync(bannerFile, fileName, _storageSettings.ImageContainerName);
    }

    public async Task<bool> DeleteFileAsync(string fileUrl)
    {
        try
        {
            var uri = new Uri(fileUrl);
            var containerName = uri.Segments[1].TrimEnd('/');
            var blobName = string.Join("/", uri.Segments.Skip(2));

            var containerClient = _blobServiceClient.GetBlobContainerClient(containerName);
            var blobClient = containerClient.GetBlobClient(blobName);

            var response = await blobClient.DeleteIfExistsAsync();
            _logger.LogInformation("Deleted file {FileUrl}, Success: {Success}", fileUrl, response.Value);
            return response.Value;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to delete file {FileUrl}", fileUrl);
            return false;
        }
    }

    public async Task<string> GetFileUrlAsync(string fileName)
    {
        var containerClient = _blobServiceClient.GetBlobContainerClient(_storageSettings.ImageContainerName);
        var blobClient = containerClient.GetBlobClient(fileName);
        return blobClient.Uri.ToString();
    }

    public async Task<Stream> DownloadFileAsync(string fileName)
    {
        var containerClient = _blobServiceClient.GetBlobContainerClient(_storageSettings.ImageContainerName);
        var blobClient = containerClient.GetBlobClient(fileName);
        var response = await blobClient.DownloadStreamingAsync();
        return response.Value.Content;
    }

    public async Task<bool> FileExistsAsync(string fileName)
    {
        try
        {
            var containerClient = _blobServiceClient.GetBlobContainerClient(_storageSettings.ImageContainerName);
            var blobClient = containerClient.GetBlobClient(fileName);
            var response = await blobClient.ExistsAsync();
            return response.Value;
        }
        catch
        {
            return false;
        }
    }

    public async Task<long> GetFileSizeAsync(string fileName)
    {
        try
        {
            var containerClient = _blobServiceClient.GetBlobContainerClient(_storageSettings.ImageContainerName);
            var blobClient = containerClient.GetBlobClient(fileName);
            var properties = await blobClient.GetPropertiesAsync();
            return properties.Value.ContentLength;
        }
        catch
        {
            return 0;
        }
    }

    public async Task<string> GenerateSignedUrlAsync(string fileName, TimeSpan expiration)
    {
        var containerClient = _blobServiceClient.GetBlobContainerClient(_storageSettings.ImageContainerName);
        var blobClient = containerClient.GetBlobClient(fileName);
        
        var sasBuilder = new BlobSasBuilder(BlobSasPermissions.Read, DateTimeOffset.UtcNow.Add(expiration))
        {
            BlobContainerName = _storageSettings.ImageContainerName,
            BlobName = fileName,
            Resource = "b"
        };

        return blobClient.GenerateSasUri(sasBuilder).ToString();
    }

    private async Task<string> UploadFileAsync(IFormFile file, string fileName, string containerName)
    {
        try
        {
            // Ensure container exists
            var containerClient = _blobServiceClient.GetBlobContainerClient(containerName);
            await containerClient.CreateIfNotExistsAsync(PublicAccessType.None);

            // Get blob client
            var blobClient = containerClient.GetBlobClient(fileName);

            // Set content type based on file extension
            var contentType = GetContentType(fileName);
            var headers = new BlobHttpHeaders
            {
                ContentType = contentType
            };

            // Upload file
            using var stream = file.OpenReadStream();
            await blobClient.UploadAsync(stream, new BlobUploadOptions
            {
                HttpHeaders = headers,
                Metadata = new Dictionary<string, string>
                {
                    ["OriginalFileName"] = file.FileName,
                    ["UploadedAt"] = DateTime.UtcNow.ToString("O"),
                    ["FileSize"] = file.Length.ToString()
                }
            });

            _logger.LogInformation("Successfully uploaded file {FileName} to container {ContainerName}", fileName, containerName);
            return blobClient.Uri.ToString();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to upload file {FileName} to container {ContainerName}", fileName, containerName);
            throw;
        }
    }

    private static string SanitizeFileName(string fileName)
    {
        // Remove or replace invalid characters
        var invalidChars = Path.GetInvalidFileNameChars();
        return string.Join("_", fileName.Split(invalidChars, StringSplitOptions.RemoveEmptyEntries));
    }

    private static string GetContentType(string fileName)
    {
        var extension = Path.GetExtension(fileName).ToLowerInvariant();
        return extension switch
        {
            ".jpg" or ".jpeg" => "image/jpeg",
            ".png" => "image/png",
            ".gif" => "image/gif",
            ".webp" => "image/webp",
            ".mp4" => "video/mp4",
            ".webm" => "video/webm",
            ".ogg" => "video/ogg",
            ".avi" => "video/avi",
            ".mov" => "video/quicktime",
            _ => "application/octet-stream"
        };
    }
}
