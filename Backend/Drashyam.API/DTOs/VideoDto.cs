namespace Drashyam.API.DTOs;

public class VideoDto
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string VideoUrl { get; set; } = string.Empty;
    public string? ThumbnailUrl { get; set; }
    public string UserId { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    public string? UserProfilePicture { get; set; }
    public int? ChannelId { get; set; }
    public string? ChannelName { get; set; }
    public VideoStatus Status { get; set; }
    public VideoType Type { get; set; }
    public VideoVisibility Visibility { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? PublishedAt { get; set; }
    public long ViewCount { get; set; }
    public long LikeCount { get; set; }
    public long DislikeCount { get; set; }
    public long CommentCount { get; set; }
    public TimeSpan Duration { get; set; }
    public long FileSize { get; set; }
    public string? Tags { get; set; }
    public string? Category { get; set; }
    public bool IsMonetized { get; set; }
    public string? ShareToken { get; set; }
    public bool IsLiked { get; set; }
    public bool IsDisliked { get; set; }
    public bool IsSubscribed { get; set; }
}

