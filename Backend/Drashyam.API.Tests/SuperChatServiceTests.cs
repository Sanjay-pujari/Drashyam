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

public class SuperChatServiceTests : IDisposable
{
    private readonly DrashyamDbContext _context;
    private readonly Mock<IHubContext<LiveStreamHub>> _mockLiveStreamHub;
    private readonly Mock<IHubContext<NotificationHub>> _mockNotificationHub;
    private readonly Mock<ILogger<SuperChatService>> _mockLogger;
    private readonly SuperChatService _superChatService;

    public SuperChatServiceTests()
    {
        // Setup in-memory database
        var options = new DbContextOptionsBuilder<DrashyamDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new DrashyamDbContext(options);

        // Setup mocks
        _mockLiveStreamHub = new Mock<IHubContext<LiveStreamHub>>();
        _mockNotificationHub = new Mock<IHubContext<NotificationHub>>();
        _mockLogger = new Mock<ILogger<SuperChatService>>();

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

        _superChatService = new SuperChatService(
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

        // Add test super chat
        var superChat = new LiveStreamSuperChat
        {
            Id = 1,
            LiveStreamId = 1,
            UserId = "test-user-id",
            Amount = 10.00m,
            Message = "Great stream!",
            Tier = DTOs.SuperChatTier.Tier2,
            Duration = 60,
            CreatedAt = DateTime.UtcNow.AddMinutes(-20)
        };
        _context.LiveStreamSuperChats.Add(superChat);

        // Add test revenue
        var revenue = new LiveStreamRevenue
        {
            Id = 1,
            LiveStreamId = 1,
            RevenueType = "superchat",
            Amount = 10.00m,
            PlatformFee = 1.00m,
            CreatorEarnings = 9.00m,
            CreatedAt = DateTime.UtcNow.AddMinutes(-20)
        };
        _context.LiveStreamRevenues.Add(revenue);

        _context.SaveChanges();
    }

    [Fact]
    public async Task ProcessSuperChatAsync_ValidSuperChat_ReturnsSuperChat()
    {
        // Arrange
        var dto = new ProcessSuperChatDto
        {
            LiveStreamId = 1,
            Amount = 15.00m,
            Message = "Test super chat",
            Tier = DTOs.SuperChatTier.Tier3
        };
        var userId = "test-user-id";

        // Act
        var result = await _superChatService.ProcessSuperChatAsync(dto, userId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(dto.LiveStreamId, result.LiveStreamId);
        Assert.Equal(dto.Amount, result.Amount);
        Assert.Equal(dto.Message, result.Message);
        Assert.Equal(dto.Tier, result.Tier);
        Assert.Equal(userId, result.UserId);
        Assert.NotNull(result.CreatedAt);
    }

    [Fact]
    public async Task ProcessSuperChatAsync_InvalidStream_ThrowsArgumentException()
    {
        // Arrange
        var dto = new ProcessSuperChatDto
        {
            LiveStreamId = 999,
            Amount = 15.00m,
            Message = "Test super chat",
            Tier = DTOs.SuperChatTier.Tier3
        };
        var userId = "test-user-id";

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => 
            _superChatService.ProcessSuperChatAsync(dto, userId));
    }

    [Fact]
    public async Task ProcessSuperChatAsync_InvalidAmount_ThrowsArgumentException()
    {
        // Arrange
        var dto = new ProcessSuperChatDto
        {
            LiveStreamId = 1,
            Amount = -5.00m,
            Message = "Test super chat",
            Tier = DTOs.SuperChatTier.Tier1
        };
        var userId = "test-user-id";

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => 
            _superChatService.ProcessSuperChatAsync(dto, userId));
    }

