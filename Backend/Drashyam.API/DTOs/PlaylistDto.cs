using Drashyam.API.Models;

namespace Drashyam.API.DTOs;

public class PlaylistDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string UserId { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    public int? ChannelId { get; set; }
    public string? ChannelName { get; set; }
    public PlaylistVisibility Visibility { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public int VideoCount { get; set; }
    public string? ThumbnailUrl { get; set; }
}

public class PlaylistCreateDto
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int? ChannelId { get; set; }
    public PlaylistVisibility Visibility { get; set; } = PlaylistVisibility.Public;
}

public class PlaylistUpdateDto
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public PlaylistVisibility Visibility { get; set; }
}

public class PlaylistVideoDto
{
    public int Id { get; set; }
    public int PlaylistId { get; set; }
    public int VideoId { get; set; }
    public int Order { get; set; }
    public DateTime AddedAt { get; set; }
    
    // Video information
    public string VideoTitle { get; set; } = string.Empty;
    public string? VideoThumbnailUrl { get; set; }
    public string VideoDuration { get; set; } = string.Empty;
    public long VideoViewCount { get; set; }
    public string ChannelName { get; set; } = string.Empty;
}

public class PlaylistVideoCreateDto
{
    public int VideoId { get; set; }
    public int? Order { get; set; }
}

public class PlaylistVideoUpdateDto
{
    public int VideoId { get; set; }
    public int Order { get; set; }
}
