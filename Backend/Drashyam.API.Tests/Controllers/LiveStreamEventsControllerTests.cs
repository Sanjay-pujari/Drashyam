using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using Drashyam.API.Controllers;
using Drashyam.API.DTOs;
using Drashyam.API.Services;

namespace Drashyam.API.Tests.Controllers;

public class LiveStreamEventsControllerTests
{
    private readonly Mock<ILiveStreamEventsService> _mockEventsService;
    private readonly Mock<ILogger<LiveStreamEventsController>> _mockLogger;
    private readonly LiveStreamEventsController _controller;

    public LiveStreamEventsControllerTests()
    {
        _mockEventsService = new Mock<ILiveStreamEventsService>();
        _mockLogger = new Mock<ILogger<LiveStreamEventsController>>();

        _controller = new LiveStreamEventsController(_mockEventsService.Object, _mockLogger.Object);
    }

    [Fact]
    public async Task GetEvents_ValidId_ReturnsOk()
    {
        // Arrange
        var liveStreamId = 1;
        var page = 1;
        var pageSize = 20;
        var expectedEvents = new PagedResult<LiveStreamEventDto>
        {
            Items = new List<LiveStreamEventDto>
            {
                new LiveStreamEventDto
                {
                    Id = 1,
                    LiveStreamId = liveStreamId,
                    EventType = LiveStreamEventType.Milestone,
                    Title = "100 Viewers Milestone",
                    Description = "Congratulations on reaching 100 viewers!",
                    IsActive = true,
                    StartTime = DateTime.UtcNow,
                    EndTime = DateTime.UtcNow.AddHours(1),
                    CreatedBy = "creator1",
                    CreatedAt = DateTime.UtcNow
                }
            },
            TotalCount = 1,
            Page = page,
            PageSize = pageSize
        };

        _mockEventsService.Setup(s => s.GetEventsAsync(liveStreamId, page, pageSize))
            .ReturnsAsync(expectedEvents);

        // Act
        var result = await _controller.GetEvents(liveStreamId, page, pageSize);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var events = Assert.IsType<PagedResult<LiveStreamEventDto>>(okResult.Value);
        Assert.Single(events.Items);
        Assert.Equal(expectedEvents.TotalCount, events.TotalCount);
        Assert.Equal(expectedEvents.Page, events.Page);
        Assert.Equal(expectedEvents.PageSize, events.PageSize);
    }

    [Fact]
    public async Task GetEvents_InvalidId_ReturnsNotFound()
    {
        // Arrange
        var liveStreamId = 999;
        var page = 1;
        var pageSize = 20;

        _mockEventsService.Setup(s => s.GetEventsAsync(liveStreamId, page, pageSize))
            .ThrowsAsync(new ArgumentException("Live stream not found"));

        // Act
        var result = await _controller.GetEvents(liveStreamId, page, pageSize);

        // Assert
        var notFoundResult = Assert.IsType<NotFoundObjectResult>(result.Result);
        Assert.Equal("Live stream not found", notFoundResult.Value);
    }

    [Fact]
    public async Task GetEvents_InvalidPage_ReturnsBadRequest()
    {
        // Arrange
        var liveStreamId = 1;
        var page = 0; // Invalid page
        var pageSize = 20;

        _mockEventsService.Setup(s => s.GetEventsAsync(liveStreamId, page, pageSize))
            .ThrowsAsync(new ArgumentException("Page must be greater than 0"));

        // Act
        var result = await _controller.GetEvents(liveStreamId, page, pageSize);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
        Assert.Equal("Page must be greater than 0", badRequestResult.Value);
    }

