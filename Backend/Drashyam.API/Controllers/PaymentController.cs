using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Drashyam.API.DTOs;
using Drashyam.API.Services;

namespace Drashyam.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PaymentController : ControllerBase
{
    private readonly IPaymentService _paymentService;
    private readonly ILogger<PaymentController> _logger;

    public PaymentController(IPaymentService paymentService, ILogger<PaymentController> logger)
    {
        _paymentService = paymentService;
        _logger = logger;
    }

    // Helper method to convert DateTime parameters to UTC for PostgreSQL compatibility
    private static (DateTime? utcStartDate, DateTime? utcEndDate) ConvertToUtc(DateTime? startDate, DateTime? endDate)
    {
        var utcStartDate = startDate?.Kind == DateTimeKind.Unspecified ? DateTime.SpecifyKind(startDate.Value, DateTimeKind.Utc) : startDate;
        var utcEndDate = endDate?.Kind == DateTimeKind.Unspecified ? DateTime.SpecifyKind(endDate.Value, DateTimeKind.Utc) : endDate;
        return (utcStartDate, utcEndDate);
    }

    [HttpPost("process")]
    [Authorize]
    public async Task<ActionResult<PaymentResultDto>> Process([FromBody] PaymentDto payment)
    {
        var result = await _paymentService.ProcessPaymentAsync(payment);
        return Ok(result);
    }

    [HttpPost("subscription")]
    [Authorize]
    public async Task<ActionResult<PaymentResultDto>> ProcessSubscription([FromBody] SubscriptionPaymentDto payment)
    {
        var result = await _paymentService.ProcessSubscriptionPaymentAsync(payment);
        return Ok(result);
    }

    [HttpPost("refund/{paymentIntentId}")]
    [Authorize]
    public async Task<ActionResult<bool>> Refund([FromRoute] string paymentIntentId, [FromQuery] decimal? amount = null)
    {
        var result = await _paymentService.RefundPaymentAsync(paymentIntentId, amount);
        return Ok(result);
    }

    [HttpPost("intent")]
    [Authorize]
    public async Task<ActionResult<PaymentResultDto>> CreateIntent([FromQuery] decimal amount, [FromQuery] string currency, [FromQuery] string customerId)
    {
        var result = await _paymentService.CreatePaymentIntentAsync(amount, currency, customerId);
        return Ok(result);
    }

    [HttpPost("confirm/{paymentIntentId}")]
    [Authorize]
    public async Task<ActionResult<PaymentResultDto>> Confirm([FromRoute] string paymentIntentId)
    {
        var result = await _paymentService.ConfirmPaymentAsync(paymentIntentId);
        return Ok(result);
    }

    [HttpGet("status/{paymentIntentId}")]
    [Authorize]
    public async Task<ActionResult<PaymentResultDto>> Status([FromRoute] string paymentIntentId)
    {
        var result = await _paymentService.GetPaymentStatusAsync(paymentIntentId);
        return Ok(result);
    }

    [HttpPost("customer")]
    [Authorize]
    public async Task<ActionResult<bool>> CreateCustomer([FromQuery] string userId, [FromQuery] string email, [FromQuery] string name)
    {
        var ok = await _paymentService.CreateCustomerAsync(userId, email, name);
        return Ok(ok);
    }

    [HttpPut("customer/{customerId}")]
    [Authorize]
    public async Task<ActionResult<bool>> UpdateCustomer([FromRoute] string customerId, [FromBody] CustomerUpdateDto updateDto)
    {
        var ok = await _paymentService.UpdateCustomerAsync(customerId, updateDto);
        return Ok(ok);
    }

    [HttpDelete("customer/{customerId}")]
    [Authorize]
    public async Task<ActionResult<bool>> DeleteCustomer([FromRoute] string customerId)
    {
        var ok = await _paymentService.DeleteCustomerAsync(customerId);
        return Ok(ok);
    }

    [HttpGet("history/{userId}")]
    [Authorize]
    public async Task<ActionResult<PagedResult<PaymentHistoryDto>>> History([FromRoute] string userId, [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        var history = await _paymentService.GetPaymentHistoryAsync(userId, page, pageSize);
        return Ok(history);
    }

    [HttpGet("revenue/{userId}")]
    [Authorize]
    public async Task<ActionResult<decimal>> Revenue([FromRoute] string userId, [FromQuery] DateTime? startDate = null, [FromQuery] DateTime? endDate = null)
    {
        var (utcStartDate, utcEndDate) = ConvertToUtc(startDate, endDate);
        var revenue = await _paymentService.CalculateRevenueAsync(userId, utcStartDate, utcEndDate);
        return Ok(revenue);
    }
}


