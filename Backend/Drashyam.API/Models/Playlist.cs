using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Drashyam.API.Models;

public class Playlist
{
    public int Id { get; set; }

    [Required]
    [MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(1000)]
    public string? Description { get; set; }

    [Required]
    public string UserId { get; set; } = string.Empty;

    public int? ChannelId { get; set; }

    public PlaylistVisibility Visibility { get; set; } = PlaylistVisibility.Public;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? UpdatedAt { get; set; }

    public int VideoCount { get; set; } = 0;

    public string? ThumbnailUrl { get; set; }

    // Navigation properties
    [ForeignKey("UserId")]
    public virtual ApplicationUser User { get; set; } = null!;

    [ForeignKey("ChannelId")]
    public virtual Channel? Channel { get; set; }

    public virtual ICollection<PlaylistVideo> PlaylistVideos { get; set; } = new List<PlaylistVideo>();
}

public class PlaylistVideo
{
    public int Id { get; set; }

    [Required]
    public int PlaylistId { get; set; }

    [Required]
    public int VideoId { get; set; }

    public int Order { get; set; }

    public DateTime AddedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    [ForeignKey("PlaylistId")]
    public virtual Playlist Playlist { get; set; } = null!;

    [ForeignKey("VideoId")]
    public virtual Video Video { get; set; } = null!;
}

public enum PlaylistVisibility
{
    Public,
    Unlisted,
    Private
}
