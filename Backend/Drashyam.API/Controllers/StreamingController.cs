using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Drashyam.API.DTOs;
using Drashyam.API.Services;

namespace Drashyam.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class StreamingController : ControllerBase
{
    private readonly IStreamingService _streamingService;
    private readonly ILogger<StreamingController> _logger;

    public StreamingController(IStreamingService streamingService, ILogger<StreamingController> logger)
    {
        _streamingService = streamingService;
        _logger = logger;
    }

    [HttpPost("{streamId:int}/start")]
    public async Task<ActionResult<StreamInfoDto>> StartStream([FromRoute] int streamId)
    {
        try
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
            var streamInfo = await _streamingService.StartStreamAsync(streamId, userId);
            return Ok(streamInfo);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error starting stream {streamId}");
            return BadRequest(ex.Message);
        }
    }

    [HttpPost("{streamId:int}/stop")]
    public async Task<ActionResult<StreamInfoDto>> StopStream([FromRoute] int streamId)
    {
        try
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
            var streamInfo = await _streamingService.StopStreamAsync(streamId, userId);
            return Ok(streamInfo);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error stopping stream {streamId}");
            return BadRequest(ex.Message);
        }
    }

    [HttpPost("{streamId:int}/pause")]
    public async Task<ActionResult<StreamInfoDto>> PauseStream([FromRoute] int streamId)
    {
        try
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
            var streamInfo = await _streamingService.PauseStreamAsync(streamId, userId);
            return Ok(streamInfo);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error pausing stream {streamId}");
            return BadRequest(ex.Message);
        }
    }

    [HttpPost("{streamId:int}/resume")]
    public async Task<ActionResult<StreamInfoDto>> ResumeStream([FromRoute] int streamId)
    {
        try
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
            var streamInfo = await _streamingService.ResumeStreamAsync(streamId, userId);
            return Ok(streamInfo);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error resuming stream {streamId}");
            return BadRequest(ex.Message);
        }
    }

    [HttpGet("{streamId:int}/quality")]
    public async Task<ActionResult<StreamQualityDto>> GetStreamQuality([FromRoute] int streamId)
    {
        try
        {
            var quality = await _streamingService.GetStreamQualityAsync(streamId);
            return Ok(quality);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error getting stream quality for {streamId}");
            return BadRequest(ex.Message);
        }
    }

    [HttpPut("{streamId:int}/quality")]
    public async Task<ActionResult<StreamQualityDto>> UpdateStreamQuality([FromRoute] int streamId, [FromBody] StreamQualitySettingsDto settings)
    {
        try
        {
            var quality = await _streamingService.UpdateStreamQualityAsync(streamId, settings);
            return Ok(quality);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error updating stream quality for {streamId}");
            return BadRequest(ex.Message);
        }
    }

    [HttpGet("qualities")]
    public async Task<ActionResult<List<StreamQualityDto>>> GetAvailableQualities()
    {
        try
        {
            var qualities = await _streamingService.GetAvailableQualitiesAsync();
            return Ok(qualities);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting available qualities");
            return BadRequest(ex.Message);
        }
    }

    [HttpPost("{streamId:int}/recording/start")]
    public async Task<ActionResult<RecordingInfoDto>> StartRecording([FromRoute] int streamId)
    {
        try
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
            var recording = await _streamingService.StartRecordingAsync(streamId, userId);
            return Ok(recording);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error starting recording for stream {streamId}");
            return BadRequest(ex.Message);
        }
    }

    [HttpPost("{streamId:int}/recording/stop")]
    public async Task<ActionResult<RecordingInfoDto>> StopRecording([FromRoute] int streamId)
    {
        try
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
            var recording = await _streamingService.StopRecordingAsync(streamId, userId);
            return Ok(recording);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error stopping recording for stream {streamId}");
            return BadRequest(ex.Message);
        }
    }

    [HttpGet("{streamId:int}/recording")]
    public async Task<ActionResult<RecordingInfoDto>> GetRecordingStatus([FromRoute] int streamId)
    {
        try
        {
            var recording = await _streamingService.GetRecordingStatusAsync(streamId);
            return Ok(recording);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error getting recording status for stream {streamId}");
            return BadRequest(ex.Message);
        }
    }

    [HttpGet("{streamId:int}/analytics")]
    public async Task<ActionResult<StreamAnalyticsDto>> GetStreamAnalytics([FromRoute] int streamId)
    {
        try
        {
            var analytics = await _streamingService.GetStreamAnalyticsAsync(streamId);
            return Ok(analytics);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error getting analytics for stream {streamId}");
            return BadRequest(ex.Message);
        }
    }

    [HttpGet("{streamId:int}/health")]
    public async Task<ActionResult<StreamHealthDto>> GetStreamHealth([FromRoute] int streamId)
    {
        try
        {
            var health = await _streamingService.GetStreamHealthAsync(streamId);
            return Ok(health);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error getting health for stream {streamId}");
            return BadRequest(ex.Message);
        }
    }

    [HttpPost("{streamId:int}/metrics")]
    public async Task<ActionResult> UpdateStreamMetrics([FromRoute] int streamId, [FromBody] StreamMetricsDto metrics)
    {
        try
        {
            await _streamingService.UpdateStreamMetricsAsync(streamId, metrics);
            return Ok();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error updating metrics for stream {streamId}");
            return BadRequest(ex.Message);
        }
    }

    [HttpGet("{streamId:int}/endpoint")]
    public async Task<ActionResult<StreamEndpointDto>> GetStreamEndpoint([FromRoute] int streamId)
    {
        try
        {
            var endpoint = await _streamingService.GetStreamEndpointAsync(streamId);
            return Ok(endpoint);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error getting endpoint for stream {streamId}");
            return BadRequest(ex.Message);
        }
    }

    [HttpGet("validate-key/{streamKey}")]
    public async Task<ActionResult<bool>> ValidateStreamKey([FromRoute] string streamKey)
    {
        try
        {
            var isValid = await _streamingService.ValidateStreamKeyAsync(streamKey);
            return Ok(isValid);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error validating stream key {streamKey}");
            return BadRequest(ex.Message);
        }
    }

    [HttpGet("{streamId:int}/config")]
    public async Task<ActionResult<StreamConfigDto>> GetStreamConfiguration([FromRoute] int streamId)
    {
        try
        {
            var config = await _streamingService.GetStreamConfigurationAsync(streamId);
            return Ok(config);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error getting configuration for stream {streamId}");
            return BadRequest(ex.Message);
        }
    }
}
