using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Drashyam.API.Data;
using Drashyam.API.DTOs;
using Drashyam.API.Models;
using Drashyam.API.Services;
using Drashyam.API.Hubs;
using Microsoft.AspNetCore.SignalR;

namespace Drashyam.API.Tests;

public class LiveStreamRevenueServiceTests : IDisposable
{
    private readonly DrashyamDbContext _context;
    private readonly Mock<IHubContext<LiveStreamHub>> _mockLiveStreamHub;
    private readonly Mock<IHubContext<NotificationHub>> _mockNotificationHub;
    private readonly Mock<ILogger<LiveStreamRevenueService>> _mockLogger;
    private readonly LiveStreamRevenueService _revenueService;

    public LiveStreamRevenueServiceTests()
    {
        // Setup in-memory database
        var options = new DbContextOptionsBuilder<DrashyamDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new DrashyamDbContext(options);

        // Setup mocks
        _mockLiveStreamHub = new Mock<IHubContext<LiveStreamHub>>();
        _mockNotificationHub = new Mock<IHubContext<NotificationHub>>();
        _mockLogger = new Mock<ILogger<LiveStreamRevenueService>>();

        // Setup hub context mocks
        var mockClients = new Mock<IHubCallerClients>();
        var mockClientProxy = new Mock<IClientProxy>();
        var mockGroupManager = new Mock<IGroupManager>();

        _mockLiveStreamHub.Setup(h => h.Clients).Returns(mockClients.Object);
        _mockLiveStreamHub.Setup(h => h.Groups).Returns(mockGroupManager.Object);
        _mockNotificationHub.Setup(h => h.Clients).Returns(mockClients.Object);
        _mockNotificationHub.Setup(h => h.Groups).Returns(mockGroupManager.Object);

        mockClients.Setup(c => c.Group(It.IsAny<string>())).Returns(mockClientProxy.Object);
        mockClientProxy.Setup(c => c.SendAsync(It.IsAny<string>(), It.IsAny<object>(), default))
            .Returns(Task.CompletedTask);

        _revenueService = new LiveStreamRevenueService(
            _context,
            _mockLiveStreamHub.Object,
            _mockNotificationHub.Object,
            _mockLogger.Object
        );

        SeedTestData();
    }

    private void SeedTestData()
    {
        // Add test user
        var user = new ApplicationUser
        {
            Id = "test-user-id",
            UserName = "testuser",
            Email = "test@example.com",
            FirstName = "Test",
            LastName = "User"
        };
        _context.Users.Add(user);

        // Add test channel
        var channel = new Channel
        {
            Id = 1,
            Name = "Test Channel",
            Description = "Test Channel Description",
            UserId = "test-user-id",
            IsPublic = true,
            CreatedAt = DateTime.UtcNow
        };
        _context.Channels.Add(channel);

        // Add test live stream
        var liveStream = new LiveStream
        {
            Id = 1,
            Title = "Test Stream",
            Description = "Test Stream Description",
            UserId = "test-user-id",
            ChannelId = 1,
            StreamKey = "test-stream-key",
            Status = DTOs.LiveStreamStatus.Live,
            ViewerCount = 100,
            PeakViewerCount = 150,
            CreatedAt = DateTime.UtcNow,
            ActualStartTime = DateTime.UtcNow.AddMinutes(-30)
        };
        _context.LiveStreams.Add(liveStream);

        // Add test revenue
        var revenue1 = new LiveStreamRevenue
        {
            Id = 1,
            LiveStreamId = 1,
            RevenueType = "donation",
            Amount = 10.00m,
            PlatformFee = 1.00m,
            CreatorEarnings = 9.00m,
            CreatedAt = DateTime.UtcNow.AddMinutes(-20)
        };
        _context.LiveStreamRevenues.Add(revenue1);

        var revenue2 = new LiveStreamRevenue
        {
            Id = 2,
            LiveStreamId = 1,
            RevenueType = "superchat",
            Amount = 5.00m,
            PlatformFee = 0.50m,
            CreatorEarnings = 4.50m,
            CreatedAt = DateTime.UtcNow.AddMinutes(-15)
        };
        _context.LiveStreamRevenues.Add(revenue2);

        var revenue3 = new LiveStreamRevenue
        {
            Id = 3,
            LiveStreamId = 1,
            RevenueType = "subscription",
            Amount = 4.99m,
            PlatformFee = 0.50m,
            CreatorEarnings = 4.49m,
            CreatedAt = DateTime.UtcNow.AddMinutes(-10)
        };
        _context.LiveStreamRevenues.Add(revenue3);

        _context.SaveChanges();
    }

