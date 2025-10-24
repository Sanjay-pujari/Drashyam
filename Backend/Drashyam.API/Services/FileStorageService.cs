using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Sas;
using Drashyam.API.Configuration;
using Microsoft.Extensions.Options;

namespace Drashyam.API.Services;

public class FileStorageService : IFileStorageService
{
    private readonly BlobServiceClient? _blobServiceClient;
    private readonly AzureStorageSettings _storageSettings;
    private readonly ILogger<FileStorageService> _logger;

    public FileStorageService(
        BlobServiceClient? blobServiceClient,
        IOptions<AzureStorageSettings> storageSettings,
        ILogger<FileStorageService> logger)
    {
        _blobServiceClient = blobServiceClient;
        _storageSettings = storageSettings.Value;
        _logger = logger;
    }

    private void EnsureAzureStorageConfigured()
    {
        if (_blobServiceClient == null)
        {
            _logger.LogWarning("Azure Storage is not configured. Using local file storage for development.");
            // Don't throw exception in development - use local storage instead
        }
    }

    public async Task<string> UploadVideoAsync(IFormFile videoFile)
    {
        EnsureAzureStorageConfigured();
        
        var fileName = $"videos/{Guid.NewGuid()}_{SanitizeFileName(videoFile.FileName)}";
        
        if (_blobServiceClient == null)
        {
            return await UploadToLocalStorageAsync(videoFile, fileName);
        }
        
        return await UploadFileAsync(videoFile, fileName, _storageSettings.VideoContainerName);
    }

    public async Task<string> UploadThumbnailAsync(IFormFile thumbnailFile)
    {
        EnsureAzureStorageConfigured();
        
        var fileName = $"thumbnails/{Guid.NewGuid()}_{SanitizeFileName(thumbnailFile.FileName)}";
        
        if (_blobServiceClient == null)
        {
            return await UploadToLocalStorageAsync(thumbnailFile, fileName);
        }
        
        return await UploadFileAsync(thumbnailFile, fileName, _storageSettings.ImageContainerName);
    }

    public async Task<string> UploadVideoAsync(Stream videoStream)
    {
        EnsureAzureStorageConfigured();
        
        var fileName = $"videos/{Guid.NewGuid()}.mp4";
        
        if (_blobServiceClient == null)
        {
            return await UploadStreamToLocalStorageAsync(videoStream, fileName);
        }
        
        return await UploadStreamAsync(videoStream, fileName, _storageSettings.VideoContainerName, "video/mp4");
    }

    public async Task<string> UploadThumbnailAsync(Stream thumbnailStream)
    {
        EnsureAzureStorageConfigured();
        
        var fileName = $"thumbnails/{Guid.NewGuid()}.jpg";
        
        if (_blobServiceClient == null)
        {
            return await UploadStreamToLocalStorageAsync(thumbnailStream, fileName);
        }
        
        return await UploadStreamAsync(thumbnailStream, fileName, _storageSettings.ImageContainerName, "image/jpeg");
    }

    public async Task<string> UploadProfilePictureAsync(IFormFile profilePicture)
    {
        EnsureAzureStorageConfigured();
        
        var fileName = $"profile-pictures/{Guid.NewGuid()}_{SanitizeFileName(profilePicture.FileName)}";
        return await UploadFileAsync(profilePicture, fileName, _storageSettings.ImageContainerName);
    }

    public async Task<string> UploadBannerAsync(IFormFile bannerFile)
    {
        EnsureAzureStorageConfigured();
        
        var fileName = $"banners/{Guid.NewGuid()}_{SanitizeFileName(bannerFile.FileName)}";
        return await UploadFileAsync(bannerFile, fileName, _storageSettings.ImageContainerName);
    }

    public async Task<bool> DeleteFileAsync(string fileUrl)
    {
        EnsureAzureStorageConfigured();
        
        try
        {
            var uri = new Uri(fileUrl);
            var containerName = uri.Segments[1].TrimEnd('/');
            var blobName = string.Join("/", uri.Segments.Skip(2));

            var containerClient = _blobServiceClient.GetBlobContainerClient(containerName);
            var blobClient = containerClient.GetBlobClient(blobName);

            var response = await blobClient.DeleteIfExistsAsync();
            return response.Value;
        }
        catch (Exception ex)
        {
            return false;
        }
    }

