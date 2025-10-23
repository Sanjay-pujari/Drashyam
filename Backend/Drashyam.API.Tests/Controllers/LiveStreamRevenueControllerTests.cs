using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using Drashyam.API.Controllers;
using Drashyam.API.DTOs;
using Drashyam.API.Services;

namespace Drashyam.API.Tests.Controllers;

public class LiveStreamRevenueControllerTests
{
    private readonly Mock<ILiveStreamRevenueService> _mockRevenueService;
    private readonly Mock<ILogger<LiveStreamRevenueController>> _mockLogger;
    private readonly LiveStreamRevenueController _controller;

    public LiveStreamRevenueControllerTests()
    {
        _mockRevenueService = new Mock<ILiveStreamRevenueService>();
        _mockLogger = new Mock<ILogger<LiveStreamRevenueController>>();

        _controller = new LiveStreamRevenueController(_mockRevenueService.Object, _mockLogger.Object);
    }

    [Fact]
    public async Task GetRevenue_ValidId_ReturnsOk()
    {
        // Arrange
        var liveStreamId = 1;
        var expectedRevenue = new LiveStreamRevenueDto
        {
            LiveStreamId = liveStreamId,
            TotalRevenue = 150.75m,
            DonationRevenue = 100.50m,
            SubscriptionRevenue = 30.25m,
            AdRevenue = 20.00m,
            SuperChatRevenue = 50.00m,
            StartTime = DateTime.UtcNow.AddHours(-2),
            EndTime = DateTime.UtcNow
        };

        _mockRevenueService.Setup(s => s.GetRevenueAsync(liveStreamId))
            .ReturnsAsync(expectedRevenue);

        // Act
        var result = await _controller.GetRevenue(liveStreamId);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var revenue = Assert.IsType<LiveStreamRevenueDto>(okResult.Value);
        Assert.Equal(expectedRevenue.LiveStreamId, revenue.LiveStreamId);
        Assert.Equal(expectedRevenue.TotalRevenue, revenue.TotalRevenue);
        Assert.Equal(expectedRevenue.DonationRevenue, revenue.DonationRevenue);
        Assert.Equal(expectedRevenue.SubscriptionRevenue, revenue.SubscriptionRevenue);
        Assert.Equal(expectedRevenue.AdRevenue, revenue.AdRevenue);
        Assert.Equal(expectedRevenue.SuperChatRevenue, revenue.SuperChatRevenue);
    }

    [Fact]
    public async Task GetRevenue_InvalidId_ReturnsNotFound()
    {
        // Arrange
        var liveStreamId = 999;
        _mockRevenueService.Setup(s => s.GetRevenueAsync(liveStreamId))
            .ThrowsAsync(new ArgumentException("Live stream not found"));

        // Act
        var result = await _controller.GetRevenue(liveStreamId);

        // Assert
        var notFoundResult = Assert.IsType<NotFoundObjectResult>(result.Result);
        Assert.Equal("Live stream not found", notFoundResult.Value);
    }

    [Fact]
    public async Task GetRevenue_ServiceException_ReturnsInternalServerError()
    {
        // Arrange
        var liveStreamId = 1;
        _mockRevenueService.Setup(s => s.GetRevenueAsync(liveStreamId))
            .ThrowsAsync(new Exception("Database connection failed"));

        // Act
        var result = await _controller.GetRevenue(liveStreamId);

        // Assert
        var statusCodeResult = Assert.IsType<ObjectResult>(result.Result);
        Assert.Equal(500, statusCodeResult.StatusCode);
        Assert.Equal("Database connection failed", statusCodeResult.Value);
    }

