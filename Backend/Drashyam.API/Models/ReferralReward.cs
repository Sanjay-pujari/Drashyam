using System.ComponentModel.DataAnnotations;
using Drashyam.API.DTOs;

namespace Drashyam.API.Models;

public class ReferralReward
{
    public int Id { get; set; }

    [Required]
    [MaxLength(450)]
    public string UserId { get; set; } = string.Empty;

    [Required]
    public int ReferralId { get; set; }

    public decimal Amount { get; set; }

    [MaxLength(50)]
    public string RewardType { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? ClaimedAt { get; set; }

    public DateTime? ExpiresAt { get; set; }

    public RewardStatus Status { get; set; } = RewardStatus.Pending;

    [MaxLength(500)]
    public string? Description { get; set; }

    // Navigation properties
    public virtual ApplicationUser User { get; set; } = null!;
    public virtual Referral Referral { get; set; } = null!;
}

