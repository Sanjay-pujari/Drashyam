using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using Drashyam.API.Controllers;
using Drashyam.API.DTOs;
using Drashyam.API.Services;

namespace Drashyam.API.Tests.Controllers;

public class LiveStreamCollaborationControllerTests
{
    private readonly Mock<ILiveStreamCollaborationService> _mockCollaborationService;
    private readonly Mock<ILogger<LiveStreamCollaborationController>> _mockLogger;
    private readonly LiveStreamCollaborationController _controller;

    public LiveStreamCollaborationControllerTests()
    {
        _mockCollaborationService = new Mock<ILiveStreamCollaborationService>();
        _mockLogger = new Mock<ILogger<LiveStreamCollaborationController>>();

        _controller = new LiveStreamCollaborationController(_mockCollaborationService.Object, _mockLogger.Object);
    }

    [Fact]
    public async Task GetCollaborations_ValidId_ReturnsOk()
    {
        // Arrange
        var liveStreamId = 1;
        var page = 1;
        var pageSize = 20;
        var expectedCollaborations = new PagedResult<LiveStreamCollaborationDto>
        {
            Items = new List<LiveStreamCollaborationDto>
            {
                new LiveStreamCollaborationDto
                {
                    Id = 1,
                    LiveStreamId = liveStreamId,
                    CollaboratorId = "collaborator1",
                    CollaborationType = LiveStreamCollaborationType.Guest,
                    Status = CollaborationStatus.Active,
                    StartTime = DateTime.UtcNow,
                    EndTime = DateTime.UtcNow.AddHours(1),
                    Collaborator = new UserDto
                    {
                        Id = "collaborator1",
                        UserName = "collaboratoruser",
                        Email = "collaborator@example.com"
                    }
                }
            },
            TotalCount = 1,
            Page = page,
            PageSize = pageSize
        };

        _mockCollaborationService.Setup(s => s.GetCollaborationsAsync(liveStreamId, page, pageSize))
            .ReturnsAsync(expectedCollaborations);

        // Act
        var result = await _controller.GetCollaborations(liveStreamId, page, pageSize);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var collaborations = Assert.IsType<PagedResult<LiveStreamCollaborationDto>>(okResult.Value);
        Assert.Single(collaborations.Items);
        Assert.Equal(expectedCollaborations.TotalCount, collaborations.TotalCount);
        Assert.Equal(expectedCollaborations.Page, collaborations.Page);
        Assert.Equal(expectedCollaborations.PageSize, collaborations.PageSize);
    }

    [Fact]
    public async Task GetCollaborations_InvalidId_ReturnsNotFound()
    {
        // Arrange
        var liveStreamId = 999;
        var page = 1;
        var pageSize = 20;

        _mockCollaborationService.Setup(s => s.GetCollaborationsAsync(liveStreamId, page, pageSize))
            .ThrowsAsync(new ArgumentException("Live stream not found"));

        // Act
        var result = await _controller.GetCollaborations(liveStreamId, page, pageSize);

        // Assert
        var notFoundResult = Assert.IsType<NotFoundObjectResult>(result.Result);
        Assert.Equal("Live stream not found", notFoundResult.Value);
    }

    [Fact]
    public async Task GetCollaborations_InvalidPage_ReturnsBadRequest()
    {
        // Arrange
        var liveStreamId = 1;
        var page = 0; // Invalid page
        var pageSize = 20;

        _mockCollaborationService.Setup(s => s.GetCollaborationsAsync(liveStreamId, page, pageSize))
            .ThrowsAsync(new ArgumentException("Page must be greater than 0"));

        // Act
        var result = await _controller.GetCollaborations(liveStreamId, page, pageSize);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
        Assert.Equal("Page must be greater than 0", badRequestResult.Value);
    }

    [Fact]
    public async Task GetCollaborations_InvalidPageSize_ReturnsBadRequest()
    {
        // Arrange
        var liveStreamId = 1;
        var page = 1;
        var pageSize = 0; // Invalid page size

        _mockCollaborationService.Setup(s => s.GetCollaborationsAsync(liveStreamId, page, pageSize))
            .ThrowsAsync(new ArgumentException("Page size must be greater than 0"));

        // Act
        var result = await _controller.GetCollaborations(liveStreamId, page, pageSize);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
        Assert.Equal("Page size must be greater than 0", badRequestResult.Value);
    }

