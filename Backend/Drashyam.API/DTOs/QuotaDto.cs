namespace Drashyam.API.DTOs;

public class QuotaStatusDto
{
    public string UserId { get; set; } = string.Empty;
    public string SubscriptionType { get; set; } = string.Empty;
    public long StorageUsed { get; set; }
    public long StorageLimit { get; set; }
    public int VideosUploaded { get; set; }
    public int VideoLimit { get; set; }
    public int ChannelsCreated { get; set; }
    public int ChannelLimit { get; set; }
    public bool HasAds { get; set; }
    public bool HasAnalytics { get; set; }
    public bool HasMonetization { get; set; }
    public bool HasLiveStreaming { get; set; }
    public double StorageUsagePercentage { get; set; }
    public double VideoUsagePercentage { get; set; }
    public double ChannelUsagePercentage { get; set; }
}

public class QuotaWarningDto
{
    public string UserId { get; set; } = string.Empty;
    public List<string> Warnings { get; set; } = new List<string>();
    public bool HasWarnings { get; set; }
    public string RecommendedAction { get; set; } = string.Empty;
}

public class SubscriptionBenefitsDto
{
    public SubscriptionPlanDto CurrentPlan { get; set; } = new SubscriptionPlanDto();
    public SubscriptionPlanDto? NextUpgradePlan { get; set; }
    public List<string> Benefits { get; set; } = new List<string>();
    public List<string> UpgradeBenefits { get; set; } = new List<string>();
    public QuotaStatusDto QuotaStatus { get; set; } = new QuotaStatusDto();
}

public class QuotaCheckDto
{
    public bool CanUpload { get; set; }
    public bool CanCreateChannel { get; set; }
    public bool CanUseFeature { get; set; }
    public string? Reason { get; set; }
    public QuotaWarningDto? Warnings { get; set; }
}
