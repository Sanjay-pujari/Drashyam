using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using Drashyam.API.Controllers;
using Drashyam.API.DTOs;
using Drashyam.API.Services;

namespace Drashyam.API.Tests.Controllers;

public class LiveStreamPollControllerTests
{
    private readonly Mock<ILiveStreamPollService> _mockPollService;
    private readonly Mock<ILogger<LiveStreamPollController>> _mockLogger;
    private readonly LiveStreamPollController _controller;

    public LiveStreamPollControllerTests()
    {
        _mockPollService = new Mock<ILiveStreamPollService>();
        _mockLogger = new Mock<ILogger<LiveStreamPollController>>();

        _controller = new LiveStreamPollController(_mockPollService.Object, _mockLogger.Object);
    }

    [Fact]
    public async Task GetPolls_ValidId_ReturnsOk()
    {
        // Arrange
        var liveStreamId = 1;
        var page = 1;
        var pageSize = 20;
        var expectedPolls = new PagedResult<LiveStreamPollDto>
        {
            Items = new List<LiveStreamPollDto>
            {
                new LiveStreamPollDto
                {
                    Id = 1,
                    LiveStreamId = liveStreamId,
                    Question = "What's your favorite game?",
                    Options = new List<string> { "Action", "RPG", "Strategy", "Sports" },
                    IsActive = true,
                    StartTime = DateTime.UtcNow,
                    EndTime = DateTime.UtcNow.AddMinutes(30),
                    CreatedBy = "creator1",
                    CreatedAt = DateTime.UtcNow
                }
            },
            TotalCount = 1,
            Page = page,
            PageSize = pageSize
        };

        _mockPollService.Setup(s => s.GetPollsAsync(liveStreamId, page, pageSize))
            .ReturnsAsync(expectedPolls);

        // Act
        var result = await _controller.GetPolls(liveStreamId, page, pageSize);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var polls = Assert.IsType<PagedResult<LiveStreamPollDto>>(okResult.Value);
        Assert.Single(polls.Items);
        Assert.Equal(expectedPolls.TotalCount, polls.TotalCount);
        Assert.Equal(expectedPolls.Page, polls.Page);
        Assert.Equal(expectedPolls.PageSize, polls.PageSize);
    }

    [Fact]
    public async Task GetPolls_InvalidId_ReturnsNotFound()
    {
        // Arrange
        var liveStreamId = 999;
        var page = 1;
        var pageSize = 20;

        _mockPollService.Setup(s => s.GetPollsAsync(liveStreamId, page, pageSize))
            .ThrowsAsync(new ArgumentException("Live stream not found"));

        // Act
        var result = await _controller.GetPolls(liveStreamId, page, pageSize);

        // Assert
        var notFoundResult = Assert.IsType<NotFoundObjectResult>(result.Result);
        Assert.Equal("Live stream not found", notFoundResult.Value);
    }

    [Fact]
    public async Task GetPolls_InvalidPage_ReturnsBadRequest()
    {
        // Arrange
        var liveStreamId = 1;
        var page = 0; // Invalid page
        var pageSize = 20;

        _mockPollService.Setup(s => s.GetPollsAsync(liveStreamId, page, pageSize))
            .ThrowsAsync(new ArgumentException("Page must be greater than 0"));

        // Act
        var result = await _controller.GetPolls(liveStreamId, page, pageSize);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
        Assert.Equal("Page must be greater than 0", badRequestResult.Value);
    }

    [Fact]
    public async Task GetPolls_InvalidPageSize_ReturnsBadRequest()
    {
        // Arrange
        var liveStreamId = 1;
        var page = 1;
        var pageSize = 0; // Invalid page size

        _mockPollService.Setup(s => s.GetPollsAsync(liveStreamId, page, pageSize))
            .ThrowsAsync(new ArgumentException("Page size must be greater than 0"));

        // Act
        var result = await _controller.GetPolls(liveStreamId, page, pageSize);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
        Assert.Equal("Page size must be greater than 0", badRequestResult.Value);
    }

    [Fact]
    public async Task GetPolls_ServiceException_ReturnsInternalServerError()
    {
        // Arrange
        var liveStreamId = 1;
        var page = 1;
        var pageSize = 20;

        _mockPollService.Setup(s => s.GetPollsAsync(liveStreamId, page, pageSize))
            .ThrowsAsync(new Exception("Database connection failed"));

        // Act
        var result = await _controller.GetPolls(liveStreamId, page, pageSize);

        // Assert
        var statusCodeResult = Assert.IsType<ObjectResult>(result.Result);
        Assert.Equal(500, statusCodeResult.StatusCode);
        Assert.Equal("Database connection failed", statusCodeResult.Value);
    }

    [Fact]
    public async Task GetPolls_ValidRequest_ReturnsEmptyList()
    {
        // Arrange
        var liveStreamId = 1;
        var page = 1;
        var pageSize = 20;
        var expectedPolls = new PagedResult<LiveStreamPollDto>
        {
            Items = new List<LiveStreamPollDto>(),
            TotalCount = 0,
            Page = page,
            PageSize = pageSize
        };

        _mockPollService.Setup(s => s.GetPollsAsync(liveStreamId, page, pageSize))
            .ReturnsAsync(expectedPolls);

        // Act
        var result = await _controller.GetPolls(liveStreamId, page, pageSize);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var polls = Assert.IsType<PagedResult<LiveStreamPollDto>>(okResult.Value);
        Assert.Empty(polls.Items);
        Assert.Equal(0, polls.TotalCount);
    }

