using System.ComponentModel.DataAnnotations;

namespace Drashyam.API.Models;

public class Referral
{
    public int Id { get; set; }

    [Required]
    [MaxLength(450)]
    public string ReferrerId { get; set; } = string.Empty;

    [Required]
    [MaxLength(450)]
    public string ReferredUserId { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? RewardedAt { get; set; }

    public ReferralStatus Status { get; set; } = ReferralStatus.Pending;

    [MaxLength(100)]
    public string? ReferralCode { get; set; }

    public decimal? RewardAmount { get; set; }

    [MaxLength(50)]
    public string? RewardType { get; set; }

    public int? ReferralPoints { get; set; }

    // Navigation properties
    public virtual ApplicationUser Referrer { get; set; } = null!;
    public virtual ApplicationUser ReferredUser { get; set; } = null!;
}

public enum ReferralStatus
{
    Pending,
    Completed,
    Rewarded,
    Cancelled
}
