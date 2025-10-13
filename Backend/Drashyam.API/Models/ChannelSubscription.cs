using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Drashyam.API.Models;

public class ChannelSubscription
{
    public int Id { get; set; }

    [Required]
    public string UserId { get; set; } = string.Empty;

    [Required]
    public int ChannelId { get; set; }

    public DateTime SubscribedAt { get; set; } = DateTime.UtcNow;

    public bool IsActive { get; set; } = true;

    public bool NotificationsEnabled { get; set; } = true;

    // Navigation properties
    [ForeignKey("UserId")]
    public virtual ApplicationUser User { get; set; } = null!;

    [ForeignKey("ChannelId")]
    public virtual Channel Channel { get; set; } = null!;
}
