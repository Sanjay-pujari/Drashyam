using AutoMapper;
using Drashyam.API.Data;
using Drashyam.API.DTOs;
using Drashyam.API.Models;
using Microsoft.EntityFrameworkCore;

namespace Drashyam.API.Services;

public class QuotaService : IQuotaService
{
    private readonly DrashyamDbContext _context;
    private readonly IMapper _mapper;
    private readonly ILogger<QuotaService> _logger;

    public QuotaService(DrashyamDbContext context, IMapper mapper, ILogger<QuotaService> logger)
    {
        _context = context;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<QuotaStatusDto> GetUserQuotaStatusAsync(string userId)
    {
        var user = await _context.Users
            .Include(u => u.Channels)
            .FirstOrDefaultAsync(u => u.Id == userId);

        if (user == null)
            throw new ArgumentException("User not found");

        // Get user's active subscription
        var subscription = await _context.Subscriptions
            .Include(s => s.Plan)
            .FirstOrDefaultAsync(s => s.UserId == userId && s.Status == SubscriptionStatus.Active);

        var plan = subscription?.Plan ?? GetDefaultFreePlan();

        // Calculate current usage
        var totalStorageUsed = await _context.Videos
            .Where(v => v.UserId == userId)
            .SumAsync(v => v.FileSize);

        var totalVideos = await _context.Videos
            .Where(v => v.UserId == userId)
            .CountAsync();

        var totalChannels = await _context.Channels
            .Where(c => c.UserId == userId)
            .CountAsync();

        return new QuotaStatusDto
        {
            UserId = userId,
            SubscriptionType = user.SubscriptionType.ToString(),
            StorageUsed = totalStorageUsed,
            StorageLimit = plan.MaxStorageGB * 1024 * 1024 * 1024, // Convert GB to bytes
            VideosUploaded = totalVideos,
            VideoLimit = plan.MaxVideosPerChannel * totalChannels,
            ChannelsCreated = totalChannels,
            ChannelLimit = plan.MaxChannels,
            HasAds = plan.HasAds,
            HasAnalytics = plan.HasAnalytics,
            HasMonetization = plan.HasMonetization,
            HasLiveStreaming = plan.HasLiveStreaming,
            StorageUsagePercentage = (double)totalStorageUsed / (plan.MaxStorageGB * 1024 * 1024 * 1024) * 100,
            VideoUsagePercentage = totalChannels > 0 ? (double)totalVideos / (plan.MaxVideosPerChannel * totalChannels) * 100 : 0,
            ChannelUsagePercentage = (double)totalChannels / plan.MaxChannels * 100
        };
    }

    public async Task<bool> CheckStorageQuotaAsync(string userId, long fileSize)
    {
        var quotaStatus = await GetUserQuotaStatusAsync(userId);
        return (quotaStatus.StorageUsed + fileSize) <= quotaStatus.StorageLimit;
    }

    public async Task<bool> CheckVideoQuotaAsync(int channelId)
    {
        var channel = await _context.Channels.FindAsync(channelId);
        if (channel == null) return false;

        var currentVideoCount = await _context.Videos
            .CountAsync(v => v.ChannelId == channelId);

        return currentVideoCount < channel.MaxVideos;
    }

    public async Task<bool> CheckChannelQuotaAsync(string userId)
    {
        var quotaStatus = await GetUserQuotaStatusAsync(userId);
        return quotaStatus.ChannelsCreated < quotaStatus.ChannelLimit;
    }

    public async Task<bool> CheckSubscriptionFeaturesAsync(string userId, string feature)
    {
        var quotaStatus = await GetUserQuotaStatusAsync(userId);
        
        return feature switch
        {
            "analytics" => quotaStatus.HasAnalytics,
            "monetization" => quotaStatus.HasMonetization,
            "live_streaming" => quotaStatus.HasLiveStreaming,
            "ads" => quotaStatus.HasAds,
            _ => false
        };
    }

    public async Task<QuotaWarningDto> GetQuotaWarningsAsync(string userId)
    {
        var quotaStatus = await GetUserQuotaStatusAsync(userId);
        var warnings = new List<string>();

        if (quotaStatus.StorageUsagePercentage >= 90)
            warnings.Add($"Storage quota: {quotaStatus.StorageUsagePercentage:F1}% used");

        if (quotaStatus.VideoUsagePercentage >= 90)
            warnings.Add($"Video quota: {quotaStatus.VideoUsagePercentage:F1}% used");

        if (quotaStatus.ChannelUsagePercentage >= 90)
            warnings.Add($"Channel quota: {quotaStatus.ChannelUsagePercentage:F1}% used");

        return new QuotaWarningDto
        {
            UserId = userId,
            Warnings = warnings,
            HasWarnings = warnings.Any(),
            RecommendedAction = GetRecommendedAction(quotaStatus)
        };
    }

    public async Task<bool> CanUploadVideoAsync(string userId, int channelId, long fileSize)
    {
        return await CheckStorageQuotaAsync(userId, fileSize) && 
               await CheckVideoQuotaAsync(channelId);
    }

    public async Task<bool> CanCreateChannelAsync(string userId)
    {
        return await CheckChannelQuotaAsync(userId);
    }

    public async Task<SubscriptionBenefitsDto> GetSubscriptionBenefitsAsync(string userId)
    {
        var quotaStatus = await GetUserQuotaStatusAsync(userId);
        var subscriptionType = Enum.Parse<DTOs.SubscriptionType>(quotaStatus.SubscriptionType);
        var currentPlan = GetPlanBySubscriptionType(subscriptionType);
        var nextPlan = GetNextUpgradePlan(subscriptionType);

        return new SubscriptionBenefitsDto
        {
            CurrentPlan = _mapper.Map<SubscriptionPlanDto>(currentPlan),
            NextUpgradePlan = nextPlan != null ? _mapper.Map<SubscriptionPlanDto>(nextPlan) : null,
            Benefits = GetPlanBenefits(currentPlan),
            UpgradeBenefits = nextPlan != null ? GetPlanBenefits(nextPlan) : new List<string>(),
            QuotaStatus = quotaStatus
        };
    }

    private SubscriptionPlan GetDefaultFreePlan()
    {
        return new SubscriptionPlan
        {
            Name = "Free",
            MaxChannels = 1,
            MaxVideosPerChannel = 10,
            MaxStorageGB = 1,
            HasAds = true,
            HasAnalytics = false,
            HasMonetization = false,
            HasLiveStreaming = false
        };
    }

    private SubscriptionPlan GetPlanBySubscriptionType(DTOs.SubscriptionType type)
    {
        return type switch
        {
            DTOs.SubscriptionType.Free => GetDefaultFreePlan(),
            DTOs.SubscriptionType.Premium => new SubscriptionPlan
            {
                Name = "Premium",
                MaxChannels = 3,
                MaxVideosPerChannel = 100,
                MaxStorageGB = 50,
                HasAds = false,
                HasAnalytics = true,
                HasMonetization = false,
                HasLiveStreaming = true
            },
            DTOs.SubscriptionType.Pro => new SubscriptionPlan
            {
                Name = "Pro",
                MaxChannels = 10,
                MaxVideosPerChannel = 1000,
                MaxStorageGB = 500,
                HasAds = false,
                HasAnalytics = true,
                HasMonetization = true,
                HasLiveStreaming = true
            },
            _ => GetDefaultFreePlan()
        };
    }

    private SubscriptionPlan? GetNextUpgradePlan(DTOs.SubscriptionType currentType)
    {
        return currentType switch
        {
            DTOs.SubscriptionType.Free => GetPlanBySubscriptionType(DTOs.SubscriptionType.Premium),
            DTOs.SubscriptionType.Premium => GetPlanBySubscriptionType(DTOs.SubscriptionType.Pro),
            _ => null
        };
    }

    private List<string> GetPlanBenefits(SubscriptionPlan plan)
    {
        var benefits = new List<string>
        {
            $"{plan.MaxChannels} channels",
            $"{plan.MaxVideosPerChannel} videos per channel",
            $"{plan.MaxStorageGB}GB storage"
        };

        if (!plan.HasAds) benefits.Add("Ad-free experience");
        if (plan.HasAnalytics) benefits.Add("Advanced analytics");
        if (plan.HasMonetization) benefits.Add("Monetization tools");
        if (plan.HasLiveStreaming) benefits.Add("Live streaming");

        return benefits;
    }

    private string GetRecommendedAction(QuotaStatusDto quotaStatus)
    {
        if (quotaStatus.StorageUsagePercentage >= 90 || 
            quotaStatus.VideoUsagePercentage >= 90 || 
            quotaStatus.ChannelUsagePercentage >= 90)
        {
            return quotaStatus.SubscriptionType == "Free" ? 
                "Upgrade to Premium for more storage and features" :
                "Upgrade to Pro for unlimited resources";
        }

        return "Your current plan is sufficient";
    }
}