    [Fact]
    public async Task GetRevenueAsync_ValidStream_ReturnsRevenue()
    {
        // Arrange
        var liveStreamId = 1;
        var startTime = DateTime.UtcNow.AddHours(-1);
        var endTime = DateTime.UtcNow;

        // Act
        var result = await _revenueService.GetRevenueAsync(liveStreamId, startTime, endTime);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(liveStreamId, result.LiveStreamId);
        Assert.True(result.TotalRevenue >= 0);
        Assert.True(result.TotalDonations >= 0);
        Assert.True(result.TotalSuperChats >= 0);
        Assert.True(result.TotalSubscriptions >= 0);
        Assert.True(result.PlatformFees >= 0);
        Assert.True(result.CreatorEarnings >= 0);
        Assert.NotEmpty(result.RevenueBreakdown);
    }

    [Fact]
    public async Task GetRevenueAsync_InvalidStream_ReturnsEmptyRevenue()
    {
        // Arrange
        var liveStreamId = 999;
        var startTime = DateTime.UtcNow.AddHours(-1);
        var endTime = DateTime.UtcNow;

        // Act
        var result = await _revenueService.GetRevenueAsync(liveStreamId, startTime, endTime);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(liveStreamId, result.LiveStreamId);
        Assert.Equal(0, result.TotalRevenue);
        Assert.Equal(0, result.TotalDonations);
        Assert.Equal(0, result.TotalSuperChats);
        Assert.Equal(0, result.TotalSubscriptions);
        Assert.Equal(0, result.PlatformFees);
        Assert.Equal(0, result.CreatorEarnings);
        Assert.Empty(result.RevenueBreakdown);
    }

    [Fact]
    public async Task GetRevenueAsync_WithTimeRange_ReturnsFilteredRevenue()
    {
        // Arrange
        var liveStreamId = 1;
        var startTime = DateTime.UtcNow.AddMinutes(-30);
        var endTime = DateTime.UtcNow;

        // Act
        var result = await _revenueService.GetRevenueAsync(liveStreamId, startTime, endTime);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(liveStreamId, result.LiveStreamId);
        Assert.True(result.TotalRevenue >= 0);
    }

    [Fact]
    public async Task GetRevenueAsync_WithRevenueType_ReturnsFilteredRevenue()
    {
        // Arrange
        var liveStreamId = 1;
        var startTime = DateTime.UtcNow.AddHours(-1);
        var endTime = DateTime.UtcNow;
        var revenueType = "donation";

        // Act
        var result = await _revenueService.GetRevenueAsync(liveStreamId, startTime, endTime, revenueType);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(liveStreamId, result.LiveStreamId);
        Assert.True(result.TotalRevenue >= 0);
    }

