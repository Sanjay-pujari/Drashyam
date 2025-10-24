using Drashyam.API.DTOs;
using Drashyam.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Drashyam.API.Controllers;

[ApiController]
[Route("api/monetization")]
[Authorize]
public class AdCampaignsController : ControllerBase
{
    private readonly IMonetizationService _monetizationService;
    private readonly ILogger<AdCampaignsController> _logger;

    public AdCampaignsController(
        IMonetizationService monetizationService,
        ILogger<AdCampaignsController> logger)
    {
        _monetizationService = monetizationService;
        _logger = logger;
    }

    [HttpGet("ad-campaigns")]
    public async Task<ActionResult<List<AdCampaignDto>>> GetAdCampaigns()
    {
        try
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized("User not authenticated");
            }

            var campaigns = await _monetizationService.GetAdCampaignsAsync(userId);
            return Ok(campaigns);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving ad campaigns");
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpGet("ad-campaigns/{id}")]
    public async Task<ActionResult<AdCampaignDto>> GetAdCampaign(int id)
    {
        try
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized("User not authenticated");
            }

            var campaign = await _monetizationService.GetAdCampaignAsync(id, userId);
            if (campaign == null)
            {
                return NotFound();
            }

            return Ok(campaign);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving ad campaign {CampaignId}", id);
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpPost("ad-campaigns")]
    public async Task<ActionResult<AdCampaignDto>> CreateAdCampaign([FromBody] AdCampaignCreateDto createDto)
    {
        try
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized("User not authenticated");
            }

            var campaign = await _monetizationService.CreateAdCampaignAsync(createDto, userId);
            return CreatedAtAction(nameof(GetAdCampaign), new { id = campaign.Id }, campaign);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating ad campaign");
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpPut("ad-campaigns/{id}")]
    public async Task<ActionResult<AdCampaignDto>> UpdateAdCampaign(int id, [FromBody] AdCampaignUpdateDto updateDto)
    {
        try
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized("User not authenticated");
            }

            var campaign = await _monetizationService.UpdateAdCampaignAsync(id, updateDto, userId);
            if (campaign == null)
            {
                return NotFound();
            }

            return Ok(campaign);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating ad campaign {CampaignId}", id);
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpDelete("ad-campaigns/{id}")]
    public async Task<ActionResult> DeleteAdCampaign(int id)
    {
        try
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized("User not authenticated");
            }

            var result = await _monetizationService.DeleteAdCampaignAsync(id, userId);
            if (!result)
            {
                return NotFound();
            }

            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting ad campaign {CampaignId}", id);
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpPost("ad-campaigns/{id}/pause")]
    public async Task<ActionResult> PauseAdCampaign(int id)
    {
        try
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized("User not authenticated");
            }

            var result = await _monetizationService.PauseAdCampaignAsync(id, userId);
            if (!result)
            {
                return NotFound();
            }

            return Ok(new { message = "Campaign paused successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error pausing ad campaign {CampaignId}", id);
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpPost("ad-campaigns/{id}/resume")]
    public async Task<ActionResult> ResumeAdCampaign(int id)
    {
        try
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized("User not authenticated");
            }

            var result = await _monetizationService.ResumeAdCampaignAsync(id, userId);
            if (!result)
            {
                return NotFound();
            }

            return Ok(new { message = "Campaign resumed successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error resuming ad campaign {CampaignId}", id);
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpGet("ad-campaigns/{id}/analytics")]
    public async Task<ActionResult<AdCampaignAnalyticsDto>> GetAdCampaignAnalytics(int id)
    {
        try
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized("User not authenticated");
            }

            var analytics = await _monetizationService.GetAdCampaignAnalyticsAsync(id, userId);
            if (analytics == null)
            {
                return NotFound();
            }

            return Ok(analytics);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving ad campaign analytics {CampaignId}", id);
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpGet("display-ads")]
    public async Task<ActionResult<List<AdDto>>> GetDisplayAds([FromQuery] int? channelId = null)
    {
        try
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized("User not authenticated");
            }

            var ads = await _monetizationService.GetDisplayAdsAsync(channelId, userId);
            return Ok(ads);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving display ads for channel {ChannelId}", channelId);
            return StatusCode(500, "Internal server error");
        }
    }
}
