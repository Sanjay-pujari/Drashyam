namespace Drashyam.API.DTOs;

public class AnalyticsDto
{
    public int VideoId { get; set; }
    public string Title { get; set; } = string.Empty;
    public int Views { get; set; }
    public int Likes { get; set; }
    public int Comments { get; set; }
    public int Shares { get; set; }
    public decimal Revenue { get; set; }
    public TimeSpan WatchTime { get; set; }
    public decimal EngagementRate { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class RevenueDto
{
    public decimal TotalRevenue { get; set; }
    public decimal AdRevenue { get; set; }
    public decimal SubscriptionRevenue { get; set; }
    public decimal DonationRevenue { get; set; }
    public DateTime Period { get; set; }
}

public class TopVideoDto
{
    public int VideoId { get; set; }
    public string Title { get; set; } = string.Empty;
    public int Views { get; set; }
    public decimal Revenue { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class AudienceInsightDto
{
    public string AgeGroup { get; set; } = string.Empty;
    public string Gender { get; set; } = string.Empty;
    public int Count { get; set; }
    public decimal Percentage { get; set; }
}

public class GeographicDataDto
{
    public string Country { get; set; } = string.Empty;
    public int Views { get; set; }
    public decimal Revenue { get; set; }
    public decimal Percentage { get; set; }
}

public class DeviceDataDto
{
    public string DeviceType { get; set; } = string.Empty;
    public int Views { get; set; }
    public decimal Percentage { get; set; }
}

public class EngagementMetricsDto
{
    public decimal LikeRate { get; set; }
    public decimal CommentRate { get; set; }
    public decimal ShareRate { get; set; }
    public decimal WatchTimeRate { get; set; }
    public decimal ClickThroughRate { get; set; }
}
