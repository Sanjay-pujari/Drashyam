using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using Drashyam.API.Controllers;
using Drashyam.API.DTOs;
using Drashyam.API.Services;

namespace Drashyam.API.Tests.Controllers;

public class LiveStreamMonetizationControllerTests
{
    private readonly Mock<ILiveStreamMonetizationService> _mockMonetizationService;
    private readonly Mock<ILogger<LiveStreamMonetizationController>> _mockLogger;
    private readonly LiveStreamMonetizationController _controller;

    public LiveStreamMonetizationControllerTests()
    {
        _mockMonetizationService = new Mock<ILiveStreamMonetizationService>();
        _mockLogger = new Mock<ILogger<LiveStreamMonetizationController>>();

        _controller = new LiveStreamMonetizationController(_mockMonetizationService.Object, _mockLogger.Object);
    }

    [Fact]
    public async Task GetDonations_ValidId_ReturnsOk()
    {
        // Arrange
        var liveStreamId = 1;
        var page = 1;
        var pageSize = 20;
        var expectedDonations = new PagedResult<LiveStreamDonationDto>
        {
            Items = new List<LiveStreamDonationDto>
            {
                new LiveStreamDonationDto
                {
                    Id = 1,
                    LiveStreamId = liveStreamId,
                    DonorId = "donor1",
                    Amount = 10.50m,
                    Message = "Great stream!",
                    IsAnonymous = false,
                    Timestamp = DateTime.UtcNow,
                    Donor = new UserDto
                    {
                        Id = "donor1",
                        UserName = "donoruser",
                        Email = "donor@example.com"
                    }
                }
            },
            TotalCount = 1,
            Page = page,
            PageSize = pageSize
        };

        _mockMonetizationService.Setup(s => s.GetDonationsAsync(liveStreamId, page, pageSize))
            .ReturnsAsync(expectedDonations);

        // Act
        var result = await _controller.GetDonations(liveStreamId, page, pageSize);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var donations = Assert.IsType<PagedResult<LiveStreamDonationDto>>(okResult.Value);
        Assert.Single(donations.Items);
        Assert.Equal(expectedDonations.TotalCount, donations.TotalCount);
        Assert.Equal(expectedDonations.Page, donations.Page);
        Assert.Equal(expectedDonations.PageSize, donations.PageSize);
    }

    [Fact]
    public async Task GetDonations_InvalidId_ReturnsNotFound()
    {
        // Arrange
        var liveStreamId = 999;
        var page = 1;
        var pageSize = 20;

        _mockMonetizationService.Setup(s => s.GetDonationsAsync(liveStreamId, page, pageSize))
            .ThrowsAsync(new ArgumentException("Live stream not found"));

        // Act
        var result = await _controller.GetDonations(liveStreamId, page, pageSize);

        // Assert
        var notFoundResult = Assert.IsType<NotFoundObjectResult>(result.Result);
        Assert.Equal("Live stream not found", notFoundResult.Value);
    }

    [Fact]
    public async Task GetDonations_InvalidPage_ReturnsBadRequest()
    {
        // Arrange
        var liveStreamId = 1;
        var page = 0; // Invalid page
        var pageSize = 20;

        _mockMonetizationService.Setup(s => s.GetDonationsAsync(liveStreamId, page, pageSize))
            .ThrowsAsync(new ArgumentException("Page must be greater than 0"));

        // Act
        var result = await _controller.GetDonations(liveStreamId, page, pageSize);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
        Assert.Equal("Page must be greater than 0", badRequestResult.Value);
    }

    [Fact]
    public async Task GetDonations_InvalidPageSize_ReturnsBadRequest()
    {
        // Arrange
        var liveStreamId = 1;
        var page = 1;
        var pageSize = 0; // Invalid page size

        _mockMonetizationService.Setup(s => s.GetDonationsAsync(liveStreamId, page, pageSize))
            .ThrowsAsync(new ArgumentException("Page size must be greater than 0"));

        // Act
        var result = await _controller.GetDonations(liveStreamId, page, pageSize);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
        Assert.Equal("Page size must be greater than 0", badRequestResult.Value);
    }

