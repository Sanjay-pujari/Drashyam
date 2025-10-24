using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Drashyam.API.DTOs;
using Drashyam.API.Services;
using System.Security.Claims;

namespace Drashyam.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class StreamingController : ControllerBase
{
    private readonly IAzureCommunicationService _azureCommunicationService;
    private readonly ILogger<StreamingController> _logger;

    public StreamingController(
        IAzureCommunicationService azureCommunicationService,
        ILogger<StreamingController> logger)
    {
        _azureCommunicationService = azureCommunicationService;
        _logger = logger;
    }

    [HttpPost("endpoints")]
    public async Task<ActionResult<StreamingEndpointDto>> CreateStreamingEndpoint([FromBody] CreateStreamingEndpointDto request)
    {
        try
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized("User not authenticated");
            }

            var endpoint = await _azureCommunicationService.CreateStreamingEndpointAsync(
                userId, 
                request.Title, 
                request.Description);

            return Ok(endpoint);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating streaming endpoint");
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpGet("endpoints/{endpointId}")]
    public async Task<ActionResult<StreamingEndpointDto>> GetStreamingEndpoint(string endpointId)
    {
        try
        {
            var endpoint = await _azureCommunicationService.GetStreamingEndpointAsync(endpointId);
            return Ok(endpoint);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting streaming endpoint");
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpGet("endpoints")]
    public async Task<ActionResult<List<StreamingEndpointDto>>> GetUserStreamingEndpoints()
    {
        try
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized("User not authenticated");
            }

            var endpoints = await _azureCommunicationService.GetUserStreamingEndpointsAsync(userId);
            return Ok(endpoints);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting user streaming endpoints");
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpPost("endpoints/{endpointId}/start")]
    public async Task<ActionResult> StartStreaming(string endpointId)
    {
        try
        {
            var success = await _azureCommunicationService.StartStreamingAsync(endpointId);
            if (success)
            {
                return Ok(new { message = "Streaming started successfully" });
            }
            return BadRequest("Failed to start streaming");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error starting streaming");
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpPost("endpoints/{endpointId}/stop")]
    public async Task<ActionResult> StopStreaming(string endpointId)
    {
        try
        {
            var success = await _azureCommunicationService.StopStreamingAsync(endpointId);
            if (success)
            {
                return Ok(new { message = "Streaming stopped successfully" });
            }
            return BadRequest("Failed to stop streaming");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error stopping streaming");
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpDelete("endpoints/{endpointId}")]
    public async Task<ActionResult> DeleteStreamingEndpoint(string endpointId)
    {
        try
        {
            var success = await _azureCommunicationService.DeleteStreamingEndpointAsync(endpointId);
            if (success)
            {
                return Ok(new { message = "Streaming endpoint deleted successfully" });
            }
            return BadRequest("Failed to delete streaming endpoint");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting streaming endpoint");
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpGet("endpoints/{endpointId}/analytics")]
    public async Task<ActionResult<StreamingAnalyticsDto>> GetStreamingAnalytics(string endpointId)
    {
        try
        {
            var analytics = await _azureCommunicationService.GetStreamingAnalyticsAsync(endpointId);
            return Ok(analytics);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting streaming analytics");
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpPut("endpoints/{endpointId}/settings")]
    public async Task<ActionResult> UpdateStreamingSettings(string endpointId, [FromBody] StreamingSettingsDto settings)
    {
        try
        {
            settings.EndpointId = endpointId;
            var success = await _azureCommunicationService.UpdateStreamingSettingsAsync(endpointId, settings);
            if (success)
            {
                return Ok(new { message = "Streaming settings updated successfully" });
            }
            return BadRequest("Failed to update streaming settings");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating streaming settings");
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpGet("endpoints/{endpointId}/health")]
    public async Task<ActionResult<StreamingHealthDto>> GetStreamingHealth(string endpointId)
    {
        try
        {
            var health = await _azureCommunicationService.GetStreamingHealthAsync(endpointId);
            return Ok(health);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting streaming health");
            return StatusCode(500, "Internal server error");
        }
    }
}

public class CreateStreamingEndpointDto
{
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string? Category { get; set; }
    public List<string>? Tags { get; set; }
    public bool IsPublic { get; set; } = true;
    public int MaxViewers { get; set; } = 1000;
    public StreamingQualityDto? QualitySettings { get; set; }
}