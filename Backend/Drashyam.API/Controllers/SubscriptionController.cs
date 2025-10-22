using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Drashyam.API.DTOs;
using Drashyam.API.Services;

namespace Drashyam.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SubscriptionController : ControllerBase
{
    private readonly ISubscriptionService _subscriptionService;
    private readonly ILogger<SubscriptionController> _logger;

    public SubscriptionController(ISubscriptionService subscriptionService, ILogger<SubscriptionController> logger)
    {
        _subscriptionService = subscriptionService;
        _logger = logger;
    }

    [HttpPost]
    [Authorize]
    public async Task<ActionResult<SubscriptionDto>> Create([FromBody] SubscriptionCreateDto createDto)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
        var sub = await _subscriptionService.CreateSubscriptionAsync(createDto, userId);
        return Ok(sub);
    }

    [HttpGet("{subscriptionId:int}")]
    [Authorize]
    public async Task<ActionResult<SubscriptionDto>> GetById([FromRoute] int subscriptionId)
    {
        var sub = await _subscriptionService.GetSubscriptionByIdAsync(subscriptionId);
        return Ok(sub);
    }

    [HttpGet("me")]
    [Authorize]
    public async Task<ActionResult<SubscriptionDto>> GetMySubscription()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
        var sub = await _subscriptionService.GetUserSubscriptionAsync(userId);
        return Ok(sub);
    }

    [HttpPut("{subscriptionId:int}")]
    [Authorize]
    public async Task<ActionResult<SubscriptionDto>> Update([FromRoute] int subscriptionId, [FromBody] SubscriptionUpdateDto updateDto)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
        var sub = await _subscriptionService.UpdateSubscriptionAsync(subscriptionId, updateDto, userId);
        return Ok(sub);
    }

    [HttpPost("{subscriptionId:int}/cancel")]
    [Authorize]
    public async Task<IActionResult> Cancel([FromRoute] int subscriptionId)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
        var result = await _subscriptionService.CancelSubscriptionAsync(subscriptionId, userId);
        if (!result) return BadRequest();
        return NoContent();
    }

    [HttpPost("{subscriptionId:int}/renew")]
    [Authorize]
    public async Task<IActionResult> Renew([FromRoute] int subscriptionId)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
        var result = await _subscriptionService.RenewSubscriptionAsync(subscriptionId, userId);
        if (!result) return BadRequest();
        return NoContent();
    }

    [HttpGet("plans")]
    public async Task<ActionResult<PagedResult<SubscriptionPlanDto>>> GetPlans([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        var plans = await _subscriptionService.GetSubscriptionPlansAsync(page, pageSize);
        return Ok(plans);
    }

    [HttpGet("plans/{planId:int}")]
    public async Task<ActionResult<SubscriptionPlanDto>> GetPlan([FromRoute] int planId)
    {
        var plan = await _subscriptionService.GetSubscriptionPlanByIdAsync(planId);
        return Ok(plan);
    }

    [HttpPost("upgrade")]
    [Authorize]
    public async Task<ActionResult<SubscriptionDto>> Upgrade([FromQuery] int newPlanId)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
        var sub = await _subscriptionService.UpgradeSubscriptionAsync(userId, newPlanId);
        return Ok(sub);
    }

    [HttpPost("downgrade")]
    [Authorize]
    public async Task<ActionResult<SubscriptionDto>> Downgrade([FromQuery] int newPlanId)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
        var sub = await _subscriptionService.DowngradeSubscriptionAsync(userId, newPlanId);
        return Ok(sub);
    }

    [HttpGet("status")]
    [Authorize]
    public async Task<ActionResult<bool>> Status()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
        var status = await _subscriptionService.CheckSubscriptionStatusAsync(userId);
        return Ok(status);
    }

    [HttpGet("expiring")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<PagedResult<SubscriptionDto>>> GetExpiring([FromQuery] int daysAhead = 7, [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        var list = await _subscriptionService.GetExpiringSubscriptionsAsync(daysAhead, page, pageSize);
        return Ok(list);
    }

    [HttpGet("history")]
    [Authorize]
    public async Task<ActionResult<PagedResult<SubscriptionHistoryDto>>> GetHistory([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
        var history = await _subscriptionService.GetUserSubscriptionHistoryAsync(userId, page, pageSize);
        return Ok(history);
    }

    [HttpGet("analytics")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<SubscriptionAnalyticsDto>> GetAnalytics([FromQuery] DateTime? startDate = null, [FromQuery] DateTime? endDate = null)
    {
        var analytics = await _subscriptionService.GetSubscriptionAnalyticsAsync(startDate, endDate);
        return Ok(analytics);
    }

    [HttpGet("channels")]
    [Authorize]
    public async Task<ActionResult<PagedResult<ChannelSubscriptionDto>>> GetSubscribedChannels([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
        var channels = await _subscriptionService.GetSubscribedChannelsAsync(userId, page, pageSize);
        return Ok(channels);
    }

    [HttpPost("{subscriptionId:int}/suspend")]
    [Authorize]
    public async Task<IActionResult> Suspend([FromRoute] int subscriptionId)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
        var result = await _subscriptionService.SuspendSubscriptionAsync(subscriptionId, userId);
        if (!result) return BadRequest();
        return NoContent();
    }

    [HttpPost("{subscriptionId:int}/reactivate")]
    [Authorize]
    public async Task<IActionResult> Reactivate([FromRoute] int subscriptionId)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
        var result = await _subscriptionService.ReactivateSubscriptionAsync(subscriptionId, userId);
        if (!result) return BadRequest();
        return NoContent();
    }
}


