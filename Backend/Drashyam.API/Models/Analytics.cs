using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Drashyam.API.Models;

public class Analytics
{
    public int Id { get; set; }

    public int? VideoId { get; set; }

    public int? ChannelId { get; set; }

    public string? UserId { get; set; }

    public DateTime Date { get; set; }

    public long Views { get; set; } = 0;

    public long UniqueViews { get; set; } = 0;

    public long Likes { get; set; } = 0;

    public long Dislikes { get; set; } = 0;

    public long Comments { get; set; } = 0;

    public long Shares { get; set; } = 0;

    public long Subscribers { get; set; } = 0;

    public decimal Revenue { get; set; } = 0;

    public TimeSpan AverageWatchTime { get; set; }

    public string? Country { get; set; }

    public string? DeviceType { get; set; }

    public string? Referrer { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    [ForeignKey("VideoId")]
    public virtual Video? Video { get; set; }

    [ForeignKey("ChannelId")]
    public virtual Channel? Channel { get; set; }

    [ForeignKey("UserId")]
    public virtual ApplicationUser? User { get; set; }
}