    [Fact]
    public async Task GetEvents_InvalidPageSize_ReturnsBadRequest()
    {
        // Arrange
        var liveStreamId = 1;
        var page = 1;
        var pageSize = 0; // Invalid page size

        _mockEventsService.Setup(s => s.GetEventsAsync(liveStreamId, page, pageSize))
            .ThrowsAsync(new ArgumentException("Page size must be greater than 0"));

        // Act
        var result = await _controller.GetEvents(liveStreamId, page, pageSize);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
        Assert.Equal("Page size must be greater than 0", badRequestResult.Value);
    }

    [Fact]
    public async Task GetEvents_ServiceException_ReturnsInternalServerError()
    {
        // Arrange
        var liveStreamId = 1;
        var page = 1;
        var pageSize = 20;

        _mockEventsService.Setup(s => s.GetEventsAsync(liveStreamId, page, pageSize))
            .ThrowsAsync(new Exception("Database connection failed"));

        // Act
        var result = await _controller.GetEvents(liveStreamId, page, pageSize);

        // Assert
        var statusCodeResult = Assert.IsType<ObjectResult>(result.Result);
        Assert.Equal(500, statusCodeResult.StatusCode);
        Assert.Equal("Database connection failed", statusCodeResult.Value);
    }

    [Fact]
    public async Task GetEvents_ValidRequest_ReturnsEmptyList()
    {
        // Arrange
        var liveStreamId = 1;
        var page = 1;
        var pageSize = 20;
        var expectedEvents = new PagedResult<LiveStreamEventDto>
        {
            Items = new List<LiveStreamEventDto>(),
            TotalCount = 0,
            Page = page,
            PageSize = pageSize
        };

        _mockEventsService.Setup(s => s.GetEventsAsync(liveStreamId, page, pageSize))
            .ReturnsAsync(expectedEvents);

        // Act
        var result = await _controller.GetEvents(liveStreamId, page, pageSize);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var events = Assert.IsType<PagedResult<LiveStreamEventDto>>(okResult.Value);
        Assert.Empty(events.Items);
        Assert.Equal(0, events.TotalCount);
    }

    [Fact]
    public async Task GetEvents_ValidRequest_ReturnsMultipleEvents()
    {
        // Arrange
        var liveStreamId = 1;
        var page = 1;
        var pageSize = 20;
        var expectedEvents = new PagedResult<LiveStreamEventDto>
        {
            Items = new List<LiveStreamEventDto>
            {
                new LiveStreamEventDto
                {
                    Id = 1,
                    LiveStreamId = liveStreamId,
                    EventType = LiveStreamEventType.Milestone,
                    Title = "100 Viewers Milestone",
                    Description = "Congratulations on reaching 100 viewers!",
                    IsActive = true,
                    StartTime = DateTime.UtcNow,
                    EndTime = DateTime.UtcNow.AddHours(1),
                    CreatedBy = "creator1",
                    CreatedAt = DateTime.UtcNow
                },
                new LiveStreamEventDto
                {
                    Id = 2,
                    LiveStreamId = liveStreamId,
                    EventType = LiveStreamEventType.Celebration,
                    Title = "Stream Anniversary",
                    Description = "Celebrating 1 year of streaming!",
                    IsActive = true,
                    StartTime = DateTime.UtcNow.AddHours(1),
                    EndTime = DateTime.UtcNow.AddHours(2),
                    CreatedBy = "creator1",
                    CreatedAt = DateTime.UtcNow
                }
            },
            TotalCount = 2,
            Page = page,
            PageSize = pageSize
        };

        _mockEventsService.Setup(s => s.GetEventsAsync(liveStreamId, page, pageSize))
            .ReturnsAsync(expectedEvents);

        // Act
        var result = await _controller.GetEvents(liveStreamId, page, pageSize);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var events = Assert.IsType<PagedResult<LiveStreamEventDto>>(okResult.Value);
        Assert.Equal(2, events.Items.Count);
        Assert.Equal(expectedEvents.TotalCount, events.TotalCount);
        Assert.Equal(expectedEvents.Page, events.Page);
        Assert.Equal(expectedEvents.PageSize, events.PageSize);
    }

