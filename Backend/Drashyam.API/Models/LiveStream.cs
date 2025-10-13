using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Drashyam.API.Models;

public class LiveStream
{
    public int Id { get; set; }

    [Required]
    [MaxLength(200)]
    public string Title { get; set; } = string.Empty;

    [MaxLength(1000)]
    public string? Description { get; set; }

    [Required]
    public string UserId { get; set; } = string.Empty;

    public int? ChannelId { get; set; }

    [Required]
    public string StreamKey { get; set; } = string.Empty;

    public string? StreamUrl { get; set; }

    public string? HlsUrl { get; set; }

    public LiveStreamStatus Status { get; set; } = LiveStreamStatus.Scheduled;

    public DateTime ScheduledStartTime { get; set; }

    public DateTime? ActualStartTime { get; set; }

    public DateTime? EndTime { get; set; }

    public long ViewerCount { get; set; } = 0;

    public long PeakViewerCount { get; set; } = 0;

    public bool IsRecording { get; set; } = false;

    public string? RecordingUrl { get; set; }

    public bool IsMonetized { get; set; } = false;

    public string? ThumbnailUrl { get; set; }

    public string? Category { get; set; }

    public string? Tags { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    [ForeignKey("UserId")]
    public virtual ApplicationUser User { get; set; } = null!;

    [ForeignKey("ChannelId")]
    public virtual Channel? Channel { get; set; }
}

public enum LiveStreamStatus
{
    Scheduled,
    Live,
    Ended,
    Cancelled
}
