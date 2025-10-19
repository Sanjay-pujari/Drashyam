using Drashyam.API.Models;

namespace Drashyam.API.DTOs;

public class PagedResult<T>
{
    public List<T> Items { get; set; } = new List<T>();
    public int TotalCount { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
    public bool HasNextPage => Page < TotalPages;
    public bool HasPreviousPage => Page > 1;
}

public class HomeFeedDto
{
    public PagedResult<VideoDto> Trending { get; set; } = new PagedResult<VideoDto>();
    public PagedResult<VideoDto> Recommended { get; set; } = new PagedResult<VideoDto>();
    public PagedResult<VideoDto> Subscribed { get; set; } = new PagedResult<VideoDto>();
}

public class VideoFilterDto
{
    public string? Search { get; set; }
    public string? Category { get; set; }
    public VideoType? Type { get; set; }
    public string? SortBy { get; set; } = "createdAt";
    public string? SortOrder { get; set; } = "desc";
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}

public class VideoUploadDto
{
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public IFormFile VideoFile { get; set; } = null!;
    public IFormFile? ThumbnailFile { get; set; }
    public int? ChannelId { get; set; }
    public VideoVisibility Visibility { get; set; } = VideoVisibility.Public;
    public string? Tags { get; set; }
    public string? Category { get; set; }
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

public class UpdateVideoStatusDto
{
    public VideoProcessingStatus Status { get; set; }
}

public enum LikeType
{
    Like,
    Dislike
}
