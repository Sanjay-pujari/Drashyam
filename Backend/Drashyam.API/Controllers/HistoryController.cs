using Drashyam.API.DTOs;
using Drashyam.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Drashyam.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class HistoryController : ControllerBase
{
    private readonly IHistoryService _historyService;
    private readonly ILogger<HistoryController> _logger;

    public HistoryController(IHistoryService historyService, ILogger<HistoryController> logger)
    {
        _historyService = historyService;
        _logger = logger;
    }

    [HttpGet]
    public async Task<ActionResult<List<HistoryDto>>> GetUserHistory([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
        
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized();
        }

        try
        {
            var history = await _historyService.GetUserHistoryAsync(userId, page, pageSize);
            return Ok(history);
        }
        catch (Exception ex)
        {
            return StatusCode(500, "An error occurred while retrieving history");
        }
    }

    [HttpPost]
    public async Task<ActionResult<HistoryDto>> AddToHistory([FromBody] HistoryCreateDto historyDto)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
        
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized();
        }

        try
        {
            var historyItem = await _historyService.AddToHistoryAsync(userId, historyDto);
            return Ok(historyItem);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            return StatusCode(500, "An error occurred while adding to history");
        }
    }

    [HttpPut("{historyId:int}")]
    public async Task<ActionResult<HistoryDto>> UpdateHistory([FromRoute] int historyId, [FromBody] HistoryUpdateDto historyDto)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
        
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized();
        }

        try
        {
            var historyItem = await _historyService.UpdateHistoryAsync(historyId, userId, historyDto);
            return Ok(historyItem);
        }
        catch (ArgumentException ex)
        {
            return NotFound(ex.Message);
        }
        catch (Exception ex)
        {
            return StatusCode(500, "An error occurred while updating history");
        }
    }

    [HttpDelete("{historyId:int}")]
    public async Task<ActionResult> RemoveFromHistory([FromRoute] int historyId)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
        
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized();
        }

        try
        {
            var success = await _historyService.RemoveFromHistoryAsync(historyId, userId);
            if (!success)
            {
                return NotFound("History item not found");
            }
            return NoContent();
        }
        catch (Exception ex)
        {
            return StatusCode(500, "An error occurred while removing from history");
        }
    }

    [HttpDelete("clear")]
    public async Task<ActionResult> ClearHistory()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
        
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized();
        }

        try
        {
            await _historyService.ClearUserHistoryAsync(userId);
            return NoContent();
        }
        catch (Exception ex)
        {
            return StatusCode(500, "An error occurred while clearing history");
        }
    }

    [HttpGet("{historyId:int}")]
    public async Task<ActionResult<HistoryDto>> GetHistoryItem([FromRoute] int historyId)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
        
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized();
        }

        try
        {
            var historyItem = await _historyService.GetHistoryItemAsync(historyId, userId);
            if (historyItem == null)
            {
                return NotFound("History item not found");
            }
            return Ok(historyItem);
        }
        catch (Exception ex)
        {
            return StatusCode(500, "An error occurred while retrieving history item");
        }
    }

    [HttpGet("check/{videoId:int}")]
    public async Task<ActionResult<bool>> IsVideoInHistory([FromRoute] int videoId)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
        
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized();
        }

        try
        {
            var isInHistory = await _historyService.IsVideoInHistoryAsync(videoId, userId);
            return Ok(isInHistory);
        }
        catch (Exception ex)
        {
            return StatusCode(500, "An error occurred while checking history");
        }
    }

    [HttpGet("count")]
    public async Task<ActionResult<int>> GetHistoryCount()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
        
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized();
        }

        try
        {
            var count = await _historyService.GetUserHistoryCountAsync(userId);
            return Ok(count);
        }
        catch (Exception ex)
        {
            return StatusCode(500, "An error occurred while getting history count");
        }
    }
}

