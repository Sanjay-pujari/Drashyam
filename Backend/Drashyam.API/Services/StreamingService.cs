using AutoMapper;
using Drashyam.API.Data;
using Drashyam.API.DTOs;
using Drashyam.API.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.SignalR;
using Drashyam.API.Hubs;

namespace Drashyam.API.Services;

public class StreamingService : IStreamingService
{
    private readonly DrashyamDbContext _context;
    private readonly IMapper _mapper;
    private readonly IHubContext<LiveStreamHub> _liveStreamHub;
    private readonly IHubContext<ChatHub> _chatHub;
    private readonly IHubContext<NotificationHub> _notificationHub;
    private readonly ILogger<StreamingService> _logger;

    public StreamingService(
        DrashyamDbContext context,
        IMapper mapper,
        IHubContext<LiveStreamHub> liveStreamHub,
        IHubContext<ChatHub> chatHub,
        IHubContext<NotificationHub> notificationHub,
        ILogger<StreamingService> logger)
    {
        _context = context;
        _mapper = mapper;
        _liveStreamHub = liveStreamHub;
        _chatHub = chatHub;
        _notificationHub = notificationHub;
        _logger = logger;
    }

    public async Task<StreamInfoDto> StartStreamAsync(int streamId, string userId)
    {
        var stream = await _context.LiveStreams
            .Include(s => s.User)
            .Include(s => s.Channel)
            .FirstOrDefaultAsync(s => s.Id == streamId);

        if (stream == null)
            throw new ArgumentException("Stream not found");

        if (stream.UserId != userId)
            throw new UnauthorizedAccessException("Unauthorized to start this stream");

        // Update stream status
        stream.Status = DTOs.LiveStreamStatus.Live;
        stream.ActualStartTime = DateTime.UtcNow;
        stream.ViewerCount = 0;

        await _context.SaveChangesAsync();

        // Generate stream URLs
        var streamInfo = new StreamInfoDto
        {
            StreamId = streamId,
            Title = stream.Title,
            StreamUrl = GenerateStreamUrl(streamId),
            HlsUrl = GenerateHlsUrl(streamId),
            RtmpUrl = GenerateRtmpUrl(streamId),
            StreamKey = stream.StreamKey,
            Status = DTOs.StreamStatus.Live,
            StartTime = stream.ActualStartTime.Value,
            ViewerCount = 0,
            IsRecording = false
        };

        // Notify all viewers
        await NotifyStreamStartedAsync(streamId);

        _logger.LogInformation($"Stream {streamId} started by user {userId}");
        return streamInfo;
    }

    public async Task<StreamInfoDto> StopStreamAsync(int streamId, string userId)
    {
        var stream = await _context.LiveStreams
            .Include(s => s.User)
            .Include(s => s.Channel)
            .FirstOrDefaultAsync(s => s.Id == streamId);

        if (stream == null)
            throw new ArgumentException("Stream not found");

        if (stream.UserId != userId)
            throw new UnauthorizedAccessException("Unauthorized to stop this stream");

        // Update stream status
        stream.Status = DTOs.LiveStreamStatus.Ended;
        stream.EndTime = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        var streamInfo = new StreamInfoDto
        {
            StreamId = streamId,
            Title = stream.Title,
            Status = DTOs.StreamStatus.Ended,
            StartTime = stream.ActualStartTime ?? DateTime.UtcNow,
            ViewerCount = stream.ViewerCount
        };

        // Notify all viewers
        await NotifyStreamEndedAsync(streamId);

        _logger.LogInformation($"Stream {streamId} stopped by user {userId}");
        return streamInfo;
    }

    public async Task<StreamInfoDto> PauseStreamAsync(int streamId, string userId)
    {
        var stream = await _context.LiveStreams
            .FirstOrDefaultAsync(s => s.Id == streamId);

        if (stream == null)
            throw new ArgumentException("Stream not found");

        if (stream.UserId != userId)
            throw new UnauthorizedAccessException("Unauthorized to pause this stream");

        // Update stream status
        stream.Status = DTOs.LiveStreamStatus.Paused;

        await _context.SaveChangesAsync();

        return new StreamInfoDto
        {
            StreamId = streamId,
            Title = stream.Title,
            Status = DTOs.StreamStatus.Paused,
            StartTime = stream.ActualStartTime ?? DateTime.UtcNow,
            ViewerCount = stream.ViewerCount
        };
    }

