using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using Drashyam.API.Controllers;
using Drashyam.API.DTOs;
using Drashyam.API.Services;

namespace Drashyam.API.Tests.Controllers;

public class LiveStreamChallengeControllerTests
{
    private readonly Mock<ILiveStreamChallengeService> _mockChallengeService;
    private readonly Mock<ILogger<LiveStreamChallengeController>> _mockLogger;
    private readonly LiveStreamChallengeController _controller;

    public LiveStreamChallengeControllerTests()
    {
        _mockChallengeService = new Mock<ILiveStreamChallengeService>();
        _mockLogger = new Mock<ILogger<LiveStreamChallengeController>>();

        _controller = new LiveStreamChallengeController(_mockChallengeService.Object, _mockLogger.Object);
    }

    [Fact]
    public async Task GetChallenges_ValidId_ReturnsOk()
    {
        // Arrange
        var liveStreamId = 1;
        var page = 1;
        var pageSize = 20;
        var expectedChallenges = new PagedResult<LiveStreamChallengeDto>
        {
            Items = new List<LiveStreamChallengeDto>
            {
                new LiveStreamChallengeDto
                {
                    Id = 1,
                    LiveStreamId = liveStreamId,
                    ChallengeType = LiveStreamChallengeType.ViewerGoal,
                    Title = "1000 Viewers Challenge",
                    Description = "Reach 1000 viewers in this stream",
                    TargetValue = 1000,
                    CurrentValue = 500,
                    IsActive = true,
                    StartTime = DateTime.UtcNow,
                    EndTime = DateTime.UtcNow.AddHours(2),
                    CreatedBy = "creator1",
                    CreatedAt = DateTime.UtcNow
                }
            },
            TotalCount = 1,
            Page = page,
            PageSize = pageSize
        };

        _mockChallengeService.Setup(s => s.GetChallengesAsync(liveStreamId, page, pageSize))
            .ReturnsAsync(expectedChallenges);

        // Act
        var result = await _controller.GetChallenges(liveStreamId, page, pageSize);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var challenges = Assert.IsType<PagedResult<LiveStreamChallengeDto>>(okResult.Value);
        Assert.Single(challenges.Items);
        Assert.Equal(expectedChallenges.TotalCount, challenges.TotalCount);
        Assert.Equal(expectedChallenges.Page, challenges.Page);
        Assert.Equal(expectedChallenges.PageSize, challenges.PageSize);
    }

    [Fact]
    public async Task GetChallenges_InvalidId_ReturnsNotFound()
    {
        // Arrange
        var liveStreamId = 999;
        var page = 1;
        var pageSize = 20;

        _mockChallengeService.Setup(s => s.GetChallengesAsync(liveStreamId, page, pageSize))
            .ThrowsAsync(new ArgumentException("Live stream not found"));

        // Act
        var result = await _controller.GetChallenges(liveStreamId, page, pageSize);

        // Assert
        var notFoundResult = Assert.IsType<NotFoundObjectResult>(result.Result);
        Assert.Equal("Live stream not found", notFoundResult.Value);
    }

    [Fact]
    public async Task GetChallenges_InvalidPage_ReturnsBadRequest()
    {
        // Arrange
        var liveStreamId = 1;
        var page = 0; // Invalid page
        var pageSize = 20;

        _mockChallengeService.Setup(s => s.GetChallengesAsync(liveStreamId, page, pageSize))
            .ThrowsAsync(new ArgumentException("Page must be greater than 0"));

        // Act
        var result = await _controller.GetChallenges(liveStreamId, page, pageSize);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
        Assert.Equal("Page must be greater than 0", badRequestResult.Value);
    }

    [Fact]
    public async Task GetChallenges_InvalidPageSize_ReturnsBadRequest()
    {
        // Arrange
        var liveStreamId = 1;
        var page = 1;
        var pageSize = 0; // Invalid page size

        _mockChallengeService.Setup(s => s.GetChallengesAsync(liveStreamId, page, pageSize))
            .ThrowsAsync(new ArgumentException("Page size must be greater than 0"));

        // Act
        var result = await _controller.GetChallenges(liveStreamId, page, pageSize);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
        Assert.Equal("Page size must be greater than 0", badRequestResult.Value);
    }

