using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using Moq;
using Drashyam.API.Controllers;
using Drashyam.API.DTOs;
using Drashyam.API.Hubs;
using Drashyam.API.Services;

namespace Drashyam.API.Tests.Controllers;

public class LiveStreamChatControllerTests
{
    private readonly Mock<ILiveStreamChatService> _mockChatService;
    private readonly Mock<IHubContext<ChatHub>> _mockChatHub;
    private readonly Mock<ILogger<LiveStreamChatController>> _mockLogger;
    private readonly LiveStreamChatController _controller;

    public LiveStreamChatControllerTests()
    {
        _mockChatService = new Mock<ILiveStreamChatService>();
        _mockChatHub = new Mock<IHubContext<ChatHub>>();
        _mockLogger = new Mock<ILogger<LiveStreamChatController>>();

        _controller = new LiveStreamChatController(_mockChatService.Object, _mockChatHub.Object, _mockLogger.Object);
    }

    [Fact]
    public async Task GetChatMessages_ValidId_ReturnsOk()
    {
        // Arrange
        var liveStreamId = 1;
        var page = 1;
        var pageSize = 20;
        var expectedMessages = new PagedResult<LiveStreamChatDto>
        {
            Items = new List<LiveStreamChatDto>
            {
                new LiveStreamChatDto
                {
                    Id = 1,
                    LiveStreamId = liveStreamId,
                    UserId = "user1",
                    Message = "Hello everyone!",
                    MessageType = ChatMessageType.Text,
                    IsModerator = false,
                    IsSubscriber = true,
                    IsVip = false,
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

        _mockChatService.Setup(s => s.GetChatMessagesAsync(liveStreamId, page, pageSize))
            .ReturnsAsync(expectedMessages);

        // Act
        var result = await _controller.GetChatMessages(liveStreamId, page, pageSize);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var messages = Assert.IsType<PagedResult<LiveStreamChatDto>>(okResult.Value);
        Assert.Single(messages.Items);
        Assert.Equal(expectedMessages.TotalCount, messages.TotalCount);
        Assert.Equal(expectedMessages.Page, messages.Page);
        Assert.Equal(expectedMessages.PageSize, messages.PageSize);
    }

    [Fact]
    public async Task GetChatMessages_InvalidId_ReturnsNotFound()
    {
        // Arrange
        var liveStreamId = 999;
        var page = 1;
        var pageSize = 20;

        _mockChatService.Setup(s => s.GetChatMessagesAsync(liveStreamId, page, pageSize))
            .ThrowsAsync(new ArgumentException("Live stream not found"));

        // Act
        var result = await _controller.GetChatMessages(liveStreamId, page, pageSize);

        // Assert
        var notFoundResult = Assert.IsType<NotFoundObjectResult>(result.Result);
        Assert.Equal("Live stream not found", notFoundResult.Value);
    }

    [Fact]
    public async Task GetChatMessages_InvalidPage_ReturnsBadRequest()
    {
        // Arrange
        var liveStreamId = 1;
        var page = 0; // Invalid page
        var pageSize = 20;

        _mockChatService.Setup(s => s.GetChatMessagesAsync(liveStreamId, page, pageSize))
            .ThrowsAsync(new ArgumentException("Page must be greater than 0"));

        // Act
        var result = await _controller.GetChatMessages(liveStreamId, page, pageSize);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
        Assert.Equal("Page must be greater than 0", badRequestResult.Value);
    }

    [Fact]
    public async Task GetChatMessages_InvalidPageSize_ReturnsBadRequest()
    {
        // Arrange
        var liveStreamId = 1;
        var page = 1;
        var pageSize = 0; // Invalid page size

        _mockChatService.Setup(s => s.GetChatMessagesAsync(liveStreamId, page, pageSize))
            .ThrowsAsync(new ArgumentException("Page size must be greater than 0"));

        // Act
        var result = await _controller.GetChatMessages(liveStreamId, page, pageSize);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
        Assert.Equal("Page size must be greater than 0", badRequestResult.Value);
    }

    [Fact]
    public async Task GetChatMessages_ServiceException_ReturnsInternalServerError()
    {
        // Arrange
        var liveStreamId = 1;
        var page = 1;
        var pageSize = 20;

        _mockChatService.Setup(s => s.GetChatMessagesAsync(liveStreamId, page, pageSize))
            .ThrowsAsync(new Exception("Database connection failed"));

        // Act
        var result = await _controller.GetChatMessages(liveStreamId, page, pageSize);

        // Assert
        var statusCodeResult = Assert.IsType<ObjectResult>(result.Result);
        Assert.Equal(500, statusCodeResult.StatusCode);
        Assert.Equal("Database connection failed", statusCodeResult.Value);
    }

    [Fact]
    public async Task GetChatMessages_ValidRequest_ReturnsEmptyList()
    {
        // Arrange
        var liveStreamId = 1;
        var page = 1;
        var pageSize = 20;
        var expectedMessages = new PagedResult<LiveStreamChatDto>
        {
            Items = new List<LiveStreamChatDto>(),
            TotalCount = 0,
            Page = page,
            PageSize = pageSize
        };

        _mockChatService.Setup(s => s.GetChatMessagesAsync(liveStreamId, page, pageSize))
            .ReturnsAsync(expectedMessages);

        // Act
        var result = await _controller.GetChatMessages(liveStreamId, page, pageSize);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var messages = Assert.IsType<PagedResult<LiveStreamChatDto>>(okResult.Value);
        Assert.Empty(messages.Items);
        Assert.Equal(0, messages.TotalCount);
    }

    [Fact]
    public async Task GetChatMessages_ValidRequest_ReturnsMultipleMessages()
    {
        // Arrange
        var liveStreamId = 1;
        var page = 1;
        var pageSize = 20;
        var expectedMessages = new PagedResult<LiveStreamChatDto>
        {
            Items = new List<LiveStreamChatDto>
            {
                new LiveStreamChatDto
                {
                    Id = 1,
                    LiveStreamId = liveStreamId,
                    UserId = "user1",
                    Message = "Hello everyone!",
                    MessageType = ChatMessageType.Text,
                    IsModerator = false,
                    IsSubscriber = true,
                    IsVip = false,
                    Timestamp = DateTime.UtcNow,
                    User = new UserDto
                    {
                        Id = "user1",
                        UserName = "testuser1",
                        Email = "test1@example.com"
                    }
                },
                new LiveStreamChatDto
                {
                    Id = 2,
                    LiveStreamId = liveStreamId,
                    UserId = "user2",
                    Message = "Great stream!",
                    MessageType = ChatMessageType.Text,
                    IsModerator = true,
                    IsSubscriber = false,
                    IsVip = true,
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

        _mockChatService.Setup(s => s.GetChatMessagesAsync(liveStreamId, page, pageSize))
            .ReturnsAsync(expectedMessages);

        // Act
        var result = await _controller.GetChatMessages(liveStreamId, page, pageSize);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var messages = Assert.IsType<PagedResult<LiveStreamChatDto>>(okResult.Value);
        Assert.Equal(2, messages.Items.Count);
        Assert.Equal(expectedMessages.TotalCount, messages.TotalCount);
        Assert.Equal(expectedMessages.Page, messages.Page);
        Assert.Equal(expectedMessages.PageSize, messages.PageSize);
    }

    [Fact]
    public async Task GetChatMessages_ValidRequest_ReturnsPagedResults()
    {
        // Arrange
        var liveStreamId = 1;
        var page = 2;
        var pageSize = 10;
        var expectedMessages = new PagedResult<LiveStreamChatDto>
        {
            Items = new List<LiveStreamChatDto>
            {
                new LiveStreamChatDto
                {
                    Id = 11,
                    LiveStreamId = liveStreamId,
                    UserId = "user11",
                    Message = "Page 2 message",
                    MessageType = ChatMessageType.Text,
                    IsModerator = false,
                    IsSubscriber = false,
                    IsVip = false,
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

        _mockChatService.Setup(s => s.GetChatMessagesAsync(liveStreamId, page, pageSize))
            .ReturnsAsync(expectedMessages);

        // Act
        var result = await _controller.GetChatMessages(liveStreamId, page, pageSize);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var messages = Assert.IsType<PagedResult<LiveStreamChatDto>>(okResult.Value);
        Assert.Single(messages.Items);
        Assert.Equal(25, messages.TotalCount);
        Assert.Equal(2, messages.Page);
        Assert.Equal(10, messages.PageSize);
    }
}
