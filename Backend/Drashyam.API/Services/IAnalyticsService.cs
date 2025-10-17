using Drashyam.API.DTOs;
using Drashyam.API.Models;

namespace Drashyam.API.Services;

public interface IAnalyticsService
{
    Task TrackInviteEventAsync(string userId, InviteEventType eventType, int? inviteId = null, string? details = null, string? source = null);
    Task TrackReferralEventAsync(string userId, ReferralEventType eventType, int? referralId = null, string? details = null, string? source = null);
    Task UpdateInviteAnalyticsAsync(string userId, DateTime date);
    Task UpdateReferralAnalyticsAsync(string userId, DateTime date);
    Task<InviteAnalyticsDto> GetInviteAnalyticsAsync(string userId, DateTime? startDate = null, DateTime? endDate = null);
    Task<ReferralAnalyticsDto> GetReferralAnalyticsAsync(string userId, DateTime? startDate = null, DateTime? endDate = null);
    Task<InviteAnalyticsDto> GetInviteAnalyticsSummaryAsync(string userId);
    Task<ReferralAnalyticsDto> GetReferralAnalyticsSummaryAsync(string userId);
    Task<List<InviteEventDto>> GetInviteEventsAsync(string userId, int page = 1, int pageSize = 20);
    Task<List<ReferralEventDto>> GetReferralEventsAsync(string userId, int page = 1, int pageSize = 20);
    
    // Video Analytics Methods
    Task<AnalyticsDto> GetVideoAnalyticsAsync(int videoId, string userId, DateTime? startDate = null, DateTime? endDate = null);
    Task<AnalyticsDto> GetChannelAnalyticsAsync(int channelId, string userId, DateTime? startDate = null, DateTime? endDate = null);
    Task<RevenueDto> GetRevenueAnalyticsAsync(string userId, DateTime? startDate = null, DateTime? endDate = null);
    Task<List<TopVideoDto>> GetTopVideosAsync(string userId, int count = 10, DateTime? startDate = null, DateTime? endDate = null);
    Task<List<AudienceInsightDto>> GetAudienceInsightsAsync(string userId, DateTime? startDate = null, DateTime? endDate = null);
    Task<List<GeographicDataDto>> GetGeographicDataAsync(string userId, DateTime? startDate = null, DateTime? endDate = null);
    Task<List<DeviceDataDto>> GetDeviceDataAsync(string userId, DateTime? startDate = null, DateTime? endDate = null);
    Task<EngagementMetricsDto> GetEngagementMetricsAsync(string userId, DateTime? startDate = null, DateTime? endDate = null);
    Task<AnalyticsDto> GetUserAnalyticsAsync(string userId, DateTime? startDate = null, DateTime? endDate = null);
}