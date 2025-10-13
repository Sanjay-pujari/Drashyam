using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Drashyam.API.Models;

public class Channel
{
    public int Id { get; set; }

    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? Description { get; set; }

    [Required]
    public string UserId { get; set; } = string.Empty;

    public string? BannerUrl { get; set; }

    public string? ProfilePictureUrl { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public long SubscriberCount { get; set; } = 0;

    public long VideoCount { get; set; } = 0;

    public long TotalViews { get; set; } = 0;

    public bool IsVerified { get; set; } = false;

    public bool IsMonetized { get; set; } = false;

    public ChannelType Type { get; set; } = ChannelType.Personal;

    public int MaxVideos { get; set; } = 10; // Based on subscription

    public int CurrentVideoCount { get; set; } = 0;

    public string? CustomUrl { get; set; }

    public string? WebsiteUrl { get; set; }

    public string? SocialLinks { get; set; } // JSON string

    // Navigation properties
    [ForeignKey("UserId")]
    public virtual ApplicationUser User { get; set; } = null!;

    public virtual ICollection<Video> Videos { get; set; } = new List<Video>();
    public virtual ICollection<ChannelSubscription> Subscriptions { get; set; } = new List<ChannelSubscription>();
}

public enum ChannelType
{
    Personal,
    Business,
    Brand
}
