using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using Drashyam.API.Controllers;
using Drashyam.API.DTOs;
using Drashyam.API.Services;

namespace Drashyam.API.Tests.Controllers;

public class LiveStreamAnalyticsControllerTests
{
    private readonly Mock<ILiveStreamAnalyticsService> _mockAnalyticsService;
    private readonly Mock<ILogger<LiveStreamAnalyticsController>> _mockLogger;
    private readonly LiveStreamAnalyticsController _controller;

    public LiveStreamAnalyticsControllerTests()
    {
        _mockAnalyticsService = new Mock<ILiveStreamAnalyticsService>();
        _mockLogger = new Mock<ILogger<LiveStreamAnalyticsController>>();

        _controller = new LiveStreamAnalyticsController(_mockAnalyticsService.Object, _mockLogger.Object);
    }

    [Fact]
    public async Task GetAnalytics_ValidId_ReturnsOk()
    {
        // Arrange
        var liveStreamId = 1;
        var expectedAnalytics = new LiveStreamAnalyticsDto
        {
            LiveStreamId = liveStreamId,
            TotalViewers = 150,
            PeakViewers = 200,
            AverageViewers = 120,
            TotalWatchTime = 3600,
            EngagementRate = 0.75m,
            ChatMessages = 50,
            Likes = 25,
            Shares = 10,
            Comments = 15,
            Revenue = 100.50m,
            StartTime = DateTime.UtcNow.AddHours(-2),
            EndTime = DateTime.UtcNow
        };

        _mockAnalyticsService.Setup(s => s.GetAnalyticsAsync(liveStreamId))
            .ReturnsAsync(expectedAnalytics);

        // Act
        var result = await _controller.GetAnalytics(liveStreamId);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var analytics = Assert.IsType<LiveStreamAnalyticsDto>(okResult.Value);
        Assert.Equal(expectedAnalytics.LiveStreamId, analytics.LiveStreamId);
        Assert.Equal(expectedAnalytics.TotalViewers, analytics.TotalViewers);
        Assert.Equal(expectedAnalytics.PeakViewers, analytics.PeakViewers);
        Assert.Equal(expectedAnalytics.AverageViewers, analytics.AverageViewers);
        Assert.Equal(expectedAnalytics.TotalWatchTime, analytics.TotalWatchTime);
        Assert.Equal(expectedAnalytics.EngagementRate, analytics.EngagementRate);
        Assert.Equal(expectedAnalytics.ChatMessages, analytics.ChatMessages);
        Assert.Equal(expectedAnalytics.Likes, analytics.Likes);
        Assert.Equal(expectedAnalytics.Shares, analytics.Shares);
        Assert.Equal(expectedAnalytics.Comments, analytics.Comments);
        Assert.Equal(expectedAnalytics.Revenue, analytics.Revenue);
    }

    [Fact]
    public async Task GetAnalytics_InvalidId_ReturnsNotFound()
    {
        // Arrange
        var liveStreamId = 999;
        _mockAnalyticsService.Setup(s => s.GetAnalyticsAsync(liveStreamId))
            .ThrowsAsync(new ArgumentException("Live stream not found"));

        // Act
        var result = await _controller.GetAnalytics(liveStreamId);

        // Assert
        var notFoundResult = Assert.IsType<NotFoundObjectResult>(result.Result);
        Assert.Equal("Live stream not found", notFoundResult.Value);
    }

    [Fact]
    public async Task GetAnalytics_ServiceException_ReturnsInternalServerError()
    {
        // Arrange
        var liveStreamId = 1;
        _mockAnalyticsService.Setup(s => s.GetAnalyticsAsync(liveStreamId))
            .ThrowsAsync(new Exception("Database connection failed"));

        // Act
        var result = await _controller.GetAnalytics(liveStreamId);

        // Assert
        var statusCodeResult = Assert.IsType<ObjectResult>(result.Result);
        Assert.Equal(500, statusCodeResult.StatusCode);
        Assert.Equal("Database connection failed", statusCodeResult.Value);
    }