    [Fact]
    public async Task GetEvents_ValidRequest_ReturnsPagedResults()
    {
        // Arrange
        var liveStreamId = 1;
        var page = 2;
        var pageSize = 10;
        var expectedEvents = new PagedResult<LiveStreamEventDto>
        {
            Items = new List<LiveStreamEventDto>
            {
                new LiveStreamEventDto
                {
                    Id = 11,
                    LiveStreamId = liveStreamId,
                    EventType = LiveStreamEventType.Milestone,
                    Title = "Page 2 Event",
                    Description = "Event on page 2",
                    IsActive = true,
                    StartTime = DateTime.UtcNow,
                    EndTime = DateTime.UtcNow.AddHours(1),
                    CreatedBy = "creator1",
                    CreatedAt = DateTime.UtcNow
                }
            },
            TotalCount = 25,
            Page = page,
            PageSize = pageSize
        };

        _mockEventsService.Setup(s => s.GetEventsAsync(liveStreamId, page, pageSize))
            .ReturnsAsync(expectedEvents);

        // Act
        var result = await _controller.GetEvents(liveStreamId, page, pageSize);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var events = Assert.IsType<PagedResult<LiveStreamEventDto>>(okResult.Value);
        Assert.Single(events.Items);
        Assert.Equal(25, events.TotalCount);
        Assert.Equal(2, events.Page);
        Assert.Equal(10, events.PageSize);
    }

    [Fact]
    public async Task GetEvents_ValidRequest_ReturnsActiveEvents()
    {
        // Arrange
        var liveStreamId = 1;
        var page = 1;
        var pageSize = 20;
        var expectedEvents = new PagedResult<LiveStreamEventDto>
        {
            Items = new List<LiveStreamEventDto>
            {
                new LiveStreamEventDto
                {
                    Id = 1,
                    LiveStreamId = liveStreamId,
                    EventType = LiveStreamEventType.Milestone,
                    Title = "Active Event",
                    Description = "This event is currently active",
                    IsActive = true,
                    StartTime = DateTime.UtcNow,
                    EndTime = DateTime.UtcNow.AddHours(1),
                    CreatedBy = "creator1",
                    CreatedAt = DateTime.UtcNow
                }
            },
            TotalCount = 1,
            Page = page,
            PageSize = pageSize
        };

        _mockEventsService.Setup(s => s.GetEventsAsync(liveStreamId, page, pageSize))
            .ReturnsAsync(expectedEvents);

        // Act
        var result = await _controller.GetEvents(liveStreamId, page, pageSize);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var events = Assert.IsType<PagedResult<LiveStreamEventDto>>(okResult.Value);
        Assert.Single(events.Items);
        Assert.True(events.Items.First().IsActive);
        Assert.Equal("Active Event", events.Items.First().Title);
    }

    [Fact]
    public async Task GetEvents_ValidRequest_ReturnsInactiveEvents()
    {
        // Arrange
        var liveStreamId = 1;
        var page = 1;
        var pageSize = 20;
        var expectedEvents = new PagedResult<LiveStreamEventDto>
        {
            Items = new List<LiveStreamEventDto>
            {
                new LiveStreamEventDto
                {
                    Id = 1,
                    LiveStreamId = liveStreamId,
                    EventType = LiveStreamEventType.Milestone,
                    Title = "Inactive Event",
                    Description = "This event is not active",
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

        _mockEventsService.Setup(s => s.GetEventsAsync(liveStreamId, page, pageSize))
            .ReturnsAsync(expectedEvents);

        // Act
        var result = await _controller.GetEvents(liveStreamId, page, pageSize);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var events = Assert.IsType<PagedResult<LiveStreamEventDto>>(okResult.Value);
        Assert.Single(events.Items);
        Assert.False(events.Items.First().IsActive);
        Assert.Equal("Inactive Event", events.Items.First().Title);
    }
}