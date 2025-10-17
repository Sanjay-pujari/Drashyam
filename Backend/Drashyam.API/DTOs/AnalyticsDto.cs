using Drashyam.API.Models;

namespace Drashyam.API.DTOs;

public class InviteAnalyticsDto
{
    public int Id { get; set; }
    public string UserId { get; set; } = string.Empty;
    public DateTime Date { get; set; }
    public int InvitesSent { get; set; }
    public int InvitesAccepted { get; set; }
    public int InvitesExpired { get; set; }
    public int InvitesCancelled { get; set; }
    public decimal ConversionRate { get; set; }
    public int EmailInvites { get; set; }
    public int SocialInvites { get; set; }
    public int DirectLinkInvites { get; set; }
    public int BulkInvites { get; set; }
    public TimeSpan AverageTimeToAccept { get; set; }
    public int Resends { get; set; }
}

public class ReferralAnalyticsDto
{
    public int Id { get; set; }
    public string UserId { get; set; } = string.Empty;
    public DateTime Date { get; set; }
    public int ReferralsCreated { get; set; }
    public int ReferralsCompleted { get; set; }
    public int ReferralsRewarded { get; set; }
    public decimal TotalRewardsEarned { get; set; }
    public decimal TotalRewardsClaimed { get; set; }
    public decimal ConversionRate { get; set; }
    public int ReferralCodesGenerated { get; set; }
    public int ReferralCodesUsed { get; set; }
    public decimal AverageRewardAmount { get; set; }
    public TimeSpan AverageTimeToComplete { get; set; }
}

public class InviteEventDto
{
    public int Id { get; set; }
    public string UserId { get; set; } = string.Empty;
    public int? InviteId { get; set; }
    public InviteEventType EventType { get; set; }
    public DateTime Timestamp { get; set; }
    public string? Details { get; set; }
    public string? Source { get; set; }
    public string? UserAgent { get; set; }
    public string? IpAddress { get; set; }
}

public class ReferralEventDto
{
    public int Id { get; set; }
    public string UserId { get; set; } = string.Empty;
    public int? ReferralId { get; set; }
    public ReferralEventType EventType { get; set; }
    public DateTime Timestamp { get; set; }
    public string? Details { get; set; }
    public string? Source { get; set; }
    public string? UserAgent { get; set; }
    public string? IpAddress { get; set; }
}

public class AnalyticsSummaryDto
{
    public InviteAnalyticsDto InviteAnalytics { get; set; } = new();
    public ReferralAnalyticsDto ReferralAnalytics { get; set; } = new();
    public int TotalInvitesSent { get; set; }
    public int TotalReferralsCreated { get; set; }
    public decimal TotalRewardsEarned { get; set; }
    public decimal TotalRewardsClaimed { get; set; }
    public decimal OverallConversionRate { get; set; }
    public DateTime LastActivity { get; set; }
}