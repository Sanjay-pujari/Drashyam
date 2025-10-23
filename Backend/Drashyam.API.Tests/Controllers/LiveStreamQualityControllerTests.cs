using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using Drashyam.API.Controllers;
using Drashyam.API.DTOs;
using Drashyam.API.Services;

namespace Drashyam.API.Tests.Controllers;

public class LiveStreamQualityControllerTests
{
    private readonly Mock<ILiveStreamQualityService> _mockQualityService;
    private readonly Mock<ILogger<LiveStreamQualityController>> _mockLogger;
    private readonly LiveStreamQualityController _controller;

    public LiveStreamQualityControllerTests()
    {
        _mockQualityService = new Mock<ILiveStreamQualityService>();
        _mockLogger = new Mock<ILogger<LiveStreamQualityController>>();

        _controller = new LiveStreamQualityController(_mockQualityService.Object, _mockLogger.Object);
    }

    [Fact]
    public async Task GetQualitySettings_ValidId_ReturnsOk()
    {
        // Arrange
        var liveStreamId = 1;
        var expectedQuality = new LiveStreamQualityDto
        {
            LiveStreamId = liveStreamId,
            Quality = "1080p",
            Bitrate = 5000,
            FrameRate = 30,
            Resolution = "1920x1080",
            IsActive = true
        };

        _mockQualityService.Setup(s => s.GetQualitySettingsAsync(liveStreamId))
            .ReturnsAsync(expectedQuality);

        // Act
        var result = await _controller.GetQualitySettings(liveStreamId);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var quality = Assert.IsType<LiveStreamQualityDto>(okResult.Value);
        Assert.Equal(expectedQuality.LiveStreamId, quality.LiveStreamId);
        Assert.Equal(expectedQuality.Quality, quality.Quality);
        Assert.Equal(expectedQuality.Bitrate, quality.Bitrate);
        Assert.Equal(expectedQuality.FrameRate, quality.FrameRate);
        Assert.Equal(expectedQuality.Resolution, quality.Resolution);
        Assert.Equal(expectedQuality.IsActive, quality.IsActive);
    }

    [Fact]
    public async Task GetQualitySettings_InvalidId_ReturnsNotFound()
    {
        // Arrange
        var liveStreamId = 999;
        _mockQualityService.Setup(s => s.GetQualitySettingsAsync(liveStreamId))
            .ThrowsAsync(new ArgumentException("Live stream not found"));

        // Act
        var result = await _controller.GetQualitySettings(liveStreamId);

        // Assert
        var notFoundResult = Assert.IsType<NotFoundObjectResult>(result.Result);
        Assert.Equal("Live stream not found", notFoundResult.Value);
    }

    [Fact]
    public async Task GetQualitySettings_ServiceException_ReturnsInternalServerError()
    {
        // Arrange
        var liveStreamId = 1;
        _mockQualityService.Setup(s => s.GetQualitySettingsAsync(liveStreamId))
            .ThrowsAsync(new Exception("Database connection failed"));

        // Act
        var result = await _controller.GetQualitySettings(liveStreamId);

        // Assert
        var statusCodeResult = Assert.IsType<ObjectResult>(result.Result);
        Assert.Equal(500, statusCodeResult.StatusCode);
        Assert.Equal("Database connection failed", statusCodeResult.Value);
    }

    [Fact]
    public async Task UpdateQualitySettings_ValidRequest_ReturnsNoContent()
    {
        // Arrange
        var liveStreamId = 1;
        var qualityUpdate = new LiveStreamQualityUpdateDto
        {
            Quality = "720p",
            Bitrate = 3000,
            FrameRate = 30,
            Resolution = "1280x720"
        };

        _mockQualityService.Setup(s => s.UpdateQualitySettingsAsync(liveStreamId, qualityUpdate))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _controller.UpdateQualitySettings(liveStreamId, qualityUpdate);

        // Assert
        Assert.IsType<NoContentResult>(result);
    }

    [Fact]
    public async Task UpdateQualitySettings_InvalidId_ReturnsNotFound()
    {
        // Arrange
        var liveStreamId = 999;
        var qualityUpdate = new LiveStreamQualityUpdateDto
        {
            Quality = "720p",
            Bitrate = 3000,
            FrameRate = 30,
            Resolution = "1280x720"
        };

        _mockQualityService.Setup(s => s.UpdateQualitySettingsAsync(liveStreamId, qualityUpdate))
            .ThrowsAsync(new ArgumentException("Live stream not found"));

        // Act
        var result = await _controller.UpdateQualitySettings(liveStreamId, qualityUpdate);

        // Assert
        var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
        Assert.Equal("Live stream not found", notFoundResult.Value);
    }

    [Fact]
    public async Task UpdateQualitySettings_InvalidQuality_ReturnsBadRequest()
    {
        // Arrange
        var liveStreamId = 1;
        var qualityUpdate = new LiveStreamQualityUpdateDto
        {
            Quality = "invalid-quality",
            Bitrate = 3000,
            FrameRate = 30,
            Resolution = "1280x720"
        };

        _mockQualityService.Setup(s => s.UpdateQualitySettingsAsync(liveStreamId, qualityUpdate))
            .ThrowsAsync(new InvalidOperationException("Invalid quality setting"));

        // Act
        var result = await _controller.UpdateQualitySettings(liveStreamId, qualityUpdate);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal("Invalid quality setting", badRequestResult.Value);
    }