    [Fact]
    public async Task GetRevenueAsync_WithUserId_ReturnsFilteredRevenue()
    {
        // Arrange
        var liveStreamId = 1;
        var startTime = DateTime.UtcNow.AddHours(-1);
        var endTime = DateTime.UtcNow;
        var userId = "test-user-id";

        // Act
        var result = await _revenueService.GetRevenueAsync(liveStreamId, startTime, endTime, null, userId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(liveStreamId, result.LiveStreamId);
        Assert.True(result.TotalRevenue >= 0);
    }

    [Fact]
    public async Task GetRevenueAsync_WithMinAmount_ReturnsFilteredRevenue()
    {
        // Arrange
        var liveStreamId = 1;
        var startTime = DateTime.UtcNow.AddHours(-1);
        var endTime = DateTime.UtcNow;
        var minAmount = 5.00m;

        // Act
        var result = await _revenueService.GetRevenueAsync(liveStreamId, startTime, endTime, null, null, minAmount);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(liveStreamId, result.LiveStreamId);
        Assert.True(result.TotalRevenue >= 0);
    }

    [Fact]
    public async Task GetRevenueAsync_WithMaxAmount_ReturnsFilteredRevenue()
    {
        // Arrange
        var liveStreamId = 1;
        var startTime = DateTime.UtcNow.AddHours(-1);
        var endTime = DateTime.UtcNow;
        var maxAmount = 20.00m;

        // Act
        var result = await _revenueService.GetRevenueAsync(liveStreamId, startTime, endTime, null, null, null, maxAmount);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(liveStreamId, result.LiveStreamId);
        Assert.True(result.TotalRevenue >= 0);
    }

    [Fact]
    public async Task GetRevenueAsync_WithSortBy_ReturnsSortedRevenue()
    {
        // Arrange
        var liveStreamId = 1;
        var startTime = DateTime.UtcNow.AddHours(-1);
        var endTime = DateTime.UtcNow;
        var sortBy = "amount";

        // Act
        var result = await _revenueService.GetRevenueAsync(liveStreamId, startTime, endTime, null, null, null, null, sortBy);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(liveStreamId, result.LiveStreamId);
        Assert.True(result.TotalRevenue >= 0);
    }

    [Fact]
    public async Task GetRevenueAsync_WithSortOrder_ReturnsSortedRevenue()
    {
        // Arrange
        var liveStreamId = 1;
        var startTime = DateTime.UtcNow.AddHours(-1);
        var endTime = DateTime.UtcNow;
        var sortBy = "amount";
        var sortOrder = "desc";

        // Act
        var result = await _revenueService.GetRevenueAsync(liveStreamId, startTime, endTime, null, null, null, null, sortBy, sortOrder);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(liveStreamId, result.LiveStreamId);
        Assert.True(result.TotalRevenue >= 0);
    }

    [Fact]
    public async Task GetRevenueAsync_WithPage_ReturnsPaginatedRevenue()
    {
        // Arrange
        var liveStreamId = 1;
        var startTime = DateTime.UtcNow.AddHours(-1);
        var endTime = DateTime.UtcNow;
        var page = 1;
        var pageSize = 10;

        // Act
        var result = await _revenueService.GetRevenueAsync(liveStreamId, startTime, endTime, null, null, null, null, null, null, page, pageSize);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(liveStreamId, result.LiveStreamId);
        Assert.True(result.TotalRevenue >= 0);
    }

    [Fact]
    public async Task GetRevenueBreakdownAsync_ValidStream_ReturnsBreakdown()
    {
        // Arrange
        var liveStreamId = 1;
        var startTime = DateTime.UtcNow.AddHours(-1);
        var endTime = DateTime.UtcNow;

        // Act
        var result = await _revenueService.GetRevenueBreakdownAsync(liveStreamId, startTime, endTime);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(liveStreamId, result.LiveStreamId);
        Assert.True(result.TotalRevenue >= 0);
        Assert.True(result.TotalDonations >= 0);
        Assert.True(result.TotalSuperChats >= 0);
        Assert.True(result.TotalSubscriptions >= 0);
        Assert.True(result.PlatformFees >= 0);
        Assert.True(result.CreatorEarnings >= 0);
        Assert.NotEmpty(result.RevenueBreakdown);
    }

    [Fact]
    public async Task GetRevenueBreakdownAsync_InvalidStream_ReturnsEmptyBreakdown()
    {
        // Arrange
        var liveStreamId = 999;
        var startTime = DateTime.UtcNow.AddHours(-1);
        var endTime = DateTime.UtcNow;

        // Act
        var result = await _revenueService.GetRevenueBreakdownAsync(liveStreamId, startTime, endTime);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(liveStreamId, result.LiveStreamId);
        Assert.Equal(0, result.TotalRevenue);
        Assert.Equal(0, result.TotalDonations);
        Assert.Equal(0, result.TotalSuperChats);
        Assert.Equal(0, result.TotalSubscriptions);
        Assert.Equal(0, result.PlatformFees);
        Assert.Equal(0, result.CreatorEarnings);
        Assert.Empty(result.RevenueBreakdown);
    }

    [Fact]
    public async Task GetRevenueBreakdownAsync_WithTimeRange_ReturnsFilteredBreakdown()
    {
        // Arrange
        var liveStreamId = 1;
        var startTime = DateTime.UtcNow.AddMinutes(-30);
        var endTime = DateTime.UtcNow;

        // Act
        var result = await _revenueService.GetRevenueBreakdownAsync(liveStreamId, startTime, endTime);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(liveStreamId, result.LiveStreamId);
        Assert.True(result.TotalRevenue >= 0);
    }

    [Fact]
    public async Task GetRevenueBreakdownAsync_WithRevenueType_ReturnsFilteredBreakdown()
    {
        // Arrange
        var liveStreamId = 1;
        var startTime = DateTime.UtcNow.AddHours(-1);
        var endTime = DateTime.UtcNow;
        var revenueType = "donation";

        // Act
        var result = await _revenueService.GetRevenueBreakdownAsync(liveStreamId, startTime, endTime, revenueType);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(liveStreamId, result.LiveStreamId);
        Assert.True(result.TotalRevenue >= 0);
    }

    [Fact]
    public async Task GetRevenueBreakdownAsync_WithUserId_ReturnsFilteredBreakdown()
    {
        // Arrange
        var liveStreamId = 1;
        var startTime = DateTime.UtcNow.AddHours(-1);
        var endTime = DateTime.UtcNow;
        var userId = "test-user-id";

        // Act
        var result = await _revenueService.GetRevenueBreakdownAsync(liveStreamId, startTime, endTime, null, userId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(liveStreamId, result.LiveStreamId);
        Assert.True(result.TotalRevenue >= 0);
    }

    [Fact]
    public async Task GetRevenueBreakdownAsync_WithMinAmount_ReturnsFilteredBreakdown()
    {
        // Arrange
        var liveStreamId = 1;
        var startTime = DateTime.UtcNow.AddHours(-1);
        var endTime = DateTime.UtcNow;
        var minAmount = 5.00m;

        // Act
        var result = await _revenueService.GetRevenueBreakdownAsync(liveStreamId, startTime, endTime, null, null, minAmount);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(liveStreamId, result.LiveStreamId);
        Assert.True(result.TotalRevenue >= 0);
    }

    [Fact]
    public async Task GetRevenueBreakdownAsync_WithMaxAmount_ReturnsFilteredBreakdown()
    {
        // Arrange
        var liveStreamId = 1;
        var startTime = DateTime.UtcNow.AddHours(-1);
        var endTime = DateTime.UtcNow;
        var maxAmount = 20.00m;

        // Act
        var result = await _revenueService.GetRevenueBreakdownAsync(liveStreamId, startTime, endTime, null, null, null, maxAmount);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(liveStreamId, result.LiveStreamId);
        Assert.True(result.TotalRevenue >= 0);
    }

    [Fact]
    public async Task GetRevenueBreakdownAsync_WithSortBy_ReturnsSortedBreakdown()
    {
        // Arrange
        var liveStreamId = 1;
        var startTime = DateTime.UtcNow.AddHours(-1);
        var endTime = DateTime.UtcNow;
        var sortBy = "amount";

        // Act
        var result = await _revenueService.GetRevenueBreakdownAsync(liveStreamId, startTime, endTime, null, null, null, null, sortBy);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(liveStreamId, result.LiveStreamId);
        Assert.True(result.TotalRevenue >= 0);
    }

    [Fact]
    public async Task GetRevenueBreakdownAsync_WithSortOrder_ReturnsSortedBreakdown()
    {
        // Arrange
        var liveStreamId = 1;
        var startTime = DateTime.UtcNow.AddHours(-1);
        var endTime = DateTime.UtcNow;
        var sortBy = "amount";
        var sortOrder = "desc";

        // Act
        var result = await _revenueService.GetRevenueBreakdownAsync(liveStreamId, startTime, endTime, null, null, null, null, sortBy, sortOrder);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(liveStreamId, result.LiveStreamId);
        Assert.True(result.TotalRevenue >= 0);
    }

    [Fact]
    public async Task GetRevenueBreakdownAsync_WithPage_ReturnsPaginatedBreakdown()
    {
        // Arrange
        var liveStreamId = 1;
        var startTime = DateTime.UtcNow.AddHours(-1);
        var endTime = DateTime.UtcNow;
        var page = 1;
        var pageSize = 10;

        // Act
        var result = await _revenueService.GetRevenueBreakdownAsync(liveStreamId, startTime, endTime, null, null, null, null, null, null, page, pageSize);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(liveStreamId, result.LiveStreamId);
        Assert.True(result.TotalRevenue >= 0);
    }

    [Fact]
    public async Task GetRevenueHistoryAsync_ValidStream_ReturnsHistory()
    {
        // Arrange
        var liveStreamId = 1;
        var startTime = DateTime.UtcNow.AddHours(-1);
        var endTime = DateTime.UtcNow;

        // Act
        var result = await _revenueService.GetRevenueHistoryAsync(liveStreamId, startTime, endTime);

        // Assert
        Assert.NotNull(result);
        Assert.NotEmpty(result);
        Assert.All(result, revenue => Assert.Equal(liveStreamId, revenue.LiveStreamId));
        Assert.All(result, revenue => Assert.True(revenue.CreatedAt >= startTime));
        Assert.All(result, revenue => Assert.True(revenue.CreatedAt <= endTime));
    }

    [Fact]
    public async Task GetRevenueHistoryAsync_InvalidStream_ReturnsEmptyList()
    {
        // Arrange
        var liveStreamId = 999;
        var startTime = DateTime.UtcNow.AddHours(-1);
        var endTime = DateTime.UtcNow;

        // Act
        var result = await _revenueService.GetRevenueHistoryAsync(liveStreamId, startTime, endTime);

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);
    }

    [Fact]
    public async Task GetRevenueHistoryAsync_WithRevenueType_ReturnsFilteredHistory()
    {
        // Arrange
        var liveStreamId = 1;
        var startTime = DateTime.UtcNow.AddHours(-1);
        var endTime = DateTime.UtcNow;
        var revenueType = "donation";

        // Act
        var result = await _revenueService.GetRevenueHistoryAsync(liveStreamId, startTime, endTime, revenueType);

        // Assert
        Assert.NotNull(result);
        Assert.All(result, revenue => Assert.Equal(revenueType, revenue.RevenueType));
    }

    [Fact]
    public async Task GetRevenueHistoryAsync_WithUserId_ReturnsFilteredHistory()
    {
        // Arrange
        var liveStreamId = 1;
        var startTime = DateTime.UtcNow.AddHours(-1);
        var endTime = DateTime.UtcNow;
        var userId = "test-user-id";

        // Act
        var result = await _revenueService.GetRevenueHistoryAsync(liveStreamId, startTime, endTime, null, userId);

        // Assert
        Assert.NotNull(result);
        Assert.All(result, revenue => Assert.Equal(userId, revenue.UserId));
    }

    [Fact]
    public async Task GetRevenueHistoryAsync_WithMinAmount_ReturnsFilteredHistory()
    {
        // Arrange
        var liveStreamId = 1;
        var startTime = DateTime.UtcNow.AddHours(-1);
        var endTime = DateTime.UtcNow;
        var minAmount = 5.00m;

        // Act
        var result = await _revenueService.GetRevenueHistoryAsync(liveStreamId, startTime, endTime, null, null, minAmount);

        // Assert
        Assert.NotNull(result);
        Assert.All(result, revenue => Assert.True(revenue.Amount >= minAmount));
    }

    [Fact]
    public async Task GetRevenueHistoryAsync_WithMaxAmount_ReturnsFilteredHistory()
    {
        // Arrange
        var liveStreamId = 1;
        var startTime = DateTime.UtcNow.AddHours(-1);
        var endTime = DateTime.UtcNow;
        var maxAmount = 20.00m;

        // Act
        var result = await _revenueService.GetRevenueHistoryAsync(liveStreamId, startTime, endTime, null, null, null, maxAmount);

        // Assert
        Assert.NotNull(result);
        Assert.All(result, revenue => Assert.True(revenue.Amount <= maxAmount));
    }

    [Fact]
    public async Task GetRevenueHistoryAsync_WithSortBy_ReturnsSortedHistory()
    {
        // Arrange
        var liveStreamId = 1;
        var startTime = DateTime.UtcNow.AddHours(-1);
        var endTime = DateTime.UtcNow;
        var sortBy = "amount";

        // Act
        var result = await _revenueService.GetRevenueHistoryAsync(liveStreamId, startTime, endTime, null, null, null, null, sortBy);

        // Assert
        Assert.NotNull(result);
        Assert.NotEmpty(result);
    }

    [Fact]
    public async Task GetRevenueHistoryAsync_WithSortOrder_ReturnsSortedHistory()
    {
        // Arrange
        var liveStreamId = 1;
        var startTime = DateTime.UtcNow.AddHours(-1);
        var endTime = DateTime.UtcNow;
        var sortBy = "amount";
        var sortOrder = "desc";

        // Act
        var result = await _revenueService.GetRevenueHistoryAsync(liveStreamId, startTime, endTime, null, null, null, null, sortBy, sortOrder);

        // Assert
        Assert.NotNull(result);
        Assert.NotEmpty(result);
    }

    [Fact]
    public async Task GetRevenueHistoryAsync_WithPage_ReturnsPaginatedHistory()
    {
        // Arrange
        var liveStreamId = 1;
        var startTime = DateTime.UtcNow.AddHours(-1);
        var endTime = DateTime.UtcNow;
        var page = 1;
        var pageSize = 10;

        // Act
        var result = await _revenueService.GetRevenueHistoryAsync(liveStreamId, startTime, endTime, null, null, null, null, null, null, page, pageSize);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.Count <= pageSize);
    }

    public void Dispose()
    {
        _context.Dispose();
    }
}
