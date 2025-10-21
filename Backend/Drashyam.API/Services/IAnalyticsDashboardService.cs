using Drashyam.API.DTOs;

namespace Drashyam.API.Services;

public interface IAnalyticsDashboardService
{
    // Dashboard Overview
    Task<AnalyticsSummaryDto> GetAnalyticsSummaryAsync(string userId, DateTime? startDate = null, DateTime? endDate = null);
    Task<List<TimeSeriesAnalyticsDto>> GetTimeSeriesDataAsync(string userId, DateTime? startDate = null, DateTime? endDate = null);
    Task<List<TopVideoAnalyticsDto>> GetTopVideosAsync(string userId, int count = 10, DateTime? startDate = null, DateTime? endDate = null);
    
    // Revenue Analytics
    Task<RevenueAnalyticsDto> GetRevenueAnalyticsAsync(string userId, DateTime? startDate = null, DateTime? endDate = null);
    Task<List<TimeSeriesAnalyticsDto>> GetRevenueTimeSeriesAsync(string userId, DateTime? startDate = null, DateTime? endDate = null);
    Task<decimal> GetTotalRevenueAsync(string userId, DateTime? startDate = null, DateTime? endDate = null);
    
    // Audience Analytics
    Task<List<GeographicAnalyticsDto>> GetGeographicDataAsync(string userId, DateTime? startDate = null, DateTime? endDate = null);
    Task<List<DeviceAnalyticsDto>> GetDeviceDataAsync(string userId, DateTime? startDate = null, DateTime? endDate = null);
    Task<List<ReferrerAnalyticsDto>> GetReferrerDataAsync(string userId, DateTime? startDate = null, DateTime? endDate = null);
    Task<List<AudienceAnalyticsDto>> GetAudienceInsightsAsync(string userId, DateTime? startDate = null, DateTime? endDate = null);
    
    // Engagement Analytics
    Task<EngagementAnalyticsDto> GetEngagementMetricsAsync(string userId, DateTime? startDate = null, DateTime? endDate = null);
    Task<List<TimeSeriesAnalyticsDto>> GetEngagementTimeSeriesAsync(string userId, DateTime? startDate = null, DateTime? endDate = null);
    
    // Video Analytics
    Task<VideoAnalyticsDto> GetVideoAnalyticsAsync(int videoId, string userId, DateTime? startDate = null, DateTime? endDate = null);
    Task<List<VideoAnalyticsDto>> GetVideoAnalyticsListAsync(string userId, DateTime? startDate = null, DateTime? endDate = null);
    
    // Channel Analytics
    Task<List<ChannelComparisonDto>> GetChannelComparisonAsync(string userId, DateTime? startDate = null, DateTime? endDate = null);
    Task<AnalyticsSummaryDto> GetChannelAnalyticsAsync(int channelId, string userId, DateTime? startDate = null, DateTime? endDate = null);
    
    // Data Tracking
    Task TrackViewAsync(int videoId, string userId, string? country = null, string? deviceType = null, string? referrer = null);
    Task TrackEngagementAsync(int videoId, string userId, string engagementType, decimal value = 1);
    Task TrackRevenueAsync(string userId, decimal amount, string revenueType, int? videoId = null, int? channelId = null);
    
    // Real-time Analytics
    Task<AnalyticsSummaryDto> GetRealTimeAnalyticsAsync(string userId);
    Task<List<VideoAnalyticsDto>> GetRealTimeVideoAnalyticsAsync(string userId);
    
    // Export and Reporting
    Task<byte[]> ExportAnalyticsReportAsync(string userId, DateTime startDate, DateTime endDate, string format = "csv");
    Task<AnalyticsDashboardDto> GenerateDashboardReportAsync(string userId, DateTime? startDate = null, DateTime? endDate = null);
}
