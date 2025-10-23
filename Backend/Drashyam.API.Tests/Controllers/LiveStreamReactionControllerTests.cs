using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using Drashyam.API.Controllers;
using Drashyam.API.DTOs;
using Drashyam.API.Services;

namespace Drashyam.API.Tests.Controllers;

public class LiveStreamReactionControllerTests
{
    private readonly Mock<ILiveStreamReactionService> _mockReactionService;
    private readonly Mock<ILogger<LiveStreamReactionController>> _mockLogger;
    private readonly LiveStreamReactionController _controller;

    public LiveStreamReactionControllerTests()
    {
        _mockReactionService = new Mock<ILiveStreamReactionService>();
        _mockLogger = new Mock<ILogger<LiveStreamReactionController>>();

        _controller = new LiveStreamReactionController(_mockReactionService.Object, _mockLogger.Object);
    }

    [Fact]
    public async Task GetReactions_ValidId_ReturnsOk()
    {
        // Arrange
        var liveStreamId = 1;
        var page = 1;
        var pageSize = 20;
        var expectedReactions = new PagedResult<LiveStreamReactionDto>
        {
            Items = new List<LiveStreamReactionDto>
            {
                new LiveStreamReactionDto
                {
                    Id = 1,
                    LiveStreamId = liveStreamId,
                    UserId = "user1",
                    ReactionType = LiveStreamReactionType.Like,
                    Timestamp = DateTime.UtcNow,
                    User = new UserDto
                    {
                        Id = "user1",
                        UserName = "testuser",
                        Email = "test@example.com"
                    }
                }
            },
            TotalCount = 1,
            Page = page,
            PageSize = pageSize
        };

        _mockReactionService.Setup(s => s.GetReactionsAsync(liveStreamId, page, pageSize))
            .ReturnsAsync(expectedReactions);

        // Act
        var result = await _controller.GetReactions(liveStreamId, page, pageSize);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var reactions = Assert.IsType<PagedResult<LiveStreamReactionDto>>(okResult.Value);
        Assert.Single(reactions.Items);
        Assert.Equal(expectedReactions.TotalCount, reactions.TotalCount);
        Assert.Equal(expectedReactions.Page, reactions.Page);
        Assert.Equal(expectedReactions.PageSize, reactions.PageSize);
    }

    [Fact]
    public async Task GetReactions_InvalidId_ReturnsNotFound()
    {
        // Arrange
        var liveStreamId = 999;
        var page = 1;
        var pageSize = 20;

        _mockReactionService.Setup(s => s.GetReactionsAsync(liveStreamId, page, pageSize))
            .ThrowsAsync(new ArgumentException("Live stream not found"));

        // Act
        var result = await _controller.GetReactions(liveStreamId, page, pageSize);

        // Assert
        var notFoundResult = Assert.IsType<NotFoundObjectResult>(result.Result);
        Assert.Equal("Live stream not found", notFoundResult.Value);
    }

    [Fact]
    public async Task GetReactions_InvalidPage_ReturnsBadRequest()
    {
        // Arrange
        var liveStreamId = 1;
        var page = 0; // Invalid page
        var pageSize = 20;

        _mockReactionService.Setup(s => s.GetReactionsAsync(liveStreamId, page, pageSize))
            .ThrowsAsync(new ArgumentException("Page must be greater than 0"));

        // Act
        var result = await _controller.GetReactions(liveStreamId, page, pageSize);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
        Assert.Equal("Page must be greater than 0", badRequestResult.Value);
    }

    [Fact]
    public async Task GetReactions_InvalidPageSize_ReturnsBadRequest()
    {
        // Arrange
        var liveStreamId = 1;
        var page = 1;
        var pageSize = 0; // Invalid page size

        _mockReactionService.Setup(s => s.GetReactionsAsync(liveStreamId, page, pageSize))
            .ThrowsAsync(new ArgumentException("Page size must be greater than 0"));

        // Act
        var result = await _controller.GetReactions(liveStreamId, page, pageSize);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
        Assert.Equal("Page size must be greater than 0", badRequestResult.Value);
    }

    [Fact]
    public async Task GetReactions_ServiceException_ReturnsInternalServerError()
    {
        // Arrange
        var liveStreamId = 1;
        var page = 1;
        var pageSize = 20;

        _mockReactionService.Setup(s => s.GetReactionsAsync(liveStreamId, page, pageSize))
            .ThrowsAsync(new Exception("Database connection failed"));

        // Act
        var result = await _controller.GetReactions(liveStreamId, page, pageSize);

        // Assert
        var statusCodeResult = Assert.IsType<ObjectResult>(result.Result);
        Assert.Equal(500, statusCodeResult.StatusCode);
        Assert.Equal("Database connection failed", statusCodeResult.Value);
    }

    [Fact]
    public async Task GetReactions_ValidRequest_ReturnsEmptyList()
    {
        // Arrange
        var liveStreamId = 1;
        var page = 1;
        var pageSize = 20;
        var expectedReactions = new PagedResult<LiveStreamReactionDto>
        {
            Items = new List<LiveStreamReactionDto>(),
            TotalCount = 0,
            Page = page,
            PageSize = pageSize
        };

        _mockReactionService.Setup(s => s.GetReactionsAsync(liveStreamId, page, pageSize))
            .ReturnsAsync(expectedReactions);

        // Act
        var result = await _controller.GetReactions(liveStreamId, page, pageSize);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var reactions = Assert.IsType<PagedResult<LiveStreamReactionDto>>(okResult.Value);
        Assert.Empty(reactions.Items);
        Assert.Equal(0, reactions.TotalCount);
    }

