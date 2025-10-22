using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Drashyam.API.Services;
using Drashyam.API.Models;

namespace Drashyam.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class VideoProcessingController : ControllerBase
{
    private readonly IVideoProcessingService _processingService;
    private readonly ILogger<VideoProcessingController> _logger;

    public VideoProcessingController(
        IVideoProcessingService processingService,
        ILogger<VideoProcessingController> logger)
    {
        _processingService = processingService;
        _logger = logger;
    }

    [HttpGet("progress/{videoId}")]
    public async Task<ActionResult<VideoProcessingProgress>> GetProcessingProgress(int videoId)
    {
        try
        {
            var progress = await _processingService.GetProcessingProgressAsync(videoId);
            return Ok(progress);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting processing progress for video {VideoId}", videoId);
            return StatusCode(500, "Error retrieving processing progress");
        }
    }

    [HttpGet("queue")]
    public async Task<ActionResult<List<VideoProcessingProgress>>> GetProcessingQueue()
    {
        try
        {
            var queue = await _processingService.GetProcessingQueueAsync();
            return Ok(queue);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting processing queue");
            return StatusCode(500, "Error retrieving processing queue");
        }
    }

    [HttpGet("status/{videoId}")]
    public async Task<ActionResult<bool>> IsVideoProcessing(int videoId)
    {
        try
        {
            var isProcessing = await _processingService.IsVideoProcessingAsync(videoId);
            return Ok(isProcessing);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking processing status for video {VideoId}", videoId);
            return StatusCode(500, "Error checking processing status");
        }
    }
}
