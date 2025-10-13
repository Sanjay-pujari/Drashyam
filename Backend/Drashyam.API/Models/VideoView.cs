using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Drashyam.API.Models;

public class VideoView
{
    public int Id { get; set; }

    [Required]
    public string UserId { get; set; } = string.Empty;

    [Required]
    public int VideoId { get; set; }

    public DateTime ViewedAt { get; set; } = DateTime.UtcNow;

    public TimeSpan WatchDuration { get; set; }

    public string? UserAgent { get; set; }

    public string? IpAddress { get; set; }

    public string? Country { get; set; }

    public string? DeviceType { get; set; }

    // Navigation properties
    [ForeignKey("UserId")]
    public virtual ApplicationUser User { get; set; } = null!;

    [ForeignKey("VideoId")]
    public virtual Video Video { get; set; } = null!;
}
