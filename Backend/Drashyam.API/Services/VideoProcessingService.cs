using Drashyam.API.Data;
using Drashyam.API.Models;
using Drashyam.API.DTOs;
using Microsoft.EntityFrameworkCore;

namespace Drashyam.API.Services;

public class VideoProcessingService : IVideoProcessingService
{
    private readonly DrashyamDbContext _context;
    private readonly ILogger<VideoProcessingService> _logger;

    public VideoProcessingService(DrashyamDbContext context, ILogger<VideoProcessingService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<VideoProcessingProgress> GetProcessingProgressAsync(int videoId)
    {
        var progress = await _context.VideoProcessingProgress
            .FirstOrDefaultAsync(p => p.VideoId == videoId);

        if (progress == null)
        {
            // Create initial progress record
            progress = new VideoProcessingProgress
            {
                VideoId = videoId,
                Status = "Queued",
                ProgressPercentage = 0,
                CurrentStep = "Waiting in queue"
            };
            
            _context.VideoProcessingProgress.Add(progress);
            await _context.SaveChangesAsync();
        }

        return progress;
    }

    public async Task UpdateProcessingProgressAsync(int videoId, string status, int progressPercentage, string? currentStep = null, string? errorMessage = null)
    {
        var progress = await _context.VideoProcessingProgress
            .FirstOrDefaultAsync(p => p.VideoId == videoId);

        if (progress == null)
        {
            progress = new VideoProcessingProgress
            {
                VideoId = videoId,
                Status = status,
                ProgressPercentage = progressPercentage,
                CurrentStep = currentStep,
                ErrorMessage = errorMessage
            };
            _context.VideoProcessingProgress.Add(progress);
        }
        else
        {
            progress.Status = status;
            progress.ProgressPercentage = progressPercentage;
            progress.CurrentStep = currentStep;
            progress.ErrorMessage = errorMessage;
            progress.UpdatedAt = DateTime.UtcNow;
        }

        await _context.SaveChangesAsync();
        _logger.LogInformation("Updated processing progress for video {VideoId}: {Status} ({Progress}%)", videoId, status, progressPercentage);
    }

    public async Task<List<VideoProcessingProgress>> GetProcessingQueueAsync()
    {
        return await _context.VideoProcessingProgress
            .Include(p => p.Video)
            .Where(p => p.Status != "Completed" && p.Status != "Failed")
            .OrderBy(p => p.CreatedAt)
            .ToListAsync();
    }

    public async Task<bool> IsVideoProcessingAsync(int videoId)
    {
        var video = await _context.Videos.FindAsync(videoId);
        return video?.Status == DTOs.VideoProcessingStatus.Processing;
    }
}
