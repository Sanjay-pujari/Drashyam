using Drashyam.API.DTOs;

namespace Drashyam.API.DTOs;

public class StreamingEndpointDto
{
    public string Id { get; set; } = string.Empty;
    public string UserId { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string RtmpUrl { get; set; } = string.Empty;
    public string HlsUrl { get; set; } = string.Empty;
    public string WebRtcUrl { get; set; } = string.Empty;
    public string StreamKey { get; set; } = string.Empty;
    public StreamingStatus Status { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public bool IsActive { get; set; }
    public int MaxViewers { get; set; }
    public StreamingQualityDto? QualitySettings { get; set; }
    public List<string>? Tags { get; set; }
    public string? Category { get; set; }
    public bool IsPublic { get; set; } = true;
    public bool IsRecording { get; set; } = false;
    public string? RecordingUrl { get; set; }
    public DateTime? ScheduledStartTime { get; set; }
    public DateTime? ActualStartTime { get; set; }
    public DateTime? EndTime { get; set; }
}

public class StreamingQualityDto
{
    public bool EnableAdaptiveBitrate { get; set; } = true;
    public int MaxBitrate { get; set; } = 5000;
    public int MinBitrate { get; set; } = 500;
    public List<string> Resolutions { get; set; } = new();
    public int FrameRate { get; set; } = 30;
    public string Codec { get; set; } = "h264";
    public string AudioCodec { get; set; } = "aac";
    public int AudioBitrate { get; set; } = 128;
}

public class StreamingSettingsDto
{
    public string EndpointId { get; set; } = string.Empty;
    public StreamingQualityDto? Quality { get; set; }
    public bool EnableRecording { get; set; } = false;
    public bool EnableChat { get; set; } = true;
    public bool EnableSuperChat { get; set; } = true;
    public bool EnableModeration { get; set; } = false;
    public List<string>? Moderators { get; set; }
    public string? StreamTitle { get; set; }
    public string? StreamDescription { get; set; }
    public List<string>? Tags { get; set; }
    public string? Category { get; set; }
    public bool IsPublic { get; set; } = true;
    public int MaxViewers { get; set; } = 1000;
}

public class StreamingAnalyticsDto
{
    public string EndpointId { get; set; } = string.Empty;
    public int CurrentViewers { get; set; }
    public int TotalViewers { get; set; }
    public TimeSpan AverageViewerDuration { get; set; }
    public int PeakViewers { get; set; }
    public int Bitrate { get; set; }
    public int Latency { get; set; }
    public int QualityScore { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime? EndTime { get; set; }
    public bool IsLive { get; set; }
    public Dictionary<string, int>? ViewerCountries { get; set; }
    public Dictionary<string, int>? ViewerDevices { get; set; }
    public List<StreamingEventDto>? Events { get; set; }
}

public class StreamingEventDto
{
    public string Id { get; set; } = string.Empty;
    public string EndpointId { get; set; } = string.Empty;
    public string EventType { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
    public Dictionary<string, object>? Metadata { get; set; }
}

public class StreamingHealthDto
{
    public string EndpointId { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public int CpuUsage { get; set; }
    public int MemoryUsage { get; set; }
    public int NetworkLatency { get; set; }
    public int PacketLoss { get; set; }
    public int Bitrate { get; set; }
    public int FrameRate { get; set; }
    public DateTime LastUpdated { get; set; }
    public bool IsHealthy { get; set; }
    public List<string>? Issues { get; set; }
    public Dictionary<string, object>? Metrics { get; set; }
}
