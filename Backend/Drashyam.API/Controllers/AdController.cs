using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Drashyam.API.DTOs;
using Drashyam.API.Services;

namespace Drashyam.API.Controllers;

[ApiController]
[Route("api/[controller]")]
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
    public async Task<ActionResult<AdCampaignDto>> CreateCampaign([FromBody] AdCampaignCreateDto campaignDto)
    {
        try
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            // Basic validations to avoid 500s from EF or mapping issues
            if (string.IsNullOrWhiteSpace(campaignDto.Name))
                return BadRequest("Name is required");

            if (campaignDto.Budget < 0 || campaignDto.CostPerView < 0 || campaignDto.CostPerClick < 0)
                return BadRequest("Budget/CPV/CPC must be non-negative");

            if (campaignDto.EndDate < campaignDto.StartDate)
                return BadRequest("EndDate must be greater than or equal to StartDate");

            campaignDto.AdvertiserId = userId;
            var campaign = await _adService.CreateCampaignAsync(campaignDto);
            return Ok(campaign);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating ad campaign");
            return StatusCode(500, ex.Message);
        }
    }

    [HttpPut("campaigns/{campaignId}")]
    public async Task<ActionResult<AdCampaignDto>> UpdateCampaign(int campaignId, [FromBody] AdCampaignUpdateDto campaignDto)
    {
        try
        {
            var campaign = await _adService.UpdateCampaignAsync(campaignId, campaignDto);
            return Ok(campaign);
        }
        catch (ArgumentException ex)
        {
            return NotFound(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating ad campaign {CampaignId}", campaignId);
            return StatusCode(500, "An error occurred while updating the campaign");
        }
    }

    [HttpDelete("campaigns/{campaignId}")]
    public async Task<IActionResult> DeleteCampaign(int campaignId)
    {
        try
        {
            var result = await _adService.DeleteCampaignAsync(campaignId);
            if (!result)
                return NotFound();

            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting ad campaign {CampaignId}", campaignId);
            return StatusCode(500, "An error occurred while deleting the campaign");
        }
    }

    [HttpGet("campaigns/{campaignId}")]
    public async Task<ActionResult<AdCampaignDto>> GetCampaign(int campaignId)
    {
        try
        {
            var campaign = await _adService.GetCampaignAsync(campaignId);
            return Ok(campaign);
        }
        catch (ArgumentException ex)
        {
            return NotFound(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting ad campaign {CampaignId}", campaignId);
            return StatusCode(500, "An error occurred while retrieving the campaign");
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

            var campaigns = await _adService.GetCampaignsAsync(userId, page, pageSize);
            return Ok(campaigns);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting ad campaigns");
            return StatusCode(500, "An error occurred while retrieving campaigns");
        }
    }

    [HttpPost("campaigns/{campaignId}/activate")]
    public async Task<IActionResult> ActivateCampaign(int campaignId)
    {
        try
        {
            var result = await _adService.ActivateCampaignAsync(campaignId);
            if (!result)
                return NotFound();

            return Ok(new { message = "Campaign activated successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error activating ad campaign {CampaignId}", campaignId);
            return StatusCode(500, "An error occurred while activating the campaign");
        }
    }

    [HttpPost("campaigns/{campaignId}/pause")]
    public async Task<IActionResult> PauseCampaign(int campaignId)
    {
        try
        {
            var result = await _adService.PauseCampaignAsync(campaignId);
            if (!result)
                return NotFound();

            return Ok(new { message = "Campaign paused successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error pausing ad campaign {CampaignId}", campaignId);
            return StatusCode(500, "An error occurred while pausing the campaign");
        }
    }

    [HttpPost("serve")]
    [AllowAnonymous]
    public async Task<ActionResult<AdServeDto>> ServeAd([FromBody] AdRequestDto request)
    {
        try
        {
            var ad = await _adService.ServeAdAsync(request);
            return Ok(ad);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error serving ad");
            return StatusCode(500, "An error occurred while serving the ad");
        }
    }

    [HttpPost("impressions")]
    [AllowAnonymous]
    public async Task<IActionResult> RecordImpression([FromBody] AdImpressionRequestDto request)
    {
        try
        {
            var result = await _adService.RecordImpressionAsync(request.CampaignId, request.UserId, request.VideoId);
            if (!result)
                return BadRequest("Failed to record impression");

            return Ok(new { message = "Impression recorded successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error recording ad impression");
            return StatusCode(500, "An error occurred while recording the impression");
        }
    }

    [HttpPost("clicks")]
    [AllowAnonymous]
    public async Task<IActionResult> RecordClick([FromBody] AdClickRequestDto request)
    {
        try
        {
            var result = await _adService.RecordClickAsync(request.CampaignId, request.UserId, request.VideoId);
            if (!result)
                return BadRequest("Failed to record click");

            return Ok(new { message = "Click recorded successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error recording ad click");
            return StatusCode(500, "An error occurred while recording the click");
        }
    }

    [HttpGet("campaigns/{campaignId}/analytics")]
    public async Task<ActionResult<AdAnalyticsDto>> GetCampaignAnalytics(int campaignId, [FromQuery] DateTime? startDate = null, [FromQuery] DateTime? endDate = null)
    {
        try
        {
            var analytics = await _adService.GetCampaignAnalyticsAsync(campaignId, startDate, endDate);
            return Ok(analytics);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting campaign analytics for {CampaignId}", campaignId);
            return StatusCode(500, "An error occurred while retrieving analytics");
        }
    }

    [HttpGet("revenue")]
    public async Task<ActionResult<decimal>> GetRevenue([FromQuery] DateTime? startDate = null, [FromQuery] DateTime? endDate = null)
    {
        try
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var revenue = await _adService.CalculateRevenueAsync(userId, startDate, endDate);
            return Ok(revenue);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calculating ad revenue");
            return StatusCode(500, "An error occurred while calculating revenue");
        }
    }
}

public class AdImpressionRequestDto
{
    public int CampaignId { get; set; }
    public string? UserId { get; set; }
    public int? VideoId { get; set; }
}

public class AdClickRequestDto
{
    public int CampaignId { get; set; }
    public string? UserId { get; set; }
    public int? VideoId { get; set; }
}
