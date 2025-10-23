namespace Drashyam.API.DTOs;

public class StreamInfoDto
{
    public int StreamId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string StreamUrl { get; set; } = string.Empty;
    public string HlsUrl { get; set; } = string.Empty;
    public string RtmpUrl { get; set; } = string.Empty;
    public string StreamKey { get; set; } = string.Empty;
    public StreamStatus Status { get; set; }
    public DateTime StartTime { get; set; }
    public long ViewerCount { get; set; }
    public StreamQualityDto? CurrentQuality { get; set; }
    public bool IsRecording { get; set; }
    public string? RecordingUrl { get; set; }
}

public class StreamQualityDto
{
    public string Name { get; set; } = string.Empty;
    public int Width { get; set; }
    public int Height { get; set; }
    public int Bitrate { get; set; }
    public int Framerate { get; set; }
    public string Codec { get; set; } = string.Empty;
    public bool IsDefault { get; set; }
    public bool IsEnabled { get; set; }
}

public class StreamQualitySettingsDto
{
    public string QualityName { get; set; } = string.Empty;
    public int? Width { get; set; }
    public int? Height { get; set; }
    public int? Bitrate { get; set; }
    public int? Framerate { get; set; }
    public string? Codec { get; set; }
    public bool? IsEnabled { get; set; }
}

public class RecordingInfoDto
{
    public int StreamId { get; set; }
    public bool IsRecording { get; set; }
    public DateTime? StartTime { get; set; }
    public DateTime? EndTime { get; set; }
    public string? RecordingUrl { get; set; }
    public string? ThumbnailUrl { get; set; }
    public long FileSize { get; set; }
    public TimeSpan Duration { get; set; }
    public RecordingStatus Status { get; set; }
}

public class StreamAnalyticsDto
{
    public int StreamId { get; set; }
    public long TotalViewers { get; set; }
    public long PeakViewers { get; set; }
    public long CurrentViewers { get; set; }
    public TimeSpan Duration { get; set; }
    public double AverageViewerCount { get; set; }
    public long TotalChatMessages { get; set; }
    public long TotalReactions { get; set; }
    public double EngagementRate { get; set; }
    public List<ViewerAnalyticsDto> ViewerData { get; set; } = new();
    public List<QualityAnalyticsDto> QualityData { get; set; } = new();
}

public class StreamHealthDto
{
    public int StreamId { get; set; }
    public StreamHealthStatus Status { get; set; }
    public double CpuUsage { get; set; }
    public double MemoryUsage { get; set; }
    public double NetworkLatency { get; set; }
    public double Bitrate { get; set; }
    public double Framerate { get; set; }
    public int DroppedFrames { get; set; }
    public DateTime LastUpdate { get; set; }
    public List<HealthAlertDto> Alerts { get; set; } = new();
}

public class StreamMetricsDto
{
    public int StreamId { get; set; }
    public long ViewerCount { get; set; }
    public double CpuUsage { get; set; }
    public double MemoryUsage { get; set; }
    public double NetworkLatency { get; set; }
    public double Bitrate { get; set; }
    public double Framerate { get; set; }
    public int DroppedFrames { get; set; }
    public DateTime Timestamp { get; set; }
}

public class StreamEndpointDto
{
    public int StreamId { get; set; }
    public string RtmpUrl { get; set; } = string.Empty;
    public string StreamKey { get; set; } = string.Empty;
    public string HlsUrl { get; set; } = string.Empty;
    public string WebRtcUrl { get; set; } = string.Empty;
    public string PlaybackUrl { get; set; } = string.Empty;
    public List<string> CdnUrls { get; set; } = new();
}

public class StreamConfigDto
{
    public int StreamId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public List<string> Tags { get; set; } = new();
    public bool IsPublic { get; set; }
    public bool IsMonetized { get; set; }
    public bool AllowChat { get; set; }
    public bool AllowReactions { get; set; }
    public bool AllowRecording { get; set; }
    public StreamQualityDto DefaultQuality { get; set; } = new();
    public List<StreamQualityDto> AvailableQualities { get; set; } = new();
}

public class ViewerAnalyticsDto
{
    public DateTime Timestamp { get; set; }
    public long ViewerCount { get; set; }
    public long NewViewers { get; set; }
    public long ReturningViewers { get; set; }
}

public class QualityAnalyticsDto
{
    public string QualityName { get; set; } = string.Empty;
    public long ViewerCount { get; set; }
    public double Bitrate { get; set; }
    public double Framerate { get; set; }
    public int DroppedFrames { get; set; }
}

public class HealthAlertDto
{
    public string Type { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public AlertLevel Level { get; set; }
    public DateTime Timestamp { get; set; }
}

public enum StreamStatus
{
    Scheduled,
    Live,
    Paused,
    Ended,
    Error
}

public enum RecordingStatus
{
    NotStarted,
    Recording,
    Paused,
    Completed,
    Failed
}

public enum StreamHealthStatus
{
    Healthy,
    Warning,
    Critical,
    Offline
}

public enum AlertLevel
{
    Info,
    Warning,
    Error,
    Critical
}

public class StreamReportDto
{
    public int StreamId { get; set; }
    public string Title { get; set; } = string.Empty;
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public TimeSpan Duration { get; set; }
    public long TotalViewers { get; set; }
    public long PeakViewers { get; set; }
    public double AverageViewers { get; set; }
    public long TotalChatMessages { get; set; }
    public long TotalReactions { get; set; }
    public double EngagementRate { get; set; }
    public List<string> TopCountries { get; set; } = new();
    public List<string> TopDevices { get; set; } = new();
    public List<string> TopBrowsers { get; set; } = new();
}

public class StreamComparisonDto
{
    public List<int> StreamIds { get; set; } = new();
    public DateTime ComparisonPeriod { get; set; }
    public List<StreamComparisonDataDto> ComparisonData { get; set; } = new();
    public string BestPerformingStream { get; set; } = string.Empty;
    public double AverageEngagementRate { get; set; }
    public long TotalViewers { get; set; }
}

public class StreamComparisonDataDto
{
    public int StreamId { get; set; }
    public string Title { get; set; } = string.Empty;
    public long Viewers { get; set; }
    public double EngagementRate { get; set; }
    public long ChatMessages { get; set; }
    public long Reactions { get; set; }
    public TimeSpan Duration { get; set; }
}
