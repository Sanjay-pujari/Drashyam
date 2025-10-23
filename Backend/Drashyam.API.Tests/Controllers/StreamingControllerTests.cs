using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using Drashyam.API.Controllers;
using Drashyam.API.DTOs;
using Drashyam.API.Services;

namespace Drashyam.API.Tests.Controllers;

public class StreamingControllerTests
{
    private readonly Mock<IStreamingService> _mockStreamingService;
    private readonly Mock<ILogger<StreamingController>> _mockLogger;
    private readonly StreamingController _controller;

    public StreamingControllerTests()
    {
        _mockStreamingService = new Mock<IStreamingService>();
        _mockLogger = new Mock<ILogger<StreamingController>>();

        _controller = new StreamingController(_mockStreamingService.Object, _mockLogger.Object);
    }

    [Fact]
    public async Task GetStreamConfiguration_ValidId_ReturnsOk()
    {
        // Arrange
        var liveStreamId = 1;
        var expectedConfig = new StreamConfigDto
        {
            LiveStreamId = liveStreamId,
            StreamKey = "test-stream-key",
            IngestUrl = "rtmp://test-ingest-server/live",
            PlaybackUrl = "https://test-cdn.com/live/test-stream-key",
            HlsUrl = "https://test-cdn.com/hls/test-stream-key.m3u8",
            Status = DTOs.LiveStreamStatus.Live,
            IsRecordingEnabled = true,
            AvailableQualities = new List<string> { "1080p", "720p", "480p" },
            CurrentQuality = "1080p",
            IsChatEnabled = true,
            IsMonetizationEnabled = true
        };

        _mockStreamingService.Setup(s => s.GetStreamConfigurationAsync(liveStreamId))
            .ReturnsAsync(expectedConfig);

        // Act
        var result = await _controller.GetStreamConfiguration(liveStreamId);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var config = Assert.IsType<StreamConfigDto>(okResult.Value);
        Assert.Equal(expectedConfig.LiveStreamId, config.LiveStreamId);
        Assert.Equal(expectedConfig.StreamKey, config.StreamKey);
        Assert.Equal(expectedConfig.IngestUrl, config.IngestUrl);
        Assert.Equal(expectedConfig.PlaybackUrl, config.PlaybackUrl);
        Assert.Equal(expectedConfig.HlsUrl, config.HlsUrl);
    }

    [Fact]
    public async Task GetStreamConfiguration_InvalidId_ReturnsNotFound()
    {
        // Arrange
        var liveStreamId = 999;
        _mockStreamingService.Setup(s => s.GetStreamConfigurationAsync(liveStreamId))
            .ThrowsAsync(new ArgumentException("Live stream not found"));

        // Act
        var result = await _controller.GetStreamConfiguration(liveStreamId);

        // Assert
        var notFoundResult = Assert.IsType<NotFoundObjectResult>(result.Result);
        Assert.Equal("Live stream not found", notFoundResult.Value);
    }

    [Fact]
    public async Task GetStreamHealth_ValidId_ReturnsOk()
    {
        // Arrange
        var liveStreamId = 1;
        var expectedHealth = new StreamHealthDto
        {
            LiveStreamId = liveStreamId,
            Status = StreamHealthStatus.Healthy,
            CpuUsage = 25.5,
            MemoryUsage = 40.2,
            NetworkIn = 10.5,
            NetworkOut = 5.3,
            Latency = 150,
            FrameRate = 30,
            Bitrate = 5000,
            Alerts = new List<StreamAlertDto>(),
            LastUpdated = DateTime.UtcNow
        };

        _mockStreamingService.Setup(s => s.GetStreamHealthAsync(liveStreamId))
            .ReturnsAsync(expectedHealth);

        // Act
        var result = await _controller.GetStreamHealth(liveStreamId);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var health = Assert.IsType<StreamHealthDto>(okResult.Value);
        Assert.Equal(expectedHealth.LiveStreamId, health.LiveStreamId);
        Assert.Equal(expectedHealth.Status, health.Status);
        Assert.Equal(expectedHealth.CpuUsage, health.CpuUsage);
        Assert.Equal(expectedHealth.MemoryUsage, health.MemoryUsage);
    }

    [Fact]
    public async Task GetStreamHealth_InvalidId_ReturnsNotFound()
    {
        // Arrange
        var liveStreamId = 999;
        _mockStreamingService.Setup(s => s.GetStreamHealthAsync(liveStreamId))
            .ThrowsAsync(new ArgumentException("Live stream not found"));

        // Act
        var result = await _controller.GetStreamHealth(liveStreamId);

        // Assert
        var notFoundResult = Assert.IsType<NotFoundObjectResult>(result.Result);
        Assert.Equal("Live stream not found", notFoundResult.Value);
    }