    [Fact]
    public async Task GetReactions_ValidRequest_ReturnsMultipleReactions()
    {
        // Arrange
        var liveStreamId = 1;
        var page = 1;
        var pageSize = 20;
        var expectedReactions = new PagedResult<LiveStreamReactionDto>
        {
            Items = new List<LiveStreamReactionDto>
            {
                new LiveStreamReactionDto
                {
                    Id = 1,
                    LiveStreamId = liveStreamId,
                    UserId = "user1",
                    ReactionType = LiveStreamReactionType.Like,
                    Timestamp = DateTime.UtcNow,
                    User = new UserDto
                    {
                        Id = "user1",
                        UserName = "testuser1",
                        Email = "test1@example.com"
                    }
                },
                new LiveStreamReactionDto
                {
                    Id = 2,
                    LiveStreamId = liveStreamId,
                    UserId = "user2",
                    ReactionType = LiveStreamReactionType.Love,
                    Timestamp = DateTime.UtcNow.AddMinutes(1),
                    User = new UserDto
                    {
                        Id = "user2",
                        UserName = "testuser2",
                        Email = "test2@example.com"
                    }
                }
            },
            TotalCount = 2,
            Page = page,
            PageSize = pageSize
        };

        _mockReactionService.Setup(s => s.GetReactionsAsync(liveStreamId, page, pageSize))
            .ReturnsAsync(expectedReactions);

        // Act
        var result = await _controller.GetReactions(liveStreamId, page, pageSize);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var reactions = Assert.IsType<PagedResult<LiveStreamReactionDto>>(okResult.Value);
        Assert.Equal(2, reactions.Items.Count);
        Assert.Equal(expectedReactions.TotalCount, reactions.TotalCount);
        Assert.Equal(expectedReactions.Page, reactions.Page);
        Assert.Equal(expectedReactions.PageSize, reactions.PageSize);
    }

    [Fact]
    public async Task GetReactions_ValidRequest_ReturnsPagedResults()
    {
        // Arrange
        var liveStreamId = 1;
        var page = 2;
        var pageSize = 10;
        var expectedReactions = new PagedResult<LiveStreamReactionDto>
        {
            Items = new List<LiveStreamReactionDto>
            {
                new LiveStreamReactionDto
                {
                    Id = 11,
                    LiveStreamId = liveStreamId,
                    UserId = "user11",
                    ReactionType = LiveStreamReactionType.Like,
                    Timestamp = DateTime.UtcNow,
                    User = new UserDto
                    {
                        Id = "user11",
                        UserName = "testuser11",
                        Email = "test11@example.com"
                    }
                }
            },
            TotalCount = 25,
            Page = page,
            PageSize = pageSize
        };

        _mockReactionService.Setup(s => s.GetReactionsAsync(liveStreamId, page, pageSize))
            .ReturnsAsync(expectedReactions);

        // Act
        var result = await _controller.GetReactions(liveStreamId, page, pageSize);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var reactions = Assert.IsType<PagedResult<LiveStreamReactionDto>>(okResult.Value);
        Assert.Single(reactions.Items);
        Assert.Equal(25, reactions.TotalCount);
        Assert.Equal(2, reactions.Page);
        Assert.Equal(10, reactions.PageSize);
    }

    [Fact]
    public async Task GetReactions_ValidRequest_ReturnsLikeReactions()
    {
        // Arrange
        var liveStreamId = 1;
        var page = 1;
        var pageSize = 20;
        var expectedReactions = new PagedResult<LiveStreamReactionDto>
        {
            Items = new List<LiveStreamReactionDto>
            {
                new LiveStreamReactionDto
                {
                    Id = 1,
                    LiveStreamId = liveStreamId,
                    UserId = "user1",
                    ReactionType = LiveStreamReactionType.Like,
                    Timestamp = DateTime.UtcNow,
                    User = new UserDto
                    {
                        Id = "user1",
                        UserName = "testuser",
                        Email = "test@example.com"
                    }
                }
            },
            TotalCount = 1,
            Page = page,
            PageSize = pageSize
        };

        _mockReactionService.Setup(s => s.GetReactionsAsync(liveStreamId, page, pageSize))
            .ReturnsAsync(expectedReactions);

        // Act
        var result = await _controller.GetReactions(liveStreamId, page, pageSize);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var reactions = Assert.IsType<PagedResult<LiveStreamReactionDto>>(okResult.Value);
        Assert.Single(reactions.Items);
        Assert.Equal(LiveStreamReactionType.Like, reactions.Items.First().ReactionType);
    }

    [Fact]
    public async Task GetReactions_ValidRequest_ReturnsLoveReactions()
    {
        // Arrange
        var liveStreamId = 1;
        var page = 1;
        var pageSize = 20;
        var expectedReactions = new PagedResult<LiveStreamReactionDto>
        {
            Items = new List<LiveStreamReactionDto>
            {
                new LiveStreamReactionDto
                {
                    Id = 1,
                    LiveStreamId = liveStreamId,
                    UserId = "user1",
                    ReactionType = LiveStreamReactionType.Love,
                    Timestamp = DateTime.UtcNow,
                    User = new UserDto
                    {
                        Id = "user1",
                        UserName = "testuser",
                        Email = "test@example.com"
                    }
                }
            },
            TotalCount = 1,
            Page = page,
            PageSize = pageSize
        };

        _mockReactionService.Setup(s => s.GetReactionsAsync(liveStreamId, page, pageSize))
            .ReturnsAsync(expectedReactions);

        // Act
        var result = await _controller.GetReactions(liveStreamId, page, pageSize);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var reactions = Assert.IsType<PagedResult<LiveStreamReactionDto>>(okResult.Value);
        Assert.Single(reactions.Items);
        Assert.Equal(LiveStreamReactionType.Love, reactions.Items.First().ReactionType);
    }
}