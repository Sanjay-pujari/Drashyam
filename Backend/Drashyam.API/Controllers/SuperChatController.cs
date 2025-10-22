using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Drashyam.API.DTOs;
using Drashyam.API.Services;

namespace Drashyam.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SuperChatController : ControllerBase
{
    private readonly ISuperChatService _superChatService;
    private readonly ILogger<SuperChatController> _logger;

    public SuperChatController(ISuperChatService superChatService, ILogger<SuperChatController> logger)
    {
        _superChatService = superChatService;
        _logger = logger;
    }

    [HttpPost]
    [Authorize]
    public async Task<ActionResult<SuperChatDto>> ProcessSuperChat([FromBody] SuperChatRequestDto request)
    {
        try
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
            var superChat = await _superChatService.ProcessSuperChatAsync(request, userId);
            return Ok(superChat);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing super chat");
            return StatusCode(500, "An error occurred while processing super chat");
        }
    }

    [HttpGet("live-stream/{liveStreamId:int}")]
    public async Task<ActionResult<List<SuperChatDisplayDto>>> GetLiveStreamSuperChats([FromRoute] int liveStreamId)
    {
        try
        {
            var superChats = await _superChatService.GetLiveStreamSuperChatsAsync(liveStreamId);
            return Ok(superChats);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving super chats for live stream {LiveStreamId}", liveStreamId);
            return StatusCode(500, "An error occurred while retrieving super chats");
        }
    }

    [HttpGet("{superChatId:int}")]
    [Authorize]
    public async Task<ActionResult<SuperChatDto>> GetSuperChat([FromRoute] int superChatId)
    {
        try
        {
            var superChat = await _superChatService.GetSuperChatByIdAsync(superChatId);
            return Ok(superChat);
        }
        catch (ArgumentException ex)
        {
            return NotFound(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving super chat {SuperChatId}", superChatId);
            return StatusCode(500, "An error occurred while retrieving super chat");
        }
    }

    [HttpGet("user")]
    [Authorize]
    public async Task<ActionResult<List<SuperChatDto>>> GetUserSuperChats([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        try
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
            var superChats = await _superChatService.GetUserSuperChatsAsync(userId, page, pageSize);
            return Ok(superChats);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving user super chats");
            return StatusCode(500, "An error occurred while retrieving super chats");
        }
    }

    [HttpGet("creator")]
    [Authorize]
    public async Task<ActionResult<List<SuperChatDto>>> GetCreatorSuperChats([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        try
        {
            var creatorId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
            var superChats = await _superChatService.GetCreatorSuperChatsAsync(creatorId, page, pageSize);
            return Ok(superChats);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving creator super chats");
            return StatusCode(500, "An error occurred while retrieving super chats");
        }
    }

    [HttpGet("revenue")]
    [Authorize]
    public async Task<ActionResult<decimal>> GetSuperChatRevenue([FromQuery] DateTime? startDate = null, [FromQuery] DateTime? endDate = null)
    {
        try
        {
            var creatorId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
            var revenue = await _superChatService.GetTotalSuperChatRevenueAsync(creatorId, startDate, endDate);
            return Ok(revenue);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving super chat revenue");
            return StatusCode(500, "An error occurred while retrieving revenue");
        }
    }
}
