using Drashyam.API.DTOs;

namespace Drashyam.API.Services;

public interface IStreamingInfrastructureService
{
    Task<string> StartStreamAsync(string streamKey, string rtmpUrl);
    Task StopStreamAsync(string streamKey);
    Task<StreamStatus> GetStreamStatusAsync(string streamKey);
    Task<string> GenerateHlsUrlAsync(string streamKey);
    Task<string> GenerateThumbnailAsync(string streamKey);
    Task<StreamMetrics> GetStreamMetricsAsync(string streamKey);
    Task<bool> IsStreamHealthyAsync(string streamKey);
    Task<List<StreamQuality>> GetAvailableQualitiesAsync(string streamKey);
    Task<string> TranscodeStreamAsync(string streamKey, string quality);
    Task<RecordingInfo> StartRecordingAsync(string streamKey);
    Task<RecordingInfo> StopRecordingAsync(string streamKey);
    Task<List<RecordingInfo>> GetRecordingsAsync(string streamKey);
}

public class StreamStatus
{
    public bool IsLive { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime StartTime { get; set; }
    public long ViewerCount { get; set; }
    public string HlsUrl { get; set; } = string.Empty;
    public string RtmpUrl { get; set; } = string.Empty;
    public StreamQuality CurrentQuality { get; set; } = new();
}

public class StreamMetrics
{
    public long Bitrate { get; set; }
    public long Fps { get; set; }
    public long Resolution { get; set; }
    public double CpuUsage { get; set; }
    public long MemoryUsage { get; set; }
    public long NetworkLatency { get; set; }
    public DateTime Timestamp { get; set; }
}

public class StreamQuality
{
    public string Name { get; set; } = string.Empty;
    public int Width { get; set; }
    public int Height { get; set; }
    public long Bitrate { get; set; }
    public string HlsUrl { get; set; } = string.Empty;
}

public class RecordingInfo
{
    public string Id { get; set; } = string.Empty;
    public string StreamKey { get; set; } = string.Empty;
    public DateTime StartTime { get; set; }
    public DateTime? EndTime { get; set; }
    public string FilePath { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;
    public long FileSize { get; set; }
    public string Status { get; set; } = string.Empty;
}
