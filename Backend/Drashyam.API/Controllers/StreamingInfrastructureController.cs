using Drashyam.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Drashyam.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class StreamingInfrastructureController : ControllerBase
{
    private readonly IStreamingInfrastructureService _streamingService;
    private readonly IAzureMediaService _azureMediaService;
    private readonly ILogger<StreamingInfrastructureController> _logger;

    public StreamingInfrastructureController(
        IStreamingInfrastructureService streamingService,
        IAzureMediaService azureMediaService,
        ILogger<StreamingInfrastructureController> logger)
    {
        _streamingService = streamingService;
        _azureMediaService = azureMediaService;
        _logger = logger;
    }

    [HttpPost("start-stream")]
    public async Task<IActionResult> StartStream([FromBody] StartStreamRequest request)
    {
        try
        {
            var hlsUrl = await _streamingService.StartStreamAsync(request.StreamKey, request.RtmpUrl);
            
            return Ok(new
            {
                StreamKey = request.StreamKey,
                HlsUrl = hlsUrl,
                RtmpUrl = request.RtmpUrl,
                Status = "Started",
                Timestamp = DateTime.UtcNow
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error starting stream: {request.StreamKey}");
            return StatusCode(500, new { Error = "Failed to start stream" });
        }
    }

    [HttpPost("stop-stream")]
    public async Task<IActionResult> StopStream([FromBody] StopStreamRequest request)
    {
        try
        {
            await _streamingService.StopStreamAsync(request.StreamKey);
            
            return Ok(new
            {
                StreamKey = request.StreamKey,
                Status = "Stopped",
                Timestamp = DateTime.UtcNow
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error stopping stream: {request.StreamKey}");
            return StatusCode(500, new { Error = "Failed to stop stream" });
        }
    }

    [HttpGet("stream-status/{streamKey}")]
    public async Task<IActionResult> GetStreamStatus(string streamKey)
    {
        try
        {
            var status = await _streamingService.GetStreamStatusAsync(streamKey);
            return Ok(status);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error getting stream status: {streamKey}");
            return StatusCode(500, new { Error = "Failed to get stream status" });
        }
    }

    [HttpGet("stream-metrics/{streamKey}")]
    public async Task<IActionResult> GetStreamMetrics(string streamKey)
    {
        try
        {
            var metrics = await _streamingService.GetStreamMetricsAsync(streamKey);
            return Ok(metrics);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error getting stream metrics: {streamKey}");
            return StatusCode(500, new { Error = "Failed to get stream metrics" });
        }
    }

    [HttpGet("stream-health/{streamKey}")]
    public async Task<IActionResult> CheckStreamHealth(string streamKey)
    {
        try
        {
            var isHealthy = await _streamingService.IsStreamHealthyAsync(streamKey);
            return Ok(new { StreamKey = streamKey, IsHealthy = isHealthy, Timestamp = DateTime.UtcNow });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error checking stream health: {streamKey}");
            return StatusCode(500, new { Error = "Failed to check stream health" });
        }
    }

    [HttpGet("stream-qualities/{streamKey}")]
    public async Task<IActionResult> GetStreamQualities(string streamKey)
    {
        try
        {
            var qualities = await _streamingService.GetAvailableQualitiesAsync(streamKey);
            return Ok(qualities);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error getting stream qualities: {streamKey}");
            return StatusCode(500, new { Error = "Failed to get stream qualities" });
        }
    }

    [HttpPost("transcode-stream")]
    public async Task<IActionResult> TranscodeStream([FromBody] TranscodeStreamRequest request)
    {
        try
        {
            var hlsUrl = await _streamingService.TranscodeStreamAsync(request.StreamKey, request.Quality);
            
            return Ok(new
            {
                StreamKey = request.StreamKey,
                Quality = request.Quality,
                HlsUrl = hlsUrl,
                Timestamp = DateTime.UtcNow
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error transcoding stream: {request.StreamKey}");
            return StatusCode(500, new { Error = "Failed to transcode stream" });
        }
    }

    [HttpPost("start-recording")]
    public async Task<IActionResult> StartRecording([FromBody] StartRecordingRequest request)
    {
        try
        {
            var recording = await _streamingService.StartRecordingAsync(request.StreamKey);
            return Ok(recording);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error starting recording: {request.StreamKey}");
            return StatusCode(500, new { Error = "Failed to start recording" });
        }
    }

    [HttpPost("stop-recording")]
    public async Task<IActionResult> StopRecording([FromBody] StopRecordingRequest request)
    {
        try
        {
            var recording = await _streamingService.StopRecordingAsync(request.StreamKey);
            return Ok(recording);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error stopping recording: {request.StreamKey}");
            return StatusCode(500, new { Error = "Failed to stop recording" });
        }
    }

    [HttpGet("recordings/{streamKey}")]
    public async Task<IActionResult> GetRecordings(string streamKey)
    {
        try
        {
            var recordings = await _streamingService.GetRecordingsAsync(streamKey);
            return Ok(recordings);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error getting recordings: {streamKey}");
            return StatusCode(500, new { Error = "Failed to get recordings" });
        }
    }

    [HttpGet("thumbnail/{streamKey}")]
    public async Task<IActionResult> GetThumbnail(string streamKey)
    {
        try
        {
            var thumbnailUrl = await _streamingService.GenerateThumbnailAsync(streamKey);
            return Ok(new { StreamKey = streamKey, ThumbnailUrl = thumbnailUrl });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error generating thumbnail: {streamKey}");
            return StatusCode(500, new { Error = "Failed to generate thumbnail" });
        }
    }
}

public class StartStreamRequest
{
    public string StreamKey { get; set; } = string.Empty;
    public string RtmpUrl { get; set; } = string.Empty;
}

public class StopStreamRequest
{
    public string StreamKey { get; set; } = string.Empty;
}

public class TranscodeStreamRequest
{
    public string StreamKey { get; set; } = string.Empty;
    public string Quality { get; set; } = string.Empty;
}

public class StartRecordingRequest
{
    public string StreamKey { get; set; } = string.Empty;
}

public class StopRecordingRequest
{
    public string StreamKey { get; set; } = string.Empty;
}
