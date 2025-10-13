namespace Drashyam.API.DTOs;

public class LiveStreamDto
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string UserId { get; set; } = string.Empty;
    public int? ChannelId { get; set; }
    public string StreamKey { get; set; } = string.Empty;
    public string StreamUrl { get; set; } = string.Empty;
    public LiveStreamStatus Status { get; set; }
    public DateTime ScheduledStartTime { get; set; }
    public DateTime? ActualStartTime { get; set; }
    public DateTime? EndTime { get; set; }
    public int ViewerCount { get; set; }
    public int MaxViewers { get; set; }
    public string? ThumbnailUrl { get; set; }
    public bool IsMonetized { get; set; }
    public string? Category { get; set; }
    public string? Tags { get; set; }
    public DateTime CreatedAt { get; set; }
    public UserDto? User { get; set; }
    public ChannelDto? Channel { get; set; }
}

public class LiveStreamCreateDto
{
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int? ChannelId { get; set; }
    public DateTime ScheduledStartTime { get; set; }
    public string? Category { get; set; }
    public string? Tags { get; set; }
    public bool IsMonetized { get; set; } = false;
}

public class LiveStreamUpdateDto
{
    public string? Title { get; set; }
    public string? Description { get; set; }
    public DateTime? ScheduledStartTime { get; set; }
    public string? Category { get; set; }
    public string? Tags { get; set; }
    public bool? IsMonetized { get; set; }
}

public enum LiveStreamStatus
{
    Scheduled,
    Live,
    Ended,
    Cancelled
}
