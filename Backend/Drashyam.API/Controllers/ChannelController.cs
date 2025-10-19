using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Drashyam.API.DTOs;
using Drashyam.API.Services;

namespace Drashyam.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ChannelController : ControllerBase
{
    private readonly IChannelService _channelService;
    private readonly ILogger<ChannelController> _logger;

    public ChannelController(IChannelService channelService, ILogger<ChannelController> logger)
    {
        _channelService = channelService;
        _logger = logger;
    }

    [HttpPost]
    [Authorize]
    public async Task<ActionResult<ChannelDto>> Create([FromBody] ChannelCreateDto createDto)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
        var created = await _channelService.CreateChannelAsync(createDto, userId);
        return CreatedAtAction(nameof(GetById), new { channelId = created.Id }, created);
    }

    [HttpGet("{channelId:int}")]
    public async Task<ActionResult<ChannelDto>> GetById([FromRoute] int channelId)
    {
        var channel = await _channelService.GetChannelByIdAsync(channelId);
        return Ok(channel);
    }

    [HttpGet("by-url/{customUrl}")]
    public async Task<ActionResult<ChannelDto>> GetByCustomUrl([FromRoute] string customUrl)
    {
        var channel = await _channelService.GetChannelByCustomUrlAsync(customUrl);
        return Ok(channel);
    }

    [HttpPut("{channelId:int}")]
    [Authorize]
    public async Task<ActionResult<ChannelDto>> Update([FromRoute] int channelId, [FromBody] ChannelUpdateDto updateDto)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
        var updated = await _channelService.UpdateChannelAsync(channelId, updateDto, userId);
        return Ok(updated);
    }

    [HttpDelete("{channelId:int}")]
    [Authorize]
    public async Task<IActionResult> Delete([FromRoute] int channelId)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
        var result = await _channelService.DeleteChannelAsync(channelId, userId);
        if (!result) return NotFound();
        return NoContent();
    }

    [HttpGet]
    public async Task<ActionResult<PagedResult<ChannelDto>>> GetAll([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        var channels = await _channelService.GetChannelsAsync(page, pageSize);
        return Ok(channels);
    }

    [HttpGet("me")]
    [Authorize]
    public async Task<ActionResult<PagedResult<ChannelDto>>> GetMyChannels([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
        var channels = await _channelService.GetUserChannelsAsync(userId, page, pageSize);
        return Ok(channels);
    }

    [HttpGet("search")]
    public async Task<ActionResult<PagedResult<ChannelDto>>> Search([FromQuery] string query, [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        var channels = await _channelService.SearchChannelsAsync(query, page, pageSize);
        return Ok(channels);
    }

    [HttpPost("{channelId:int}/subscribe")]
    [Authorize]
    public async Task<ActionResult<ChannelDto>> Subscribe([FromRoute] int channelId)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
        var result = await _channelService.SubscribeToChannelAsync(channelId, userId);
        return Ok(result);
    }

    [HttpPost("{channelId:int}/unsubscribe")]
    [Authorize]
    public async Task<ActionResult<ChannelDto>> Unsubscribe([FromRoute] int channelId)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
        var result = await _channelService.UnsubscribeFromChannelAsync(channelId, userId);
        return Ok(result);
    }

    [HttpGet("{channelId:int}/is-subscribed")]
    [Authorize]
    public async Task<ActionResult<bool>> IsSubscribed([FromRoute] int channelId)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
        var result = await _channelService.IsSubscribedAsync(channelId, userId);
        return Ok(result);
    }

    [HttpGet("{channelId:int}/subscribers")]
    public async Task<ActionResult<PagedResult<UserDto>>> GetSubscribers([FromRoute] int channelId, [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        var result = await _channelService.GetChannelSubscribersAsync(channelId, page, pageSize);
        return Ok(result);
    }

    [HttpGet("subscribed")]
    [Authorize]
    public async Task<ActionResult<PagedResult<ChannelDto>>> GetSubscribedChannels([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
        var result = await _channelService.GetSubscribedChannelsAsync(userId, page, pageSize);
        return Ok(result);
    }

    [HttpPost("{channelId:int}/banner")]
    [Authorize]
    public async Task<ActionResult<ChannelDto>> UpdateBanner([FromRoute] int channelId, [FromForm] IFormFile bannerFile)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
        var result = await _channelService.UpdateChannelBannerAsync(channelId, bannerFile, userId);
        return Ok(result);
    }

    [HttpPost("{channelId:int}/profile-picture")]
    [Authorize]
    public async Task<ActionResult<ChannelDto>> UpdateProfilePicture([FromRoute] int channelId, [FromForm] IFormFile profilePicture)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
        var result = await _channelService.UpdateChannelProfilePictureAsync(channelId, profilePicture, userId);
        return Ok(result);
    }
}


