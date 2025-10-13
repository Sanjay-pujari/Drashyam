using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Drashyam.API.Models;

public class Subscription
{
    public int Id { get; set; }

    [Required]
    public string UserId { get; set; } = string.Empty;

    public SubscriptionPlan Plan { get; set; }

    public DateTime StartDate { get; set; } = DateTime.UtcNow;

    public DateTime EndDate { get; set; }

    public bool IsActive { get; set; } = true;

    public bool AutoRenew { get; set; } = true;

    public decimal Amount { get; set; }

    public string? StripeSubscriptionId { get; set; }

    public string? PaymentMethodId { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? CancelledAt { get; set; }

    // Navigation properties
    [ForeignKey("UserId")]
    public virtual ApplicationUser User { get; set; } = null!;
}

public class SubscriptionPlan
{
    public int Id { get; set; }

    [Required]
    [MaxLength(50)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(200)]
    public string? Description { get; set; }

    public decimal Price { get; set; }

    public BillingCycle BillingCycle { get; set; }

    public int MaxChannels { get; set; } = 1;

    public int MaxVideosPerChannel { get; set; } = 10;

    public long MaxStorageGB { get; set; } = 1;

    public bool HasAds { get; set; } = true;

    public bool HasAnalytics { get; set; } = false;

    public bool HasMonetization { get; set; } = false;

    public bool HasLiveStreaming { get; set; } = false;

    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

public enum BillingCycle
{
    Monthly,
    Yearly
}
