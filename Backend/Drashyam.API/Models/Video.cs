using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Drashyam.API.Models;

public class Video
{
    public int Id { get; set; }

    [Required]
    [MaxLength(200)]
    public string Title { get; set; } = string.Empty;

    [MaxLength(1000)]
    public string? Description { get; set; }

    [Required]
    public string VideoUrl { get; set; } = string.Empty;

    public string? ThumbnailUrl { get; set; }

    [Required]
    public string UserId { get; set; } = string.Empty;

    public int? ChannelId { get; set; }

    public VideoProcessingStatus Status { get; set; } = VideoProcessingStatus.Processing;

    public VideoType Type { get; set; } = VideoType.Uploaded;

    public VideoVisibility Visibility { get; set; } = VideoVisibility.Public;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? PublishedAt { get; set; }

    public long ViewCount { get; set; } = 0;

    public long LikeCount { get; set; } = 0;

    public long DislikeCount { get; set; } = 0;

    public long CommentCount { get; set; } = 0;

    public TimeSpan Duration { get; set; }

    public long FileSize { get; set; }

    public string? Tags { get; set; }

    public string? Category { get; set; }

    public bool IsMonetized { get; set; } = false;

    public decimal? Revenue { get; set; }

    public string? ShareToken { get; set; }

    // Navigation properties
    [ForeignKey("UserId")]
    public virtual ApplicationUser User { get; set; } = null!;

    [ForeignKey("ChannelId")]
    public virtual Channel? Channel { get; set; }

    public virtual ICollection<Comment> Comments { get; set; } = new List<Comment>();
    public virtual ICollection<VideoLike> Likes { get; set; } = new List<VideoLike>();
    public virtual ICollection<VideoView> Views { get; set; } = new List<VideoView>();
}

public enum VideoProcessingStatus
{
    Processing,
    Ready,
    Failed,
    Deleted
}

public class VideoProcessingProgress
{
    public int Id { get; set; }
    public int VideoId { get; set; }
    public string Status { get; set; } = string.Empty;
    public int ProgressPercentage { get; set; }
    public string? CurrentStep { get; set; }
    public string? ErrorMessage { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public virtual Video Video { get; set; } = null!;
}

public enum VideoType
{
    Uploaded,
    LiveStream,
    Short
}

public enum VideoVisibility
{
    Public,
    Unlisted,
    Private
}
