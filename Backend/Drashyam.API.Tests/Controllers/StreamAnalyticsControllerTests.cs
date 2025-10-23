using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using Drashyam.API.Controllers;
using Drashyam.API.DTOs;
using Drashyam.API.Services;

namespace Drashyam.API.Tests.Controllers;

public class StreamAnalyticsControllerTests
{
    private readonly Mock<IStreamAnalyticsService> _mockAnalyticsService;
    private readonly Mock<ILogger<StreamAnalyticsController>> _mockLogger;
    private readonly StreamAnalyticsController _controller;

    public StreamAnalyticsControllerTests()
    {
        _mockAnalyticsService = new Mock<IStreamAnalyticsService>();
        _mockLogger = new Mock<ILogger<StreamAnalyticsController>>();

        _controller = new StreamAnalyticsController(_mockAnalyticsService.Object, _mockLogger.Object);
    }

    [Fact]
    public async Task GetStreamAnalytics_ValidId_ReturnsOk()
    {
        // Arrange
        var liveStreamId = 1;
        var expectedAnalytics = new StreamAnalyticsDto
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

        _mockAnalyticsService.Setup(s => s.GetStreamAnalyticsAsync(liveStreamId))
            .ReturnsAsync(expectedAnalytics);

        // Act
        var result = await _controller.GetStreamAnalytics(liveStreamId);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var analytics = Assert.IsType<StreamAnalyticsDto>(okResult.Value);
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
    public async Task GetStreamAnalytics_InvalidId_ReturnsNotFound()
    {
        // Arrange
        var liveStreamId = 999;
        _mockAnalyticsService.Setup(s => s.GetStreamAnalyticsAsync(liveStreamId))
            .ThrowsAsync(new ArgumentException("Live stream not found"));

        // Act
        var result = await _controller.GetStreamAnalytics(liveStreamId);

        // Assert
        var notFoundResult = Assert.IsType<NotFoundObjectResult>(result.Result);
        Assert.Equal("Live stream not found", notFoundResult.Value);
    }

    [Fact]
    public async Task GetStreamAnalytics_ServiceException_ReturnsInternalServerError()
    {
        // Arrange
        var liveStreamId = 1;
        _mockAnalyticsService.Setup(s => s.GetStreamAnalyticsAsync(liveStreamId))
            .ThrowsAsync(new Exception("Database connection failed"));

        // Act
        var result = await _controller.GetStreamAnalytics(liveStreamId);

        // Assert
        var statusCodeResult = Assert.IsType<ObjectResult>(result.Result);
        Assert.Equal(500, statusCodeResult.StatusCode);
        Assert.Equal("Database connection failed", statusCodeResult.Value);
    }

    [Fact]
    public async Task GetStreamAnalyticsByDateRange_ValidRequest_ReturnsOk()
    {
        // Arrange
        var liveStreamId = 1;
        var startDate = DateTime.UtcNow.AddDays(-7);
        var endDate = DateTime.UtcNow;
        var expectedAnalytics = new List<StreamAnalyticsDto>
        {
            new StreamAnalyticsDto
            {
                LiveStreamId = liveStreamId,
                TotalViewers = 100,
                PeakViewers = 150,
                AverageViewers = 80,
                TotalWatchTime = 2400,
                EngagementRate = 0.65m,
                ChatMessages = 30,
                Likes = 15,
                Shares = 5,
                Comments = 10,
                Revenue = 75.25m,
                StartTime = startDate,
                EndTime = startDate.AddHours(2)
            }
        };

        _mockAnalyticsService.Setup(s => s.GetStreamAnalyticsByDateRangeAsync(liveStreamId, startDate, endDate))
            .ReturnsAsync(expectedAnalytics);

        // Act
        var result = await _controller.GetStreamAnalyticsByDateRange(liveStreamId, startDate, endDate);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var analytics = Assert.IsType<List<StreamAnalyticsDto>>(okResult.Value);
        Assert.Single(analytics);
        Assert.Equal(expectedAnalytics[0].LiveStreamId, analytics[0].LiveStreamId);
        Assert.Equal(expectedAnalytics[0].TotalViewers, analytics[0].TotalViewers);
        Assert.Equal(expectedAnalytics[0].PeakViewers, analytics[0].PeakViewers);
    }

    [Fact]
    public async Task GetStreamAnalyticsByDateRange_InvalidId_ReturnsNotFound()
    {
        // Arrange
        var liveStreamId = 999;
        var startDate = DateTime.UtcNow.AddDays(-7);
        var endDate = DateTime.UtcNow;

        _mockAnalyticsService.Setup(s => s.GetStreamAnalyticsByDateRangeAsync(liveStreamId, startDate, endDate))
            .ThrowsAsync(new ArgumentException("Live stream not found"));

        // Act
        var result = await _controller.GetStreamAnalyticsByDateRange(liveStreamId, startDate, endDate);

        // Assert
        var notFoundResult = Assert.IsType<NotFoundObjectResult>(result.Result);
        Assert.Equal("Live stream not found", notFoundResult.Value);
    }

    [Fact]
    public async Task GetStreamAnalyticsByDateRange_InvalidDateRange_ReturnsBadRequest()
    {
        // Arrange
        var liveStreamId = 1;
        var startDate = DateTime.UtcNow;
        var endDate = DateTime.UtcNow.AddDays(-7); // End date before start date

        _mockAnalyticsService.Setup(s => s.GetStreamAnalyticsByDateRangeAsync(liveStreamId, startDate, endDate))
            .ThrowsAsync(new ArgumentException("Start date must be before end date"));

        // Act
        var result = await _controller.GetStreamAnalyticsByDateRange(liveStreamId, startDate, endDate);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
        Assert.Equal("Start date must be before end date", badRequestResult.Value);
    }

    [Fact]
    public async Task GetStreamAnalyticsByDateRange_ServiceException_ReturnsInternalServerError()
    {
        // Arrange
        var liveStreamId = 1;
        var startDate = DateTime.UtcNow.AddDays(-7);
        var endDate = DateTime.UtcNow;

        _mockAnalyticsService.Setup(s => s.GetStreamAnalyticsByDateRangeAsync(liveStreamId, startDate, endDate))
            .ThrowsAsync(new Exception("Database connection failed"));

        // Act
        var result = await _controller.GetStreamAnalyticsByDateRange(liveStreamId, startDate, endDate);

        // Assert
        var statusCodeResult = Assert.IsType<ObjectResult>(result.Result);
        Assert.Equal(500, statusCodeResult.StatusCode);
        Assert.Equal("Database connection failed", statusCodeResult.Value);
    }

    [Fact]
    public async Task GetStreamAnalyticsByDateRange_ValidRequest_ReturnsEmptyList()
    {
        // Arrange
        var liveStreamId = 1;
        var startDate = DateTime.UtcNow.AddDays(-7);
        var endDate = DateTime.UtcNow;
        var expectedAnalytics = new List<StreamAnalyticsDto>();

        _mockAnalyticsService.Setup(s => s.GetStreamAnalyticsByDateRangeAsync(liveStreamId, startDate, endDate))
            .ReturnsAsync(expectedAnalytics);

        // Act
        var result = await _controller.GetStreamAnalyticsByDateRange(liveStreamId, startDate, endDate);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var analytics = Assert.IsType<List<StreamAnalyticsDto>>(okResult.Value);
        Assert.Empty(analytics);
    }

    [Fact]
    public async Task GetStreamAnalyticsByDateRange_ValidRequest_ReturnsMultipleAnalytics()
    {
        // Arrange
        var liveStreamId = 1;
        var startDate = DateTime.UtcNow.AddDays(-7);
        var endDate = DateTime.UtcNow;
        var expectedAnalytics = new List<StreamAnalyticsDto>
        {
            new StreamAnalyticsDto
            {
                LiveStreamId = liveStreamId,
                TotalViewers = 100,
                PeakViewers = 150,
                AverageViewers = 80,
                TotalWatchTime = 2400,
                EngagementRate = 0.65m,
                ChatMessages = 30,
                Likes = 15,
                Shares = 5,
                Comments = 10,
                Revenue = 75.25m,
                StartTime = startDate,
                EndTime = startDate.AddHours(2)
            },
            new StreamAnalyticsDto
            {
                LiveStreamId = liveStreamId,
                TotalViewers = 120,
                PeakViewers = 180,
                AverageViewers = 90,
                TotalWatchTime = 3000,
                EngagementRate = 0.70m,
                ChatMessages = 40,
                Likes = 20,
                Shares = 8,
                Comments = 12,
                Revenue = 85.50m,
                StartTime = startDate.AddDays(1),
                EndTime = startDate.AddDays(1).AddHours(2)
            }
        };

        _mockAnalyticsService.Setup(s => s.GetStreamAnalyticsByDateRangeAsync(liveStreamId, startDate, endDate))
            .ReturnsAsync(expectedAnalytics);

        // Act
        var result = await _controller.GetStreamAnalyticsByDateRange(liveStreamId, startDate, endDate);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var analytics = Assert.IsType<List<StreamAnalyticsDto>>(okResult.Value);
        Assert.Equal(2, analytics.Count);
        Assert.Equal(expectedAnalytics[0].LiveStreamId, analytics[0].LiveStreamId);
        Assert.Equal(expectedAnalytics[1].LiveStreamId, analytics[1].LiveStreamId);
        Assert.Equal(expectedAnalytics[0].TotalViewers, analytics[0].TotalViewers);
        Assert.Equal(expectedAnalytics[1].TotalViewers, analytics[1].TotalViewers);
    }
}
