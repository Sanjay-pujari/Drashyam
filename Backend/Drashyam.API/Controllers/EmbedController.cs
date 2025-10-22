using Microsoft.AspNetCore.Mvc;
using Drashyam.API.Services;
using Drashyam.API.DTOs;

namespace Drashyam.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class EmbedController : ControllerBase
{
    private readonly IVideoService _videoService;
    private readonly ILogger<EmbedController> _logger;

    public EmbedController(IVideoService videoService, ILogger<EmbedController> logger)
    {
        _videoService = videoService;
        _logger = logger;
    }

    [HttpGet("{id:int}/embed")]
    public async Task<ActionResult<object>> GetEmbedCode([FromRoute] int id)
    {
        try
        {
            var video = await _videoService.GetVideoByIdAsync(id);
            var baseUrl = $"{Request.Scheme}://{Request.Host}";
            var embedUrl = $"{baseUrl}/embed/{video.ShareToken}";
            var embedCode = $"<iframe src=\"{embedUrl}\" width=\"560\" height=\"315\" frameborder=\"0\" allowfullscreen></iframe>";
            
            return Ok(new { 
                embedUrl = embedUrl,
                embedCode = embedCode,
                width = 560,
                height = 315,
                videoId = video.Id,
                title = video.Title
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating embed code for video {VideoId}", id);
            return NotFound("Video not found");
        }
    }
}
