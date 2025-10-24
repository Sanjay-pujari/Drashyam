using Drashyam.API.DTOs;

namespace Drashyam.API.Services;

public interface IAzureMediaService
{
    Task<LiveEventInfo> CreateLiveEventAsync(string eventName, string description);
    Task<LiveEventInfo> StartLiveEventAsync(string eventName);
    Task<LiveEventInfo> StopLiveEventAsync(string eventName);
    Task<LiveEventInfo> DeleteLiveEventAsync(string eventName);
    Task<StreamingEndpointInfo> CreateStreamingEndpointAsync(string endpointName);
    Task<AssetInfo> CreateAssetAsync(string assetName);
    Task<JobInfo> CreateTranscodingJobAsync(string inputAssetId, string outputAssetId, string preset);
    Task<StreamingLocatorInfo> CreateStreamingLocatorAsync(string assetId, string locatorName);
    Task<string> GetStreamingUrlAsync(string locatorId);
    Task<AnalyticsInfo> GetStreamAnalyticsAsync(string eventName);
    Task<List<LiveEventInfo>> GetLiveEventsAsync();
    Task<bool> IsLiveEventHealthyAsync(string eventName);
}

public class LiveEventInfo
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string State { get; set; } = string.Empty;
    public string InputUrl { get; set; } = string.Empty;
    public string PreviewUrl { get; set; } = string.Empty;
    public List<StreamingEndpointInfo> StreamingEndpoints { get; set; } = new();
    public DateTime CreatedAt { get; set; }
    public DateTime? StartedAt { get; set; }
    public DateTime? StoppedAt { get; set; }
}

public class StreamingEndpointInfo
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;
    public string State { get; set; } = string.Empty;
    public List<string> SupportedFormats { get; set; } = new();
    public long ScaleUnits { get; set; }
}

public class AssetInfo
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string State { get; set; } = string.Empty;
    public string ContainerName { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public long Size { get; set; }
}

public class JobInfo
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string State { get; set; } = string.Empty;
    public string InputAssetId { get; set; } = string.Empty;
    public string OutputAssetId { get; set; } = string.Empty;
    public string Preset { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime? StartedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public double Progress { get; set; }
}

public class StreamingLocatorInfo
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string AssetId { get; set; } = string.Empty;
    public string StreamingPolicyName { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime? ExpiresAt { get; set; }
    public string Url { get; set; } = string.Empty;
}

public class AnalyticsInfo
{
    public string EventName { get; set; } = string.Empty;
    public long ViewerCount { get; set; }
    public long PeakViewerCount { get; set; }
    public TimeSpan Duration { get; set; }
    public long TotalBytes { get; set; }
    public double AverageBitrate { get; set; }
    public List<ViewerLocation> ViewerLocations { get; set; } = new();
    public DateTime Timestamp { get; set; }
}

public class ViewerLocation
{
    public string Country { get; set; } = string.Empty;
    public string Region { get; set; } = string.Empty;
    public long ViewerCount { get; set; }
}
