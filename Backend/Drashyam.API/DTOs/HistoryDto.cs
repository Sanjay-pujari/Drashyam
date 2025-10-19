namespace Drashyam.API.DTOs;

public class HistoryDto
{
    public int Id { get; set; }
    public int VideoId { get; set; }
    public string VideoTitle { get; set; } = string.Empty;
    public string VideoThumbnailUrl { get; set; } = string.Empty;
    public string VideoUrl { get; set; } = string.Empty;
    public string ChannelName { get; set; } = string.Empty;
    public string ChannelId { get; set; } = string.Empty;
    public long ViewCount { get; set; }
    public TimeSpan Duration { get; set; }
    public DateTime ViewedAt { get; set; }
    public TimeSpan WatchDuration { get; set; }
    public string? UserAgent { get; set; }
    public string? DeviceType { get; set; }
}

public class HistoryCreateDto
{
    public int VideoId { get; set; }
    public double WatchDurationSeconds { get; set; }
    public string? UserAgent { get; set; }
    public string? DeviceType { get; set; }
}

public class HistoryUpdateDto
{
    public double WatchDurationSeconds { get; set; }
}