    public async Task<string> GetFileUrlAsync(string fileName)
    {
        EnsureAzureStorageConfigured();
        
        var containerClient = _blobServiceClient.GetBlobContainerClient(_storageSettings.ImageContainerName);
        var blobClient = containerClient.GetBlobClient(fileName);
        return blobClient.Uri.ToString();
    }

    public async Task<Stream> DownloadFileAsync(string fileName)
    {
        EnsureAzureStorageConfigured();
        
        var containerClient = _blobServiceClient.GetBlobContainerClient(_storageSettings.ImageContainerName);
        var blobClient = containerClient.GetBlobClient(fileName);
        var response = await blobClient.DownloadStreamingAsync();
        return response.Value.Content;
    }

    public async Task<bool> FileExistsAsync(string fileName)
    {
        EnsureAzureStorageConfigured();
        
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
        EnsureAzureStorageConfigured();
        
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
        EnsureAzureStorageConfigured();
        
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

            return blobClient.Uri.ToString();
        }
        catch (Exception ex)
        {
            throw;
        }
    }

    private static string SanitizeFileName(string fileName)
    {
        // Remove or replace invalid characters
        var invalidChars = Path.GetInvalidFileNameChars();
        return string.Join("_", fileName.Split(invalidChars, StringSplitOptions.RemoveEmptyEntries));
    }

    private async Task<string> UploadStreamAsync(Stream stream, string fileName, string containerName, string contentType)
    {
        EnsureAzureStorageConfigured();
        
        try
        {
            // Ensure container exists
            var containerClient = _blobServiceClient.GetBlobContainerClient(containerName);
            await containerClient.CreateIfNotExistsAsync(PublicAccessType.None);

            // Get blob client
            var blobClient = containerClient.GetBlobClient(fileName);

            // Set content type
            var headers = new BlobHttpHeaders
            {
                ContentType = contentType
            };

            // Upload stream
            await blobClient.UploadAsync(stream, new BlobUploadOptions
            {
                HttpHeaders = headers,
                Metadata = new Dictionary<string, string>
                {
                    ["UploadedAt"] = DateTime.UtcNow.ToString("O")
                }
            });

            return blobClient.Uri.ToString();
        }
        catch (Exception ex)
        {
            throw;
        }
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

    private async Task<string> UploadToLocalStorageAsync(IFormFile file, string fileName)
    {
        try
        {
            // Create uploads directory if it doesn't exist
            var uploadsPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads");
            Directory.CreateDirectory(uploadsPath);

            var filePath = Path.Combine(uploadsPath, fileName);
            var directory = Path.GetDirectoryName(filePath);
            if (!string.IsNullOrEmpty(directory))
            {
                Directory.CreateDirectory(directory);
            }

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            // Return a URL that can be accessed by the frontend
            var baseUrl = "https://localhost:56379"; // Use the API base URL
            var relativePath = Path.Combine("uploads", fileName).Replace("\\", "/");
            return $"{baseUrl}/{relativePath}";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error uploading file to local storage: {FileName}", fileName);
            throw new InvalidOperationException($"Failed to upload file: {ex.Message}", ex);
        }
    }

    private async Task<string> UploadStreamToLocalStorageAsync(Stream stream, string fileName)
    {
        try
        {
            // Create uploads directory if it doesn't exist
            var uploadsPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads");
            Directory.CreateDirectory(uploadsPath);

            var filePath = Path.Combine(uploadsPath, fileName);
            var directory = Path.GetDirectoryName(filePath);
            if (!string.IsNullOrEmpty(directory))
            {
                Directory.CreateDirectory(directory);
            }

            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                await stream.CopyToAsync(fileStream);
            }

            // Return a URL that can be accessed by the frontend
            var baseUrl = "https://localhost:56379"; // Use the API base URL
            var relativePath = Path.Combine("uploads", fileName).Replace("\\", "/");
            return $"{baseUrl}/{relativePath}";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error uploading stream to local storage: {FileName}", fileName);
            throw new InvalidOperationException($"Failed to upload stream: {ex.Message}", ex);
        }
    }
}
