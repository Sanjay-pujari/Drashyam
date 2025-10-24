using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Drashyam.API.DTOs;
using Drashyam.API.Services;

namespace Drashyam.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class MerchandiseController : ControllerBase
{
    private readonly IMonetizationService _monetizationService;
    private readonly ILogger<MerchandiseController> _logger;

    public MerchandiseController(IMonetizationService monetizationService, ILogger<MerchandiseController> logger)
    {
        _monetizationService = monetizationService;
        _logger = logger;
    }

    [HttpGet]
    public async Task<ActionResult<List<MerchandiseDto>>> GetMerchandise([FromQuery] MerchandiseFilterDto filter)
    {
        try
        {
            var merchandise = await _monetizationService.GetMerchandiseAsync(filter);
            return Ok(merchandise);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting merchandise");
            return StatusCode(500, "Error retrieving merchandise");
        }
    }

    [HttpGet("{merchandiseId:int}")]
    public async Task<ActionResult<MerchandiseDto>> GetMerchandiseDetails([FromRoute] int merchandiseId)
    {
        try
        {
            var merchandise = await _monetizationService.GetMerchandiseDetailsAsync(merchandiseId);
            if (merchandise == null) return NotFound();
            return Ok(merchandise);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting merchandise details");
            return StatusCode(500, "Error retrieving merchandise details");
        }
    }

    [HttpPost]
    [Authorize]
    public async Task<ActionResult<MerchandiseDto>> CreateMerchandise([FromBody] MerchandiseRequestDto request)
    {
        try
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
            var merchandise = await _monetizationService.CreateMerchandiseAsync(request, userId);
            return CreatedAtAction(nameof(GetMerchandiseDetails), new { merchandiseId = merchandise.Id }, merchandise);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating merchandise");
            return StatusCode(500, "Error creating merchandise");
        }
    }

    [HttpPut("{merchandiseId:int}")]
    [Authorize]
    public async Task<ActionResult<MerchandiseDto>> UpdateMerchandise([FromRoute] int merchandiseId, [FromBody] MerchandiseRequestDto request)
    {
        try
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
            var merchandise = await _monetizationService.UpdateMerchandiseAsync(merchandiseId, request, userId);
            return Ok(merchandise);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating merchandise");
            return StatusCode(500, "Error updating merchandise");
        }
    }

    [HttpDelete("{merchandiseId:int}")]
    [Authorize]
    public async Task<IActionResult> DeleteMerchandise([FromRoute] int merchandiseId)
    {
        try
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
            await _monetizationService.DeleteMerchandiseAsync(merchandiseId, userId);
            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting merchandise");
            return StatusCode(500, "Error deleting merchandise");
        }
    }

    [HttpGet("orders")]
    [Authorize]
    public async Task<ActionResult<PagedResult<MerchandiseOrderDto>>> GetOrders([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        try
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
            var orders = await _monetizationService.GetMerchandiseOrdersAsync(userId, page, pageSize);
            return Ok(orders);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting merchandise orders");
            return StatusCode(500, "Error retrieving merchandise orders");
        }
    }

    [HttpPost("orders")]
    [Authorize]
    public async Task<ActionResult<MerchandiseOrderDto>> CreateOrder([FromBody] MerchandiseOrderRequestDto request)
    {
        try
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
            var order = await _monetizationService.CreateMerchandiseOrderAsync(request, userId);
            return CreatedAtAction(nameof(GetOrder), new { orderId = order.Id }, order);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating merchandise order");
            return StatusCode(500, "Error creating merchandise order");
        }
    }

    [HttpGet("orders/{orderId:int}")]
    [Authorize]
    public async Task<ActionResult<MerchandiseOrderDto>> GetOrder([FromRoute] int orderId)
    {
        try
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
            var order = await _monetizationService.GetMerchandiseOrderAsync(orderId, userId);
            if (order == null) return NotFound();
            return Ok(order);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting merchandise order");
            return StatusCode(500, "Error retrieving merchandise order");
        }
    }

    [HttpPut("orders/{orderId:int}")]
    [Authorize]
    public async Task<ActionResult<MerchandiseOrderDto>> UpdateOrder([FromRoute] int orderId, [FromBody] MerchandiseOrderUpdateDto update)
    {
        try
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
            var order = await _monetizationService.UpdateMerchandiseOrderAsync(orderId, update, userId);
            return Ok(order);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating merchandise order");
            return StatusCode(500, "Error updating merchandise order");
        }
    }

    [HttpGet("analytics")]
    [Authorize]
    public async Task<ActionResult<MerchandiseAnalyticsDto>> GetAnalytics()
    {
        try
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
            var analytics = await _monetizationService.GetMerchandiseAnalyticsAsync(userId);
            return Ok(analytics);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting merchandise analytics");
            return StatusCode(500, "Error retrieving merchandise analytics");
        }
    }
}
