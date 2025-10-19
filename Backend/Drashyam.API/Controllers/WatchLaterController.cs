using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Drashyam.API.DTOs;
using Drashyam.API.Services;

namespace Drashyam.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class WatchLaterController : ControllerBase
{
    private readonly IWatchLaterService _watchLaterService;
    private readonly ILogger<WatchLaterController> _logger;

    public WatchLaterController(IWatchLaterService watchLaterService, ILogger<WatchLaterController> logger)
    {
        _watchLaterService = watchLaterService;
        _logger = logger;
    }

    [HttpGet]
    public async Task<ActionResult<PagedResult<WatchLaterResponseDto>>> GetWatchLater([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
        var result = await _watchLaterService.GetUserWatchLaterAsync(userId, page, pageSize);
        return Ok(result);
    }

    [HttpPost]
    public async Task<ActionResult<WatchLaterResponseDto>> AddToWatchLater([FromBody] WatchLaterCreateDto createDto)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
        var result = await _watchLaterService.AddToWatchLaterAsync(userId, createDto);
        return CreatedAtAction(nameof(GetWatchLater), new { }, result);
    }

    [HttpDelete("{videoId:int}")]
    public async Task<ActionResult> RemoveFromWatchLater([FromRoute] int videoId)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
        var removed = await _watchLaterService.RemoveFromWatchLaterAsync(userId, videoId);
        
        if (!removed)
        {
            return NotFound();
        }
        
        return NoContent();
    }

    [HttpGet("check/{videoId:int}")]
    public async Task<ActionResult<bool>> IsVideoInWatchLater([FromRoute] int videoId)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
        var isInWatchLater = await _watchLaterService.IsVideoInWatchLaterAsync(userId, videoId);
        return Ok(isInWatchLater);
    }

    [HttpDelete("clear")]
    public async Task<ActionResult> ClearWatchLater()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
        await _watchLaterService.ClearWatchLaterAsync(userId);
        return NoContent();
    }

    [HttpGet("count")]
    public async Task<ActionResult<int>> GetWatchLaterCount()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
        var count = await _watchLaterService.GetWatchLaterCountAsync(userId);
        return Ok(count);
    }
}
