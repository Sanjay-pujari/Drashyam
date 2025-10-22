using Drashyam.API.Data;
using Drashyam.API.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace Drashyam.API.Services;

public class VideoProcessingBackgroundService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<VideoProcessingBackgroundService> _logger;
    private readonly TimeSpan _processingInterval = TimeSpan.FromSeconds(30);

    public VideoProcessingBackgroundService(
        IServiceProvider serviceProvider,
        ILogger<VideoProcessingBackgroundService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Video Processing Background Service started");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ProcessVideosAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while processing videos");
            }

            await Task.Delay(_processingInterval, stoppingToken);
        }

        _logger.LogInformation("Video Processing Background Service stopped");
    }

    private async Task ProcessVideosAsync()
    {
        using var scope = _serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<DrashyamDbContext>();
        var fileStorage = scope.ServiceProvider.GetRequiredService<IFileStorageService>();
        var notificationService = scope.ServiceProvider.GetRequiredService<INotificationService>();
        var processingService = scope.ServiceProvider.GetRequiredService<IVideoProcessingService>();

        // Get videos that need processing
        var videosToProcess = await context.Videos
            .Where(v => v.Status == VideoProcessingStatus.Processing)
            .OrderBy(v => v.CreatedAt)
            .Take(5) // Process up to 5 videos at a time
            .ToListAsync();

        foreach (var video in videosToProcess)
        {
            try
            {
                _logger.LogInformation("Processing video {VideoId}: {Title}", video.Id, video.Title);
                
                // Update progress to processing
                await processingService.UpdateProcessingProgressAsync(video.Id, "Processing", 10, "Starting video processing");

                // Process the video
                var processingResult = await ProcessVideoAsync(video, fileStorage, processingService);
                
                if (processingResult.Success)
                {
                    // Update video with processed data
                    video.Status = VideoProcessingStatus.Ready;
                    video.Duration = processingResult.Duration;
                    video.ThumbnailUrl = processingResult.ThumbnailUrl;
                    video.PublishedAt = DateTime.UtcNow;
                    
                    await context.SaveChangesAsync();
                    
                    // Update progress to completed
                    await processingService.UpdateProcessingProgressAsync(video.Id, "Completed", 100, "Video processing completed successfully");
                    
                    _logger.LogInformation("Successfully processed video {VideoId}", video.Id);
                    
                    // Send notification to user
                    await notificationService.CreateNotificationAsync(
                        video.UserId,
                        "Video Processing Complete",
                        $"Your video '{video.Title}' has been processed and is now live!",
                        "VideoProcessed"
                    );
                }
                else
                {
                    // Mark as failed
                    video.Status = VideoProcessingStatus.Failed;
                    await context.SaveChangesAsync();
                    
                    // Update progress to failed
                    await processingService.UpdateProcessingProgressAsync(video.Id, "Failed", 0, "Video processing failed", processingResult.Error);
                    
                    _logger.LogError("Failed to process video {VideoId}: {Error}", video.Id, processingResult.Error);
                    
                    // Send failure notification
                    await notificationService.CreateNotificationAsync(
                        video.UserId,
                        "Video Processing Failed",
                        $"Sorry, we couldn't process your video '{video.Title}'. Please try uploading again.",
                        "VideoProcessingFailed"
                    );
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing video {VideoId}", video.Id);
                
                // Mark as failed
                video.Status = VideoProcessingStatus.Failed;
                await context.SaveChangesAsync();
            }
        }
    }

    private async Task<VideoProcessingResult> ProcessVideoAsync(Video video, IFileStorageService fileStorage, IVideoProcessingService processingService)
    {
        try
        {
            // Update progress
            await processingService.UpdateProcessingProgressAsync(video.Id, "Processing", 20, "Downloading video file");
            
            // Download the original video file
            var tempVideoPath = Path.GetTempFileName();
            using var videoStream = await fileStorage.DownloadFileAsync(video.VideoUrl);
            using var fileStream = File.Create(tempVideoPath);
            await videoStream.CopyToAsync(fileStream);

            // Update progress
            await processingService.UpdateProcessingProgressAsync(video.Id, "Processing", 30, "Extracting video metadata");

            // Extract video metadata
            var metadata = await ExtractVideoMetadataAsync(tempVideoPath);
            
            // Update progress
            await processingService.UpdateProcessingProgressAsync(video.Id, "Processing", 50, "Generating thumbnail");
            
            // Generate thumbnail
            var thumbnailUrl = await GenerateThumbnailAsync(tempVideoPath, video.Id, fileStorage);
            
            // Update progress
            await processingService.UpdateProcessingProgressAsync(video.Id, "Processing", 70, "Generating multiple quality versions");
            
            // Generate multiple quality versions
            await GenerateMultipleQualitiesAsync(tempVideoPath, video.Id, fileStorage);

            // Update progress
            await processingService.UpdateProcessingProgressAsync(video.Id, "Processing", 90, "Finalizing processing");

            // Clean up temp file
            File.Delete(tempVideoPath);

            return new VideoProcessingResult
            {
                Success = true,
                Duration = metadata.Duration,
                ThumbnailUrl = thumbnailUrl
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing video {VideoId}", video.Id);
            return new VideoProcessingResult
            {
                Success = false,
                Error = ex.Message
            };
        }
    }

    private async Task<VideoMetadata> ExtractVideoMetadataAsync(string videoPath)
    {
        try
        {
            var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "ffprobe",
                    Arguments = $"-v quiet -print_format json -show_format -show_streams \"{videoPath}\"",
                    RedirectStandardOutput = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                }
            };

            process.Start();
            var output = await process.StandardOutput.ReadToEndAsync();
            await process.WaitForExitAsync();

            if (process.ExitCode != 0)
                throw new Exception("FFprobe failed to extract metadata");

            // Parse JSON output to extract duration and other metadata
            var metadata = System.Text.Json.JsonSerializer.Deserialize<FFprobeOutput>(output);
            var videoStream = metadata?.streams?.FirstOrDefault(s => s.codec_type == "video");
            
            if (videoStream == null)
                throw new Exception("No video stream found");

            return new VideoMetadata
            {
                Duration = TimeSpan.FromSeconds(double.Parse(metadata.format.duration)),
                Width = videoStream.width,
                Height = videoStream.height,
                Codec = videoStream.codec_name,
                Bitrate = int.Parse(metadata.format.bit_rate)
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error extracting video metadata");
            throw;
        }
    }

    private async Task<string> GenerateThumbnailAsync(string videoPath, int videoId, IFileStorageService fileStorage)
    {
        try
        {
            var thumbnailPath = Path.GetTempFileName() + ".jpg";
            
            var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "ffmpeg",
                    Arguments = $"-i \"{videoPath}\" -ss 00:00:01 -vframes 1 -q:v 2 \"{thumbnailPath}\"",
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                }
            };

            process.Start();
            await process.WaitForExitAsync();

            if (process.ExitCode != 0)
                throw new Exception("FFmpeg failed to generate thumbnail");

            // Upload thumbnail to storage
            using var thumbnailStream = File.OpenRead(thumbnailPath);
            var thumbnailUrl = await fileStorage.UploadThumbnailAsync(thumbnailStream);
            
            // Clean up temp file
            File.Delete(thumbnailPath);
            
            return thumbnailUrl;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating thumbnail for video {VideoId}", videoId);
            throw;
        }
    }

    private async Task GenerateMultipleQualitiesAsync(string videoPath, int videoId, IFileStorageService fileStorage)
    {
        var qualities = new[]
        {
            new { Name = "360p", Resolution = "640x360", Bitrate = "800k" },
            new { Name = "720p", Resolution = "1280x720", Bitrate = "2500k" },
            new { Name = "1080p", Resolution = "1920x1080", Bitrate = "5000k" }
        };

        foreach (var quality in qualities)
        {
            try
            {
                var outputPath = Path.GetTempFileName() + ".mp4";
                
                var process = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = "ffmpeg",
                        Arguments = $"-i \"{videoPath}\" -vf scale={quality.Resolution} -b:v {quality.Bitrate} -c:v libx264 -preset fast -c:a aac \"{outputPath}\"",
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        UseShellExecute = false,
                        CreateNoWindow = true
                    }
                };

                process.Start();
                await process.WaitForExitAsync();

                if (process.ExitCode == 0)
                {
                    // Upload processed video
                    using var videoStream = File.OpenRead(outputPath);
                    var videoUrl = await fileStorage.UploadVideoAsync(videoStream);
                    
                    _logger.LogInformation("Generated {Quality} version for video {VideoId}", quality.Name, videoId);
                }

                // Clean up temp file
                File.Delete(outputPath);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating {Quality} version for video {VideoId}", quality.Name, videoId);
            }
        }
    }
}

public class VideoProcessingResult
{
    public bool Success { get; set; }
    public TimeSpan Duration { get; set; }
    public string? ThumbnailUrl { get; set; }
    public string? Error { get; set; }
}

public class VideoMetadata
{
    public TimeSpan Duration { get; set; }
    public int Width { get; set; }
    public int Height { get; set; }
    public string Codec { get; set; } = string.Empty;
    public int Bitrate { get; set; }
}

public class FFprobeOutput
{
    public FFprobeFormat format { get; set; } = new();
    public FFprobeStream[] streams { get; set; } = Array.Empty<FFprobeStream>();
}

public class FFprobeFormat
{
    public string duration { get; set; } = "0";
    public string bit_rate { get; set; } = "0";
}

public class FFprobeStream
{
    public string codec_type { get; set; } = string.Empty;
    public string codec_name { get; set; } = string.Empty;
    public int width { get; set; }
    public int height { get; set; }
}
