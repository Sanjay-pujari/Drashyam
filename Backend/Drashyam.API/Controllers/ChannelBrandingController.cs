using Drashyam.API.DTOs;
using Drashyam.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Drashyam.API.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class ChannelBrandingController : ControllerBase
{
    private readonly IChannelBrandingService _brandingService;
    private readonly ILogger<ChannelBrandingController> _logger;

    public ChannelBrandingController(IChannelBrandingService brandingService, ILogger<ChannelBrandingController> logger)
    {
        _brandingService = brandingService;
        _logger = logger;
    }

    [HttpGet("channel/{channelId}")]
    public async Task<ActionResult<ChannelBrandingDto>> GetChannelBranding(int channelId)
    {
        try
        {
            var branding = await _brandingService.GetChannelBrandingAsync(channelId);
            return Ok(branding);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving channel branding for channel {ChannelId}", channelId);
            return BadRequest("Failed to retrieve channel branding");
        }
    }

    [HttpPost("channel/{channelId}")]
    public async Task<ActionResult<ChannelBrandingDto>> CreateChannelBranding(int channelId, [FromBody] ChannelBrandingCreateDto createDto)
    {
        try
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var branding = await _brandingService.CreateChannelBrandingAsync(createDto, channelId, userId);
            return CreatedAtAction(nameof(GetChannelBranding), new { channelId }, branding);
        }
        catch (UnauthorizedAccessException ex)
        {
            return Forbid(ex.Message);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating channel branding for channel {ChannelId}", channelId);
            return BadRequest("Failed to create channel branding");
        }
    }

    [HttpPut("channel/{channelId}")]
    public async Task<ActionResult<ChannelBrandingDto>> UpdateChannelBranding(int channelId, [FromBody] ChannelBrandingUpdateDto updateDto)
    {
        try
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var branding = await _brandingService.UpdateChannelBrandingAsync(channelId, updateDto, userId);
            return Ok(branding);
        }
        catch (UnauthorizedAccessException ex)
        {
            return Forbid(ex.Message);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating channel branding for channel {ChannelId}", channelId);
            return BadRequest("Failed to update channel branding");
        }
    }

    [HttpDelete("channel/{channelId}")]
    public async Task<ActionResult> DeleteChannelBranding(int channelId)
    {
        try
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var success = await _brandingService.DeleteChannelBrandingAsync(channelId, userId);
            if (!success)
                return NotFound();

            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting channel branding for channel {ChannelId}", channelId);
            return BadRequest("Failed to delete channel branding");
        }
    }

    [HttpPost("channel/{channelId}/activate")]
    public async Task<ActionResult> ActivateChannelBranding(int channelId)
    {
        try
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var success = await _brandingService.ActivateChannelBrandingAsync(channelId, userId);
            if (!success)
                return BadRequest("Failed to activate channel branding");

            return Ok();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error activating channel branding for channel {ChannelId}", channelId);
            return BadRequest("Failed to activate channel branding");
        }
    }

    [HttpPost("channel/{channelId}/deactivate")]
    public async Task<ActionResult> DeactivateChannelBranding(int channelId)
    {
        try
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var success = await _brandingService.DeactivateChannelBrandingAsync(channelId, userId);
            if (!success)
                return BadRequest("Failed to deactivate channel branding");

            return Ok();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deactivating channel branding for channel {ChannelId}", channelId);
            return BadRequest("Failed to deactivate channel branding");
        }
    }
}