    [Fact]
    public async Task UpdateQualitySettings_ServiceException_ReturnsInternalServerError()
    {
        // Arrange
        var liveStreamId = 1;
        var qualityUpdate = new LiveStreamQualityUpdateDto
        {
            Quality = "720p",
            Bitrate = 3000,
            FrameRate = 30,
            Resolution = "1280x720"
        };

        _mockQualityService.Setup(s => s.UpdateQualitySettingsAsync(liveStreamId, qualityUpdate))
            .ThrowsAsync(new Exception("Database connection failed"));

        // Act
        var result = await _controller.UpdateQualitySettings(liveStreamId, qualityUpdate);

        // Assert
        var statusCodeResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal(500, statusCodeResult.StatusCode);
        Assert.Equal("Database connection failed", statusCodeResult.Value);
    }

    [Fact]
    public async Task GetQualitySettings_ValidRequest_Returns1080pQuality()
    {
        // Arrange
        var liveStreamId = 1;
        var expectedQuality = new LiveStreamQualityDto
        {
            LiveStreamId = liveStreamId,
            Quality = "1080p",
            Bitrate = 5000,
            FrameRate = 30,
            Resolution = "1920x1080",
            IsActive = true
        };

        _mockQualityService.Setup(s => s.GetQualitySettingsAsync(liveStreamId))
            .ReturnsAsync(expectedQuality);

        // Act
        var result = await _controller.GetQualitySettings(liveStreamId);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var quality = Assert.IsType<LiveStreamQualityDto>(okResult.Value);
        Assert.Equal("1080p", quality.Quality);
        Assert.Equal(5000, quality.Bitrate);
        Assert.Equal(30, quality.FrameRate);
        Assert.Equal("1920x1080", quality.Resolution);
    }

    [Fact]
    public async Task GetQualitySettings_ValidRequest_Returns720pQuality()
    {
        // Arrange
        var liveStreamId = 1;
        var expectedQuality = new LiveStreamQualityDto
        {
            LiveStreamId = liveStreamId,
            Quality = "720p",
            Bitrate = 3000,
            FrameRate = 30,
            Resolution = "1280x720",
            IsActive = true
        };

        _mockQualityService.Setup(s => s.GetQualitySettingsAsync(liveStreamId))
            .ReturnsAsync(expectedQuality);

        // Act
        var result = await _controller.GetQualitySettings(liveStreamId);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var quality = Assert.IsType<LiveStreamQualityDto>(okResult.Value);
        Assert.Equal("720p", quality.Quality);
        Assert.Equal(3000, quality.Bitrate);
        Assert.Equal(30, quality.FrameRate);
        Assert.Equal("1280x720", quality.Resolution);
    }

    [Fact]
    public async Task GetQualitySettings_ValidRequest_Returns480pQuality()
    {
        // Arrange
        var liveStreamId = 1;
        var expectedQuality = new LiveStreamQualityDto
        {
            LiveStreamId = liveStreamId,
            Quality = "480p",
            Bitrate = 1500,
            FrameRate = 30,
            Resolution = "854x480",
            IsActive = true
        };

        _mockQualityService.Setup(s => s.GetQualitySettingsAsync(liveStreamId))
            .ReturnsAsync(expectedQuality);

        // Act
        var result = await _controller.GetQualitySettings(liveStreamId);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var quality = Assert.IsType<LiveStreamQualityDto>(okResult.Value);
        Assert.Equal("480p", quality.Quality);
        Assert.Equal(1500, quality.Bitrate);
        Assert.Equal(30, quality.FrameRate);
        Assert.Equal("854x480", quality.Resolution);
    }

    [Fact]
    public async Task GetQualitySettings_ValidRequest_ReturnsInactiveQuality()
    {
        // Arrange
        var liveStreamId = 1;
        var expectedQuality = new LiveStreamQualityDto
        {
            LiveStreamId = liveStreamId,
            Quality = "1080p",
            Bitrate = 5000,
            FrameRate = 30,
            Resolution = "1920x1080",
            IsActive = false
        };

        _mockQualityService.Setup(s => s.GetQualitySettingsAsync(liveStreamId))
            .ReturnsAsync(expectedQuality);

        // Act
        var result = await _controller.GetQualitySettings(liveStreamId);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var quality = Assert.IsType<LiveStreamQualityDto>(okResult.Value);
        Assert.False(quality.IsActive);
    }

    [Fact]
    public async Task UpdateQualitySettings_ValidRequest_UpdatesTo720p()
    {
        // Arrange
        var liveStreamId = 1;
        var qualityUpdate = new LiveStreamQualityUpdateDto
        {
            Quality = "720p",
            Bitrate = 3000,
            FrameRate = 30,
            Resolution = "1280x720"
        };

        _mockQualityService.Setup(s => s.UpdateQualitySettingsAsync(liveStreamId, qualityUpdate))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _controller.UpdateQualitySettings(liveStreamId, qualityUpdate);

        // Assert
        Assert.IsType<NoContentResult>(result);
        _mockQualityService.Verify(s => s.UpdateQualitySettingsAsync(liveStreamId, qualityUpdate), Times.Once);
    }

    [Fact]
    public async Task UpdateQualitySettings_ValidRequest_UpdatesTo480p()
    {
        // Arrange
        var liveStreamId = 1;
        var qualityUpdate = new LiveStreamQualityUpdateDto
        {
            Quality = "480p",
            Bitrate = 1500,
            FrameRate = 30,
            Resolution = "854x480"
        };

        _mockQualityService.Setup(s => s.UpdateQualitySettingsAsync(liveStreamId, qualityUpdate))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _controller.UpdateQualitySettings(liveStreamId, qualityUpdate);

        // Assert
        Assert.IsType<NoContentResult>(result);
        _mockQualityService.Verify(s => s.UpdateQualitySettingsAsync(liveStreamId, qualityUpdate), Times.Once);
    }
}