using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Drashyam.API.Models;

public class AnalyticsDashboard
{
    public int Id { get; set; }

    [Required]
    public string UserId { get; set; } = string.Empty;

    public int? ChannelId { get; set; }

    public DateTime Date { get; set; }

    // Overview Metrics
    public long TotalViews { get; set; } = 0;
    public long UniqueViews { get; set; } = 0;
    public long TotalLikes { get; set; } = 0;
    public long TotalDislikes { get; set; } = 0;
    public long TotalComments { get; set; } = 0;
    public long TotalShares { get; set; } = 0;
    public long TotalSubscribers { get; set; } = 0;

    // Revenue Metrics
    public decimal TotalRevenue { get; set; } = 0;
    public decimal AdRevenue { get; set; } = 0;
    public decimal SubscriptionRevenue { get; set; } = 0;
    public decimal PremiumContentRevenue { get; set; } = 0;
    public decimal MerchandiseRevenue { get; set; } = 0;
    public decimal DonationRevenue { get; set; } = 0;

    // Engagement Metrics
    public decimal AverageWatchTime { get; set; } = 0;
    public decimal EngagementRate { get; set; } = 0;
    public decimal LikeRate { get; set; } = 0;
    public decimal CommentRate { get; set; } = 0;
    public decimal ShareRate { get; set; } = 0;
    public decimal ClickThroughRate { get; set; } = 0;

    // Audience Metrics
    public string? TopCountry { get; set; }
    public string? TopDeviceType { get; set; }
    public string? TopReferrer { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    // Navigation properties
    [ForeignKey("UserId")]
    public virtual ApplicationUser User { get; set; } = null!;

    [ForeignKey("ChannelId")]
    public virtual Channel? Channel { get; set; }
}

public class VideoAnalytics
{
    public int Id { get; set; }

    [Required]
    public int VideoId { get; set; }

    [Required]
    public string UserId { get; set; } = string.Empty;

    public DateTime Date { get; set; }

    // Video-specific metrics
    public long Views { get; set; } = 0;
    public long UniqueViews { get; set; } = 0;
    public long Likes { get; set; } = 0;
    public long Dislikes { get; set; } = 0;
    public long Comments { get; set; } = 0;
    public long Shares { get; set; } = 0;
    public decimal Revenue { get; set; } = 0;
    public decimal AverageWatchTime { get; set; } = 0;
    public decimal EngagementRate { get; set; } = 0;

    // Geographic data
    public string? Country { get; set; }
    public string? DeviceType { get; set; }
    public string? Referrer { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    [ForeignKey("VideoId")]
    public virtual Video Video { get; set; } = null!;

    [ForeignKey("UserId")]
    public virtual ApplicationUser User { get; set; } = null!;
}

public class RevenueAnalytics
{
    public int Id { get; set; }

    [Required]
    public string UserId { get; set; } = string.Empty;

    public int? ChannelId { get; set; }

    public DateTime Date { get; set; }

    // Revenue breakdown
    public decimal TotalRevenue { get; set; } = 0;
    public decimal AdRevenue { get; set; } = 0;
    public decimal SubscriptionRevenue { get; set; } = 0;
    public decimal PremiumContentRevenue { get; set; } = 0;
    public decimal MerchandiseRevenue { get; set; } = 0;
    public decimal DonationRevenue { get; set; } = 0;
    public decimal ReferralRevenue { get; set; } = 0;

    // Revenue metrics
    public decimal RevenuePerView { get; set; } = 0;
    public decimal RevenuePerSubscriber { get; set; } = 0;
    public decimal RevenueGrowthRate { get; set; } = 0;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    [ForeignKey("UserId")]
    public virtual ApplicationUser User { get; set; } = null!;

    [ForeignKey("ChannelId")]
    public virtual Channel? Channel { get; set; }
}

public class AudienceAnalytics
{
    public int Id { get; set; }

    [Required]
    public string UserId { get; set; } = string.Empty;

    public int? ChannelId { get; set; }

    public DateTime Date { get; set; }

    // Demographic data
    public string? AgeGroup { get; set; }
    public string? Gender { get; set; }
    public string? Country { get; set; }
    public string? DeviceType { get; set; }
    public string? Referrer { get; set; }

    // Metrics
    public long ViewCount { get; set; } = 0;
    public decimal WatchTime { get; set; } = 0;
    public decimal EngagementScore { get; set; } = 0;
    public decimal Revenue { get; set; } = 0;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    [ForeignKey("UserId")]
    public virtual ApplicationUser User { get; set; } = null!;

    [ForeignKey("ChannelId")]
    public virtual Channel? Channel { get; set; }
}

public class EngagementAnalytics
{
    public int Id { get; set; }

    [Required]
    public string UserId { get; set; } = string.Empty;

    public int? ChannelId { get; set; }

    public DateTime Date { get; set; }

    // Engagement metrics
    public decimal LikeRate { get; set; } = 0;
    public decimal CommentRate { get; set; } = 0;
    public decimal ShareRate { get; set; } = 0;
    public decimal WatchTimeRate { get; set; } = 0;
    public decimal ClickThroughRate { get; set; } = 0;
    public decimal RetentionRate { get; set; } = 0;

    // Interaction counts
    public long TotalLikes { get; set; } = 0;
    public long TotalComments { get; set; } = 0;
    public long TotalShares { get; set; } = 0;
    public long TotalViews { get; set; } = 0;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    [ForeignKey("UserId")]
    public virtual ApplicationUser User { get; set; } = null!;

    [ForeignKey("ChannelId")]
    public virtual Channel? Channel { get; set; }
}
