using System.ComponentModel.DataAnnotations;

namespace Drashyam.API.Models;

public class InviteAnalytics
{
    public int Id { get; set; }

    [Required]
    [MaxLength(450)]
    public string UserId { get; set; } = string.Empty;

    public DateTime Date { get; set; } = DateTime.UtcNow.Date;

    public int InvitesSent { get; set; } = 0;

    public int InvitesAccepted { get; set; } = 0;

    public int InvitesExpired { get; set; } = 0;

    public int InvitesCancelled { get; set; } = 0;

    public decimal ConversionRate { get; set; } = 0;

    public int EmailInvites { get; set; } = 0;

    public int SocialInvites { get; set; } = 0;

    public int DirectLinkInvites { get; set; } = 0;

    public int BulkInvites { get; set; } = 0;

    public TimeSpan AverageTimeToAccept { get; set; } = TimeSpan.Zero;

    public int Resends { get; set; } = 0;

    // Navigation properties
    public virtual ApplicationUser User { get; set; } = null!;
}

public class ReferralAnalytics
{
    public int Id { get; set; }

    [Required]
    [MaxLength(450)]
    public string UserId { get; set; } = string.Empty;

    public DateTime Date { get; set; } = DateTime.UtcNow.Date;

    public int ReferralsCreated { get; set; } = 0;

    public int ReferralsCompleted { get; set; } = 0;

    public int ReferralsRewarded { get; set; } = 0;

    public decimal TotalRewardsEarned { get; set; } = 0;

    public decimal TotalRewardsClaimed { get; set; } = 0;

    public decimal ConversionRate { get; set; } = 0;

    public int ReferralCodesGenerated { get; set; } = 0;

    public int ReferralCodesUsed { get; set; } = 0;

    public decimal AverageRewardAmount { get; set; } = 0;

    public TimeSpan AverageTimeToComplete { get; set; } = TimeSpan.Zero;

    // Navigation properties
    public virtual ApplicationUser User { get; set; } = null!;
}

public class InviteEvent
{
    public int Id { get; set; }

    [Required]
    [MaxLength(450)]
    public string UserId { get; set; } = string.Empty;

    public int? InviteId { get; set; }

    public InviteEventType EventType { get; set; }

    public DateTime Timestamp { get; set; } = DateTime.UtcNow;

    [MaxLength(500)]
    public string? Details { get; set; }

    [MaxLength(50)]
    public string? Source { get; set; }

    [MaxLength(100)]
    public string? UserAgent { get; set; }

    [MaxLength(45)]
    public string? IpAddress { get; set; }

    // Navigation properties
    public virtual ApplicationUser User { get; set; } = null!;
    public virtual UserInvite? Invite { get; set; }
}

public class ReferralEvent
{
    public int Id { get; set; }

    [Required]
    [MaxLength(450)]
    public string UserId { get; set; } = string.Empty;

    public int? ReferralId { get; set; }

    public ReferralEventType EventType { get; set; }

    public DateTime Timestamp { get; set; } = DateTime.UtcNow;

    [MaxLength(500)]
    public string? Details { get; set; }

    [MaxLength(50)]
    public string? Source { get; set; }

    [MaxLength(100)]
    public string? UserAgent { get; set; }

    [MaxLength(45)]
    public string? IpAddress { get; set; }

    // Navigation properties
    public virtual ApplicationUser User { get; set; } = null!;
    public virtual Referral? Referral { get; set; }
}

public enum InviteEventType
{
    Created,
    Sent,
    Opened,
    Clicked,
    Accepted,
    Expired,
    Cancelled,
    Resent
}

public enum ReferralEventType
{
    Created,
    CodeGenerated,
    CodeUsed,
    Completed,
    Rewarded,
    Claimed,
    Expired
}
