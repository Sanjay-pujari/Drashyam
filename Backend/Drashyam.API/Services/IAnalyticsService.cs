using Drashyam.API.DTOs;

namespace Drashyam.API.Services;

public interface IAnalyticsService
{
    Task<AnalyticsDto> GetVideoAnalyticsAsync(int videoId, string userId, DateTime? startDate = null, DateTime? endDate = null);
    Task<AnalyticsDto> GetChannelAnalyticsAsync(int channelId, string userId, DateTime? startDate = null, DateTime? endDate = null);
    Task<AnalyticsDto> GetUserAnalyticsAsync(string userId, DateTime? startDate = null, DateTime? endDate = null);
    Task<List<RevenueDto>> GetRevenueAnalyticsAsync(string userId, DateTime? startDate = null, DateTime? endDate = null);
    Task<List<TopVideoDto>> GetTopVideosAsync(string userId, int count = 10);
    Task<List<AudienceInsightDto>> GetAudienceInsightsAsync(string userId);
    Task<List<GeographicDataDto>> GetGeographicDataAsync(string userId, DateTime? startDate = null, DateTime? endDate = null);
    Task<List<DeviceDataDto>> GetDeviceDataAsync(string userId, DateTime? startDate = null, DateTime? endDate = null);
    Task<EngagementMetricsDto> GetEngagementMetricsAsync(string userId, DateTime? startDate = null, DateTime? endDate = null);
    Task RecordVideoViewAsync(int videoId, string userId, string? country = null, string? deviceType = null, string? referrer = null);
    Task RecordAdImpressionAsync(int videoId, string userId, decimal revenue);
    Task RecordSubscriptionAsync(string userId, decimal amount, string planName);
}