    [Fact]
    public async Task UpdateStreamHealth_ValidRequest_ReturnsNoContent()
    {
        // Arrange
        var liveStreamId = 1;
        var healthUpdate = new StreamHealthUpdateDto
        {
            Status = StreamHealthStatus.Healthy,
            CpuUsage = 25.5,
            MemoryUsage = 40.2,
            NetworkIn = 10.5,
            NetworkOut = 5.3,
            Latency = 150,
            FrameRate = 30,
            Bitrate = 5000
        };

        _mockStreamingService.Setup(s => s.UpdateStreamHealthAsync(liveStreamId, healthUpdate))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _controller.UpdateStreamHealth(liveStreamId, healthUpdate);

        // Assert
        Assert.IsType<NoContentResult>(result);
    }

    [Fact]
    public async Task UpdateStreamHealth_InvalidId_ReturnsNotFound()
    {
        // Arrange
        var liveStreamId = 999;
        var healthUpdate = new StreamHealthUpdateDto
        {
            Status = StreamHealthStatus.Healthy,
            CpuUsage = 25.5,
            MemoryUsage = 40.2,
            NetworkIn = 10.5,
            NetworkOut = 5.3,
            Latency = 150,
            FrameRate = 30,
            Bitrate = 5000
        };

        _mockStreamingService.Setup(s => s.UpdateStreamHealthAsync(liveStreamId, healthUpdate))
            .ThrowsAsync(new ArgumentException("Live stream not found"));

        // Act
        var result = await _controller.UpdateStreamHealth(liveStreamId, healthUpdate);

        // Assert
        var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
        Assert.Equal("Live stream not found", notFoundResult.Value);
    }

    [Fact]
    public async Task GetStreamQuality_ValidId_ReturnsOk()
    {
        // Arrange
        var liveStreamId = 1;
        var expectedQuality = new StreamQualityDto
        {
            LiveStreamId = liveStreamId,
            Quality = "1080p",
            Bitrate = 5000,
            FrameRate = 30,
            Resolution = "1920x1080",
            IsActive = true
        };

        _mockStreamingService.Setup(s => s.GetStreamQualityAsync(liveStreamId))
            .ReturnsAsync(expectedQuality);

        // Act
        var result = await _controller.GetStreamQuality(liveStreamId);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var quality = Assert.IsType<StreamQualityDto>(okResult.Value);
        Assert.Equal(expectedQuality.LiveStreamId, quality.LiveStreamId);
        Assert.Equal(expectedQuality.Quality, quality.Quality);
        Assert.Equal(expectedQuality.Bitrate, quality.Bitrate);
        Assert.Equal(expectedQuality.FrameRate, quality.FrameRate);
        Assert.Equal(expectedQuality.Resolution, quality.Resolution);
    }

    [Fact]
    public async Task GetStreamQuality_InvalidId_ReturnsNotFound()
    {
        // Arrange
        var liveStreamId = 999;
        _mockStreamingService.Setup(s => s.GetStreamQualityAsync(liveStreamId))
            .ThrowsAsync(new ArgumentException("Live stream not found"));

        // Act
        var result = await _controller.GetStreamQuality(liveStreamId);

        // Assert
        var notFoundResult = Assert.IsType<NotFoundObjectResult>(result.Result);
        Assert.Equal("Live stream not found", notFoundResult.Value);
    }

    [Fact]
    public async Task UpdateStreamQuality_ValidRequest_ReturnsNoContent()
    {
        // Arrange
        var liveStreamId = 1;
        var quality = "720p";

        _mockStreamingService.Setup(s => s.UpdateStreamQualityAsync(liveStreamId, quality))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _controller.UpdateStreamQuality(liveStreamId, quality);

        // Assert
        Assert.IsType<NoContentResult>(result);
    }

    [Fact]
    public async Task UpdateStreamQuality_InvalidId_ReturnsNotFound()
    {
        // Arrange
        var liveStreamId = 999;
        var quality = "720p";

        _mockStreamingService.Setup(s => s.UpdateStreamQualityAsync(liveStreamId, quality))
            .ThrowsAsync(new ArgumentException("Live stream not found"));

        // Act
        var result = await _controller.UpdateStreamQuality(liveStreamId, quality);

        // Assert
        var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
        Assert.Equal("Live stream not found", notFoundResult.Value);
    }

    [Fact]
    public async Task UpdateStreamQuality_InvalidQuality_ReturnsBadRequest()
    {
        // Arrange
        var liveStreamId = 1;
        var quality = "invalid-quality";

        _mockStreamingService.Setup(s => s.UpdateStreamQualityAsync(liveStreamId, quality))
            .ThrowsAsync(new InvalidOperationException("Invalid quality"));

        // Act
        var result = await _controller.UpdateStreamQuality(liveStreamId, quality);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal("Invalid quality", badRequestResult.Value);
    }

