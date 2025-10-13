using System.ComponentModel.DataAnnotations;

namespace Drashyam.API.Models;

public class SubscriptionPlan
{
    public int Id { get; set; }

    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? Description { get; set; }

    [Required]
    public decimal Price { get; set; }

    public BillingCycle BillingCycle { get; set; } = BillingCycle.Monthly;

    public int MaxChannels { get; set; } = 1;

    public int MaxVideosPerChannel { get; set; } = 10;

    public int MaxStorageGB { get; set; } = 1;

    public bool HasAds { get; set; } = true;

    public bool HasAnalytics { get; set; } = false;

    public bool HasMonetization { get; set; } = false;

    public bool HasLiveStreaming { get; set; } = false;

    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? UpdatedAt { get; set; }

    // Navigation properties
    public virtual ICollection<Subscription> Subscriptions { get; set; } = new List<Subscription>();
}

public enum BillingCycle
{
    Monthly,
    Yearly
}
