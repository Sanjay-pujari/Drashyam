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

public class VideoUploadDto
{
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public VideoVisibility Visibility { get; set; } = VideoVisibility.Public;
    public int? ChannelId { get; set; }
    public string? Tags { get; set; }
    public string? Category { get; set; }
    public IFormFile VideoFile { get; set; } = null!;
    public IFormFile? ThumbnailFile { get; set; }
}

public class VideoUpdateDto
{
    public string? Title { get; set; }
    public string? Description { get; set; }
    public VideoVisibility? Visibility { get; set; }
    public string? Tags { get; set; }
    public string? Category { get; set; }
    public IFormFile? ThumbnailFile { get; set; }
}

public class VideoFilterDto
{
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
    public string? Search { get; set; }
    public string? Category { get; set; }
    public VideoType? Type { get; set; }
    public VideoVisibility? Visibility { get; set; }
    public string? SortBy { get; set; } = "createdAt";
    public string? SortOrder { get; set; } = "desc";
}

public class PagedResult<T>
{
    public List<T> Items { get; set; } = new();
    public int TotalCount { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
    public bool HasNextPage => Page < TotalPages;
    public bool HasPreviousPage => Page > 1;
}
