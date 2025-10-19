using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Drashyam.API.DTOs;
using Drashyam.API.Models;
using Drashyam.API.Services;

namespace Drashyam.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class VideoController : ControllerBase
{
    private readonly IVideoService _videoService;
    private readonly ILogger<VideoController> _logger;

    public VideoController(IVideoService videoService, ILogger<VideoController> logger)
    {
        _videoService = videoService;
        _logger = logger;
    }

    [HttpPost("upload")]
    [Authorize]
    public async Task<ActionResult<VideoDto>> Upload([FromForm] VideoUploadDto uploadDto)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
        var video = await _videoService.UploadVideoAsync(uploadDto, userId);
        return CreatedAtAction(nameof(GetById), new { id = video.Id }, video);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<VideoDto>> GetById([FromRoute] int id)
    {
        var video = await _videoService.GetVideoByIdAsync(id);
        return Ok(video);
    }

    [HttpGet("share/{token}")]
    public async Task<ActionResult<VideoDto>> GetByShareToken([FromRoute] string token)
    {
        var video = await _videoService.GetVideoByShareTokenAsync(token);
        return Ok(video);
    }

    [HttpGet]
    public async Task<ActionResult<PagedResult<VideoDto>>> Get([FromQuery] VideoFilterDto filter)
    {
        var videos = await _videoService.GetVideosAsync(filter);
        return Ok(videos);
    }

    [HttpGet("me")]
    [Authorize]
    public async Task<ActionResult<PagedResult<VideoDto>>> GetMyVideos([FromQuery] VideoFilterDto filter)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
        var videos = await _videoService.GetUserVideosAsync(userId, filter);
        return Ok(videos);
    }

    [HttpGet("channel/{channelId:int}")]
    public async Task<ActionResult<PagedResult<VideoDto>>> GetChannelVideos([FromRoute] int channelId, [FromQuery] VideoFilterDto filter)
    {
        var videos = await _videoService.GetChannelVideosAsync(channelId, filter);
        return Ok(videos);
    }

    [HttpPut("{id:int}")]
    [Authorize]
    public async Task<ActionResult<VideoDto>> Update([FromRoute] int id, [FromForm] VideoUpdateDto updateDto)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
        var updated = await _videoService.UpdateVideoAsync(id, updateDto, userId);
        return Ok(updated);
    }

    [HttpDelete("{id:int}")]
    [Authorize]
    public async Task<IActionResult> Delete([FromRoute] int id)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
        await _videoService.DeleteVideoAsync(id, userId);
        return NoContent();
    }

    [HttpPost("{id:int}/like")]
    [Authorize]
    public async Task<ActionResult<VideoDto>> Like([FromRoute] int id, [FromQuery] Drashyam.API.DTOs.LikeType type = Drashyam.API.DTOs.LikeType.Like)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
        var result = await _videoService.LikeVideoAsync(id, userId, type);
        return Ok(result);
    }

    [HttpPost("{id:int}/unlike")]
    [Authorize]
    public async Task<ActionResult<VideoDto>> Unlike([FromRoute] int id)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
        var result = await _videoService.UnlikeVideoAsync(id, userId);
        return Ok(result);
    }

    [HttpPost("{id:int}/view")]
    [Authorize]
    public async Task<ActionResult<VideoDto>> RecordView([FromRoute] int id, [FromQuery] int secondsWatched, [FromHeader] string? userAgent = null)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
        _logger.LogInformation("Recording view for video {VideoId} by user {UserId}, duration: {Duration} seconds", id, userId, secondsWatched);
        var updatedVideo = await _videoService.RecordVideoViewAsync(id, userId, TimeSpan.FromSeconds(secondsWatched), userAgent, Request.HttpContext.Connection.RemoteIpAddress?.ToString());
        _logger.LogInformation("View recorded successfully. New view count: {ViewCount}", updatedVideo.ViewCount);
        return Ok(updatedVideo);
    }

    [HttpPost("{id:int}/share")]
    [Authorize]
    public async Task<ActionResult<object>> GenerateShareLink([FromRoute] int id)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
        var token = await _videoService.GenerateShareLinkAsync(id, userId);
        return Ok(new { shareToken = token });
    }

    [HttpGet("shared/{token}")]
    public async Task<ActionResult<VideoDto>> GetVideoByShareToken([FromRoute] string token)
    {
        var video = await _videoService.GetVideoByShareTokenAsync(token);
        return Ok(video);
    }

    [HttpGet("search/all")]
    public async Task<ActionResult<PagedResult<VideoDto>>> Search([FromQuery] string query, [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        var filter = new VideoFilterDto { Page = page, PageSize = pageSize };
        var results = await _videoService.SearchVideosAsync(query, filter);
        return Ok(results);
    }

    [HttpGet("trending")]
    public async Task<ActionResult<PagedResult<VideoDto>>> Trending([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        var filter = new VideoFilterDto { Page = page, PageSize = pageSize };
        var results = await _videoService.GetTrendingVideosAsync(filter);
        return Ok(results);
    }

    [HttpGet("recommended")]
    [Authorize]
    public async Task<ActionResult<PagedResult<VideoDto>>> Recommended([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
        var filter = new VideoFilterDto { Page = page, PageSize = pageSize };
        var results = await _videoService.GetRecommendedVideosAsync(userId, filter);
        return Ok(results);
    }

    [HttpGet("favorites")]
    [Authorize]
    public async Task<ActionResult<PagedResult<VideoDto>>> Favorites([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
        var filter = new VideoFilterDto { Page = page, PageSize = pageSize };
        var results = await _videoService.GetUserFavoriteVideosAsync(userId, filter);
        return Ok(results);
    }

    [HttpGet("subscribed-feed")]
    [Authorize]
    public async Task<ActionResult<PagedResult<VideoDto>>> SubscribedFeed([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
        var filter = new VideoFilterDto { Page = page, PageSize = pageSize };
        var results = await _videoService.GetSubscribedChannelsVideosAsync(userId, filter);
        return Ok(results);
    }

    [HttpGet("home-feed")]
    [Authorize]
    public async Task<ActionResult<HomeFeedDto>> HomeFeed([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
        var filter = new VideoFilterDto { Page = page, PageSize = pageSize };

        var feed = new HomeFeedDto
        {
            Trending = await _videoService.GetTrendingVideosAsync(filter),
            Recommended = await _videoService.GetRecommendedVideosAsync(userId, filter),
            Subscribed = await _videoService.GetSubscribedChannelsVideosAsync(userId, filter)
        };

        return Ok(feed);
    }

    [HttpPatch("{id:int}/status")]
    [Authorize]
    public async Task<ActionResult<VideoDto>> UpdateVideoStatus([FromRoute] int id, [FromBody] UpdateVideoStatusDto statusDto)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
        var video = await _videoService.UpdateVideoStatusAsync(id, userId, statusDto.Status);
        return Ok(video);
    }

    [HttpPost("update-processing-videos")]
    [Authorize]
    public async Task<ActionResult<int>> UpdateProcessingVideosToReady()
    {
        var updatedCount = await _videoService.UpdateProcessingVideosToReadyAsync();
        return Ok(new { updatedCount, message = $"Updated {updatedCount} videos from Processing to Ready status" });
    }
}