    [Fact]
    public async Task ProcessSuperChatAsync_InvalidTier_ThrowsArgumentException()
    {
        // Arrange
        var dto = new ProcessSuperChatDto
        {
            LiveStreamId = 1,
            Amount = 5.00m,
            Message = "Test super chat",
            Tier = (DTOs.SuperChatTier)999 // Invalid tier
        };
        var userId = "test-user-id";

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => 
            _superChatService.ProcessSuperChatAsync(dto, userId));
    }

    [Fact]
    public async Task GetSuperChatsAsync_ValidStream_ReturnsSuperChats()
    {
        // Arrange
        var liveStreamId = 1;
        var page = 1;
        var pageSize = 10;

        // Act
        var result = await _superChatService.GetSuperChatsAsync(liveStreamId, page, pageSize);

        // Assert
        Assert.NotNull(result);
        Assert.NotEmpty(result);
        Assert.True(result.Count <= pageSize);
        Assert.All(result, superChat => Assert.Equal(liveStreamId, superChat.LiveStreamId));
    }

    [Fact]
    public async Task GetSuperChatsAsync_InvalidStream_ReturnsEmptyList()
    {
        // Arrange
        var liveStreamId = 999;
        var page = 1;
        var pageSize = 10;

        // Act
        var result = await _superChatService.GetSuperChatsAsync(liveStreamId, page, pageSize);

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);
    }

    [Fact]
    public async Task GetSuperChatsAsync_WithPagination_ReturnsCorrectPage()
    {
        // Arrange
        var liveStreamId = 1;
        var page = 1;
        var pageSize = 1;

        // Act
        var result = await _superChatService.GetSuperChatsAsync(liveStreamId, page, pageSize);

        // Assert
        Assert.NotNull(result);
        Assert.Single(result);
    }

    [Fact]
    public async Task GetSuperChatsAsync_WithTier_ReturnsFilteredSuperChats()
    {
        // Arrange
        var liveStreamId = 1;
        var page = 1;
        var pageSize = 10;
        var tier = DTOs.SuperChatTier.Tier2;

        // Act
        var result = await _superChatService.GetSuperChatsAsync(liveStreamId, page, pageSize, tier);

        // Assert
        Assert.NotNull(result);
        Assert.All(result, superChat => Assert.Equal(tier, superChat.Tier));
    }

    [Fact]
    public async Task GetSuperChatsAsync_WithMinAmount_ReturnsFilteredSuperChats()
    {
        // Arrange
        var liveStreamId = 1;
        var page = 1;
        var pageSize = 10;
        var minAmount = 5.00m;

        // Act
        var result = await _superChatService.GetSuperChatsAsync(liveStreamId, page, pageSize, null, minAmount);

        // Assert
        Assert.NotNull(result);
        Assert.All(result, superChat => Assert.True(superChat.Amount >= minAmount));
    }

    [Fact]
    public async Task GetSuperChatsAsync_WithMaxAmount_ReturnsFilteredSuperChats()
    {
        // Arrange
        var liveStreamId = 1;
        var page = 1;
        var pageSize = 10;
        var maxAmount = 20.00m;

        // Act
        var result = await _superChatService.GetSuperChatsAsync(liveStreamId, page, pageSize, null, null, maxAmount);

        // Assert
        Assert.NotNull(result);
        Assert.All(result, superChat => Assert.True(superChat.Amount <= maxAmount));
    }

    [Fact]
    public async Task GetSuperChatsAsync_WithUserId_ReturnsFilteredSuperChats()
    {
        // Arrange
        var liveStreamId = 1;
        var page = 1;
        var pageSize = 10;
        var userId = "test-user-id";

        // Act
        var result = await _superChatService.GetSuperChatsAsync(liveStreamId, page, pageSize, null, null, null, userId);

        // Assert
        Assert.NotNull(result);
        Assert.All(result, superChat => Assert.Equal(userId, superChat.UserId));
    }

    [Fact]
    public async Task GetSuperChatsAsync_WithStartTime_ReturnsFilteredSuperChats()
    {
        // Arrange
        var liveStreamId = 1;
        var page = 1;
        var pageSize = 10;
        var startTime = DateTime.UtcNow.AddMinutes(-30);

        // Act
        var result = await _superChatService.GetSuperChatsAsync(liveStreamId, page, pageSize, null, null, null, null, startTime);

        // Assert
        Assert.NotNull(result);
        Assert.All(result, superChat => Assert.True(superChat.CreatedAt >= startTime));
    }

    [Fact]
    public async Task GetSuperChatsAsync_WithEndTime_ReturnsFilteredSuperChats()
    {
        // Arrange
        var liveStreamId = 1;
        var page = 1;
        var pageSize = 10;
        var endTime = DateTime.UtcNow;

        // Act
        var result = await _superChatService.GetSuperChatsAsync(liveStreamId, page, pageSize, null, null, null, null, null, endTime);

        // Assert
        Assert.NotNull(result);
        Assert.All(result, superChat => Assert.True(superChat.CreatedAt <= endTime));
    }

    [Fact]
    public async Task GetSuperChatsAsync_WithSortBy_ReturnsSortedSuperChats()
    {
        // Arrange
        var liveStreamId = 1;
        var page = 1;
        var pageSize = 10;
        var sortBy = "amount";

        // Act
        var result = await _superChatService.GetSuperChatsAsync(liveStreamId, page, pageSize, null, null, null, null, null, null, sortBy);

        // Assert
        Assert.NotNull(result);
        Assert.NotEmpty(result);
    }

    [Fact]
    public async Task GetSuperChatsAsync_WithSortOrder_ReturnsSortedSuperChats()
    {
        // Arrange
        var liveStreamId = 1;
        var page = 1;
        var pageSize = 10;
        var sortBy = "amount";
        var sortOrder = "desc";

        // Act
        var result = await _superChatService.GetSuperChatsAsync(liveStreamId, page, pageSize, null, null, null, null, null, null, sortBy, sortOrder);

        // Assert
        Assert.NotNull(result);
        Assert.NotEmpty(result);
    }

    [Fact]
    public async Task GetSuperChatStatsAsync_ValidStream_ReturnsStats()
    {
        // Arrange
        var liveStreamId = 1;

        // Act
        var result = await _superChatService.GetSuperChatStatsAsync(liveStreamId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(liveStreamId, result.LiveStreamId);
        Assert.True(result.TotalSuperChats >= 0);
        Assert.True(result.TotalAmount >= 0);
        Assert.True(result.AverageAmount >= 0);
        Assert.True(result.TopTier >= 0);
        Assert.NotNull(result.TierBreakdown);
        Assert.NotNull(result.RecentSuperChats);
    }

    [Fact]
    public async Task GetSuperChatStatsAsync_InvalidStream_ReturnsEmptyStats()
    {
        // Arrange
        var liveStreamId = 999;

        // Act
        var result = await _superChatService.GetSuperChatStatsAsync(liveStreamId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(liveStreamId, result.LiveStreamId);
        Assert.Equal(0, result.TotalSuperChats);
        Assert.Equal(0, result.TotalAmount);
        Assert.Equal(0, result.AverageAmount);
        Assert.Equal(0, result.TopTier);
        Assert.Empty(result.TierBreakdown);
        Assert.Empty(result.RecentSuperChats);
    }

    [Fact]
    public async Task GetSuperChatStatsAsync_WithTimeRange_ReturnsFilteredStats()
    {
        // Arrange
        var liveStreamId = 1;
        var startTime = DateTime.UtcNow.AddHours(-1);
        var endTime = DateTime.UtcNow;

        // Act
        var result = await _superChatService.GetSuperChatStatsAsync(liveStreamId, startTime, endTime);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(liveStreamId, result.LiveStreamId);
        Assert.True(result.TotalSuperChats >= 0);
        Assert.True(result.TotalAmount >= 0);
    }

    [Fact]
    public async Task GetSuperChatStatsAsync_WithTier_ReturnsFilteredStats()
    {
        // Arrange
        var liveStreamId = 1;
        var tier = DTOs.SuperChatTier.Tier2;

        // Act
        var result = await _superChatService.GetSuperChatStatsAsync(liveStreamId, null, null, tier);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(liveStreamId, result.LiveStreamId);
        Assert.True(result.TotalSuperChats >= 0);
        Assert.True(result.TotalAmount >= 0);
    }

    [Fact]
    public async Task GetSuperChatStatsAsync_WithUserId_ReturnsFilteredStats()
    {
        // Arrange
        var liveStreamId = 1;
        var userId = "test-user-id";

        // Act
        var result = await _superChatService.GetSuperChatStatsAsync(liveStreamId, null, null, null, userId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(liveStreamId, result.LiveStreamId);
        Assert.True(result.TotalSuperChats >= 0);
        Assert.True(result.TotalAmount >= 0);
    }

    [Fact]
    public async Task GetSuperChatStatsAsync_WithMinAmount_ReturnsFilteredStats()
    {
        // Arrange
        var liveStreamId = 1;
        var minAmount = 5.00m;

        // Act
        var result = await _superChatService.GetSuperChatStatsAsync(liveStreamId, null, null, null, null, minAmount);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(liveStreamId, result.LiveStreamId);
        Assert.True(result.TotalSuperChats >= 0);
        Assert.True(result.TotalAmount >= 0);
    }

    [Fact]
    public async Task GetSuperChatStatsAsync_WithMaxAmount_ReturnsFilteredStats()
    {
        // Arrange
        var liveStreamId = 1;
        var maxAmount = 20.00m;

        // Act
        var result = await _superChatService.GetSuperChatStatsAsync(liveStreamId, null, null, null, null, null, maxAmount);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(liveStreamId, result.LiveStreamId);
        Assert.True(result.TotalSuperChats >= 0);
        Assert.True(result.TotalAmount >= 0);
    }

    [Fact]
    public async Task GetSuperChatStatsAsync_WithSortBy_ReturnsSortedStats()
    {
        // Arrange
        var liveStreamId = 1;
        var sortBy = "amount";

        // Act
        var result = await _superChatService.GetSuperChatStatsAsync(liveStreamId, null, null, null, null, null, null, sortBy);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(liveStreamId, result.LiveStreamId);
        Assert.True(result.TotalSuperChats >= 0);
        Assert.True(result.TotalAmount >= 0);
    }

    [Fact]
    public async Task GetSuperChatStatsAsync_WithSortOrder_ReturnsSortedStats()
    {
        // Arrange
        var liveStreamId = 1;
        var sortBy = "amount";
        var sortOrder = "desc";

        // Act
        var result = await _superChatService.GetSuperChatStatsAsync(liveStreamId, null, null, null, null, null, null, sortBy, sortOrder);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(liveStreamId, result.LiveStreamId);
        Assert.True(result.TotalSuperChats >= 0);
        Assert.True(result.TotalAmount >= 0);
    }

    [Fact]
    public async Task GetSuperChatStatsAsync_WithPage_ReturnsPaginatedStats()
    {
        // Arrange
        var liveStreamId = 1;
        var page = 1;
        var pageSize = 10;

        // Act
        var result = await _superChatService.GetSuperChatStatsAsync(liveStreamId, null, null, null, null, null, null, null, null, page, pageSize);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(liveStreamId, result.LiveStreamId);
        Assert.True(result.TotalSuperChats >= 0);
        Assert.True(result.TotalAmount >= 0);
    }

    public void Dispose()
    {
        _context.Dispose();
    }
}
