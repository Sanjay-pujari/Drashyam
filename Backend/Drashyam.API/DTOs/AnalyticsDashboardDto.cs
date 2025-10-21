namespace Drashyam.API.DTOs;

public class AnalyticsDashboardDto
{
    public int Id { get; set; }
    public string UserId { get; set; } = string.Empty;
    public int? ChannelId { get; set; }
    public DateTime Date { get; set; }

    // Overview Metrics
    public long TotalViews { get; set; }
    public long UniqueViews { get; set; }
    public long TotalLikes { get; set; }
    public long TotalDislikes { get; set; }
    public long TotalComments { get; set; }
    public long TotalShares { get; set; }
    public long TotalSubscribers { get; set; }

    // Revenue Metrics
    public decimal TotalRevenue { get; set; }
    public decimal AdRevenue { get; set; }
    public decimal SubscriptionRevenue { get; set; }
    public decimal PremiumContentRevenue { get; set; }
    public decimal MerchandiseRevenue { get; set; }
    public decimal DonationRevenue { get; set; }

    // Engagement Metrics
    public decimal AverageWatchTime { get; set; }
    public decimal EngagementRate { get; set; }
    public decimal LikeRate { get; set; }
    public decimal CommentRate { get; set; }
    public decimal ShareRate { get; set; }
    public decimal ClickThroughRate { get; set; }

    // Audience Metrics
    public string? TopCountry { get; set; }
    public string? TopDeviceType { get; set; }
    public string? TopReferrer { get; set; }

    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

public class VideoAnalyticsDto
{
    public int Id { get; set; }
    public int VideoId { get; set; }
    public string VideoTitle { get; set; } = string.Empty;
    public string VideoThumbnailUrl { get; set; } = string.Empty;
    public string UserId { get; set; } = string.Empty;
    public DateTime Date { get; set; }

    // Video-specific metrics
    public long Views { get; set; }
    public long UniqueViews { get; set; }
    public long Likes { get; set; }
    public long Dislikes { get; set; }
    public long Comments { get; set; }
    public long Shares { get; set; }
    public decimal Revenue { get; set; }
    public decimal AverageWatchTime { get; set; }
    public decimal EngagementRate { get; set; }

    // Geographic data
    public string? Country { get; set; }
    public string? DeviceType { get; set; }
    public string? Referrer { get; set; }

    public DateTime CreatedAt { get; set; }
}

public class RevenueAnalyticsDto
{
    public int Id { get; set; }
    public string UserId { get; set; } = string.Empty;
    public int? ChannelId { get; set; }
    public DateTime Date { get; set; }

    // Revenue breakdown
    public decimal TotalRevenue { get; set; }
    public decimal AdRevenue { get; set; }
    public decimal SubscriptionRevenue { get; set; }
    public decimal PremiumContentRevenue { get; set; }
    public decimal MerchandiseRevenue { get; set; }
    public decimal DonationRevenue { get; set; }
    public decimal ReferralRevenue { get; set; }

    // Revenue metrics
    public decimal RevenuePerView { get; set; }
    public decimal RevenuePerSubscriber { get; set; }
    public decimal RevenueGrowthRate { get; set; }

    public DateTime CreatedAt { get; set; }
}

public class AudienceAnalyticsDto
{
    public int Id { get; set; }
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
    public long ViewCount { get; set; }
    public decimal WatchTime { get; set; }
    public decimal EngagementScore { get; set; }
    public decimal Revenue { get; set; }

    public DateTime CreatedAt { get; set; }
}

public class EngagementAnalyticsDto
{
    public int Id { get; set; }
    public string UserId { get; set; } = string.Empty;
    public int? ChannelId { get; set; }
    public DateTime Date { get; set; }

    // Engagement metrics
    public decimal LikeRate { get; set; }
    public decimal CommentRate { get; set; }
    public decimal ShareRate { get; set; }
    public decimal WatchTimeRate { get; set; }
    public decimal ClickThroughRate { get; set; }
    public decimal RetentionRate { get; set; }

    // Interaction counts
    public long TotalLikes { get; set; }
    public long TotalComments { get; set; }
    public long TotalShares { get; set; }
    public long TotalViews { get; set; }

    public DateTime CreatedAt { get; set; }
}

public class AnalyticsSummaryDto
{
    public long TotalViews { get; set; }
    public long TotalLikes { get; set; }
    public long TotalComments { get; set; }
    public long TotalShares { get; set; }
    public long TotalSubscribers { get; set; }
    public decimal TotalRevenue { get; set; }
    public decimal AverageWatchTime { get; set; }
    public decimal EngagementRate { get; set; }
    public decimal RevenueGrowth { get; set; }
    public decimal SubscriberGrowth { get; set; }
}

public class TopVideoAnalyticsDto
{
    public int VideoId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string ThumbnailUrl { get; set; } = string.Empty;
    public long Views { get; set; }
    public long Likes { get; set; }
    public long Comments { get; set; }
    public decimal Revenue { get; set; }
    public decimal EngagementRate { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class GeographicAnalyticsDto
{
    public string Country { get; set; } = string.Empty;
    public string CountryCode { get; set; } = string.Empty;
    public long Views { get; set; }
    public decimal Revenue { get; set; }
    public decimal Percentage { get; set; }
    public long Subscribers { get; set; }
}

public class DeviceAnalyticsDto
{
    public string DeviceType { get; set; } = string.Empty;
    public long Views { get; set; }
    public decimal Percentage { get; set; }
    public decimal AverageWatchTime { get; set; }
    public decimal EngagementRate { get; set; }
}

public class ReferrerAnalyticsDto
{
    public string Referrer { get; set; } = string.Empty;
    public long Views { get; set; }
    public decimal Percentage { get; set; }
    public decimal ConversionRate { get; set; }
}

public class TimeSeriesAnalyticsDto
{
    public DateTime Date { get; set; }
    public long Views { get; set; }
    public long Likes { get; set; }
    public long Comments { get; set; }
    public long Shares { get; set; }
    public decimal Revenue { get; set; }
    public decimal EngagementRate { get; set; }
}

public class ChannelComparisonDto
{
    public int ChannelId { get; set; }
    public string ChannelName { get; set; } = string.Empty;
    public long Views { get; set; }
    public long Subscribers { get; set; }
    public decimal Revenue { get; set; }
    public decimal EngagementRate { get; set; }
    public decimal GrowthRate { get; set; }
}

public class AnalyticsFilterDto
{
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public int? ChannelId { get; set; }
    public string? Country { get; set; }
    public string? DeviceType { get; set; }
    public string? Referrer { get; set; }
    public string? VideoCategory { get; set; }
}