    public async Task<StreamInfoDto> ResumeStreamAsync(int streamId, string userId)
    {
        var stream = await _context.LiveStreams
            .FirstOrDefaultAsync(s => s.Id == streamId);

        if (stream == null)
            throw new ArgumentException("Stream not found");

        if (stream.UserId != userId)
            throw new UnauthorizedAccessException("Unauthorized to resume this stream");

        // Update stream status
        stream.Status = DTOs.LiveStreamStatus.Live;

        await _context.SaveChangesAsync();

        return new StreamInfoDto
        {
            StreamId = streamId,
            Title = stream.Title,
            Status = DTOs.StreamStatus.Live,
            StartTime = stream.ActualStartTime ?? DateTime.UtcNow,
            ViewerCount = stream.ViewerCount
        };
    }

    public async Task<StreamQualityDto> GetStreamQualityAsync(int streamId)
    {
        var stream = await _context.LiveStreams
            .FirstOrDefaultAsync(s => s.Id == streamId);

        if (stream == null)
            throw new ArgumentException("Stream not found");

        // Get current stream quality from database or streaming service
        return new StreamQualityDto
        {
            Name = "1080p",
            Width = 1920,
            Height = 1080,
            Bitrate = 5000,
            Framerate = 30,
            Codec = "H.264",
            IsDefault = true,
            IsEnabled = true
        };
    }

    public async Task<StreamQualityDto> UpdateStreamQualityAsync(int streamId, StreamQualitySettingsDto settings)
    {
        var stream = await _context.LiveStreams
            .FirstOrDefaultAsync(s => s.Id == streamId);

        if (stream == null)
            throw new ArgumentException("Stream not found");

        // Update stream quality settings
        var quality = new StreamQualityDto
        {
            Name = settings.QualityName,
            Width = settings.Width ?? 1920,
            Height = settings.Height ?? 1080,
            Bitrate = settings.Bitrate ?? 5000,
            Framerate = settings.Framerate ?? 30,
            Codec = settings.Codec ?? "H.264",
            IsEnabled = settings.IsEnabled ?? true
        };

        // Notify viewers of quality change
        await _liveStreamHub.Clients.Group($"stream_{streamId}").SendAsync("StreamQualityChanged", quality);

        return quality;
    }

    public async Task<List<StreamQualityDto>> GetAvailableQualitiesAsync()
    {
        return new List<StreamQualityDto>
        {
            new() { Name = "720p", Width = 1280, Height = 720, Bitrate = 2500, Framerate = 30, Codec = "H.264", IsDefault = false, IsEnabled = true },
            new() { Name = "1080p", Width = 1920, Height = 1080, Bitrate = 5000, Framerate = 30, Codec = "H.264", IsDefault = true, IsEnabled = true },
            new() { Name = "1440p", Width = 2560, Height = 1440, Bitrate = 8000, Framerate = 30, Codec = "H.264", IsDefault = false, IsEnabled = true },
            new() { Name = "4K", Width = 3840, Height = 2160, Bitrate = 15000, Framerate = 30, Codec = "H.264", IsDefault = false, IsEnabled = true }
        };
    }

    public async Task<RecordingInfoDto> StartRecordingAsync(int streamId, string userId)
    {
        var stream = await _context.LiveStreams
            .FirstOrDefaultAsync(s => s.Id == streamId);

        if (stream == null)
            throw new ArgumentException("Stream not found");

        if (stream.UserId != userId)
            throw new UnauthorizedAccessException("Unauthorized to start recording");

        // Start recording
        stream.IsRecording = true;
        stream.RecordingUrl = GenerateRecordingUrl(streamId);

        await _context.SaveChangesAsync();

        return new RecordingInfoDto
        {
            StreamId = streamId,
            IsRecording = true,
            StartTime = DateTime.UtcNow,
            RecordingUrl = stream.RecordingUrl,
            Status = RecordingStatus.Recording
        };
    }