    [Fact]
    public async Task GetRevenue_ValidRequest_ReturnsZeroRevenue()
    {
        // Arrange
        var liveStreamId = 1;
        var expectedRevenue = new LiveStreamRevenueDto
        {
            LiveStreamId = liveStreamId,
            TotalRevenue = 0.00m,
            DonationRevenue = 0.00m,
            SubscriptionRevenue = 0.00m,
            AdRevenue = 0.00m,
            SuperChatRevenue = 0.00m,
            StartTime = DateTime.UtcNow.AddHours(-2),
            EndTime = DateTime.UtcNow
        };

        _mockRevenueService.Setup(s => s.GetRevenueAsync(liveStreamId))
            .ReturnsAsync(expectedRevenue);

        // Act
        var result = await _controller.GetRevenue(liveStreamId);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var revenue = Assert.IsType<LiveStreamRevenueDto>(okResult.Value);
        Assert.Equal(0.00m, revenue.TotalRevenue);
        Assert.Equal(0.00m, revenue.DonationRevenue);
        Assert.Equal(0.00m, revenue.SubscriptionRevenue);
        Assert.Equal(0.00m, revenue.AdRevenue);
        Assert.Equal(0.00m, revenue.SuperChatRevenue);
    }

    [Fact]
    public async Task GetRevenue_ValidRequest_ReturnsHighRevenue()
    {
        // Arrange
        var liveStreamId = 1;
        var expectedRevenue = new LiveStreamRevenueDto
        {
            LiveStreamId = liveStreamId,
            TotalRevenue = 1000.00m,
            DonationRevenue = 500.00m,
            SubscriptionRevenue = 200.00m,
            AdRevenue = 150.00m,
            SuperChatRevenue = 150.00m,
            StartTime = DateTime.UtcNow.AddHours(-2),
            EndTime = DateTime.UtcNow
        };

        _mockRevenueService.Setup(s => s.GetRevenueAsync(liveStreamId))
            .ReturnsAsync(expectedRevenue);

        // Act
        var result = await _controller.GetRevenue(liveStreamId);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var revenue = Assert.IsType<LiveStreamRevenueDto>(okResult.Value);
        Assert.Equal(1000.00m, revenue.TotalRevenue);
        Assert.Equal(500.00m, revenue.DonationRevenue);
        Assert.Equal(200.00m, revenue.SubscriptionRevenue);
        Assert.Equal(150.00m, revenue.AdRevenue);
        Assert.Equal(150.00m, revenue.SuperChatRevenue);
    }

    [Fact]
    public async Task GetRevenue_ValidRequest_ReturnsDonationRevenueOnly()
    {
        // Arrange
        var liveStreamId = 1;
        var expectedRevenue = new LiveStreamRevenueDto
        {
            LiveStreamId = liveStreamId,
            TotalRevenue = 100.00m,
            DonationRevenue = 100.00m,
            SubscriptionRevenue = 0.00m,
            AdRevenue = 0.00m,
            SuperChatRevenue = 0.00m,
            StartTime = DateTime.UtcNow.AddHours(-2),
            EndTime = DateTime.UtcNow
        };

        _mockRevenueService.Setup(s => s.GetRevenueAsync(liveStreamId))
            .ReturnsAsync(expectedRevenue);

        // Act
        var result = await _controller.GetRevenue(liveStreamId);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var revenue = Assert.IsType<LiveStreamRevenueDto>(okResult.Value);
        Assert.Equal(100.00m, revenue.TotalRevenue);
        Assert.Equal(100.00m, revenue.DonationRevenue);
        Assert.Equal(0.00m, revenue.SubscriptionRevenue);
        Assert.Equal(0.00m, revenue.AdRevenue);
        Assert.Equal(0.00m, revenue.SuperChatRevenue);
    }

    [Fact]
    public async Task GetRevenue_ValidRequest_ReturnsSubscriptionRevenueOnly()
    {
        // Arrange
        var liveStreamId = 1;
        var expectedRevenue = new LiveStreamRevenueDto
        {
            LiveStreamId = liveStreamId,
            TotalRevenue = 50.00m,
            DonationRevenue = 0.00m,
            SubscriptionRevenue = 50.00m,
            AdRevenue = 0.00m,
            SuperChatRevenue = 0.00m,
            StartTime = DateTime.UtcNow.AddHours(-2),
            EndTime = DateTime.UtcNow
        };

        _mockRevenueService.Setup(s => s.GetRevenueAsync(liveStreamId))
            .ReturnsAsync(expectedRevenue);

        // Act
        var result = await _controller.GetRevenue(liveStreamId);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var revenue = Assert.IsType<LiveStreamRevenueDto>(okResult.Value);
        Assert.Equal(50.00m, revenue.TotalRevenue);
        Assert.Equal(0.00m, revenue.DonationRevenue);
        Assert.Equal(50.00m, revenue.SubscriptionRevenue);
        Assert.Equal(0.00m, revenue.AdRevenue);
        Assert.Equal(0.00m, revenue.SuperChatRevenue);
    }

