using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Drashyam.API.DTOs;
using Drashyam.API.Services;

namespace Drashyam.API.Controllers;

[ApiController]
[Route("api/analytics")]
[Authorize]
public class AnalyticsDashboardController : ControllerBase
{
    private readonly IAnalyticsDashboardService _analyticsService;
    private readonly IQuotaService _quotaService;
    private readonly ILogger<AnalyticsDashboardController> _logger;

    public AnalyticsDashboardController(
        IAnalyticsDashboardService analyticsService,
        IQuotaService quotaService,
        ILogger<AnalyticsDashboardController> logger)
    {
        _analyticsService = analyticsService;
        _quotaService = quotaService;
        _logger = logger;
    }

    // Helper method to convert DateTime parameters to UTC for PostgreSQL compatibility
    private static (DateTime? utcStartDate, DateTime? utcEndDate) ConvertToUtc(DateTime? startDate, DateTime? endDate)
    {
        var utcStartDate = startDate?.Kind == DateTimeKind.Unspecified ? DateTime.SpecifyKind(startDate.Value, DateTimeKind.Utc) : startDate;
        var utcEndDate = endDate?.Kind == DateTimeKind.Unspecified ? DateTime.SpecifyKind(endDate.Value, DateTimeKind.Utc) : endDate;
        return (utcStartDate, utcEndDate);
    }

    // Helper method to check if user has analytics access
    private async Task<bool> HasAnalyticsAccessAsync(string userId)
    {
        try
        {
            return await _quotaService.CheckSubscriptionFeaturesAsync(userId, "analytics");
        }
        catch
        {
            return false;
        }
    }