    public async Task<RecordingInfoDto> StopRecordingAsync(int streamId, string userId)
    {
        var stream = await _context.LiveStreams
            .FirstOrDefaultAsync(s => s.Id == streamId);

        if (stream == null)
            throw new ArgumentException("Stream not found");

        if (stream.UserId != userId)
            throw new UnauthorizedAccessException("Unauthorized to stop recording");

        // Stop recording
        stream.IsRecording = false;

        await _context.SaveChangesAsync();

        return new RecordingInfoDto
        {
            StreamId = streamId,
            IsRecording = false,
            EndTime = DateTime.UtcNow,
            RecordingUrl = stream.RecordingUrl,
            Status = RecordingStatus.Completed
        };
    }

    public async Task<RecordingInfoDto> GetRecordingStatusAsync(int streamId)
    {
        var stream = await _context.LiveStreams
            .FirstOrDefaultAsync(s => s.Id == streamId);

        if (stream == null)
            throw new ArgumentException("Stream not found");

        return new RecordingInfoDto
        {
            StreamId = streamId,
            IsRecording = stream.IsRecording,
            RecordingUrl = stream.RecordingUrl,
            Status = stream.IsRecording ? RecordingStatus.Recording : RecordingStatus.NotStarted
        };
    }

    public async Task<StreamAnalyticsDto> GetStreamAnalyticsAsync(int streamId)
    {
        var stream = await _context.LiveStreams
            .FirstOrDefaultAsync(s => s.Id == streamId);

        if (stream == null)
            throw new ArgumentException("Stream not found");

        // Get analytics data from database
        var analytics = new StreamAnalyticsDto
        {
            StreamId = streamId,
            TotalViewers = stream.ViewerCount,
            PeakViewers = stream.PeakViewerCount,
            CurrentViewers = stream.ViewerCount,
            Duration = stream.ActualStartTime.HasValue ? 
                (stream.EndTime ?? DateTime.UtcNow) - stream.ActualStartTime.Value : TimeSpan.Zero,
            AverageViewerCount = stream.ViewerCount,
            TotalChatMessages = 0, // Get from chat service
            TotalReactions = 0, // Get from reactions
            EngagementRate = 0.0
        };

        return analytics;
    }

    public async Task<StreamHealthDto> GetStreamHealthAsync(int streamId)
    {
        var stream = await _context.LiveStreams
            .FirstOrDefaultAsync(s => s.Id == streamId);

        if (stream == null)
            throw new ArgumentException("Stream not found");

        // Get health data from streaming service
        return new StreamHealthDto
        {
            StreamId = streamId,
            Status = StreamHealthStatus.Healthy,
            CpuUsage = 0.0,
            MemoryUsage = 0.0,
            NetworkLatency = 0.0,
            Bitrate = 5000.0,
            Framerate = 30.0,
            DroppedFrames = 0,
            LastUpdate = DateTime.UtcNow
        };
    }

    public async Task UpdateStreamMetricsAsync(int streamId, StreamMetricsDto metrics)
    {
        var stream = await _context.LiveStreams
            .FirstOrDefaultAsync(s => s.Id == streamId);

        if (stream == null)
            throw new ArgumentException("Stream not found");

        // Update stream metrics
        stream.ViewerCount = metrics.ViewerCount;
        if (metrics.ViewerCount > stream.PeakViewerCount)
        {
            stream.PeakViewerCount = metrics.ViewerCount;
        }

        await _context.SaveChangesAsync();

        // Notify viewers of metrics update
        await _liveStreamHub.Clients.Group($"stream_{streamId}").SendAsync("StreamMetricsUpdated", metrics);
    }

