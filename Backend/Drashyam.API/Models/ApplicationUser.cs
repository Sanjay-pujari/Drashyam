using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace Drashyam.API.Models;

public class ApplicationUser : IdentityUser
{
    [Required]
    [MaxLength(100)]
    public string FirstName { get; set; } = string.Empty;

    [Required]
    [MaxLength(100)]
    public string LastName { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? Bio { get; set; }

    public string? ProfilePictureUrl { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? LastLoginAt { get; set; }

    public bool IsActive { get; set; } = true;

    public SubscriptionType SubscriptionType { get; set; } = SubscriptionType.Free;

    public DateTime? SubscriptionExpiresAt { get; set; }

    public string? StripeCustomerId { get; set; }

    // Navigation properties
    public virtual ICollection<Channel> Channels { get; set; } = new List<Channel>();
    public virtual ICollection<Video> Videos { get; set; } = new List<Video>();
    public virtual ICollection<Comment> Comments { get; set; } = new List<Comment>();
    public virtual ICollection<VideoLike> VideoLikes { get; set; } = new List<VideoLike>();
    public virtual ICollection<Subscription> Subscriptions { get; set; } = new List<Subscription>();
    public virtual ICollection<ChannelSubscription> ChannelSubscriptions { get; set; } = new List<ChannelSubscription>();
}

public enum SubscriptionType
{
    Free,
    Premium,
    Pro
}
