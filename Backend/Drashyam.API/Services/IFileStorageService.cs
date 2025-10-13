namespace Drashyam.API.Services;

public interface IFileStorageService
{
    Task<string> UploadVideoAsync(IFormFile videoFile);
    Task<string> UploadThumbnailAsync(IFormFile thumbnailFile);
    Task<string> UploadProfilePictureAsync(IFormFile profilePicture);
    Task<string> UploadBannerAsync(IFormFile bannerFile);
    Task<bool> DeleteFileAsync(string fileUrl);
    Task<string> GetFileUrlAsync(string fileName);
    Task<Stream> DownloadFileAsync(string fileName);
    Task<bool> FileExistsAsync(string fileName);
    Task<long> GetFileSizeAsync(string fileName);
    Task<string> GenerateSignedUrlAsync(string fileName, TimeSpan expiration);
}
