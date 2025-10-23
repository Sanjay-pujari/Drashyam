using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Drashyam.API.DTOs;
using Drashyam.API.Services;

namespace Drashyam.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class LiveStreamMonetizationController : ControllerBase
{
    private readonly ILiveStreamMonetizationService _monetizationService;
    private readonly ILogger<LiveStreamMonetizationController> _logger;

    public LiveStreamMonetizationController(ILiveStreamMonetizationService monetizationService, ILogger<LiveStreamMonetizationController> logger)
    {
        _monetizationService = monetizationService;
        _logger = logger;
    }

    [HttpPost("donation")]
    public async Task<ActionResult<LiveStreamDonationDto>> SendDonation([FromBody] SendDonationDto dto)
    {
        try
        {
            var userId = User.FindFirst("sub")?.Value ?? User.FindFirst("id")?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var donation = await _monetizationService.SendDonationAsync(dto, userId);
            return Ok(donation);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending donation");
            return BadRequest(ex.Message);
        }
    }

    [HttpGet("donations/{liveStreamId}")]
    public async Task<ActionResult<List<LiveStreamDonationDto>>> GetDonations(int liveStreamId, [FromQuery] int page = 1, [FromQuery] int pageSize = 50)
    {
        try
        {
            var donations = await _monetizationService.GetDonationsAsync(liveStreamId, page, pageSize);
            return Ok(donations);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting donations");
            return BadRequest(ex.Message);
        }
    }

    [HttpPost("superchat")]
    public async Task<ActionResult<LiveStreamSuperChatDto>> SendSuperChat([FromBody] SendSuperChatDto dto)
    {
        try
        {
            var userId = User.FindFirst("sub")?.Value ?? User.FindFirst("id")?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var superChat = await _monetizationService.SendSuperChatAsync(dto, userId);
            return Ok(superChat);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending super chat");
            return BadRequest(ex.Message);
        }
    }

    [HttpGet("superchats/{liveStreamId}")]
    public async Task<ActionResult<List<LiveStreamSuperChatDto>>> GetSuperChats(int liveStreamId, [FromQuery] int page = 1, [FromQuery] int pageSize = 50)
    {
        try
        {
            var superChats = await _monetizationService.GetSuperChatsAsync(liveStreamId, page, pageSize);
            return Ok(superChats);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting super chats");
            return BadRequest(ex.Message);
        }
    }

    [HttpPost("subscription")]
    public async Task<ActionResult<LiveStreamSubscriptionDto>> SubscribeToLiveStream([FromBody] SubscribeToLiveStreamDto dto)
    {
        try
        {
            var userId = User.FindFirst("sub")?.Value ?? User.FindFirst("id")?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var subscription = await _monetizationService.SubscribeToLiveStreamAsync(dto, userId);
            return Ok(subscription);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error subscribing to live stream");
            return BadRequest(ex.Message);
        }
    }

    [HttpGet("subscriptions/{liveStreamId}")]
    public async Task<ActionResult<List<LiveStreamSubscriptionDto>>> GetSubscriptions(int liveStreamId, [FromQuery] int page = 1, [FromQuery] int pageSize = 50)
    {
        try
        {
            var subscriptions = await _monetizationService.GetSubscriptionsAsync(liveStreamId, page, pageSize);
            return Ok(subscriptions);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting subscriptions");
            return BadRequest(ex.Message);
        }
    }

    [HttpDelete("subscription/{liveStreamId}")]
    public async Task<ActionResult<bool>> UnsubscribeFromLiveStream(int liveStreamId)
    {
        try
        {
            var userId = User.FindFirst("sub")?.Value ?? User.FindFirst("id")?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var result = await _monetizationService.UnsubscribeFromLiveStreamAsync(liveStreamId, userId);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error unsubscribing from live stream");
            return BadRequest(ex.Message);
        }
    }

    [HttpGet("revenue/{liveStreamId}")]
    public async Task<ActionResult<LiveStreamRevenueDto>> GetRevenue(int liveStreamId)
    {
        try
        {
            var revenue = await _monetizationService.GetRevenueAsync(liveStreamId);
            return Ok(revenue);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting revenue");
            return BadRequest(ex.Message);
        }
    }

