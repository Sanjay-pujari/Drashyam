using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Drashyam.API.DTOs;
using Drashyam.API.Services;

namespace Drashyam.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PremiumContentController : ControllerBase
{
    private readonly IPremiumContentService _premiumContentService;
    private readonly ILogger<PremiumContentController> _logger;

    public PremiumContentController(IPremiumContentService premiumContentService, ILogger<PremiumContentController> logger)
    {
        _premiumContentService = premiumContentService;
        _logger = logger;
    }

    [HttpPost("videos")]
    [Authorize]
    public async Task<ActionResult<PremiumVideoDto>> CreatePremiumVideo([FromBody] PremiumVideoCreateDto premiumVideoDto)
    {
        try
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            premiumVideoDto.CreatorId = userId;
            var premiumVideo = await _premiumContentService.CreatePremiumVideoAsync(premiumVideoDto);
            return Ok(premiumVideo);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (UnauthorizedAccessException ex)
        {
            return Forbid(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating premium video");
            return StatusCode(500, "An error occurred while creating the premium video");
        }
    }

    [HttpPut("videos/{premiumVideoId}")]
    [Authorize]
    public async Task<ActionResult<PremiumVideoDto>> UpdatePremiumVideo(int premiumVideoId, [FromBody] PremiumVideoUpdateDto premiumVideoDto)
    {
        try
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            premiumVideoDto.CreatorId = userId;
            var premiumVideo = await _premiumContentService.UpdatePremiumVideoAsync(premiumVideoId, premiumVideoDto);
            return Ok(premiumVideo);
        }
        catch (ArgumentException ex)
        {
            return NotFound(ex.Message);
        }
        catch (UnauthorizedAccessException ex)
        {
            return Forbid(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating premium video {PremiumVideoId}", premiumVideoId);
            return StatusCode(500, "An error occurred while updating the premium video");
        }
    }

    [HttpDelete("videos/{premiumVideoId}")]
    [Authorize]
    public async Task<IActionResult> DeletePremiumVideo(int premiumVideoId)
    {
        try
        {
            var result = await _premiumContentService.DeletePremiumVideoAsync(premiumVideoId);
            if (!result)
                return NotFound();

            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting premium video {PremiumVideoId}", premiumVideoId);
            return StatusCode(500, "An error occurred while deleting the premium video");
        }
    }

    [HttpGet("videos/{premiumVideoId}")]
    public async Task<ActionResult<PremiumVideoDto>> GetPremiumVideo(int premiumVideoId)
    {
        try
        {
            var premiumVideo = await _premiumContentService.GetPremiumVideoAsync(premiumVideoId);
            return Ok(premiumVideo);
        }
        catch (ArgumentException ex)
        {
            return NotFound(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting premium video {PremiumVideoId}", premiumVideoId);
            return StatusCode(500, "An error occurred while retrieving the premium video");
        }
    }

    [HttpGet("videos")]
    public async Task<ActionResult<PagedResult<PremiumVideoDto>>> GetPremiumVideos([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        try
        {
            var premiumVideos = await _premiumContentService.GetPremiumVideosAsync(page, pageSize);
            return Ok(premiumVideos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting premium videos");
            return StatusCode(500, "An error occurred while retrieving premium videos");
        }
    }

    [HttpGet("videos/{videoId}/is-premium")]
    public async Task<ActionResult<bool>> IsVideoPremium(int videoId)
    {
        try
        {
            var isPremium = await _premiumContentService.IsVideoPremiumAsync(videoId);
            return Ok(isPremium);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking if video {VideoId} is premium", videoId);
            return StatusCode(500, "An error occurred while checking premium status");
        }
    }

    [HttpGet("videos/{premiumVideoId}/has-purchased")]
    [Authorize]
    public async Task<ActionResult<bool>> HasUserPurchased(int premiumVideoId)
    {
        try
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var hasPurchased = await _premiumContentService.HasUserPurchasedAsync(premiumVideoId, userId);
            return Ok(hasPurchased);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking purchase status for premium video {PremiumVideoId}", premiumVideoId);
            return StatusCode(500, "An error occurred while checking purchase status");
        }
    }

    [HttpPost("purchases")]
    [Authorize]
    public async Task<ActionResult<PremiumPurchaseDto>> CreatePurchase([FromBody] PremiumPurchaseCreateDto purchaseDto)
    {
        try
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            purchaseDto.UserId = userId;
            var purchase = await _premiumContentService.CreatePurchaseAsync(purchaseDto);
            return Ok(purchase);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating premium purchase");
            return StatusCode(500, "An error occurred while creating the purchase");
        }
    }

    [HttpPost("purchases/{paymentIntentId}/complete")]
    [Authorize]
    public async Task<IActionResult> CompletePurchase(string paymentIntentId)
    {
        try
        {
            var result = await _premiumContentService.CompletePurchaseAsync(paymentIntentId);
            if (!result)
                return BadRequest("Purchase not found or already completed");

            return Ok(new { message = "Purchase completed successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error completing purchase {PaymentIntentId}", paymentIntentId);
            return StatusCode(500, "An error occurred while completing the purchase");
        }
    }

    [HttpPost("purchases/{purchaseId}/refund")]
    [Authorize]
    public async Task<IActionResult> RefundPurchase(int purchaseId)
    {
        try
        {
            var result = await _premiumContentService.RefundPurchaseAsync(purchaseId);
            if (!result)
                return NotFound();

            return Ok(new { message = "Purchase refunded successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error refunding purchase {PurchaseId}", purchaseId);
            return StatusCode(500, "An error occurred while refunding the purchase");
        }
    }

    [HttpGet("purchases")]
    [Authorize]
    public async Task<ActionResult<PagedResult<PremiumPurchaseDto>>> GetUserPurchases([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        try
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var purchases = await _premiumContentService.GetUserPurchasesAsync(userId, page, pageSize);
            return Ok(purchases);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting user purchases");
            return StatusCode(500, "An error occurred while retrieving purchases");
        }
    }

    [HttpGet("videos/{premiumVideoId}/analytics")]
    [Authorize]
    public async Task<ActionResult<PremiumContentAnalyticsDto>> GetPremiumContentAnalytics(int premiumVideoId, [FromQuery] DateTime? startDate = null, [FromQuery] DateTime? endDate = null)
    {
        try
        {
            var analytics = await _premiumContentService.GetPremiumContentAnalyticsAsync(premiumVideoId, startDate, endDate);
            return Ok(analytics);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting premium content analytics for {PremiumVideoId}", premiumVideoId);
            return StatusCode(500, "An error occurred while retrieving analytics");
        }
    }

    [HttpGet("revenue")]
    [Authorize]
    public async Task<ActionResult<decimal>> GetPremiumRevenue([FromQuery] DateTime? startDate = null, [FromQuery] DateTime? endDate = null)
    {
        try
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var revenue = await _premiumContentService.CalculatePremiumRevenueAsync(userId, startDate, endDate);
            return Ok(revenue);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calculating premium revenue");
            return StatusCode(500, "An error occurred while calculating revenue");
        }
    }
}
