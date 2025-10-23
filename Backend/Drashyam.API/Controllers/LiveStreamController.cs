using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;
using Drashyam.API.DTOs;
using Drashyam.API.Services;
using Drashyam.API.Hubs;

namespace Drashyam.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class LiveStreamController : ControllerBase
{
    private readonly ILiveStreamService _liveStreamService;
    private readonly IHubContext<LiveStreamHub> _liveStreamHub;
    private readonly IHubContext<ChatHub> _chatHub;
    private readonly IHubContext<NotificationHub> _notificationHub;
    private readonly ILogger<LiveStreamController> _logger;

    public LiveStreamController(
        ILiveStreamService liveStreamService, 
        IHubContext<LiveStreamHub> liveStreamHub,
        IHubContext<ChatHub> chatHub,
        IHubContext<NotificationHub> notificationHub,
        ILogger<LiveStreamController> logger)
    {
        _liveStreamService = liveStreamService;
        _liveStreamHub = liveStreamHub;
        _chatHub = chatHub;
        _notificationHub = notificationHub;
        _logger = logger;
    }

    [HttpPost]
    [Authorize]
    public async Task<ActionResult<LiveStreamDto>> Create([FromBody] LiveStreamCreateDto createDto)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
        var stream = await _liveStreamService.CreateLiveStreamAsync(createDto, userId);
        return Ok(stream);
    }

    [HttpGet("{streamId:int}")]
    public async Task<ActionResult<LiveStreamDto>> GetById([FromRoute] int streamId)
    {
        var stream = await _liveStreamService.GetLiveStreamByIdAsync(streamId);
        return Ok(stream);
    }

    [HttpGet("by-key/{streamKey}")]
    public async Task<ActionResult<LiveStreamDto>> GetByKey([FromRoute] string streamKey)
    {
        var stream = await _liveStreamService.GetLiveStreamByStreamKeyAsync(streamKey);
        return Ok(stream);
    }

    [HttpPut("{streamId:int}")]
    [Authorize]
    public async Task<ActionResult<LiveStreamDto>> Update([FromRoute] int streamId, [FromBody] LiveStreamUpdateDto updateDto)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
        var stream = await _liveStreamService.UpdateLiveStreamAsync(streamId, updateDto, userId);
        return Ok(stream);
    }

    [HttpDelete("{streamId:int}")]
    [Authorize]
    public async Task<IActionResult> Delete([FromRoute] int streamId)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
        var result = await _liveStreamService.DeleteLiveStreamAsync(streamId, userId);
        if (!result) return NotFound();
        return NoContent();
    }

    [HttpGet("active")]
    public async Task<ActionResult<PagedResult<LiveStreamDto>>> GetActive([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        var streams = await _liveStreamService.GetActiveLiveStreamsAsync(page, pageSize);
        return Ok(streams);
    }

    [HttpGet("me")]
    [Authorize]
    public async Task<ActionResult<PagedResult<LiveStreamDto>>> GetMyStreams([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
        var streams = await _liveStreamService.GetUserLiveStreamsAsync(userId, page, pageSize);
        return Ok(streams);
    }

    [HttpGet("channel/{channelId:int}")]
    public async Task<ActionResult<PagedResult<LiveStreamDto>>> GetChannelStreams([FromRoute] int channelId, [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        var streams = await _liveStreamService.GetChannelLiveStreamsAsync(channelId, page, pageSize);
        return Ok(streams);
    }

    [HttpPost("{streamId:int}/start")]
    [Authorize]
    public async Task<ActionResult<LiveStreamDto>> Start([FromRoute] int streamId)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
        var stream = await _liveStreamService.StartLiveStreamAsync(streamId, userId);
        
        // Notify all viewers that stream started
        await _liveStreamHub.Clients.Group($"stream_{streamId}").SendAsync("StreamStarted", new
        {
            StreamId = streamId,
            StartedBy = userId,
            StartTime = DateTime.UtcNow,
            StreamUrl = stream.StreamUrl,
            HlsUrl = stream.HlsUrl
        });
        
        return Ok(stream);
    }

    [HttpPost("{streamId:int}/stop")]
    [Authorize]
    public async Task<ActionResult<LiveStreamDto>> Stop([FromRoute] int streamId)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
        var stream = await _liveStreamService.StopLiveStreamAsync(streamId, userId);
        
        // Notify all viewers that stream ended
        await _liveStreamHub.Clients.Group($"stream_{streamId}").SendAsync("StreamEnded", new
        {
            StreamId = streamId,
            EndedBy = userId,
            EndTime = DateTime.UtcNow
        });
        
        return Ok(stream);
    }

    [HttpPost("{streamId:int}/generate-key")]
    [Authorize]
    public async Task<ActionResult<object>> GenerateKey([FromRoute] int streamId)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
        var key = await _liveStreamService.GenerateStreamKeyAsync(streamId, userId);
        return Ok(new { streamKey = key });
    }

    [HttpGet("validate-key/{streamKey}")]
    public async Task<ActionResult<bool>> ValidateKey([FromRoute] string streamKey)
    {
        var valid = await _liveStreamService.ValidateStreamKeyAsync(streamKey);
        return Ok(valid);
    }

    [HttpPost("{streamId:int}/join")]
    [Authorize]
    public async Task<ActionResult<LiveStreamDto>> Join([FromRoute] int streamId)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
        var stream = await _liveStreamService.JoinLiveStreamAsync(streamId, userId);
        return Ok(stream);
    }

    [HttpPost("{streamId:int}/leave")]
    [Authorize]
    public async Task<ActionResult<LiveStreamDto>> Leave([FromRoute] int streamId)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
        var stream = await _liveStreamService.LeaveLiveStreamAsync(streamId, userId);
        return Ok(stream);
    }

    [HttpGet("{streamId:int}/viewers")]
    public async Task<ActionResult<int>> GetViewerCount([FromRoute] int streamId)
    {
        var count = await _liveStreamService.GetLiveStreamViewerCountAsync(streamId);
        return Ok(count);
    }

    [HttpPost("{streamId:int}/viewers/update")]
    [Authorize]
    public async Task<ActionResult> UpdateViewerCount([FromRoute] int streamId, [FromBody] long viewerCount)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
        
        // Update viewer count in database
        await _liveStreamService.UpdateViewerCountAsync(streamId, viewerCount, userId);
        
        // Broadcast viewer count update to all stream viewers
        await _liveStreamHub.Clients.Group($"stream_{streamId}").SendAsync("ViewerCountUpdated", new
        {
            StreamId = streamId,
            ViewerCount = viewerCount,
            Timestamp = DateTime.UtcNow
        });
        
        return Ok();
    }
}