    public async Task<StreamEndpointDto> GetStreamEndpointAsync(int streamId)
    {
        var stream = await _context.LiveStreams
            .FirstOrDefaultAsync(s => s.Id == streamId);

        if (stream == null)
            throw new ArgumentException("Stream not found");

        return new StreamEndpointDto
        {
            StreamId = streamId,
            RtmpUrl = GenerateRtmpUrl(streamId),
            StreamKey = stream.StreamKey,
            HlsUrl = GenerateHlsUrl(streamId),
            WebRtcUrl = GenerateWebRtcUrl(streamId),
            PlaybackUrl = GeneratePlaybackUrl(streamId),
            CdnUrls = GenerateCdnUrls(streamId)
        };
    }

    public async Task<bool> ValidateStreamKeyAsync(string streamKey)
    {
        var stream = await _context.LiveStreams
            .FirstOrDefaultAsync(s => s.StreamKey == streamKey);

        return stream != null;
    }

    public async Task<StreamConfigDto> GetStreamConfigurationAsync(int streamId)
    {
        var stream = await _context.LiveStreams
            .Include(s => s.Channel)
            .FirstOrDefaultAsync(s => s.Id == streamId);

        if (stream == null)
            throw new ArgumentException("Stream not found");

        var availableQualities = await GetAvailableQualitiesAsync();
        var defaultQuality = availableQualities.FirstOrDefault(q => q.IsDefault) ?? availableQualities.First();

        return new StreamConfigDto
        {
            StreamId = streamId,
            Title = stream.Title,
            Description = stream.Description ?? string.Empty,
            Category = stream.Category ?? string.Empty,
            Tags = stream.Tags?.Split(',').ToList() ?? new List<string>(),
            IsPublic = true, // Default to public
            IsMonetized = stream.IsMonetized,
            AllowChat = true,
            AllowReactions = true,
            AllowRecording = true,
            DefaultQuality = defaultQuality,
            AvailableQualities = availableQualities
        };
    }

    public async Task NotifyStreamStartedAsync(int streamId)
    {
        await _liveStreamHub.Clients.Group($"stream_{streamId}").SendAsync("StreamStarted", new
        {
            StreamId = streamId,
            StartTime = DateTime.UtcNow
        });
    }

    public async Task NotifyStreamEndedAsync(int streamId)
    {
        await _liveStreamHub.Clients.Group($"stream_{streamId}").SendAsync("StreamEnded", new
        {
            StreamId = streamId,
            EndTime = DateTime.UtcNow
        });
    }

    public async Task NotifyViewerJoinedAsync(int streamId, string userId)
    {
        await _liveStreamHub.Clients.Group($"stream_{streamId}").SendAsync("ViewerJoined", new
        {
            StreamId = streamId,
            UserId = userId,
            Timestamp = DateTime.UtcNow
        });
    }

    public async Task NotifyViewerLeftAsync(int streamId, string userId)
    {
        await _liveStreamHub.Clients.Group($"stream_{streamId}").SendAsync("ViewerLeft", new
        {
            StreamId = streamId,
            UserId = userId,
            Timestamp = DateTime.UtcNow
        });
    }

    private string GenerateStreamUrl(int streamId)
    {
        return $"https://stream.drashyam.com/live/{streamId}";
    }

    private string GenerateHlsUrl(int streamId)
    {
        return $"https://stream.drashyam.com/hls/{streamId}/index.m3u8";
    }

    private string GenerateRtmpUrl(int streamId)
    {
        return $"rtmp://stream.drashyam.com/live/{streamId}";
    }

    private string GenerateWebRtcUrl(int streamId)
    {
        return $"wss://stream.drashyam.com/webrtc/{streamId}";
    }

    private string GeneratePlaybackUrl(int streamId)
    {
        return $"https://stream.drashyam.com/playback/{streamId}";
    }

    private string GenerateRecordingUrl(int streamId)
    {
        return $"https://recordings.drashyam.com/{streamId}/{DateTime.UtcNow:yyyyMMddHHmmss}.mp4";
    }

    private List<string> GenerateCdnUrls(int streamId)
    {
        return new List<string>
        {
            $"https://cdn1.drashyam.com/live/{streamId}",
            $"https://cdn2.drashyam.com/live/{streamId}",
            $"https://cdn3.drashyam.com/live/{streamId}"
        };
    }
}
