using Drashyam.API.Services;
using Drashyam.API.DTOs;
using System.Diagnostics;
using System.Text.Json;

namespace Drashyam.API.Services;

public class StreamingInfrastructureService : IStreamingInfrastructureService
{
    private readonly ILogger<StreamingInfrastructureService> _logger;
    private readonly IConfiguration _configuration;
    private readonly IFileStorageService _fileStorageService;
    private readonly Dictionary<string, StreamProcess> _activeStreams = new();
    private readonly Dictionary<string, RecordingInfo> _recordings = new();

    public StreamingInfrastructureService(
        ILogger<StreamingInfrastructureService> logger,
        IConfiguration configuration,
        IFileStorageService fileStorageService)
    {
        _logger = logger;
        _configuration = configuration;
        _fileStorageService = fileStorageService;
    }

    public async Task<string> StartStreamAsync(string streamKey, string rtmpUrl)
    {
        try
        {
            _logger.LogInformation($"Starting stream for key: {streamKey}");

            var outputDir = Path.Combine(_configuration["Streaming:OutputDirectory"] ?? "streams", streamKey);
            Directory.CreateDirectory(outputDir);

            var hlsUrl = await GenerateHlsUrlAsync(streamKey);
            var ffmpegArgs = BuildFFmpegArgs(streamKey, rtmpUrl, outputDir);

            var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = _configuration["Streaming:FFmpegPath"] ?? "ffmpeg",
                    Arguments = ffmpegArgs,
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true
                }
            };

            process.Start();
            
            var streamProcess = new StreamProcess
            {
                Process = process,
                StreamKey = streamKey,
                StartTime = DateTime.UtcNow,
                OutputDirectory = outputDir
            };

            _activeStreams[streamKey] = streamProcess;

            // Start monitoring task
            _ = Task.Run(() => MonitorStreamAsync(streamKey));