    [Fact]
    public async Task GetAnalytics_ValidRequest_ReturnsZeroViewers()
    {
        // Arrange
        var liveStreamId = 1;
        var expectedAnalytics = new LiveStreamAnalyticsDto
        {
            LiveStreamId = liveStreamId,
            TotalViewers = 0,
            PeakViewers = 0,
            AverageViewers = 0,
            TotalWatchTime = 0,
            EngagementRate = 0.00m,
            ChatMessages = 0,
            Likes = 0,
            Shares = 0,
            Comments = 0,
            Revenue = 0.00m,
            StartTime = DateTime.UtcNow.AddHours(-2),
            EndTime = DateTime.UtcNow
        };

        _mockAnalyticsService.Setup(s => s.GetAnalyticsAsync(liveStreamId))
            .ReturnsAsync(expectedAnalytics);

        // Act
        var result = await _controller.GetAnalytics(liveStreamId);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var analytics = Assert.IsType<LiveStreamAnalyticsDto>(okResult.Value);
        Assert.Equal(0, analytics.TotalViewers);
        Assert.Equal(0, analytics.PeakViewers);
        Assert.Equal(0, analytics.AverageViewers);
        Assert.Equal(0, analytics.TotalWatchTime);
        Assert.Equal(0.00m, analytics.EngagementRate);
        Assert.Equal(0, analytics.ChatMessages);
        Assert.Equal(0, analytics.Likes);
        Assert.Equal(0, analytics.Shares);
        Assert.Equal(0, analytics.Comments);
        Assert.Equal(0.00m, analytics.Revenue);
    }

    [Fact]
    public async Task GetAnalytics_ValidRequest_ReturnsHighViewers()
    {
        // Arrange
        var liveStreamId = 1;
        var expectedAnalytics = new LiveStreamAnalyticsDto
        {
            LiveStreamId = liveStreamId,
            TotalViewers = 1000,
            PeakViewers = 1500,
            AverageViewers = 800,
            TotalWatchTime = 7200,
            EngagementRate = 0.85m,
            ChatMessages = 200,
            Likes = 100,
            Shares = 50,
            Comments = 75,
            Revenue = 500.00m,
            StartTime = DateTime.UtcNow.AddHours(-2),
            EndTime = DateTime.UtcNow
        };

        _mockAnalyticsService.Setup(s => s.GetAnalyticsAsync(liveStreamId))
            .ReturnsAsync(expectedAnalytics);

        // Act
        var result = await _controller.GetAnalytics(liveStreamId);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var analytics = Assert.IsType<LiveStreamAnalyticsDto>(okResult.Value);
        Assert.Equal(1000, analytics.TotalViewers);
        Assert.Equal(1500, analytics.PeakViewers);
        Assert.Equal(800, analytics.AverageViewers);
        Assert.Equal(7200, analytics.TotalWatchTime);
        Assert.Equal(0.85m, analytics.EngagementRate);
        Assert.Equal(200, analytics.ChatMessages);
        Assert.Equal(100, analytics.Likes);
        Assert.Equal(50, analytics.Shares);
        Assert.Equal(75, analytics.Comments);
        Assert.Equal(500.00m, analytics.Revenue);
    }

    [Fact]
    public async Task GetAnalytics_ValidRequest_ReturnsHighEngagement()
    {
        // Arrange
        var liveStreamId = 1;
        var expectedAnalytics = new LiveStreamAnalyticsDto
        {
            LiveStreamId = liveStreamId,
            TotalViewers = 500,
            PeakViewers = 600,
            AverageViewers = 400,
            TotalWatchTime = 3600,
            EngagementRate = 0.95m,
            ChatMessages = 300,
            Likes = 150,
            Shares = 75,
            Comments = 100,
            Revenue = 250.00m,
            StartTime = DateTime.UtcNow.AddHours(-2),
            EndTime = DateTime.UtcNow
        };

        _mockAnalyticsService.Setup(s => s.GetAnalyticsAsync(liveStreamId))
            .ReturnsAsync(expectedAnalytics);

        // Act
        var result = await _controller.GetAnalytics(liveStreamId);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var analytics = Assert.IsType<LiveStreamAnalyticsDto>(okResult.Value);
        Assert.Equal(0.95m, analytics.EngagementRate);
        Assert.Equal(300, analytics.ChatMessages);
        Assert.Equal(150, analytics.Likes);
        Assert.Equal(75, analytics.Shares);
        Assert.Equal(100, analytics.Comments);
    }

