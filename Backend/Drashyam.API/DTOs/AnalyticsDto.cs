namespace Drashyam.API.DTOs;

public class AnalyticsDto
{
    public long TotalViews { get; set; }
    public long UniqueViews { get; set; }
    public long TotalLikes { get; set; }
    public long TotalDislikes { get; set; }
    public long TotalComments { get; set; }
    public long TotalShares { get; set; }
    public long TotalSubscribers { get; set; }
    public decimal TotalRevenue { get; set; }
    public TimeSpan AverageWatchTime { get; set; }
    public double EngagementRate { get; set; }
    public List<DailyMetricsDto> DailyMetrics { get; set; } = new();
    public List<VideoMetricsDto> TopVideos { get; set; } = new();
    public List<GeographicDataDto> GeographicData { get; set; } = new();
    public List<DeviceDataDto> DeviceData { get; set; } = new();
    public List<AgeGroupDataDto> AgeGroupData { get; set; } = new();
    public List<GenderDataDto> GenderData { get; set; } = new();
}

public class DailyMetricsDto
{
    public DateTime Date { get; set; }
    public long Views { get; set; }
    public long Likes { get; set; }
    public long Comments { get; set; }
    public long Shares { get; set; }
    public long Subscribers { get; set; }
    public decimal Revenue { get; set; }
    public TimeSpan AverageWatchTime { get; set; }
}

public class VideoMetricsDto
{
    public int VideoId { get; set; }
    public string Title { get; set; } = string.Empty;
    public long Views { get; set; }
    public long Likes { get; set; }
    public long Comments { get; set; }
    public decimal Revenue { get; set; }
    public TimeSpan AverageWatchTime { get; set; }
    public double EngagementRate { get; set; }
}

public class RevenueDto
{
    public DateTime Date { get; set; }
    public decimal Amount { get; set; }
    public string Source { get; set; } = string.Empty; // Ads, Subscriptions, Donations, etc.
    public string Description { get; set; } = string.Empty;
}

public class TopVideoDto
{
    public int VideoId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string ThumbnailUrl { get; set; } = string.Empty;
    public long Views { get; set; }
    public long Likes { get; set; }
    public decimal Revenue { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class AudienceInsightDto
{
    public string Category { get; set; } = string.Empty;
    public long Count { get; set; }
    public double Percentage { get; set; }
}

public class GeographicDataDto
{
    public string Country { get; set; } = string.Empty;
    public string CountryCode { get; set; } = string.Empty;
    public long Views { get; set; }
    public double Percentage { get; set; }
    public decimal Revenue { get; set; }
}

public class DeviceDataDto
{
    public string DeviceType { get; set; } = string.Empty; // Mobile, Desktop, Tablet, TV
    public long Views { get; set; }
    public double Percentage { get; set; }
    public TimeSpan AverageWatchTime { get; set; }
}

public class AgeGroupDataDto
{
    public string AgeGroup { get; set; } = string.Empty; // 18-24, 25-34, 35-44, etc.
    public long Views { get; set; }
    public double Percentage { get; set; }
}

public class GenderDataDto
{
    public string Gender { get; set; } = string.Empty; // Male, Female, Other
    public long Views { get; set; }
    public double Percentage { get; set; }
}

public class EngagementMetricsDto
{
    public double LikeRate { get; set; }
    public double CommentRate { get; set; }
    public double ShareRate { get; set; }
    public double SubscriptionRate { get; set; }
    public TimeSpan AverageWatchTime { get; set; }
    public double RetentionRate { get; set; }
    public List<EngagementTrendDto> Trends { get; set; } = new();
}

public class EngagementTrendDto
{
    public DateTime Date { get; set; }
    public double LikeRate { get; set; }
    public double CommentRate { get; set; }
    public double ShareRate { get; set; }
    public double SubscriptionRate { get; set; }
}