    [Fact]
    public async Task GetDonations_ServiceException_ReturnsInternalServerError()
    {
        // Arrange
        var liveStreamId = 1;
        var page = 1;
        var pageSize = 20;

        _mockMonetizationService.Setup(s => s.GetDonationsAsync(liveStreamId, page, pageSize))
            .ThrowsAsync(new Exception("Database connection failed"));

        // Act
        var result = await _controller.GetDonations(liveStreamId, page, pageSize);

        // Assert
        var statusCodeResult = Assert.IsType<ObjectResult>(result.Result);
        Assert.Equal(500, statusCodeResult.StatusCode);
        Assert.Equal("Database connection failed", statusCodeResult.Value);
    }

    [Fact]
    public async Task GetDonations_ValidRequest_ReturnsEmptyList()
    {
        // Arrange
        var liveStreamId = 1;
        var page = 1;
        var pageSize = 20;
        var expectedDonations = new PagedResult<LiveStreamDonationDto>
        {
            Items = new List<LiveStreamDonationDto>(),
            TotalCount = 0,
            Page = page,
            PageSize = pageSize
        };

        _mockMonetizationService.Setup(s => s.GetDonationsAsync(liveStreamId, page, pageSize))
            .ReturnsAsync(expectedDonations);

        // Act
        var result = await _controller.GetDonations(liveStreamId, page, pageSize);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var donations = Assert.IsType<PagedResult<LiveStreamDonationDto>>(okResult.Value);
        Assert.Empty(donations.Items);
        Assert.Equal(0, donations.TotalCount);
    }

    [Fact]
    public async Task GetDonations_ValidRequest_ReturnsMultipleDonations()
    {
        // Arrange
        var liveStreamId = 1;
        var page = 1;
        var pageSize = 20;
        var expectedDonations = new PagedResult<LiveStreamDonationDto>
        {
            Items = new List<LiveStreamDonationDto>
            {
                new LiveStreamDonationDto
                {
                    Id = 1,
                    LiveStreamId = liveStreamId,
                    DonorId = "donor1",
                    Amount = 10.50m,
                    Message = "Great stream!",
                    IsAnonymous = false,
                    Timestamp = DateTime.UtcNow,
                    Donor = new UserDto
                    {
                        Id = "donor1",
                        UserName = "donoruser1",
                        Email = "donor1@example.com"
                    }
                },
                new LiveStreamDonationDto
                {
                    Id = 2,
                    LiveStreamId = liveStreamId,
                    DonorId = "donor2",
                    Amount = 25.00m,
                    Message = "Keep up the great work!",
                    IsAnonymous = true,
                    Timestamp = DateTime.UtcNow.AddMinutes(1),
                    Donor = new UserDto
                    {
                        Id = "donor2",
                        UserName = "donoruser2",
                        Email = "donor2@example.com"
                    }
                }
            },
            TotalCount = 2,
            Page = page,
            PageSize = pageSize
        };

        _mockMonetizationService.Setup(s => s.GetDonationsAsync(liveStreamId, page, pageSize))
            .ReturnsAsync(expectedDonations);

        // Act
        var result = await _controller.GetDonations(liveStreamId, page, pageSize);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var donations = Assert.IsType<PagedResult<LiveStreamDonationDto>>(okResult.Value);
        Assert.Equal(2, donations.Items.Count);
        Assert.Equal(expectedDonations.TotalCount, donations.TotalCount);
        Assert.Equal(expectedDonations.Page, donations.Page);
        Assert.Equal(expectedDonations.PageSize, donations.PageSize);
    }

