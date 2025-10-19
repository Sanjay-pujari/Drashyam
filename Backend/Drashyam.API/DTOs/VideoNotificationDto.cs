namespace Drashyam.API.DTOs;

public class VideoNotificationDto
{
    public int Id { get; set; }
    public int VideoId { get; set; }
    public string VideoTitle { get; set; } = string.Empty;
    public string VideoThumbnailUrl { get; set; } = string.Empty;
    public TimeSpan VideoDuration { get; set; }
    public long VideoViewCount { get; set; }
    public int ChannelId { get; set; }
    public string ChannelName { get; set; } = string.Empty;
    public string ChannelProfilePictureUrl { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public bool IsRead { get; set; }
    public DateTime? ReadAt { get; set; }
    public string NotificationType { get; set; } = "new_video";
    public string Message { get; set; } = string.Empty;
}