    [Fact]
    public async Task GetPolls_ValidRequest_ReturnsMultiplePolls()
    {
        // Arrange
        var liveStreamId = 1;
        var page = 1;
        var pageSize = 20;
        var expectedPolls = new PagedResult<LiveStreamPollDto>
        {
            Items = new List<LiveStreamPollDto>
            {
                new LiveStreamPollDto
                {
                    Id = 1,
                    LiveStreamId = liveStreamId,
                    Question = "What's your favorite game?",
                    Options = new List<string> { "Action", "RPG", "Strategy", "Sports" },
                    IsActive = true,
                    StartTime = DateTime.UtcNow,
                    EndTime = DateTime.UtcNow.AddMinutes(30),
                    CreatedBy = "creator1",
                    CreatedAt = DateTime.UtcNow
                },
                new LiveStreamPollDto
                {
                    Id = 2,
                    LiveStreamId = liveStreamId,
                    Question = "What should I play next?",
                    Options = new List<string> { "Minecraft", "Fortnite", "Among Us", "Fall Guys" },
                    IsActive = true,
                    StartTime = DateTime.UtcNow.AddMinutes(30),
                    EndTime = DateTime.UtcNow.AddMinutes(60),
                    CreatedBy = "creator1",
                    CreatedAt = DateTime.UtcNow
                }
            },
            TotalCount = 2,
            Page = page,
            PageSize = pageSize
        };

        _mockPollService.Setup(s => s.GetPollsAsync(liveStreamId, page, pageSize))
            .ReturnsAsync(expectedPolls);

        // Act
        var result = await _controller.GetPolls(liveStreamId, page, pageSize);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var polls = Assert.IsType<PagedResult<LiveStreamPollDto>>(okResult.Value);
        Assert.Equal(2, polls.Items.Count);
        Assert.Equal(expectedPolls.TotalCount, polls.TotalCount);
        Assert.Equal(expectedPolls.Page, polls.Page);
        Assert.Equal(expectedPolls.PageSize, polls.PageSize);
    }

    [Fact]
    public async Task GetPolls_ValidRequest_ReturnsPagedResults()
    {
        // Arrange
        var liveStreamId = 1;
        var page = 2;
        var pageSize = 10;
        var expectedPolls = new PagedResult<LiveStreamPollDto>
        {
            Items = new List<LiveStreamPollDto>
            {
                new LiveStreamPollDto
                {
                    Id = 11,
                    LiveStreamId = liveStreamId,
                    Question = "Page 2 Poll",
                    Options = new List<string> { "Option 1", "Option 2", "Option 3" },
                    IsActive = true,
                    StartTime = DateTime.UtcNow,
                    EndTime = DateTime.UtcNow.AddMinutes(30),
                    CreatedBy = "creator1",
                    CreatedAt = DateTime.UtcNow
                }
            },
            TotalCount = 25,
            Page = page,
            PageSize = pageSize
        };

        _mockPollService.Setup(s => s.GetPollsAsync(liveStreamId, page, pageSize))
            .ReturnsAsync(expectedPolls);

        // Act
        var result = await _controller.GetPolls(liveStreamId, page, pageSize);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var polls = Assert.IsType<PagedResult<LiveStreamPollDto>>(okResult.Value);
        Assert.Single(polls.Items);
        Assert.Equal(25, polls.TotalCount);
        Assert.Equal(2, polls.Page);
        Assert.Equal(10, polls.PageSize);
    }

    [Fact]
    public async Task GetPolls_ValidRequest_ReturnsActivePolls()
    {
        // Arrange
        var liveStreamId = 1;
        var page = 1;
        var pageSize = 20;
        var expectedPolls = new PagedResult<LiveStreamPollDto>
        {
            Items = new List<LiveStreamPollDto>
            {
                new LiveStreamPollDto
                {
                    Id = 1,
                    LiveStreamId = liveStreamId,
                    Question = "Active Poll",
                    Options = new List<string> { "Option 1", "Option 2", "Option 3" },
                    IsActive = true,
                    StartTime = DateTime.UtcNow,
                    EndTime = DateTime.UtcNow.AddMinutes(30),
                    CreatedBy = "creator1",
                    CreatedAt = DateTime.UtcNow
                }
            },
            TotalCount = 1,
            Page = page,
            PageSize = pageSize
        };

        _mockPollService.Setup(s => s.GetPollsAsync(liveStreamId, page, pageSize))
            .ReturnsAsync(expectedPolls);

        // Act
        var result = await _controller.GetPolls(liveStreamId, page, pageSize);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var polls = Assert.IsType<PagedResult<LiveStreamPollDto>>(okResult.Value);
        Assert.Single(polls.Items);
        Assert.True(polls.Items.First().IsActive);
        Assert.Equal("Active Poll", polls.Items.First().Question);
    }

    [Fact]
    public async Task GetPolls_ValidRequest_ReturnsInactivePolls()
    {
        // Arrange
        var liveStreamId = 1;
        var page = 1;
        var pageSize = 20;
        var expectedPolls = new PagedResult<LiveStreamPollDto>
        {
            Items = new List<LiveStreamPollDto>
            {
                new LiveStreamPollDto
                {
                    Id = 1,
                    LiveStreamId = liveStreamId,
                    Question = "Inactive Poll",
                    Options = new List<string> { "Option 1", "Option 2", "Option 3" },
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

        _mockPollService.Setup(s => s.GetPollsAsync(liveStreamId, page, pageSize))
            .ReturnsAsync(expectedPolls);

        // Act
        var result = await _controller.GetPolls(liveStreamId, page, pageSize);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var polls = Assert.IsType<PagedResult<LiveStreamPollDto>>(okResult.Value);
        Assert.Single(polls.Items);
        Assert.False(polls.Items.First().IsActive);
        Assert.Equal("Inactive Poll", polls.Items.First().Question);
    }
}