    [Fact]
    public async Task GetDonations_ValidRequest_ReturnsPagedResults()
    {
        // Arrange
        var liveStreamId = 1;
        var page = 2;
        var pageSize = 10;
        var expectedDonations = new PagedResult<LiveStreamDonationDto>
        {
            Items = new List<LiveStreamDonationDto>
            {
                new LiveStreamDonationDto
                {
                    Id = 11,
                    LiveStreamId = liveStreamId,
                    DonorId = "donor11",
                    Amount = 15.75m,
                    Message = "Page 2 donation",
                    IsAnonymous = false,
                    Timestamp = DateTime.UtcNow,
                    Donor = new UserDto
                    {
                        Id = "donor11",
                        UserName = "donoruser11",
                        Email = "donor11@example.com"
                    }
                }
            },
            TotalCount = 25,
            Page = page,
            PageSize = pageSize
        };

        _mockMonetizationService.Setup(s => s.GetDonationsAsync(liveStreamId, page, pageSize))
            .ReturnsAsync(expectedDonations);

        // Act
        var result = await _controller.GetDonations(liveStreamId, page, pageSize);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var donations = Assert.IsType<PagedResult<LiveStreamDonationDto>>(okResult.Value);
        Assert.Single(donations.Items);
        Assert.Equal(25, donations.TotalCount);
        Assert.Equal(2, donations.Page);
        Assert.Equal(10, donations.PageSize);
    }

    [Fact]
    public async Task GetDonations_ValidRequest_ReturnsAnonymousDonations()
    {
        // Arrange
        var liveStreamId = 1;
        var page = 1;
        var pageSize = 20;
        var expectedDonations = new PagedResult<LiveStreamDonationDto>
        {
            Items = new List<LiveStreamDonationDto>
            {
                new LiveStreamDonationDto
                {
                    Id = 1,
                    LiveStreamId = liveStreamId,
                    DonorId = "donor1",
                    Amount = 10.50m,
                    Message = "Anonymous donation",
                    IsAnonymous = true,
                    Timestamp = DateTime.UtcNow,
                    Donor = new UserDto
                    {
                        Id = "donor1",
                        UserName = "donoruser",
                        Email = "donor@example.com"
                    }
                }
            },
            TotalCount = 1,
            Page = page,
            PageSize = pageSize
        };

        _mockMonetizationService.Setup(s => s.GetDonationsAsync(liveStreamId, page, pageSize))
            .ReturnsAsync(expectedDonations);

        // Act
        var result = await _controller.GetDonations(liveStreamId, page, pageSize);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var donations = Assert.IsType<PagedResult<LiveStreamDonationDto>>(okResult.Value);
        Assert.Single(donations.Items);
        Assert.True(donations.Items.First().IsAnonymous);
        Assert.Equal("Anonymous donation", donations.Items.First().Message);
    }

    [Fact]
    public async Task GetDonations_ValidRequest_ReturnsNonAnonymousDonations()
    {
        // Arrange
        var liveStreamId = 1;
        var page = 1;
        var pageSize = 20;
        var expectedDonations = new PagedResult<LiveStreamDonationDto>
        {
            Items = new List<LiveStreamDonationDto>
            {
                new LiveStreamDonationDto
                {
                    Id = 1,
                    LiveStreamId = liveStreamId,
                    DonorId = "donor1",
                    Amount = 10.50m,
                    Message = "Public donation",
                    IsAnonymous = false,
                    Timestamp = DateTime.UtcNow,
                    Donor = new UserDto
                    {
                        Id = "donor1",
                        UserName = "donoruser",
                        Email = "donor@example.com"
                    }
                }
            },
            TotalCount = 1,
            Page = page,
            PageSize = pageSize
        };

        _mockMonetizationService.Setup(s => s.GetDonationsAsync(liveStreamId, page, pageSize))
            .ReturnsAsync(expectedDonations);

        // Act
        var result = await _controller.GetDonations(liveStreamId, page, pageSize);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var donations = Assert.IsType<PagedResult<LiveStreamDonationDto>>(okResult.Value);
        Assert.Single(donations.Items);
        Assert.False(donations.Items.First().IsAnonymous);
        Assert.Equal("Public donation", donations.Items.First().Message);
    }
}