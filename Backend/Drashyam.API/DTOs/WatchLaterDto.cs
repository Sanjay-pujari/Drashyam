namespace Drashyam.API.DTOs;

public class WatchLaterDto
{
    public int Id { get; set; }
    public string UserId { get; set; } = string.Empty;
    public int VideoId { get; set; }
    public DateTime AddedAt { get; set; }
    
    // Video information
    public string VideoTitle { get; set; } = string.Empty;
    public string? VideoThumbnailUrl { get; set; }
    public string VideoDuration { get; set; } = string.Empty;
    public long VideoViewCount { get; set; }
    public string ChannelName { get; set; } = string.Empty;
}

public class WatchLaterCreateDto
{
    public int VideoId { get; set; }
}

public class WatchLaterResponseDto
{
    public int Id { get; set; }
    public int VideoId { get; set; }
    public string VideoTitle { get; set; } = string.Empty;
    public string? VideoThumbnailUrl { get; set; }
    public string VideoDuration { get; set; } = string.Empty;
    public long VideoViewCount { get; set; }
    public string ChannelName { get; set; } = string.Empty;
    public DateTime AddedAt { get; set; }
}
