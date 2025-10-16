using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Drashyam.API.DTOs;
using Drashyam.API.Services;

namespace Drashyam.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AnalyticsController : ControllerBase
{
    private readonly IAnalyticsService _analyticsService;
    private readonly ILogger<AnalyticsController> _logger;

    public AnalyticsController(IAnalyticsService analyticsService, ILogger<AnalyticsController> logger)
    {
        _analyticsService = analyticsService;
        _logger = logger;
    }

    [HttpGet("video/{videoId:int}")]
    [Authorize]
    public async Task<ActionResult<AnalyticsDto>> Video([FromRoute] int videoId, [FromQuery] DateTime? startDate = null, [FromQuery] DateTime? endDate = null)
    {
        var userId = User.Identity?.Name ?? string.Empty;
        var analytics = await _analyticsService.GetVideoAnalyticsAsync(videoId, userId, startDate, endDate);
        return Ok(analytics);
    }

    [HttpGet("channel/{channelId:int}")]
    [Authorize]
    public async Task<ActionResult<AnalyticsDto>> Channel([FromRoute] int channelId, [FromQuery] DateTime? startDate = null, [FromQuery] DateTime? endDate = null)
    {
        var userId = User.Identity?.Name ?? string.Empty;
        var analytics = await _analyticsService.GetChannelAnalyticsAsync(channelId, userId, startDate, endDate);
        return Ok(analytics);
    }

    [HttpGet("me")]
    [Authorize]
    public async Task<ActionResult<AnalyticsDto>> Me([FromQuery] DateTime? startDate = null, [FromQuery] DateTime? endDate = null)
    {
        var userId = User.Identity?.Name ?? string.Empty;
        var analytics = await _analyticsService.GetUserAnalyticsAsync(userId, startDate, endDate);
        return Ok(analytics);
    }

    [HttpGet("revenue")]
    [Authorize]
    public async Task<ActionResult<List<RevenueDto>>> Revenue([FromQuery] DateTime? startDate = null, [FromQuery] DateTime? endDate = null)
    {
        var userId = User.Identity?.Name ?? string.Empty;
        var data = await _analyticsService.GetRevenueAnalyticsAsync(userId, startDate, endDate);
        return Ok(data);
    }

    [HttpGet("top-videos")]
    [Authorize]
    public async Task<ActionResult<List<TopVideoDto>>> TopVideos([FromQuery] int count = 10)
    {
        var userId = User.Identity?.Name ?? string.Empty;
        var data = await _analyticsService.GetTopVideosAsync(userId, count);
        return Ok(data);
    }

    [HttpGet("audience")]
    [Authorize]
    public async Task<ActionResult<List<AudienceInsightDto>>> Audience()
    {
        var userId = User.Identity?.Name ?? string.Empty;
        var data = await _analyticsService.GetAudienceInsightsAsync(userId);
        return Ok(data);
    }

    [HttpGet("geographic")]
    [Authorize]
    public async Task<ActionResult<List<GeographicDataDto>>> Geographic([FromQuery] DateTime? startDate = null, [FromQuery] DateTime? endDate = null)
    {
        var userId = User.Identity?.Name ?? string.Empty;
        var data = await _analyticsService.GetGeographicDataAsync(userId, startDate, endDate);
        return Ok(data);
    }

    [HttpGet("devices")]
    [Authorize]
    public async Task<ActionResult<List<DeviceDataDto>>> Devices([FromQuery] DateTime? startDate = null, [FromQuery] DateTime? endDate = null)
    {
        var userId = User.Identity?.Name ?? string.Empty;
        var data = await _analyticsService.GetDeviceDataAsync(userId, startDate, endDate);
        return Ok(data);
    }

    [HttpGet("engagement")]
    [Authorize]
    public async Task<ActionResult<EngagementMetricsDto>> Engagement([FromQuery] DateTime? startDate = null, [FromQuery] DateTime? endDate = null)
    {
        var userId = User.Identity?.Name ?? string.Empty;
        var data = await _analyticsService.GetEngagementMetricsAsync(userId, startDate, endDate);
        return Ok(data);
    }
}


