using Drashyam.API.DTOs;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Drashyam.API.Models;

public class Subscription
{
    public int Id { get; set; }

    [Required]
    public string UserId { get; set; } = string.Empty;

    public int SubscriptionPlanId { get; set; }
    public virtual SubscriptionPlan Plan { get; set; } = null!;

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
    public SubscriptionStatus Status { get; internal set; }
}