            _logger.LogInformation($"Stream started successfully for key: {streamKey}");
            return hlsUrl;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error starting stream for key: {streamKey}");
            throw;
        }
    }

    public async Task StopStreamAsync(string streamKey)
    {
        try
        {
            if (_activeStreams.TryGetValue(streamKey, out var streamProcess))
            {
                _logger.LogInformation($"Stopping stream for key: {streamKey}");

                if (!streamProcess.Process.HasExited)
                {
                    streamProcess.Process.Kill();
                    await streamProcess.Process.WaitForExitAsync();
                }

                _activeStreams.Remove(streamKey);
                _logger.LogInformation($"Stream stopped for key: {streamKey}");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error stopping stream for key: {streamKey}");
            throw;
        }
    }

    public async Task<StreamStatus> GetStreamStatusAsync(string streamKey)
    {
        try
        {
            if (_activeStreams.TryGetValue(streamKey, out var streamProcess))
            {
                var hlsUrl = await GenerateHlsUrlAsync(streamKey);
                return new StreamStatus
                {
                    IsLive = !streamProcess.Process.HasExited,
                    Status = streamProcess.Process.HasExited ? "Stopped" : "Live",
                    StartTime = streamProcess.StartTime,
                    ViewerCount = await GetViewerCountAsync(streamKey),
                    HlsUrl = hlsUrl,
                    RtmpUrl = _configuration["Streaming:RtmpUrl"] ?? "rtmp://localhost:1935/live",
                    CurrentQuality = new StreamQuality { Name = "Auto", Width = 1920, Height = 1080, Bitrate = 5000000 }
                };
            }

            return new StreamStatus { IsLive = false, Status = "Not Found" };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error getting stream status for key: {streamKey}");
            throw;
        }
    }

    public async Task<string> GenerateHlsUrlAsync(string streamKey)
    {
        var baseUrl = _configuration["Streaming:HlsBaseUrl"] ?? "https://localhost:5000";
        return $"{baseUrl}/streams/{streamKey}/playlist.m3u8";
    }

    public async Task<string> GenerateThumbnailAsync(string streamKey)
    {
        try
        {
            if (_activeStreams.TryGetValue(streamKey, out var streamProcess))
            {
                var thumbnailPath = Path.Combine(streamProcess.OutputDirectory, "thumbnail.jpg");
                var ffmpegArgs = $"-i {streamProcess.OutputDirectory}/index.m3u8 -ss 00:00:01 -vframes 1 -q:v 2 {thumbnailPath}";

                var process = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = _configuration["Streaming:FFmpegPath"] ?? "ffmpeg",
                        Arguments = ffmpegArgs,
                        UseShellExecute = false,
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        CreateNoWindow = true
                    }
                };

                process.Start();
                await process.WaitForExitAsync();

                if (File.Exists(thumbnailPath))
                {
                    using var stream = File.OpenRead(thumbnailPath);
                    var thumbnailUrl = await _fileStorageService.UploadThumbnailAsync(stream);
                    return thumbnailUrl;
                }
            }

            return string.Empty;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error generating thumbnail for stream: {streamKey}");
            return string.Empty;
        }
    }

    public async Task<StreamMetrics> GetStreamMetricsAsync(string streamKey)
    {
        try
        {
            if (_activeStreams.TryGetValue(streamKey, out var streamProcess))
            {
                // In a real implementation, you would parse FFmpeg output for metrics
                return new StreamMetrics
                {
                    Bitrate = 5000000,
                    Fps = 30,
                    Resolution = 1920 * 1080,
                    CpuUsage = streamProcess.Process.TotalProcessorTime.TotalMilliseconds,
                    MemoryUsage = streamProcess.Process.WorkingSet64,
                    NetworkLatency = 50,
                    Timestamp = DateTime.UtcNow
                };
            }

            return new StreamMetrics();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error getting stream metrics for key: {streamKey}");
            throw;
        }
    }

    public async Task<bool> IsStreamHealthyAsync(string streamKey)
    {
        try
        {
            if (_activeStreams.TryGetValue(streamKey, out var streamProcess))
            {
                return !streamProcess.Process.HasExited && streamProcess.Process.Responding;
            }
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error checking stream health for key: {streamKey}");
            return false;
        }
    }

    public async Task<List<StreamQuality>> GetAvailableQualitiesAsync(string streamKey)
    {
        return new List<StreamQuality>
        {
            new() { Name = "1080p", Width = 1920, Height = 1080, Bitrate = 5000000, HlsUrl = $"/streams/{streamKey}/1080p/playlist.m3u8" },
            new() { Name = "720p", Width = 1280, Height = 720, Bitrate = 3000000, HlsUrl = $"/streams/{streamKey}/720p/playlist.m3u8" },
            new() { Name = "480p", Width = 854, Height = 480, Bitrate = 1500000, HlsUrl = $"/streams/{streamKey}/480p/playlist.m3u8" },
            new() { Name = "360p", Width = 640, Height = 360, Bitrate = 800000, HlsUrl = $"/streams/{streamKey}/360p/playlist.m3u8" }
        };
    }

    public async Task<string> TranscodeStreamAsync(string streamKey, string quality)
    {
        try
        {
            _logger.LogInformation($"Transcoding stream {streamKey} to {quality}");
            
            // In a real implementation, you would start additional FFmpeg processes for different qualities
            await Task.Delay(1000);
            
            return $"/streams/{streamKey}/{quality}/playlist.m3u8";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error transcoding stream {streamKey} to {quality}");
            throw;
        }
    }

    public async Task<RecordingInfo> StartRecordingAsync(string streamKey)
    {
        try
        {
            if (_activeStreams.TryGetValue(streamKey, out var streamProcess))
            {
                var recordingId = Guid.NewGuid().ToString();
                var recordingPath = Path.Combine(streamProcess.OutputDirectory, $"{recordingId}.mp4");

                var recording = new RecordingInfo
                {
                    Id = recordingId,
                    StreamKey = streamKey,
                    StartTime = DateTime.UtcNow,
                    FilePath = recordingPath,
                    Status = "Recording"
                };

                _recordings[recordingId] = recording;
                _logger.LogInformation($"Started recording {recordingId} for stream {streamKey}");
                
                return recording;
            }

            throw new InvalidOperationException($"Stream {streamKey} not found");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error starting recording for stream: {streamKey}");
            throw;
        }
    }

    public async Task<RecordingInfo> StopRecordingAsync(string streamKey)
    {
        try
        {
            var recording = _recordings.Values.FirstOrDefault(r => r.StreamKey == streamKey && r.Status == "Recording");
            if (recording != null)
            {
                recording.EndTime = DateTime.UtcNow;
                recording.Status = "Completed";
                recording.FileSize = File.Exists(recording.FilePath) ? new FileInfo(recording.FilePath).Length : 0;
                using var recordingStream = File.OpenRead(recording.FilePath);
                recording.Url = await _fileStorageService.UploadVideoAsync(recordingStream);
                
                _logger.LogInformation($"Stopped recording {recording.Id} for stream {streamKey}");
                return recording;
            }

            throw new InvalidOperationException($"No active recording found for stream {streamKey}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error stopping recording for stream: {streamKey}");
            throw;
        }
    }

    public async Task<List<RecordingInfo>> GetRecordingsAsync(string streamKey)
    {
        return _recordings.Values.Where(r => r.StreamKey == streamKey).ToList();
    }

    private string BuildFFmpegArgs(string streamKey, string rtmpUrl, string outputDir)
    {
        var rtmpInput = $"{rtmpUrl}/{streamKey}";
        var hlsOutput = Path.Combine(outputDir, "index.m3u8");
        
        return $"-i {rtmpInput} " +
               "-c:v libx264 -c:a aac " +
               "-preset ultrafast -tune zerolatency " +
               "-f hls -hls_time 2 -hls_list_size 3 -hls_flags delete_segments " +
               $"-hls_segment_filename {outputDir}/segment_%03d.ts " +
               $"{hlsOutput}";
    }

    private async Task<long> GetViewerCountAsync(string streamKey)
    {
        // In a real implementation, you would query your analytics service
        return await Task.FromResult(Random.Shared.NextInt64(1, 1000));
    }

    private async Task MonitorStreamAsync(string streamKey)
    {
        try
        {
            if (_activeStreams.TryGetValue(streamKey, out var streamProcess))
            {
                while (!streamProcess.Process.HasExited)
                {
                    await Task.Delay(5000);
                    
                    if (streamProcess.Process.HasExited)
                    {
                        _logger.LogWarning($"Stream {streamKey} process exited unexpectedly");
                        break;
                    }
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error monitoring stream {streamKey}");
        }
    }

    private class StreamProcess
    {
        public Process Process { get; set; } = null!;
        public string StreamKey { get; set; } = string.Empty;
        public DateTime StartTime { get; set; }
        public string OutputDirectory { get; set; } = string.Empty;
    }
}