    [Fact]
    public async Task GetAnalytics_ValidRequest_ReturnsLowEngagement()
    {
        // Arrange
        var liveStreamId = 1;
        var expectedAnalytics = new LiveStreamAnalyticsDto
        {
            LiveStreamId = liveStreamId,
            TotalViewers = 100,
            PeakViewers = 120,
            AverageViewers = 80,
            TotalWatchTime = 1800,
            EngagementRate = 0.25m,
            ChatMessages = 10,
            Likes = 5,
            Shares = 2,
            Comments = 3,
            Revenue = 25.00m,
            StartTime = DateTime.UtcNow.AddHours(-2),
            EndTime = DateTime.UtcNow
        };

        _mockAnalyticsService.Setup(s => s.GetAnalyticsAsync(liveStreamId))
            .ReturnsAsync(expectedAnalytics);

        // Act
        var result = await _controller.GetAnalytics(liveStreamId);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var analytics = Assert.IsType<LiveStreamAnalyticsDto>(okResult.Value);
        Assert.Equal(0.25m, analytics.EngagementRate);
        Assert.Equal(10, analytics.ChatMessages);
        Assert.Equal(5, analytics.Likes);
        Assert.Equal(2, analytics.Shares);
        Assert.Equal(3, analytics.Comments);
    }

    [Fact]
    public async Task GetAnalytics_ValidRequest_ReturnsHighRevenue()
    {
        // Arrange
        var liveStreamId = 1;
        var expectedAnalytics = new LiveStreamAnalyticsDto
        {
            LiveStreamId = liveStreamId,
            TotalViewers = 800,
            PeakViewers = 1000,
            AverageViewers = 600,
            TotalWatchTime = 5400,
            EngagementRate = 0.80m,
            ChatMessages = 150,
            Likes = 75,
            Shares = 30,
            Comments = 45,
            Revenue = 1000.00m,
            StartTime = DateTime.UtcNow.AddHours(-2),
            EndTime = DateTime.UtcNow
        };

        _mockAnalyticsService.Setup(s => s.GetAnalyticsAsync(liveStreamId))
            .ReturnsAsync(expectedAnalytics);

        // Act
        var result = await _controller.GetAnalytics(liveStreamId);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var analytics = Assert.IsType<LiveStreamAnalyticsDto>(okResult.Value);
        Assert.Equal(1000.00m, analytics.Revenue);
    }

    [Fact]
    public async Task GetAnalytics_ValidRequest_ReturnsZeroRevenue()
    {
        // Arrange
        var liveStreamId = 1;
        var expectedAnalytics = new LiveStreamAnalyticsDto
        {
            LiveStreamId = liveStreamId,
            TotalViewers = 200,
            PeakViewers = 250,
            AverageViewers = 150,
            TotalWatchTime = 2700,
            EngagementRate = 0.60m,
            ChatMessages = 50,
            Likes = 25,
            Shares = 10,
            Comments = 15,
            Revenue = 0.00m,
            StartTime = DateTime.UtcNow.AddHours(-2),
            EndTime = DateTime.UtcNow
        };

        _mockAnalyticsService.Setup(s => s.GetAnalyticsAsync(liveStreamId))
            .ReturnsAsync(expectedAnalytics);

        // Act
        var result = await _controller.GetAnalytics(liveStreamId);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var analytics = Assert.IsType<LiveStreamAnalyticsDto>(okResult.Value);
        Assert.Equal(0.00m, analytics.Revenue);
    }
}