    [Fact]
    public async Task GetChallenges_ServiceException_ReturnsInternalServerError()
    {
        // Arrange
        var liveStreamId = 1;
        var page = 1;
        var pageSize = 20;

        _mockChallengeService.Setup(s => s.GetChallengesAsync(liveStreamId, page, pageSize))
            .ThrowsAsync(new Exception("Database connection failed"));

        // Act
        var result = await _controller.GetChallenges(liveStreamId, page, pageSize);

        // Assert
        var statusCodeResult = Assert.IsType<ObjectResult>(result.Result);
        Assert.Equal(500, statusCodeResult.StatusCode);
        Assert.Equal("Database connection failed", statusCodeResult.Value);
    }

    [Fact]
    public async Task GetChallenges_ValidRequest_ReturnsEmptyList()
    {
        // Arrange
        var liveStreamId = 1;
        var page = 1;
        var pageSize = 20;
        var expectedChallenges = new PagedResult<LiveStreamChallengeDto>
        {
            Items = new List<LiveStreamChallengeDto>(),
            TotalCount = 0,
            Page = page,
            PageSize = pageSize
        };

        _mockChallengeService.Setup(s => s.GetChallengesAsync(liveStreamId, page, pageSize))
            .ReturnsAsync(expectedChallenges);

        // Act
        var result = await _controller.GetChallenges(liveStreamId, page, pageSize);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var challenges = Assert.IsType<PagedResult<LiveStreamChallengeDto>>(okResult.Value);
        Assert.Empty(challenges.Items);
        Assert.Equal(0, challenges.TotalCount);
    }

    [Fact]
    public async Task GetChallenges_ValidRequest_ReturnsMultipleChallenges()
    {
        // Arrange
        var liveStreamId = 1;
        var page = 1;
        var pageSize = 20;
        var expectedChallenges = new PagedResult<LiveStreamChallengeDto>
        {
            Items = new List<LiveStreamChallengeDto>
            {
                new LiveStreamChallengeDto
                {
                    Id = 1,
                    LiveStreamId = liveStreamId,
                    ChallengeType = LiveStreamChallengeType.ViewerGoal,
                    Title = "1000 Viewers Challenge",
                    Description = "Reach 1000 viewers in this stream",
                    TargetValue = 1000,
                    CurrentValue = 500,
                    IsActive = true,
                    StartTime = DateTime.UtcNow,
                    EndTime = DateTime.UtcNow.AddHours(2),
                    CreatedBy = "creator1",
                    CreatedAt = DateTime.UtcNow
                },
                new LiveStreamChallengeDto
                {
                    Id = 2,
                    LiveStreamId = liveStreamId,
                    ChallengeType = LiveStreamChallengeType.DonationGoal,
                    Title = "$500 Donation Challenge",
                    Description = "Raise $500 in donations",
                    TargetValue = 500,
                    CurrentValue = 250,
                    IsActive = true,
                    StartTime = DateTime.UtcNow,
                    EndTime = DateTime.UtcNow.AddHours(2),
                    CreatedBy = "creator1",
                    CreatedAt = DateTime.UtcNow
                }
            },
            TotalCount = 2,
            Page = page,
            PageSize = pageSize
        };

        _mockChallengeService.Setup(s => s.GetChallengesAsync(liveStreamId, page, pageSize))
            .ReturnsAsync(expectedChallenges);

        // Act
        var result = await _controller.GetChallenges(liveStreamId, page, pageSize);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var challenges = Assert.IsType<PagedResult<LiveStreamChallengeDto>>(okResult.Value);
        Assert.Equal(2, challenges.Items.Count);
        Assert.Equal(expectedChallenges.TotalCount, challenges.TotalCount);
        Assert.Equal(expectedChallenges.Page, challenges.Page);
        Assert.Equal(expectedChallenges.PageSize, challenges.PageSize);
    }

