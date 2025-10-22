using Drashyam.API.DTOs;
using Drashyam.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Drashyam.API.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class AdController : ControllerBase
{
    private readonly IAdService _adService;
    private readonly ILogger<AdController> _logger;

    public AdController(IAdService adService, ILogger<AdController> logger)
    {
        _adService = adService;
        _logger = logger;
    }

    [HttpPost("campaigns")]
    public async Task<ActionResult<AdCampaignDto>> CreateCampaign([FromBody] AdCampaignCreateDto createDto)
    {
        try
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var campaign = await _adService.CreateAdCampaignAsync(createDto, userId);
            return CreatedAtAction(nameof(GetCampaign), new { id = campaign.Id }, campaign);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating ad campaign");
            return BadRequest("Failed to create ad campaign");
        }
    }

    [HttpGet("campaigns/{id}")]
    public async Task<ActionResult<AdCampaignDto>> GetCampaign(int id)
    {
        try
        {
            var campaign = await _adService.GetAdCampaignByIdAsync(id);
            return Ok(campaign);
        }
        catch (ArgumentException)
        {
            return NotFound();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving ad campaign {CampaignId}", id);
            return BadRequest("Failed to retrieve ad campaign");
        }
    }

    [HttpGet("campaigns")]
    public async Task<ActionResult<PagedResult<AdCampaignDto>>> GetCampaigns([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        try
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var campaigns = await _adService.GetAdCampaignsAsync(userId, page, pageSize);
            return Ok(campaigns);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving ad campaigns");
            return BadRequest("Failed to retrieve ad campaigns");
        }
    }

    [HttpPut("campaigns/{id}")]
    public async Task<ActionResult<AdCampaignDto>> UpdateCampaign(int id, [FromBody] AdCampaignUpdateDto updateDto)
    {
        try
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var campaign = await _adService.UpdateAdCampaignAsync(id, updateDto, userId);
            return Ok(campaign);
        }
        catch (ArgumentException)
        {
            return NotFound();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating ad campaign {CampaignId}", id);
            return BadRequest("Failed to update ad campaign");
        }
    }

    [HttpDelete("campaigns/{id}")]
    public async Task<ActionResult> DeleteCampaign(int id)
    {
        try
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var success = await _adService.DeleteAdCampaignAsync(id, userId);
            if (!success)
                return NotFound();

            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting ad campaign {CampaignId}", id);
            return BadRequest("Failed to delete ad campaign");
        }
    }

    [HttpPost("campaigns/{id}/activate")]
    public async Task<ActionResult> ActivateCampaign(int id)
    {
        try
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var success = await _adService.ActivateAdCampaignAsync(id, userId);
            if (!success)
                return NotFound();

            return Ok();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error activating ad campaign {CampaignId}", id);
            return BadRequest("Failed to activate ad campaign");
        }
    }

    [HttpPost("campaigns/{id}/pause")]
    public async Task<ActionResult> PauseCampaign(int id)
    {
        try
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var success = await _adService.PauseAdCampaignAsync(id, userId);
            if (!success)
                return NotFound();

            return Ok();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error pausing ad campaign {CampaignId}", id);
            return BadRequest("Failed to pause ad campaign");
        }
    }

    [HttpGet("serve")]
    public async Task<ActionResult<AdDto?>> GetAd([FromQuery] int? videoId = null, [FromQuery] Models.AdType? type = null)
    {
        try
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var ad = await _adService.GetAdForUserAsync(userId, videoId, type);
            return Ok(ad);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error serving ad");
            return BadRequest("Failed to serve ad");
        }
    }

    [HttpPost("impressions")]
    public async Task<ActionResult> RecordImpression([FromBody] AdImpressionRequestDto request)
    {
        try
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var success = await _adService.RecordAdImpressionAsync(request.CampaignId, userId, request.VideoId);
            if (!success)
                return BadRequest("Failed to record impression");

            return Ok();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error recording ad impression");
            return BadRequest("Failed to record impression");
        }
    }

    [HttpPost("clicks")]
    public async Task<ActionResult> RecordClick([FromBody] AdClickRequestDto request)
    {
        try
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var success = await _adService.RecordAdClickAsync(request.CampaignId, userId, request.VideoId);
            if (!success)
                return BadRequest("Failed to record click");

            return Ok();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error recording ad click");
            return BadRequest("Failed to record click");
        }
    }

    [HttpGet("revenue")]
    public async Task<ActionResult<AdRevenueDto>> GetAdRevenue([FromQuery] DateTime? startDate = null, [FromQuery] DateTime? endDate = null)
    {
        try
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var revenue = await _adService.GetAdRevenueBreakdownAsync(userId, startDate, endDate);
            return Ok(revenue);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving ad revenue");
            return BadRequest("Failed to retrieve ad revenue");
        }
    }

    [HttpGet("campaigns/{id}/analytics")]
    public async Task<ActionResult<AdAnalyticsDto>> GetCampaignAnalytics(int id, [FromQuery] DateTime? startDate = null, [FromQuery] DateTime? endDate = null)
    {
        try
        {
            var analytics = await _adService.GetAdCampaignAnalyticsAsync(id, startDate, endDate);
            return Ok(analytics);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving campaign analytics for {CampaignId}", id);
            return BadRequest("Failed to retrieve campaign analytics");
        }
    }

    [HttpGet("analytics")]
    public async Task<ActionResult<AdAnalyticsDto>> GetAdvertiserAnalytics([FromQuery] DateTime? startDate = null, [FromQuery] DateTime? endDate = null)
    {
        try
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var analytics = await _adService.GetAdvertiserAnalyticsAsync(userId, startDate, endDate);
            return Ok(analytics);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving advertiser analytics");
            return BadRequest("Failed to retrieve advertiser analytics");
        }
    }
}

public class AdImpressionRequestDto
{
    public int CampaignId { get; set; }
    public int? VideoId { get; set; }
}

public class AdClickRequestDto
{
    public int CampaignId { get; set; }
    public int? VideoId { get; set; }
}