    [Fact]
    public async Task StartRecording_ValidId_ReturnsOk()
    {
        // Arrange
        var liveStreamId = 1;
        var expectedUrl = "https://test-storage.com/recordings/stream-1-recording.mp4";

        _mockStreamingService.Setup(s => s.StartRecordingAsync(liveStreamId))
            .ReturnsAsync(expectedUrl);

        // Act
        var result = await _controller.StartRecording(liveStreamId);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var response = Assert.IsType<object>(okResult.Value);
        Assert.NotNull(response);
    }

    [Fact]
    public async Task StartRecording_InvalidId_ReturnsNotFound()
    {
        // Arrange
        var liveStreamId = 999;
        _mockStreamingService.Setup(s => s.StartRecordingAsync(liveStreamId))
            .ThrowsAsync(new ArgumentException("Live stream not found"));

        // Act
        var result = await _controller.StartRecording(liveStreamId);

        // Assert
        var notFoundResult = Assert.IsType<NotFoundObjectResult>(result.Result);
        Assert.Equal("Live stream not found", notFoundResult.Value);
    }

    [Fact]
    public async Task StartRecording_InvalidOperation_ReturnsBadRequest()
    {
        // Arrange
        var liveStreamId = 1;
        _mockStreamingService.Setup(s => s.StartRecordingAsync(liveStreamId))
            .ThrowsAsync(new InvalidOperationException("Stream is not live"));

        // Act
        var result = await _controller.StartRecording(liveStreamId);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
        Assert.Equal("Stream is not live", badRequestResult.Value);
    }

    [Fact]
    public async Task StopRecording_ValidId_ReturnsOk()
    {
        // Arrange
        var liveStreamId = 1;
        var expectedUrl = "https://test-storage.com/recordings/stream-1-recording.mp4";

        _mockStreamingService.Setup(s => s.StopRecordingAsync(liveStreamId))
            .ReturnsAsync(expectedUrl);

        // Act
        var result = await _controller.StopRecording(liveStreamId);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var response = Assert.IsType<object>(okResult.Value);
        Assert.NotNull(response);
    }

    [Fact]
    public async Task StopRecording_InvalidId_ReturnsNotFound()
    {
        // Arrange
        var liveStreamId = 999;
        _mockStreamingService.Setup(s => s.StopRecordingAsync(liveStreamId))
            .ThrowsAsync(new ArgumentException("Live stream not found"));

        // Act
        var result = await _controller.StopRecording(liveStreamId);

        // Assert
        var notFoundResult = Assert.IsType<NotFoundObjectResult>(result.Result);
        Assert.Equal("Live stream not found", notFoundResult.Value);
    }

    [Fact]
    public async Task StopRecording_InvalidOperation_ReturnsBadRequest()
    {
        // Arrange
        var liveStreamId = 1;
        _mockStreamingService.Setup(s => s.StopRecordingAsync(liveStreamId))
            .ThrowsAsync(new InvalidOperationException("Stream is not being recorded"));

        // Act
        var result = await _controller.StopRecording(liveStreamId);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
        Assert.Equal("Stream is not being recorded", badRequestResult.Value);
    }

    [Fact]
    public async Task PauseStream_ValidId_ReturnsNoContent()
    {
        // Arrange
        var liveStreamId = 1;
        _mockStreamingService.Setup(s => s.PauseStreamAsync(liveStreamId))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _controller.PauseStream(liveStreamId);

        // Assert
        Assert.IsType<NoContentResult>(result);
    }

    [Fact]
    public async Task PauseStream_InvalidId_ReturnsNotFound()
    {
        // Arrange
        var liveStreamId = 999;
        _mockStreamingService.Setup(s => s.PauseStreamAsync(liveStreamId))
            .ThrowsAsync(new ArgumentException("Live stream not found"));

        // Act
        var result = await _controller.PauseStream(liveStreamId);

        // Assert
        var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
        Assert.Equal("Live stream not found", notFoundResult.Value);
    }

    [Fact]
    public async Task PauseStream_InvalidOperation_ReturnsBadRequest()
    {
        // Arrange
        var liveStreamId = 1;
        _mockStreamingService.Setup(s => s.PauseStreamAsync(liveStreamId))
            .ThrowsAsync(new InvalidOperationException("Stream is not live"));

        // Act
        var result = await _controller.PauseStream(liveStreamId);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal("Stream is not live", badRequestResult.Value);
    }

