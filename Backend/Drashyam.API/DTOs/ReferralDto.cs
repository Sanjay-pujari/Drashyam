using Drashyam.API.Models;
using System.ComponentModel.DataAnnotations;

namespace Drashyam.API.DTOs;

public class ReferralDto
{
    public int Id { get; set; }
    public string ReferrerId { get; set; } = string.Empty;
    public string ReferrerName { get; set; } = string.Empty;
    public string ReferredUserId { get; set; } = string.Empty;
    public string ReferredUserName { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime? RewardedAt { get; set; }
    public ReferralStatus Status { get; set; }
    public string? ReferralCode { get; set; }
    public decimal? RewardAmount { get; set; }
    public string? RewardType { get; set; }
    public int? ReferralPoints { get; set; }
}

public class CreateReferralDto
{
    [Required]
    public string ReferredUserId { get; set; } = string.Empty;

    [MaxLength(100)]
    public string? ReferralCode { get; set; }
}

public class ReferralStatsDto
{
    public int TotalReferrals { get; set; }
    public int CompletedReferrals { get; set; }
    public int PendingReferrals { get; set; }
    public decimal TotalRewards { get; set; }
    public decimal PendingRewards { get; set; }
    public decimal ConversionRate { get; set; }
}

public class ReferralRewardDto
{
    public int Id { get; set; }
    public string UserId { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    public string ReferralId { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string RewardType { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime? ClaimedAt { get; set; }
    public DateTime? ExpiresAt { get; set; }
    public RewardStatus Status { get; set; }
    public string? Description { get; set; }
}

public class ClaimRewardDto
{
    [Required]
    public int RewardId { get; set; }
}

public class ReferralCodeDto
{
    public string Code { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime? ExpiresAt { get; set; }
    public int UsageCount { get; set; }
    public int MaxUsage { get; set; }
    public bool IsActive { get; set; }
}

public class CreateReferralCodeDto
{
    [MaxLength(50)]
    public string? Code { get; set; }

    public int? MaxUsage { get; set; }

    public DateTime? ExpiresAt { get; set; }

    public decimal? RewardAmount { get; set; }

    [MaxLength(50)]
    public string? RewardType { get; set; } = "Points";
}