    [Fact]
    public async Task GetRevenue_ValidRequest_ReturnsAdRevenueOnly()
    {
        // Arrange
        var liveStreamId = 1;
        var expectedRevenue = new LiveStreamRevenueDto
        {
            LiveStreamId = liveStreamId,
            TotalRevenue = 25.00m,
            DonationRevenue = 0.00m,
            SubscriptionRevenue = 0.00m,
            AdRevenue = 25.00m,
            SuperChatRevenue = 0.00m,
            StartTime = DateTime.UtcNow.AddHours(-2),
            EndTime = DateTime.UtcNow
        };

        _mockRevenueService.Setup(s => s.GetRevenueAsync(liveStreamId))
            .ReturnsAsync(expectedRevenue);

        // Act
        var result = await _controller.GetRevenue(liveStreamId);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var revenue = Assert.IsType<LiveStreamRevenueDto>(okResult.Value);
        Assert.Equal(25.00m, revenue.TotalRevenue);
        Assert.Equal(0.00m, revenue.DonationRevenue);
        Assert.Equal(0.00m, revenue.SubscriptionRevenue);
        Assert.Equal(25.00m, revenue.AdRevenue);
        Assert.Equal(0.00m, revenue.SuperChatRevenue);
    }

    [Fact]
    public async Task GetRevenue_ValidRequest_ReturnsSuperChatRevenueOnly()
    {
        // Arrange
        var liveStreamId = 1;
        var expectedRevenue = new LiveStreamRevenueDto
        {
            LiveStreamId = liveStreamId,
            TotalRevenue = 75.00m,
            DonationRevenue = 0.00m,
            SubscriptionRevenue = 0.00m,
            AdRevenue = 0.00m,
            SuperChatRevenue = 75.00m,
            StartTime = DateTime.UtcNow.AddHours(-2),
            EndTime = DateTime.UtcNow
        };

        _mockRevenueService.Setup(s => s.GetRevenueAsync(liveStreamId))
            .ReturnsAsync(expectedRevenue);

        // Act
        var result = await _controller.GetRevenue(liveStreamId);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var revenue = Assert.IsType<LiveStreamRevenueDto>(okResult.Value);
        Assert.Equal(75.00m, revenue.TotalRevenue);
        Assert.Equal(0.00m, revenue.DonationRevenue);
        Assert.Equal(0.00m, revenue.SubscriptionRevenue);
        Assert.Equal(0.00m, revenue.AdRevenue);
        Assert.Equal(75.00m, revenue.SuperChatRevenue);
    }

    [Fact]
    public async Task GetRevenue_ValidRequest_ReturnsMixedRevenue()
    {
        // Arrange
        var liveStreamId = 1;
        var expectedRevenue = new LiveStreamRevenueDto
        {
            LiveStreamId = liveStreamId,
            TotalRevenue = 300.00m,
            DonationRevenue = 100.00m,
            SubscriptionRevenue = 75.00m,
            AdRevenue = 50.00m,
            SuperChatRevenue = 75.00m,
            StartTime = DateTime.UtcNow.AddHours(-2),
            EndTime = DateTime.UtcNow
        };

        _mockRevenueService.Setup(s => s.GetRevenueAsync(liveStreamId))
            .ReturnsAsync(expectedRevenue);

        // Act
        var result = await _controller.GetRevenue(liveStreamId);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var revenue = Assert.IsType<LiveStreamRevenueDto>(okResult.Value);
        Assert.Equal(300.00m, revenue.TotalRevenue);
        Assert.Equal(100.00m, revenue.DonationRevenue);
        Assert.Equal(75.00m, revenue.SubscriptionRevenue);
        Assert.Equal(50.00m, revenue.AdRevenue);
        Assert.Equal(75.00m, revenue.SuperChatRevenue);
    }
}