    [Fact]
    public async Task ResumeStream_ValidId_ReturnsNoContent()
    {
        // Arrange
        var liveStreamId = 1;
        _mockStreamingService.Setup(s => s.ResumeStreamAsync(liveStreamId))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _controller.ResumeStream(liveStreamId);

        // Assert
        Assert.IsType<NoContentResult>(result);
    }

    [Fact]
    public async Task ResumeStream_InvalidId_ReturnsNotFound()
    {
        // Arrange
        var liveStreamId = 999;
        _mockStreamingService.Setup(s => s.ResumeStreamAsync(liveStreamId))
            .ThrowsAsync(new ArgumentException("Live stream not found"));

        // Act
        var result = await _controller.ResumeStream(liveStreamId);

        // Assert
        var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
        Assert.Equal("Live stream not found", notFoundResult.Value);
    }

    [Fact]
    public async Task ResumeStream_InvalidOperation_ReturnsBadRequest()
    {
        // Arrange
        var liveStreamId = 1;
        _mockStreamingService.Setup(s => s.ResumeStreamAsync(liveStreamId))
            .ThrowsAsync(new InvalidOperationException("Stream is not paused"));

        // Act
        var result = await _controller.ResumeStream(liveStreamId);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal("Stream is not paused", badRequestResult.Value);
    }

    [Fact]
    public async Task GenerateStreamKey_ValidId_ReturnsOk()
    {
        // Arrange
        var liveStreamId = 1;
        var expectedKey = "new-stream-key-123";

        _mockStreamingService.Setup(s => s.GenerateStreamKeyAsync(liveStreamId))
            .ReturnsAsync(expectedKey);

        // Act
        var result = await _controller.GenerateStreamKey(liveStreamId);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var response = Assert.IsType<object>(okResult.Value);
        Assert.NotNull(response);
    }

    [Fact]
    public async Task GenerateStreamKey_InvalidId_ReturnsNotFound()
    {
        // Arrange
        var liveStreamId = 999;
        _mockStreamingService.Setup(s => s.GenerateStreamKeyAsync(liveStreamId))
            .ThrowsAsync(new ArgumentException("Live stream not found"));

        // Act
        var result = await _controller.GenerateStreamKey(liveStreamId);

        // Assert
        var notFoundResult = Assert.IsType<NotFoundObjectResult>(result.Result);
        Assert.Equal("Live stream not found", notFoundResult.Value);
    }

    [Fact]
    public async Task GetStreamPlaybackUrl_ValidId_ReturnsOk()
    {
        // Arrange
        var liveStreamId = 1;
        var expectedUrl = "https://test-cdn.com/live/test-stream-key";

        _mockStreamingService.Setup(s => s.GetStreamPlaybackUrlAsync(liveStreamId))
            .ReturnsAsync(expectedUrl);

        // Act
        var result = await _controller.GetStreamPlaybackUrl(liveStreamId);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var response = Assert.IsType<object>(okResult.Value);
        Assert.NotNull(response);
    }

    [Fact]
    public async Task GetStreamPlaybackUrl_InvalidId_ReturnsNotFound()
    {
        // Arrange
        var liveStreamId = 999;
        _mockStreamingService.Setup(s => s.GetStreamPlaybackUrlAsync(liveStreamId))
            .ThrowsAsync(new ArgumentException("Live stream not found"));

        // Act
        var result = await _controller.GetStreamPlaybackUrl(liveStreamId);

        // Assert
        var notFoundResult = Assert.IsType<NotFoundObjectResult>(result.Result);
        Assert.Equal("Live stream not found", notFoundResult.Value);
    }

    [Fact]
    public async Task GetStreamIngestUrl_ValidId_ReturnsOk()
    {
        // Arrange
        var liveStreamId = 1;
        var expectedUrl = "rtmp://test-ingest-server/live";

        _mockStreamingService.Setup(s => s.GetStreamIngestUrlAsync(liveStreamId))
            .ReturnsAsync(expectedUrl);

        // Act
        var result = await _controller.GetStreamIngestUrl(liveStreamId);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var response = Assert.IsType<object>(okResult.Value);
        Assert.NotNull(response);
    }

    [Fact]
    public async Task GetStreamIngestUrl_InvalidId_ReturnsNotFound()
    {
        // Arrange
        var liveStreamId = 999;
        _mockStreamingService.Setup(s => s.GetStreamIngestUrlAsync(liveStreamId))
            .ThrowsAsync(new ArgumentException("Live stream not found"));

        // Act
        var result = await _controller.GetStreamIngestUrl(liveStreamId);

        // Assert
        var notFoundResult = Assert.IsType<NotFoundObjectResult>(result.Result);
        Assert.Equal("Live stream not found", notFoundResult.Value);
    }
}