    [Fact]
    public async Task GetChallenges_ValidRequest_ReturnsPagedResults()
    {
        // Arrange
        var liveStreamId = 1;
        var page = 2;
        var pageSize = 10;
        var expectedChallenges = new PagedResult<LiveStreamChallengeDto>
        {
            Items = new List<LiveStreamChallengeDto>
            {
                new LiveStreamChallengeDto
                {
                    Id = 11,
                    LiveStreamId = liveStreamId,
                    ChallengeType = LiveStreamChallengeType.ViewerGoal,
                    Title = "Page 2 Challenge",
                    Description = "Challenge on page 2",
                    TargetValue = 1000,
                    CurrentValue = 500,
                    IsActive = true,
                    StartTime = DateTime.UtcNow,
                    EndTime = DateTime.UtcNow.AddHours(2),
                    CreatedBy = "creator1",
                    CreatedAt = DateTime.UtcNow
                }
            },
            TotalCount = 25,
            Page = page,
            PageSize = pageSize
        };

        _mockChallengeService.Setup(s => s.GetChallengesAsync(liveStreamId, page, pageSize))
            .ReturnsAsync(expectedChallenges);

        // Act
        var result = await _controller.GetChallenges(liveStreamId, page, pageSize);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var challenges = Assert.IsType<PagedResult<LiveStreamChallengeDto>>(okResult.Value);
        Assert.Single(challenges.Items);
        Assert.Equal(25, challenges.TotalCount);
        Assert.Equal(2, challenges.Page);
        Assert.Equal(10, challenges.PageSize);
    }

    [Fact]
    public async Task GetChallenges_ValidRequest_ReturnsActiveChallenges()
    {
        // Arrange
        var liveStreamId = 1;
        var page = 1;
        var pageSize = 20;
        var expectedChallenges = new PagedResult<LiveStreamChallengeDto>
        {
            Items = new List<LiveStreamChallengeDto>
            {
                new LiveStreamChallengeDto
                {
                    Id = 1,
                    LiveStreamId = liveStreamId,
                    ChallengeType = LiveStreamChallengeType.ViewerGoal,
                    Title = "Active Challenge",
                    Description = "This challenge is currently active",
                    TargetValue = 1000,
                    CurrentValue = 500,
                    IsActive = true,
                    StartTime = DateTime.UtcNow,
                    EndTime = DateTime.UtcNow.AddHours(2),
                    CreatedBy = "creator1",
                    CreatedAt = DateTime.UtcNow
                }
            },
            TotalCount = 1,
            Page = page,
            PageSize = pageSize
        };

        _mockChallengeService.Setup(s => s.GetChallengesAsync(liveStreamId, page, pageSize))
            .ReturnsAsync(expectedChallenges);

        // Act
        var result = await _controller.GetChallenges(liveStreamId, page, pageSize);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var challenges = Assert.IsType<PagedResult<LiveStreamChallengeDto>>(okResult.Value);
        Assert.Single(challenges.Items);
        Assert.True(challenges.Items.First().IsActive);
        Assert.Equal("Active Challenge", challenges.Items.First().Title);
    }

    [Fact]
    public async Task GetChallenges_ValidRequest_ReturnsInactiveChallenges()
    {
        // Arrange
        var liveStreamId = 1;
        var page = 1;
        var pageSize = 20;
        var expectedChallenges = new PagedResult<LiveStreamChallengeDto>
        {
            Items = new List<LiveStreamChallengeDto>
            {
                new LiveStreamChallengeDto
                {
                    Id = 1,
                    LiveStreamId = liveStreamId,
                    ChallengeType = LiveStreamChallengeType.ViewerGoal,
                    Title = "Inactive Challenge",
                    Description = "This challenge is not active",
                    TargetValue = 1000,
                    CurrentValue = 1000,
                    IsActive = false,
                    StartTime = DateTime.UtcNow.AddHours(-2),
                    EndTime = DateTime.UtcNow.AddHours(-1),
                    CreatedBy = "creator1",
                    CreatedAt = DateTime.UtcNow.AddHours(-2)
                }
            },
            TotalCount = 1,
            Page = page,
            PageSize = pageSize
        };

        _mockChallengeService.Setup(s => s.GetChallengesAsync(liveStreamId, page, pageSize))
            .ReturnsAsync(expectedChallenges);

        // Act
        var result = await _controller.GetChallenges(liveStreamId, page, pageSize);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var challenges = Assert.IsType<PagedResult<LiveStreamChallengeDto>>(okResult.Value);
        Assert.Single(challenges.Items);
        Assert.False(challenges.Items.First().IsActive);
        Assert.Equal("Inactive Challenge", challenges.Items.First().Title);
    }
}