    [HttpGet("stats/{liveStreamId}")]
    public async Task<ActionResult<LiveStreamMonetizationStatsDto>> GetMonetizationStats(int liveStreamId)
    {
        try
        {
            var stats = await _monetizationService.GetMonetizationStatsAsync(liveStreamId);
            return Ok(stats);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting monetization stats");
            return BadRequest(ex.Message);
        }
    }

    [HttpGet("user/donations")]
    public async Task<ActionResult<List<LiveStreamDonationDto>>> GetUserDonations([FromQuery] int page = 1, [FromQuery] int pageSize = 50)
    {
        try
        {
            var userId = User.FindFirst("sub")?.Value ?? User.FindFirst("id")?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var donations = await _monetizationService.GetUserDonationsAsync(userId, page, pageSize);
            return Ok(donations);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting user donations");
            return BadRequest(ex.Message);
        }
    }

    [HttpGet("user/superchats")]
    public async Task<ActionResult<List<LiveStreamSuperChatDto>>> GetUserSuperChats([FromQuery] int page = 1, [FromQuery] int pageSize = 50)
    {
        try
        {
            var userId = User.FindFirst("sub")?.Value ?? User.FindFirst("id")?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var superChats = await _monetizationService.GetUserSuperChatsAsync(userId, page, pageSize);
            return Ok(superChats);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting user super chats");
            return BadRequest(ex.Message);
        }
    }

    [HttpGet("user/subscriptions")]
    public async Task<ActionResult<List<LiveStreamSubscriptionDto>>> GetUserSubscriptions([FromQuery] int page = 1, [FromQuery] int pageSize = 50)
    {
        try
        {
            var userId = User.FindFirst("sub")?.Value ?? User.FindFirst("id")?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var subscriptions = await _monetizationService.GetUserSubscriptionsAsync(userId, page, pageSize);
            return Ok(subscriptions);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting user subscriptions");
            return BadRequest(ex.Message);
        }
    }

    [HttpGet("user/total-revenue")]
    public async Task<ActionResult<decimal>> GetTotalRevenue([FromQuery] DateTime? startDate = null, [FromQuery] DateTime? endDate = null)
    {
        try
        {
            var userId = User.FindFirst("sub")?.Value ?? User.FindFirst("id")?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var revenue = await _monetizationService.GetTotalRevenueAsync(userId, startDate, endDate);
            return Ok(revenue);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting total revenue");
            return BadRequest(ex.Message);
        }
    }

    [HttpGet("livestream/{liveStreamId}/revenue")]
    public async Task<ActionResult<decimal>> GetLiveStreamRevenue(int liveStreamId)
    {
        try
        {
            var revenue = await _monetizationService.GetLiveStreamRevenueAsync(liveStreamId);
            return Ok(revenue);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting live stream revenue");
            return BadRequest(ex.Message);
        }
    }

    [HttpPost("payment/process")]
    public async Task<ActionResult<bool>> ProcessPayment([FromBody] ProcessPaymentDto dto)
    {
        try
        {
            var userId = User.FindFirst("sub")?.Value ?? User.FindFirst("id")?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var result = await _monetizationService.ProcessPaymentAsync(userId, dto.Amount, dto.Currency, dto.PaymentMethod);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing payment");
            return BadRequest(ex.Message);
        }
    }

    [HttpPost("payment/refund")]
    public async Task<ActionResult<bool>> RefundPayment([FromBody] RefundPaymentDto dto)
    {
        try
        {
            var result = await _monetizationService.RefundPaymentAsync(dto.TransactionId, dto.Reason);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error refunding payment");
            return BadRequest(ex.Message);
        }
    }
}

public class ProcessPaymentDto
{
    public decimal Amount { get; set; }
    public string Currency { get; set; } = string.Empty;
    public string PaymentMethod { get; set; } = string.Empty;
}

public class RefundPaymentDto
{
    public int TransactionId { get; set; }
    public string Reason { get; set; } = string.Empty;
}
