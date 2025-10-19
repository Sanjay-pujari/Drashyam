using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Drashyam.API.DTOs;
using Drashyam.API.Services;

namespace Drashyam.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class PlaylistController : ControllerBase
{
    private readonly IPlaylistService _playlistService;
    private readonly ILogger<PlaylistController> _logger;

    public PlaylistController(IPlaylistService playlistService, ILogger<PlaylistController> logger)
    {
        _playlistService = playlistService;
        _logger = logger;
    }

    [HttpGet]
    public async Task<ActionResult<PagedResult<PlaylistDto>>> GetPlaylists([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
        var result = await _playlistService.GetUserPlaylistsAsync(userId, page, pageSize);
        return Ok(result);
    }

    [HttpPost]
    public async Task<ActionResult<PlaylistDto>> CreatePlaylist([FromBody] PlaylistCreateDto createDto)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
        var result = await _playlistService.CreatePlaylistAsync(userId, createDto);
        return CreatedAtAction(nameof(GetPlaylist), new { id = result.Id }, result);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<PlaylistDto>> GetPlaylist([FromRoute] int id)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
        var result = await _playlistService.GetPlaylistByIdAsync(id, userId);
        return Ok(result);
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<PlaylistDto>> UpdatePlaylist([FromRoute] int id, [FromBody] PlaylistUpdateDto updateDto)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
        var result = await _playlistService.UpdatePlaylistAsync(id, userId, updateDto);
        return Ok(result);
    }

    [HttpDelete("{id:int}")]
    public async Task<ActionResult> DeletePlaylist([FromRoute] int id)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
        var deleted = await _playlistService.DeletePlaylistAsync(id, userId);
        
        if (!deleted)
        {
            return NotFound();
        }
        
        return NoContent();
    }

    [HttpGet("{id:int}/videos")]
    public async Task<ActionResult<PagedResult<PlaylistVideoDto>>> GetPlaylistVideos([FromRoute] int id, [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
        var result = await _playlistService.GetPlaylistVideosAsync(id, userId, page, pageSize);
        return Ok(result);
    }

    [HttpPost("{id:int}/videos")]
    public async Task<ActionResult<PlaylistVideoDto>> AddVideoToPlaylist([FromRoute] int id, [FromBody] PlaylistVideoCreateDto createDto)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
        var result = await _playlistService.AddVideoToPlaylistAsync(id, userId, createDto);
        return CreatedAtAction(nameof(GetPlaylistVideos), new { id }, result);
    }

    [HttpDelete("{id:int}/videos/{videoId:int}")]
    public async Task<ActionResult> RemoveVideoFromPlaylist([FromRoute] int id, [FromRoute] int videoId)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
        var removed = await _playlistService.RemoveVideoFromPlaylistAsync(id, videoId, userId);
        
        if (!removed)
        {
            return NotFound();
        }
        
        return NoContent();
    }

    [HttpPut("{id:int}/videos/reorder")]
    public async Task<ActionResult> ReorderPlaylistVideos([FromRoute] int id, [FromBody] List<PlaylistVideoUpdateDto> updates)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
        var reordered = await _playlistService.ReorderPlaylistVideosAsync(id, userId, updates);
        
        if (!reordered)
        {
            return NotFound();
        }
        
        return NoContent();
    }
}
