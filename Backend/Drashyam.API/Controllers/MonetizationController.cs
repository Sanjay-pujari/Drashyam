using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Drashyam.API.DTOs;
using Drashyam.API.Services;

namespace Drashyam.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class MonetizationController : ControllerBase
{
    private readonly IMonetizationService _monetizationService;
    private readonly ILogger<MonetizationController> _logger;

    public MonetizationController(IMonetizationService monetizationService, ILogger<MonetizationController> logger)
    {
        _monetizationService = monetizationService;
        _logger = logger;
    }

    [HttpGet("status")]
    [Authorize]
    public async Task<ActionResult<MonetizationStatusDto>> Status()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
        var status = await _monetizationService.GetMonetizationStatusAsync(userId);
        return Ok(status);
    }

    [HttpPost("enable")]
    [Authorize]
    public async Task<ActionResult<MonetizationStatusDto>> Enable([FromBody] MonetizationRequestDto request)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
        var status = await _monetizationService.EnableMonetizationAsync(userId, request);
        return Ok(status);
    }

    [HttpPost("disable")]
    [Authorize]
    public async Task<ActionResult<MonetizationStatusDto>> Disable()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
        var status = await _monetizationService.DisableMonetizationAsync(userId);
        return Ok(status);
    }

    [HttpGet("video/{videoId:int}/ads")]
    [Authorize]
    public async Task<ActionResult<List<AdPlacementDto>>> GetAdPlacements([FromRoute] int videoId)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
        var list = await _monetizationService.GetAdPlacementsAsync(videoId, userId);
        return Ok(list);
    }

    [HttpPost("video/{videoId:int}/ads")]
    [Authorize]
    public async Task<ActionResult<AdPlacementDto>> CreateAdPlacement([FromRoute] int videoId, [FromBody] AdPlacementRequestDto request)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
        var placement = await _monetizationService.CreateAdPlacementAsync(videoId, request, userId);
        return Ok(placement);
    }

    [HttpPut("ads/{placementId:int}")]
    [Authorize]
    public async Task<ActionResult<AdPlacementDto>> UpdateAdPlacement([FromRoute] int placementId, [FromBody] AdPlacementRequestDto request)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
        var placement = await _monetizationService.UpdateAdPlacementAsync(placementId, request, userId);
        return Ok(placement);
    }

    [HttpDelete("ads/{placementId:int}")]
    [Authorize]
    public async Task<IActionResult> DeleteAdPlacement([FromRoute] int placementId)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
        await _monetizationService.DeleteAdPlacementAsync(placementId, userId);
        return NoContent();
    }

    [HttpGet("sponsors")]
    [Authorize]
    public async Task<ActionResult<List<SponsorDto>>> GetSponsors()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
        var list = await _monetizationService.GetSponsorsAsync(userId);
        return Ok(list);
    }

    [HttpPost("sponsors")]
    [Authorize]
    public async Task<ActionResult<SponsorDto>> CreateSponsor([FromBody] SponsorRequestDto request)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
        var sponsor = await _monetizationService.CreateSponsorAsync(request, userId);
        return Ok(sponsor);
    }

    [HttpPut("sponsors/{sponsorId:int}")]
    [Authorize]
    public async Task<ActionResult<SponsorDto>> UpdateSponsor([FromRoute] int sponsorId, [FromBody] SponsorRequestDto request)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
        var sponsor = await _monetizationService.UpdateSponsorAsync(sponsorId, request, userId);
        return Ok(sponsor);
    }

    [HttpDelete("sponsors/{sponsorId:int}")]
    [Authorize]
    public async Task<IActionResult> DeleteSponsor([FromRoute] int sponsorId)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
        await _monetizationService.DeleteSponsorAsync(sponsorId, userId);
        return NoContent();
    }

    [HttpGet("donations")]
    [Authorize]
    public async Task<ActionResult<List<DonationDto>>> GetDonations([FromQuery] DateTime? startDate = null, [FromQuery] DateTime? endDate = null)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
        var list = await _monetizationService.GetDonationsAsync(userId, startDate, endDate);
        return Ok(list);
    }

    [HttpPost("donations")]
    [Authorize]
    public async Task<ActionResult<DonationDto>> ProcessDonation([FromBody] DonationRequestDto request)
    {
        var donation = await _monetizationService.ProcessDonationAsync(request);
        return Ok(donation);
    }

    [HttpGet("merchandise")]
    [Authorize]
    public async Task<ActionResult<List<MerchandiseDto>>> GetMerchandise()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
        var list = await _monetizationService.GetMerchandiseAsync(userId);
        return Ok(list);
    }

    [HttpPost("merchandise")]
    [Authorize]
    public async Task<ActionResult<MerchandiseDto>> CreateMerchandise([FromBody] MerchandiseRequestDto request)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
        var item = await _monetizationService.CreateMerchandiseAsync(request, userId);
        return Ok(item);
    }

    [HttpPut("merchandise/{merchandiseId:int}")]
    [Authorize]
    public async Task<ActionResult<MerchandiseDto>> UpdateMerchandise([FromRoute] int merchandiseId, [FromBody] MerchandiseRequestDto request)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
        var item = await _monetizationService.UpdateMerchandiseAsync(merchandiseId, request, userId);
        return Ok(item);
    }

    [HttpDelete("merchandise/{merchandiseId:int}")]
    [Authorize]
    public async Task<IActionResult> DeleteMerchandise([FromRoute] int merchandiseId)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
        await _monetizationService.DeleteMerchandiseAsync(merchandiseId, userId);
        return NoContent();
    }

    [HttpGet("revenue-report")]
    [Authorize]
    public async Task<ActionResult<RevenueReportDto>> RevenueReport([FromQuery] DateTime? startDate = null, [FromQuery] DateTime? endDate = null)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
        var report = await _monetizationService.GetRevenueReportAsync(userId, startDate, endDate);
        return Ok(report);
    }

    // Merchandise Orders
    [HttpGet("merchandise/orders")]
    [Authorize]
    public async Task<ActionResult<List<MerchandiseOrderDto>>> GetMerchandiseOrders([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
        var orders = await _monetizationService.GetMerchandiseOrdersAsync(userId, page, pageSize);
        return Ok(orders);
    }

    [HttpPost("merchandise/orders")]
    [Authorize]
    public async Task<ActionResult<MerchandiseOrderDto>> CreateMerchandiseOrder([FromBody] MerchandiseOrderRequestDto request)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
        var order = await _monetizationService.CreateMerchandiseOrderAsync(request, userId);
        return Ok(order);
    }

    [HttpPut("merchandise/orders/{orderId:int}")]
    [Authorize]
    public async Task<ActionResult<MerchandiseOrderDto>> UpdateMerchandiseOrder([FromRoute] int orderId, [FromBody] MerchandiseOrderUpdateDto request)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
        var order = await _monetizationService.UpdateMerchandiseOrderStatusAsync(orderId, request.Status, userId, request.TrackingNumber);
        return Ok(order);
    }

    // Public endpoints for customers to browse merchandise
    [HttpGet("channels/{channelId:int}/merchandise")]
    public async Task<ActionResult<List<MerchandiseDto>>> GetChannelMerchandise([FromRoute] int channelId)
    {
        var merchandise = await _monetizationService.GetChannelMerchandiseAsync(channelId);
        return Ok(merchandise);
    }

    [HttpGet("merchandise/{merchandiseId:int}")]
    public async Task<ActionResult<MerchandiseDto>> GetMerchandiseDetails([FromRoute] int merchandiseId)
    {
        var merchandise = await _monetizationService.GetMerchandiseDetailsAsync(merchandiseId);
        if (merchandise == null) return NotFound();
        return Ok(merchandise);
    }

    // Merchandise Analytics
    [HttpGet("merchandise/analytics")]
    [Authorize]
    public async Task<ActionResult<MerchandiseAnalyticsDto>> GetMerchandiseAnalytics([FromQuery] DateTime? startDate = null, [FromQuery] DateTime? endDate = null)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
        var analytics = await _monetizationService.GetMerchandiseAnalyticsAsync(userId, startDate, endDate);
        return Ok(analytics);
    }
}