    [Fact]
    public async Task GetCollaborations_ServiceException_ReturnsInternalServerError()
    {
        // Arrange
        var liveStreamId = 1;
        var page = 1;
        var pageSize = 20;

        _mockCollaborationService.Setup(s => s.GetCollaborationsAsync(liveStreamId, page, pageSize))
            .ThrowsAsync(new Exception("Database connection failed"));

        // Act
        var result = await _controller.GetCollaborations(liveStreamId, page, pageSize);

        // Assert
        var statusCodeResult = Assert.IsType<ObjectResult>(result.Result);
        Assert.Equal(500, statusCodeResult.StatusCode);
        Assert.Equal("Database connection failed", statusCodeResult.Value);
    }

    [Fact]
    public async Task GetCollaborations_ValidRequest_ReturnsEmptyList()
    {
        // Arrange
        var liveStreamId = 1;
        var page = 1;
        var pageSize = 20;
        var expectedCollaborations = new PagedResult<LiveStreamCollaborationDto>
        {
            Items = new List<LiveStreamCollaborationDto>(),
            TotalCount = 0,
            Page = page,
            PageSize = pageSize
        };

        _mockCollaborationService.Setup(s => s.GetCollaborationsAsync(liveStreamId, page, pageSize))
            .ReturnsAsync(expectedCollaborations);

        // Act
        var result = await _controller.GetCollaborations(liveStreamId, page, pageSize);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var collaborations = Assert.IsType<PagedResult<LiveStreamCollaborationDto>>(okResult.Value);
        Assert.Empty(collaborations.Items);
        Assert.Equal(0, collaborations.TotalCount);
    }

    [Fact]
    public async Task GetCollaborations_ValidRequest_ReturnsMultipleCollaborations()
    {
        // Arrange
        var liveStreamId = 1;
        var page = 1;
        var pageSize = 20;
        var expectedCollaborations = new PagedResult<LiveStreamCollaborationDto>
        {
            Items = new List<LiveStreamCollaborationDto>
            {
                new LiveStreamCollaborationDto
                {
                    Id = 1,
                    LiveStreamId = liveStreamId,
                    CollaboratorId = "collaborator1",
                    CollaborationType = LiveStreamCollaborationType.Guest,
                    Status = CollaborationStatus.Active,
                    StartTime = DateTime.UtcNow,
                    EndTime = DateTime.UtcNow.AddHours(1),
                    Collaborator = new UserDto
                    {
                        Id = "collaborator1",
                        UserName = "collaboratoruser1",
                        Email = "collaborator1@example.com"
                    }
                },
                new LiveStreamCollaborationDto
                {
                    Id = 2,
                    LiveStreamId = liveStreamId,
                    CollaboratorId = "collaborator2",
                    CollaborationType = LiveStreamCollaborationType.CoHost,
                    Status = CollaborationStatus.Pending,
                    StartTime = DateTime.UtcNow.AddHours(1),
                    EndTime = DateTime.UtcNow.AddHours(2),
                    Collaborator = new UserDto
                    {
                        Id = "collaborator2",
                        UserName = "collaboratoruser2",
                        Email = "collaborator2@example.com"
                    }
                }
            },
            TotalCount = 2,
            Page = page,
            PageSize = pageSize
        };

        _mockCollaborationService.Setup(s => s.GetCollaborationsAsync(liveStreamId, page, pageSize))
            .ReturnsAsync(expectedCollaborations);

        // Act
        var result = await _controller.GetCollaborations(liveStreamId, page, pageSize);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var collaborations = Assert.IsType<PagedResult<LiveStreamCollaborationDto>>(okResult.Value);
        Assert.Equal(2, collaborations.Items.Count);
        Assert.Equal(expectedCollaborations.TotalCount, collaborations.TotalCount);
        Assert.Equal(expectedCollaborations.Page, collaborations.Page);
        Assert.Equal(expectedCollaborations.PageSize, collaborations.PageSize);
    }