    [HttpGet("summary")]
    public async Task<ActionResult<AnalyticsSummaryDto>> GetSummary(
        [FromQuery] DateTime? startDate = null,
        [FromQuery] DateTime? endDate = null)
    {
        try
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
            
            // Check if user has analytics access
            if (!await HasAnalyticsAccessAsync(userId))
            {
                return Forbid("Analytics access requires a paid subscription");
            }
            
            var (utcStartDate, utcEndDate) = ConvertToUtc(startDate, endDate);
            
            var summary = await _analyticsService.GetAnalyticsSummaryAsync(userId, utcStartDate, utcEndDate);
            return Ok(summary);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting analytics summary");
            return StatusCode(500, "An error occurred while retrieving analytics summary");
        }
    }

    [HttpGet("time-series")]
    public async Task<ActionResult<List<TimeSeriesAnalyticsDto>>> GetTimeSeries(
        [FromQuery] DateTime? startDate = null,
        [FromQuery] DateTime? endDate = null)
    {
        try
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
            
            // Check if user has analytics access
            if (!await HasAnalyticsAccessAsync(userId))
            {
                return Forbid("Analytics access requires a paid subscription");
            }
            
            var (utcStartDate, utcEndDate) = ConvertToUtc(startDate, endDate);
            var timeSeries = await _analyticsService.GetTimeSeriesDataAsync(userId, utcStartDate, utcEndDate);
            return Ok(timeSeries);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting time series data");
            return StatusCode(500, "An error occurred while retrieving time series data");
        }
    }

    [HttpGet("top-videos")]
    public async Task<ActionResult<List<TopVideoAnalyticsDto>>> GetTopVideos(
        [FromQuery] int count = 10,
        [FromQuery] DateTime? startDate = null,
        [FromQuery] DateTime? endDate = null)
    {
        try
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
            var (utcStartDate, utcEndDate) = ConvertToUtc(startDate, endDate);
            var topVideos = await _analyticsService.GetTopVideosAsync(userId, count, utcStartDate, utcEndDate);
            return Ok(topVideos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting top videos");
            return StatusCode(500, "An error occurred while retrieving top videos");
        }
    }

    [HttpGet("revenue")]
    public async Task<ActionResult<RevenueAnalyticsDto>> GetRevenue(
        [FromQuery] DateTime? startDate = null,
        [FromQuery] DateTime? endDate = null)
    {
        try
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
            var (utcStartDate, utcEndDate) = ConvertToUtc(startDate, endDate);
            var revenue = await _analyticsService.GetRevenueAnalyticsAsync(userId, utcStartDate, utcEndDate);
            return Ok(revenue);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting revenue analytics");
            return StatusCode(500, "An error occurred while retrieving revenue analytics");
        }
    }

    [HttpGet("revenue/time-series")]
    public async Task<ActionResult<List<TimeSeriesAnalyticsDto>>> GetRevenueTimeSeries(
        [FromQuery] DateTime? startDate = null,
        [FromQuery] DateTime? endDate = null)
    {
        try
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
            var (utcStartDate, utcEndDate) = ConvertToUtc(startDate, endDate);
            var revenueTimeSeries = await _analyticsService.GetRevenueTimeSeriesAsync(userId, utcStartDate, utcEndDate);
            return Ok(revenueTimeSeries);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting revenue time series");
            return StatusCode(500, "An error occurred while retrieving revenue time series");
        }
    }

    [HttpGet("revenue/total")]
    public async Task<ActionResult<decimal>> GetTotalRevenue(
        [FromQuery] DateTime? startDate = null,
        [FromQuery] DateTime? endDate = null)
    {
        try
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
            var (utcStartDate, utcEndDate) = ConvertToUtc(startDate, endDate);
            var totalRevenue = await _analyticsService.GetTotalRevenueAsync(userId, utcStartDate, utcEndDate);
            return Ok(totalRevenue);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting total revenue");
            return StatusCode(500, "An error occurred while retrieving total revenue");
        }
    }

    [HttpGet("geographic")]
    public async Task<ActionResult<List<GeographicAnalyticsDto>>> GetGeographicData(
        [FromQuery] DateTime? startDate = null,
        [FromQuery] DateTime? endDate = null)
    {
        try
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
            var (utcStartDate, utcEndDate) = ConvertToUtc(startDate, endDate);
            var geographicData = await _analyticsService.GetGeographicDataAsync(userId, utcStartDate, utcEndDate);
            return Ok(geographicData);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting geographic data");
            return StatusCode(500, "An error occurred while retrieving geographic data");
        }
    }

    [HttpGet("devices")]
    public async Task<ActionResult<List<DeviceAnalyticsDto>>> GetDeviceData(
        [FromQuery] DateTime? startDate = null,
        [FromQuery] DateTime? endDate = null)
    {
        try
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
            var (utcStartDate, utcEndDate) = ConvertToUtc(startDate, endDate);
            var deviceData = await _analyticsService.GetDeviceDataAsync(userId, utcStartDate, utcEndDate);
            return Ok(deviceData);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting device data");
            return StatusCode(500, "An error occurred while retrieving device data");
        }
    }

    [HttpGet("referrers")]
    public async Task<ActionResult<List<ReferrerAnalyticsDto>>> GetReferrerData(
        [FromQuery] DateTime? startDate = null,
        [FromQuery] DateTime? endDate = null)
    {
        try
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
            var (utcStartDate, utcEndDate) = ConvertToUtc(startDate, endDate);
            var referrerData = await _analyticsService.GetReferrerDataAsync(userId, utcStartDate, utcEndDate);
            return Ok(referrerData);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting referrer data");
            return StatusCode(500, "An error occurred while retrieving referrer data");
        }
    }

    [HttpGet("audience")]
    public async Task<ActionResult<List<AudienceAnalyticsDto>>> GetAudienceInsights(
        [FromQuery] DateTime? startDate = null,
        [FromQuery] DateTime? endDate = null)
    {
        try
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
            var (utcStartDate, utcEndDate) = ConvertToUtc(startDate, endDate);
            var audienceData = await _analyticsService.GetAudienceInsightsAsync(userId, utcStartDate, utcEndDate);
            return Ok(audienceData);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting audience insights");
            return StatusCode(500, "An error occurred while retrieving audience insights");
        }
    }

    [HttpGet("engagement")]
    public async Task<ActionResult<EngagementAnalyticsDto>> GetEngagementMetrics(
        [FromQuery] DateTime? startDate = null,
        [FromQuery] DateTime? endDate = null)
    {
        try
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
            var (utcStartDate, utcEndDate) = ConvertToUtc(startDate, endDate);
            var engagement = await _analyticsService.GetEngagementMetricsAsync(userId, utcStartDate, utcEndDate);
            return Ok(engagement);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting engagement metrics");
            return StatusCode(500, "An error occurred while retrieving engagement metrics");
        }
    }

    [HttpGet("engagement/time-series")]
    public async Task<ActionResult<List<TimeSeriesAnalyticsDto>>> GetEngagementTimeSeries(
        [FromQuery] DateTime? startDate = null,
        [FromQuery] DateTime? endDate = null)
    {
        try
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
            var (utcStartDate, utcEndDate) = ConvertToUtc(startDate, endDate);
            var engagementTimeSeries = await _analyticsService.GetEngagementTimeSeriesAsync(userId, utcStartDate, utcEndDate);
            return Ok(engagementTimeSeries);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting engagement time series");
            return StatusCode(500, "An error occurred while retrieving engagement time series");
        }
    }

    [HttpGet("videos/{videoId:int}")]
    public async Task<ActionResult<VideoAnalyticsDto>> GetVideoAnalytics(
        [FromRoute] int videoId,
        [FromQuery] DateTime? startDate = null,
        [FromQuery] DateTime? endDate = null)
    {
        try
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
            var (utcStartDate, utcEndDate) = ConvertToUtc(startDate, endDate);
            var videoAnalytics = await _analyticsService.GetVideoAnalyticsAsync(videoId, userId, utcStartDate, utcEndDate);
            return Ok(videoAnalytics);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting video analytics for video {VideoId}", videoId);
            return StatusCode(500, "An error occurred while retrieving video analytics");
        }
    }

    [HttpGet("videos")]
    public async Task<ActionResult<List<VideoAnalyticsDto>>> GetVideoAnalyticsList(
        [FromQuery] DateTime? startDate = null,
        [FromQuery] DateTime? endDate = null)
    {
        try
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
            var (utcStartDate, utcEndDate) = ConvertToUtc(startDate, endDate);
            var videoAnalytics = await _analyticsService.GetVideoAnalyticsListAsync(userId, utcStartDate, utcEndDate);
            return Ok(videoAnalytics);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting video analytics list");
            return StatusCode(500, "An error occurred while retrieving video analytics list");
        }
    }

    [HttpGet("channels/comparison")]
    public async Task<ActionResult<List<ChannelComparisonDto>>> GetChannelComparison(
        [FromQuery] DateTime? startDate = null,
        [FromQuery] DateTime? endDate = null)
    {
        try
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
            var (utcStartDate, utcEndDate) = ConvertToUtc(startDate, endDate);
            var channelComparison = await _analyticsService.GetChannelComparisonAsync(userId, utcStartDate, utcEndDate);
            return Ok(channelComparison);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting channel comparison");
            return StatusCode(500, "An error occurred while retrieving channel comparison");
        }
    }

    [HttpGet("channels/{channelId:int}")]
    public async Task<ActionResult<AnalyticsSummaryDto>> GetChannelAnalytics(
        [FromRoute] int channelId,
        [FromQuery] DateTime? startDate = null,
        [FromQuery] DateTime? endDate = null)
    {
        try
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
            var (utcStartDate, utcEndDate) = ConvertToUtc(startDate, endDate);
            var channelAnalytics = await _analyticsService.GetChannelAnalyticsAsync(channelId, userId, utcStartDate, utcEndDate);
            return Ok(channelAnalytics);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting channel analytics for channel {ChannelId}", channelId);
            return StatusCode(500, "An error occurred while retrieving channel analytics");
        }
    }

    [HttpGet("real-time")]
    public async Task<ActionResult<AnalyticsSummaryDto>> GetRealTimeAnalytics()
    {
        try
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
            var realTimeAnalytics = await _analyticsService.GetRealTimeAnalyticsAsync(userId);
            return Ok(realTimeAnalytics);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting real-time analytics");
            return StatusCode(500, "An error occurred while retrieving real-time analytics");
        }
    }

    [HttpGet("real-time/videos")]
    public async Task<ActionResult<List<VideoAnalyticsDto>>> GetRealTimeVideoAnalytics()
    {
        try
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
            var realTimeVideoAnalytics = await _analyticsService.GetRealTimeVideoAnalyticsAsync(userId);
            return Ok(realTimeVideoAnalytics);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting real-time video analytics");
            return StatusCode(500, "An error occurred while retrieving real-time video analytics");
        }
    }

    [HttpPost("track/view")]
    public async Task<IActionResult> TrackView(
        [FromBody] TrackViewRequest request)
    {
        try
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
            await _analyticsService.TrackViewAsync(
                request.VideoId,
                userId,
                request.Country,
                request.DeviceType,
                request.Referrer);

            return Ok(new { message = "View tracked successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error tracking view");
            return StatusCode(500, "An error occurred while tracking view");
        }
    }

    [HttpPost("track/engagement")]
    public async Task<IActionResult> TrackEngagement(
        [FromBody] TrackEngagementRequest request)
    {
        try
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
            await _analyticsService.TrackEngagementAsync(
                request.VideoId,
                userId,
                request.EngagementType,
                request.Value);

            return Ok(new { message = "Engagement tracked successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error tracking engagement");
            return StatusCode(500, "An error occurred while tracking engagement");
        }
    }

    [HttpPost("track/revenue")]
    public async Task<IActionResult> TrackRevenue(
        [FromBody] TrackRevenueRequest request)
    {
        try
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
            await _analyticsService.TrackRevenueAsync(
                userId,
                request.Amount,
                request.RevenueType,
                request.VideoId,
                request.ChannelId);

            return Ok(new { message = "Revenue tracked successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error tracking revenue");
            return StatusCode(500, "An error occurred while tracking revenue");
        }
    }

    [HttpGet("export")]
    public async Task<IActionResult> ExportAnalyticsReport(
        [FromQuery] DateTime startDate,
        [FromQuery] DateTime endDate,
        [FromQuery] string format = "csv")
    {
        try
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
            
            // Convert DateTime parameters to UTC for PostgreSQL compatibility
            var utcStartDate = startDate.Kind == DateTimeKind.Unspecified ? DateTime.SpecifyKind(startDate, DateTimeKind.Utc) : startDate;
            var utcEndDate = endDate.Kind == DateTimeKind.Unspecified ? DateTime.SpecifyKind(endDate, DateTimeKind.Utc) : endDate;
            
            var reportData = await _analyticsService.ExportAnalyticsReportAsync(userId, utcStartDate, utcEndDate, format);

            var contentType = format.ToLower() switch
            {
                "csv" => "text/csv",
                "json" => "application/json",
                "xlsx" => "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                _ => "application/octet-stream"
            };

            var fileName = $"analytics-report-{utcStartDate:yyyy-MM-dd}-to-{utcEndDate:yyyy-MM-dd}.{format}";

            return File(reportData, contentType, fileName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error exporting analytics report");
            return StatusCode(500, "An error occurred while exporting analytics report");
        }
    }

    [HttpGet("dashboard")]
    public async Task<ActionResult<AnalyticsDashboardDto>> GetDashboardReport(
        [FromQuery] DateTime? startDate = null,
        [FromQuery] DateTime? endDate = null)
    {
        try
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
            var (utcStartDate, utcEndDate) = ConvertToUtc(startDate, endDate);
            var dashboardReport = await _analyticsService.GenerateDashboardReportAsync(userId, utcStartDate, utcEndDate);
            return Ok(dashboardReport);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating dashboard report");
            return StatusCode(500, "An error occurred while generating dashboard report");
        }
    }
}

// Request DTOs for tracking
public class TrackViewRequest
{
    public int VideoId { get; set; }
    public string? Country { get; set; }
    public string? DeviceType { get; set; }
    public string? Referrer { get; set; }
}

public class TrackEngagementRequest
{
    public int VideoId { get; set; }
    public string EngagementType { get; set; } = string.Empty;
    public decimal Value { get; set; } = 1;
}

public class TrackRevenueRequest
{
    public decimal Amount { get; set; }
    public string RevenueType { get; set; } = string.Empty;
    public int? VideoId { get; set; }
    public int? ChannelId { get; set; }
}
