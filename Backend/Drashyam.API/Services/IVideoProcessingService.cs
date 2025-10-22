using Drashyam.API.Models;

namespace Drashyam.API.Services;

public interface IVideoProcessingService
{
    Task<VideoProcessingProgress> GetProcessingProgressAsync(int videoId);
    Task UpdateProcessingProgressAsync(int videoId, string status, int progressPercentage, string? currentStep = null, string? errorMessage = null);
    Task<List<VideoProcessingProgress>> GetProcessingQueueAsync();
    Task<bool> IsVideoProcessingAsync(int videoId);
}
