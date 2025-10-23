using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using Drashyam.API.Controllers;
using Drashyam.API.DTOs;
using Drashyam.API.Services;
using Drashyam.API.Hubs;
using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;

namespace Drashyam.API.Tests.Controllers;

public class LiveStreamControllerTests
{
    private readonly Mock<ILiveStreamService> _mockLiveStreamService;
    private readonly Mock<IHubContext<LiveStreamHub>> _mockLiveStreamHub;
    private readonly Mock<IHubContext<ChatHub>> _mockChatHub;
    private readonly Mock<IHubContext<NotificationHub>> _mockNotificationHub;
    private readonly Mock<ILogger<LiveStreamController>> _mockLogger;
    private readonly LiveStreamController _controller;

    public LiveStreamControllerTests()
    {
        _mockLiveStreamService = new Mock<ILiveStreamService>();
        _mockLiveStreamHub = new Mock<IHubContext<LiveStreamHub>>();
        _mockChatHub = new Mock<IHubContext<ChatHub>>();
        _mockNotificationHub = new Mock<IHubContext<NotificationHub>>();
        _mockLogger = new Mock<ILogger<LiveStreamController>>();

        _controller = new LiveStreamController(
            _mockLiveStreamService.Object,
            _mockLiveStreamHub.Object,
            _mockChatHub.Object,
            _mockNotificationHub.Object,
            _mockLogger.Object
        );

        // Setup user claims
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, "test-user-id"),
            new Claim("sub", "test-user-id")
        };
        var identity = new ClaimsIdentity(claims, "test");
        var principal = new ClaimsPrincipal(identity);
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new Microsoft.AspNetCore.Http.DefaultHttpContext
            {
                User = principal
            }
        };
    }

    [Fact]
    public async Task CreateLiveStream_ValidRequest_ReturnsOk()
    {
        // Arrange
        var dto = new CreateLiveStreamDto
        {
            Title = "Test Stream",
            Description = "Test Description",
            ChannelId = 1,
            IsPublic = true,
            IsChatEnabled = true,
            IsMonetizationEnabled = true,
            IsRecording = false,
            ScheduledStartTime = null
        };

        var expectedStream = new LiveStreamDto
        {
            Id = 1,
            Title = "Test Stream",
            Description = "Test Description",
            ChannelId = 1,
            UserId = "test-user-id",
            Status = DTOs.LiveStreamStatus.Scheduled,
            CreatedAt = DateTime.UtcNow
        };

        _mockLiveStreamService.Setup(s => s.CreateLiveStreamAsync(dto, "test-user-id"))
            .ReturnsAsync(expectedStream);

        // Act
        var result = await _controller.CreateLiveStream(dto);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var stream = Assert.IsType<LiveStreamDto>(okResult.Value);
        Assert.Equal(expectedStream.Id, stream.Id);
        Assert.Equal(expectedStream.Title, stream.Title);
    }

    [Fact]
    public async Task CreateLiveStream_InvalidRequest_ReturnsBadRequest()
    {
        // Arrange
        var dto = new CreateLiveStreamDto
        {
            Title = "",
            Description = "Test Description",
            ChannelId = 1,
            IsPublic = true,
            IsChatEnabled = true,
            IsMonetizationEnabled = true,
            IsRecording = false,
            ScheduledStartTime = null
        };

        _mockLiveStreamService.Setup(s => s.CreateLiveStreamAsync(dto, "test-user-id"))
            .ThrowsAsync(new ArgumentException("Title is required"));

        // Act
        var result = await _controller.CreateLiveStream(dto);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
        Assert.Equal("Title is required", badRequestResult.Value);
    }

    [Fact]
    public async Task GetLiveStream_ValidId_ReturnsOk()
    {
        // Arrange
        var streamId = 1;
        var expectedStream = new LiveStreamDto
        {
            Id = streamId,
            Title = "Test Stream",
            Description = "Test Description",
            ChannelId = 1,
            UserId = "test-user-id",
            Status = DTOs.LiveStreamStatus.Live,
            CreatedAt = DateTime.UtcNow
        };

        _mockLiveStreamService.Setup(s => s.GetLiveStreamAsync(streamId))
            .ReturnsAsync(expectedStream);

        // Act
        var result = await _controller.GetLiveStream(streamId);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var stream = Assert.IsType<LiveStreamDto>(okResult.Value);
        Assert.Equal(expectedStream.Id, stream.Id);
        Assert.Equal(expectedStream.Title, stream.Title);
    }

    [Fact]
    public async Task GetLiveStream_InvalidId_ReturnsNotFound()
    {
        // Arrange
        var streamId = 999;
        _mockLiveStreamService.Setup(s => s.GetLiveStreamAsync(streamId))
            .ThrowsAsync(new ArgumentException("Live stream not found"));

        // Act
        var result = await _controller.GetLiveStream(streamId);

        // Assert
        var notFoundResult = Assert.IsType<NotFoundObjectResult>(result.Result);
        Assert.Equal("Live stream not found", notFoundResult.Value);
    }

    [Fact]
    public async Task GetLiveStreams_ValidRequest_ReturnsOk()
    {
        // Arrange
        var userId = "test-user-id";
        var page = 1;
        var pageSize = 10;
        var expectedStreams = new List<LiveStreamDto>
        {
            new LiveStreamDto
            {
                Id = 1,
                Title = "Test Stream 1",
                Description = "Test Description 1",
                ChannelId = 1,
                UserId = userId,
                Status = DTOs.LiveStreamStatus.Live,
                CreatedAt = DateTime.UtcNow
            },
            new LiveStreamDto
            {
                Id = 2,
                Title = "Test Stream 2",
                Description = "Test Description 2",
                ChannelId = 1,
                UserId = userId,
                Status = DTOs.LiveStreamStatus.Scheduled,
                CreatedAt = DateTime.UtcNow
            }
        };

        _mockLiveStreamService.Setup(s => s.GetLiveStreamsAsync(userId, page, pageSize))
            .ReturnsAsync(expectedStreams);

        // Act
        var result = await _controller.GetLiveStreams(userId, page, pageSize);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var streams = Assert.IsType<List<LiveStreamDto>>(okResult.Value);
        Assert.Equal(2, streams.Count);
    }

    [Fact]
    public async Task UpdateLiveStream_ValidRequest_ReturnsOk()
    {
        // Arrange
        var streamId = 1;
        var dto = new UpdateLiveStreamDto
        {
            Title = "Updated Test Stream",
            Description = "Updated Test Description",
            IsPublic = false,
            IsChatEnabled = false,
            IsMonetizationEnabled = false,
            IsRecording = true
        };

        var expectedStream = new LiveStreamDto
        {
            Id = streamId,
            Title = "Updated Test Stream",
            Description = "Updated Test Description",
            ChannelId = 1,
            UserId = "test-user-id",
            Status = DTOs.LiveStreamStatus.Live,
            UpdatedAt = DateTime.UtcNow
        };

        _mockLiveStreamService.Setup(s => s.UpdateLiveStreamAsync(streamId, dto, "test-user-id"))
            .ReturnsAsync(expectedStream);

        // Act
        var result = await _controller.UpdateLiveStream(streamId, dto);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var stream = Assert.IsType<LiveStreamDto>(okResult.Value);
        Assert.Equal(expectedStream.Id, stream.Id);
        Assert.Equal(expectedStream.Title, stream.Title);
    }

    [Fact]
    public async Task UpdateLiveStream_InvalidId_ReturnsNotFound()
    {
        // Arrange
        var streamId = 999;
        var dto = new UpdateLiveStreamDto
        {
            Title = "Updated Test Stream",
            Description = "Updated Test Description",
            IsPublic = false,
            IsChatEnabled = false,
            IsMonetizationEnabled = false,
            IsRecording = true
        };

        _mockLiveStreamService.Setup(s => s.UpdateLiveStreamAsync(streamId, dto, "test-user-id"))
            .ThrowsAsync(new ArgumentException("Live stream not found"));

        // Act
        var result = await _controller.UpdateLiveStream(streamId, dto);

        // Assert
        var notFoundResult = Assert.IsType<NotFoundObjectResult>(result.Result);
        Assert.Equal("Live stream not found", notFoundResult.Value);
    }

    [Fact]
    public async Task DeleteLiveStream_ValidId_ReturnsNoContent()
    {
        // Arrange
        var streamId = 1;
        _mockLiveStreamService.Setup(s => s.DeleteLiveStreamAsync(streamId, "test-user-id"))
            .ReturnsAsync(true);

        // Act
        var result = await _controller.DeleteLiveStream(streamId);

        // Assert
        Assert.IsType<NoContentResult>(result);
    }

    [Fact]
    public async Task DeleteLiveStream_InvalidId_ReturnsNotFound()
    {
        // Arrange
        var streamId = 999;
        _mockLiveStreamService.Setup(s => s.DeleteLiveStreamAsync(streamId, "test-user-id"))
            .ThrowsAsync(new ArgumentException("Live stream not found"));

        // Act
        var result = await _controller.DeleteLiveStream(streamId);

        // Assert
        var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
        Assert.Equal("Live stream not found", notFoundResult.Value);
    }

    [Fact]
    public async Task StartLiveStream_ValidId_ReturnsOk()
    {
        // Arrange
        var streamId = 1;
        var expectedStream = new LiveStreamDto
        {
            Id = streamId,
            Title = "Test Stream",
            Description = "Test Description",
            ChannelId = 1,
            UserId = "test-user-id",
            Status = DTOs.LiveStreamStatus.Live,
            ActualStartTime = DateTime.UtcNow
        };

        _mockLiveStreamService.Setup(s => s.StartLiveStreamAsync(streamId, "test-user-id"))
            .ReturnsAsync(expectedStream);

        // Act
        var result = await _controller.Start(streamId);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var stream = Assert.IsType<LiveStreamDto>(okResult.Value);
        Assert.Equal(expectedStream.Id, stream.Id);
        Assert.Equal(DTOs.LiveStreamStatus.Live, stream.Status);
    }

    [Fact]
    public async Task StartLiveStream_InvalidId_ReturnsNotFound()
    {
        // Arrange
        var streamId = 999;
        _mockLiveStreamService.Setup(s => s.StartLiveStreamAsync(streamId, "test-user-id"))
            .ThrowsAsync(new ArgumentException("Live stream not found"));

        // Act
        var result = await _controller.Start(streamId);

        // Assert
        var notFoundResult = Assert.IsType<NotFoundObjectResult>(result.Result);
        Assert.Equal("Live stream not found", notFoundResult.Value);
    }

    [Fact]
    public async Task StopLiveStream_ValidId_ReturnsOk()
    {
        // Arrange
        var streamId = 1;
        var expectedStream = new LiveStreamDto
        {
            Id = streamId,
            Title = "Test Stream",
            Description = "Test Description",
            ChannelId = 1,
            UserId = "test-user-id",
            Status = DTOs.LiveStreamStatus.Ended,
            ActualEndTime = DateTime.UtcNow
        };

        _mockLiveStreamService.Setup(s => s.StopLiveStreamAsync(streamId, "test-user-id"))
            .ReturnsAsync(expectedStream);

        // Act
        var result = await _controller.Stop(streamId);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var stream = Assert.IsType<LiveStreamDto>(okResult.Value);
        Assert.Equal(expectedStream.Id, stream.Id);
        Assert.Equal(DTOs.LiveStreamStatus.Ended, stream.Status);
    }

    [Fact]
    public async Task StopLiveStream_InvalidId_ReturnsNotFound()
    {
        // Arrange
        var streamId = 999;
        _mockLiveStreamService.Setup(s => s.StopLiveStreamAsync(streamId, "test-user-id"))
            .ThrowsAsync(new ArgumentException("Live stream not found"));

        // Act
        var result = await _controller.Stop(streamId);

        // Assert
        var notFoundResult = Assert.IsType<NotFoundObjectResult>(result.Result);
        Assert.Equal("Live stream not found", notFoundResult.Value);
    }

    [Fact]
    public async Task UpdateViewerCount_ValidRequest_ReturnsOk()
    {
        // Arrange
        var streamId = 1;
        var viewerCount = 150L;

        _mockLiveStreamService.Setup(s => s.UpdateViewerCountAsync(streamId, viewerCount, "test-user-id"))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _controller.UpdateViewerCount(streamId, viewerCount);

        // Assert
        Assert.IsType<OkResult>(result);
    }

    [Fact]
    public async Task UpdateViewerCount_InvalidId_ReturnsNotFound()
    {
        // Arrange
        var streamId = 999;
        var viewerCount = 150L;

        _mockLiveStreamService.Setup(s => s.UpdateViewerCountAsync(streamId, viewerCount, "test-user-id"))
            .ThrowsAsync(new ArgumentException("Live stream not found"));

        // Act
        var result = await _controller.UpdateViewerCount(streamId, viewerCount);

        // Assert
        var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
        Assert.Equal("Live stream not found", notFoundResult.Value);
    }

    [Fact]
    public async Task GetViewerCount_ValidId_ReturnsOk()
    {
        // Arrange
        var streamId = 1;
        var expectedCount = 150L;

        _mockLiveStreamService.Setup(s => s.GetLiveStreamViewerCountAsync(streamId))
            .ReturnsAsync(expectedCount);

        // Act
        var result = await _controller.GetViewerCount(streamId);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var count = Assert.IsType<long>(okResult.Value);
        Assert.Equal(expectedCount, count);
    }

    [Fact]
    public async Task GetViewerCount_InvalidId_ReturnsNotFound()
    {
        // Arrange
        var streamId = 999;
        _mockLiveStreamService.Setup(s => s.GetLiveStreamViewerCountAsync(streamId))
            .ThrowsAsync(new ArgumentException("Live stream not found"));

        // Act
        var result = await _controller.GetViewerCount(streamId);

        // Assert
        var notFoundResult = Assert.IsType<NotFoundObjectResult>(result.Result);
        Assert.Equal("Live stream not found", notFoundResult.Value);
    }
}
