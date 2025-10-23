using Drashyam.API.DTOs;

namespace Drashyam.API.Services;

public interface IStreamAnalyticsService
{
    // Real-time Analytics
    Task<StreamAnalyticsDto> GetRealTimeAnalyticsAsync(int streamId);
    Task<StreamHealthDto> GetStreamHealthAsync(int streamId);
    Task<List<ViewerAnalyticsDto>> GetViewerAnalyticsAsync(int streamId, DateTime startTime, DateTime endTime);
    Task<List<QualityAnalyticsDto>> GetQualityAnalyticsAsync(int streamId, DateTime startTime, DateTime endTime);
    
    // Analytics Events
    Task RecordViewerJoinAsync(int streamId, string userId);
    Task RecordViewerLeaveAsync(int streamId, string userId);
    Task RecordChatMessageAsync(int streamId, string userId);
    Task RecordReactionAsync(int streamId, string userId, string reactionType);
    Task RecordStreamEventAsync(int streamId, string eventType, object eventData);
    
    // Analytics Reports
    Task<StreamReportDto> GenerateStreamReportAsync(int streamId, DateTime startTime, DateTime endTime);
    Task<List<StreamReportDto>> GenerateUserStreamReportsAsync(string userId, DateTime startTime, DateTime endTime);
    Task<StreamComparisonDto> CompareStreamsAsync(List<int> streamIds);
    
    // Analytics Dashboard
    Task<AnalyticsDashboardDto> GetStreamDashboardAsync(int streamId);
    Task<AnalyticsDashboardDto> GetUserDashboardAsync(string userId);
    Task<AnalyticsDashboardDto> GetGlobalDashboardAsync();
}
