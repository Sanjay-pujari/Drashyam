using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Drashyam.API.DTOs;
using Drashyam.API.Services;

namespace Drashyam.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class StreamAnalyticsController : ControllerBase
{
    private readonly IStreamAnalyticsService _analyticsService;
    private readonly ILogger<StreamAnalyticsController> _logger;

    public StreamAnalyticsController(IStreamAnalyticsService analyticsService, ILogger<StreamAnalyticsController> logger)
    {
        _analyticsService = analyticsService;
        _logger = logger;
    }

    [HttpGet("stream/{streamId:int}/realtime")]
    public async Task<ActionResult<StreamAnalyticsDto>> GetRealTimeAnalytics([FromRoute] int streamId)
    {
        try
        {
            var analytics = await _analyticsService.GetRealTimeAnalyticsAsync(streamId);
            return Ok(analytics);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error getting real-time analytics for stream {streamId}");
            return BadRequest(ex.Message);
        }
    }

    [HttpGet("stream/{streamId:int}/health")]
    public async Task<ActionResult<StreamHealthDto>> GetStreamHealth([FromRoute] int streamId)
    {
        try
        {
            var health = await _analyticsService.GetStreamHealthAsync(streamId);
            return Ok(health);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error getting stream health for stream {streamId}");
            return BadRequest(ex.Message);
        }
    }

    [HttpGet("stream/{streamId:int}/viewers")]
    public async Task<ActionResult<List<ViewerAnalyticsDto>>> GetViewerAnalytics(
        [FromRoute] int streamId, 
        [FromQuery] DateTime startTime, 
        [FromQuery] DateTime endTime)
    {
        try
        {
            var analytics = await _analyticsService.GetViewerAnalyticsAsync(streamId, startTime, endTime);
            return Ok(analytics);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error getting viewer analytics for stream {streamId}");
            return BadRequest(ex.Message);
        }
    }

    [HttpGet("stream/{streamId:int}/quality")]
    public async Task<ActionResult<List<QualityAnalyticsDto>>> GetQualityAnalytics(
        [FromRoute] int streamId, 
        [FromQuery] DateTime startTime, 
        [FromQuery] DateTime endTime)
    {
        try
        {
            var analytics = await _analyticsService.GetQualityAnalyticsAsync(streamId, startTime, endTime);
            return Ok(analytics);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error getting quality analytics for stream {streamId}");
            return BadRequest(ex.Message);
        }
    }

    [HttpPost("stream/{streamId:int}/viewer-join")]
    public async Task<ActionResult> RecordViewerJoin([FromRoute] int streamId, [FromBody] string userId)
    {
        try
        {
            await _analyticsService.RecordViewerJoinAsync(streamId, userId);
            return Ok();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error recording viewer join for stream {streamId}");
            return BadRequest(ex.Message);
        }
    }

    [HttpPost("stream/{streamId:int}/viewer-leave")]
    public async Task<ActionResult> RecordViewerLeave([FromRoute] int streamId, [FromBody] string userId)
    {
        try
        {
            await _analyticsService.RecordViewerLeaveAsync(streamId, userId);
            return Ok();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error recording viewer leave for stream {streamId}");
            return BadRequest(ex.Message);
        }
    }

    [HttpPost("stream/{streamId:int}/chat-message")]
    public async Task<ActionResult> RecordChatMessage([FromRoute] int streamId, [FromBody] string userId)
    {
        try
        {
            await _analyticsService.RecordChatMessageAsync(streamId, userId);
            return Ok();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error recording chat message for stream {streamId}");
            return BadRequest(ex.Message);
        }
    }

    [HttpPost("stream/{streamId:int}/reaction")]
    public async Task<ActionResult> RecordReaction([FromRoute] int streamId, [FromBody] RecordReactionDto dto)
    {
        try
        {
            await _analyticsService.RecordReactionAsync(streamId, dto.UserId, dto.ReactionType);
            return Ok();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error recording reaction for stream {streamId}");
            return BadRequest(ex.Message);
        }
    }

    [HttpPost("stream/{streamId:int}/event")]
    public async Task<ActionResult> RecordStreamEvent([FromRoute] int streamId, [FromBody] RecordEventDto dto)
    {
        try
        {
            await _analyticsService.RecordStreamEventAsync(streamId, dto.EventType, dto.EventData);
            return Ok();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error recording stream event for stream {streamId}");
            return BadRequest(ex.Message);
        }
    }

    [HttpGet("stream/{streamId:int}/report")]
    public async Task<ActionResult<StreamReportDto>> GenerateStreamReport(
        [FromRoute] int streamId, 
        [FromQuery] DateTime startTime, 
        [FromQuery] DateTime endTime)
    {
        try
        {
            var report = await _analyticsService.GenerateStreamReportAsync(streamId, startTime, endTime);
            return Ok(report);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error generating stream report for stream {streamId}");
            return BadRequest(ex.Message);
        }
    }

    [HttpGet("user/{userId}/reports")]
    public async Task<ActionResult<List<StreamReportDto>>> GenerateUserStreamReports(
        [FromRoute] string userId, 
        [FromQuery] DateTime startTime, 
        [FromQuery] DateTime endTime)
    {
        try
        {
            var reports = await _analyticsService.GenerateUserStreamReportsAsync(userId, startTime, endTime);
            return Ok(reports);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error generating user stream reports for user {userId}");
            return BadRequest(ex.Message);
        }
    }

    [HttpPost("compare")]
    public async Task<ActionResult<StreamComparisonDto>> CompareStreams([FromBody] List<int> streamIds)
    {
        try
        {
            var comparison = await _analyticsService.CompareStreamsAsync(streamIds);
            return Ok(comparison);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error comparing streams");
            return BadRequest(ex.Message);
        }
    }

    [HttpGet("stream/{streamId:int}/dashboard")]
    public async Task<ActionResult<AnalyticsDashboardDto>> GetStreamDashboard([FromRoute] int streamId)
    {
        try
        {
            var dashboard = await _analyticsService.GetStreamDashboardAsync(streamId);
            return Ok(dashboard);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error getting stream dashboard for stream {streamId}");
            return BadRequest(ex.Message);
        }
    }

    [HttpGet("user/{userId}/dashboard")]
    public async Task<ActionResult<AnalyticsDashboardDto>> GetUserDashboard([FromRoute] string userId)
    {
        try
        {
            var dashboard = await _analyticsService.GetUserDashboardAsync(userId);
            return Ok(dashboard);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error getting user dashboard for user {userId}");
            return BadRequest(ex.Message);
        }
    }

    [HttpGet("global/dashboard")]
    public async Task<ActionResult<AnalyticsDashboardDto>> GetGlobalDashboard()
    {
        try
        {
            var dashboard = await _analyticsService.GetGlobalDashboardAsync();
            return Ok(dashboard);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting global dashboard");
            return BadRequest(ex.Message);
        }
    }
}

public class RecordReactionDto
{
    public string UserId { get; set; } = string.Empty;
    public string ReactionType { get; set; } = string.Empty;
}

public class RecordEventDto
{
    public string EventType { get; set; } = string.Empty;
    public object EventData { get; set; } = new();
}