    [Fact]
    public async Task GetCollaborations_ValidRequest_ReturnsPagedResults()
    {
        // Arrange
        var liveStreamId = 1;
        var page = 2;
        var pageSize = 10;
        var expectedCollaborations = new PagedResult<LiveStreamCollaborationDto>
        {
            Items = new List<LiveStreamCollaborationDto>
            {
                new LiveStreamCollaborationDto
                {
                    Id = 11,
                    LiveStreamId = liveStreamId,
                    CollaboratorId = "collaborator11",
                    CollaborationType = LiveStreamCollaborationType.Guest,
                    Status = CollaborationStatus.Active,
                    StartTime = DateTime.UtcNow,
                    EndTime = DateTime.UtcNow.AddHours(1),
                    Collaborator = new UserDto
                    {
                        Id = "collaborator11",
                        UserName = "collaboratoruser11",
                        Email = "collaborator11@example.com"
                    }
                }
            },
            TotalCount = 25,
            Page = page,
            PageSize = pageSize
        };

        _mockCollaborationService.Setup(s => s.GetCollaborationsAsync(liveStreamId, page, pageSize))
            .ReturnsAsync(expectedCollaborations);

        // Act
        var result = await _controller.GetCollaborations(liveStreamId, page, pageSize);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var collaborations = Assert.IsType<PagedResult<LiveStreamCollaborationDto>>(okResult.Value);
        Assert.Single(collaborations.Items);
        Assert.Equal(25, collaborations.TotalCount);
        Assert.Equal(2, collaborations.Page);
        Assert.Equal(10, collaborations.PageSize);
    }

    [Fact]
    public async Task GetCollaborations_ValidRequest_ReturnsActiveCollaborations()
    {
        // Arrange
        var liveStreamId = 1;
        var page = 1;
        var pageSize = 20;
        var expectedCollaborations = new PagedResult<LiveStreamCollaborationDto>
        {
            Items = new List<LiveStreamCollaborationDto>
            {
                new LiveStreamCollaborationDto
                {
                    Id = 1,
                    LiveStreamId = liveStreamId,
                    CollaboratorId = "collaborator1",
                    CollaborationType = LiveStreamCollaborationType.Guest,
                    Status = CollaborationStatus.Active,
                    StartTime = DateTime.UtcNow,
                    EndTime = DateTime.UtcNow.AddHours(1),
                    Collaborator = new UserDto
                    {
                        Id = "collaborator1",
                        UserName = "collaboratoruser",
                        Email = "collaborator@example.com"
                    }
                }
            },
            TotalCount = 1,
            Page = page,
            PageSize = pageSize
        };

        _mockCollaborationService.Setup(s => s.GetCollaborationsAsync(liveStreamId, page, pageSize))
            .ReturnsAsync(expectedCollaborations);

        // Act
        var result = await _controller.GetCollaborations(liveStreamId, page, pageSize);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var collaborations = Assert.IsType<PagedResult<LiveStreamCollaborationDto>>(okResult.Value);
        Assert.Single(collaborations.Items);
        Assert.Equal(CollaborationStatus.Active, collaborations.Items.First().Status);
        Assert.Equal(LiveStreamCollaborationType.Guest, collaborations.Items.First().CollaborationType);
    }

    [Fact]
    public async Task GetCollaborations_ValidRequest_ReturnsPendingCollaborations()
    {
        // Arrange
        var liveStreamId = 1;
        var page = 1;
        var pageSize = 20;
        var expectedCollaborations = new PagedResult<LiveStreamCollaborationDto>
        {
            Items = new List<LiveStreamCollaborationDto>
            {
                new LiveStreamCollaborationDto
                {
                    Id = 1,
                    LiveStreamId = liveStreamId,
                    CollaboratorId = "collaborator1",
                    CollaborationType = LiveStreamCollaborationType.CoHost,
                    Status = CollaborationStatus.Pending,
                    StartTime = DateTime.UtcNow.AddHours(1),
                    EndTime = DateTime.UtcNow.AddHours(2),
                    Collaborator = new UserDto
                    {
                        Id = "collaborator1",
                        UserName = "collaboratoruser",
                        Email = "collaborator@example.com"
                    }
                }
            },
            TotalCount = 1,
            Page = page,
            PageSize = pageSize
        };

        _mockCollaborationService.Setup(s => s.GetCollaborationsAsync(liveStreamId, page, pageSize))
            .ReturnsAsync(expectedCollaborations);

        // Act
        var result = await _controller.GetCollaborations(liveStreamId, page, pageSize);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var collaborations = Assert.IsType<PagedResult<LiveStreamCollaborationDto>>(okResult.Value);
        Assert.Single(collaborations.Items);
        Assert.Equal(CollaborationStatus.Pending, collaborations.Items.First().Status);
        Assert.Equal(LiveStreamCollaborationType.CoHost, collaborations.Items.First().CollaborationType);
    }
}