using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using Drashyam.API.Controllers;
using Drashyam.API.DTOs;
using Drashyam.API.Models;
using Drashyam.API.Services;

namespace Drashyam.API.Tests;

public class InviteControllerTests
{
    private readonly Mock<IInviteService> _mockInviteService;
    private readonly Mock<ILogger<InviteController>> _mockLogger;
    private readonly InviteController _controller;

    public InviteControllerTests()
    {
        _mockInviteService = new Mock<IInviteService>();
        _mockLogger = new Mock<ILogger<InviteController>>();
        _controller = new InviteController(_mockInviteService.Object, _mockLogger.Object);
    }

    [Fact]
    public async Task CreateInvite_ValidData_ReturnsOkResult()
    {
        // Arrange
        var createDto = new CreateInviteDto
        {
            InviteeEmail = "test@example.com",
            InviteeFirstName = "John",
            InviteeLastName = "Doe",
            Type = InviteType.Email
        };

        var expectedInvite = new UserInviteDto
        {
            Id = 1,
            InviteeEmail = "test@example.com",
            Status = InviteStatus.Pending
        };

        _mockInviteService.Setup(x => x.CreateInviteAsync(It.IsAny<string>(), createDto))
            .ReturnsAsync(expectedInvite);

        // Act
        var result = await _controller.CreateInvite(createDto);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var invite = Assert.IsType<UserInviteDto>(okResult.Value);
        Assert.Equal("test@example.com", invite.InviteeEmail);
    }

    [Fact]
    public async Task CreateInvite_InvalidOperation_ReturnsBadRequest()
    {
        // Arrange
        var createDto = new CreateInviteDto
        {
            InviteeEmail = "existing@example.com",
            Type = InviteType.Email
        };

        _mockInviteService.Setup(x => x.CreateInviteAsync(It.IsAny<string>(), createDto))
            .ThrowsAsync(new InvalidOperationException("User already exists"));

        // Act
        var result = await _controller.CreateInvite(createDto);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
        Assert.Equal("User already exists", badRequestResult.Value);
    }

    [Fact]
    public async Task GetInviteByToken_ValidToken_ReturnsOkResult()
    {
        // Arrange
        var token = "valid-token";
        var expectedInvite = new UserInviteDto
        {
            Id = 1,
            InviteToken = token,
            Status = InviteStatus.Pending
        };

        _mockInviteService.Setup(x => x.GetInviteByTokenAsync(token))
            .ReturnsAsync(expectedInvite);

        // Act
        var result = await _controller.GetInviteByToken(token);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var invite = Assert.IsType<UserInviteDto>(okResult.Value);
        Assert.Equal(token, invite.InviteToken);
    }

    [Fact]
    public async Task GetInviteByToken_InvalidToken_ReturnsNotFound()
    {
        // Arrange
        var token = "invalid-token";
        _mockInviteService.Setup(x => x.GetInviteByTokenAsync(token))
            .ThrowsAsync(new ArgumentException("Invalid invite token"));

        // Act
        var result = await _controller.GetInviteByToken(token);

        // Assert
        var notFoundResult = Assert.IsType<NotFoundObjectResult>(result.Result);
        Assert.Equal("Invalid invite token", notFoundResult.Value);
    }

    [Fact]
    public async Task AcceptInvite_ValidData_ReturnsOkResult()
    {
        // Arrange
        var token = "valid-token";
        var acceptDto = new AcceptInviteDto
        {
            InviteToken = token,
            FirstName = "John",
            LastName = "Doe",
            Password = "password123"
        };

        var expectedInvite = new UserInviteDto
        {
            Id = 1,
            Status = InviteStatus.Accepted
        };

        _mockInviteService.Setup(x => x.AcceptInviteAsync(token, acceptDto))
            .ReturnsAsync(expectedInvite);

        // Act
        var result = await _controller.AcceptInvite(token, acceptDto);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var invite = Assert.IsType<UserInviteDto>(okResult.Value);
        Assert.Equal(InviteStatus.Accepted, invite.Status);
    }

    [Fact]
    public async Task GetMyInvites_ReturnsPagedResult()
    {
        // Arrange
        var expectedResult = new PagedResult<UserInviteDto>
        {
            Items = new List<UserInviteDto>
            {
                new() { Id = 1, InviteeEmail = "test1@example.com" },
                new() { Id = 2, InviteeEmail = "test2@example.com" }
            },
            TotalCount = 2,
            Page = 1,
            PageSize = 10
        };

        _mockInviteService.Setup(x => x.GetUserInvitesAsync(It.IsAny<string>(), 1, 10))
            .ReturnsAsync(expectedResult);

        // Act
        var result = await _controller.GetMyInvites();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var pagedResult = Assert.IsType<PagedResult<UserInviteDto>>(okResult.Value);
        Assert.Equal(2, pagedResult.TotalCount);
        Assert.Equal(2, pagedResult.Items.Count);
    }

    [Fact]
    public async Task GetInviteStats_ReturnsStats()
    {
        // Arrange
        var expectedStats = new InviteStatsDto
        {
            TotalInvites = 10,
            PendingInvites = 3,
            AcceptedInvites = 5,
            ExpiredInvites = 2,
            ConversionRate = 50.0m
        };

        _mockInviteService.Setup(x => x.GetInviteStatsAsync(It.IsAny<string>()))
            .ReturnsAsync(expectedStats);

        // Act
        var result = await _controller.GetInviteStats();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var stats = Assert.IsType<InviteStatsDto>(okResult.Value);
        Assert.Equal(10, stats.TotalInvites);
        Assert.Equal(50.0m, stats.ConversionRate);
    }
}
