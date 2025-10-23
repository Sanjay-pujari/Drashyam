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

public class LiveStreamMonetizationServiceTests : IDisposable
{
    private readonly DrashyamDbContext _context;
    private readonly Mock<IHubContext<LiveStreamHub>> _mockLiveStreamHub;
    private readonly Mock<IHubContext<NotificationHub>> _mockNotificationHub;
    private readonly Mock<ILogger<LiveStreamMonetizationService>> _mockLogger;
    private readonly LiveStreamMonetizationService _monetizationService;

    public LiveStreamMonetizationServiceTests()
    {
        // Setup in-memory database
        var options = new DbContextOptionsBuilder<DrashyamDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new DrashyamDbContext(options);

        // Setup mocks
        _mockLiveStreamHub = new Mock<IHubContext<LiveStreamHub>>();
        _mockNotificationHub = new Mock<IHubContext<NotificationHub>>();
        _mockLogger = new Mock<ILogger<LiveStreamMonetizationService>>();

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

        _monetizationService = new LiveStreamMonetizationService(
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

        // Add test donations
        var donation1 = new LiveStreamDonation
        {
            Id = 1,
            LiveStreamId = 1,
            DonorId = "test-user-id",
            Amount = 10.00m,
            Message = "Great stream!",
            IsAnonymous = false,
            CreatedAt = DateTime.UtcNow.AddMinutes(-20)
        };
        _context.LiveStreamDonations.Add(donation1);

        var donation2 = new LiveStreamDonation
        {
            Id = 2,
            LiveStreamId = 1,
            DonorId = "test-user-id",
            Amount = 25.00m,
            Message = "Keep up the good work!",
            IsAnonymous = true,
            CreatedAt = DateTime.UtcNow.AddMinutes(-10)
        };
        _context.LiveStreamDonations.Add(donation2);

        // Add test super chats
        var superChat1 = new LiveStreamSuperChat
        {
            Id = 1,
            LiveStreamId = 1,
            UserId = "test-user-id",
            Amount = 5.00m,
            Message = "Hello from super chat!",
            Tier = DTOs.SuperChatTier.Tier1,
            Duration = 30,
            CreatedAt = DateTime.UtcNow.AddMinutes(-15)
        };
        _context.LiveStreamSuperChats.Add(superChat1);

        // Add test subscriptions
        var subscription1 = new LiveStreamSubscription
        {
            Id = 1,
            LiveStreamId = 1,
            SubscriberId = "test-user-id",
            Tier = DTOs.SubscriptionTier.Tier1,
            Amount = 4.99m,
            IsGift = false,
            GiftRecipientId = null,
            CreatedAt = DateTime.UtcNow.AddMinutes(-25)
        };
        _context.LiveStreamSubscriptions.Add(subscription1);

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

        _context.SaveChanges();
    }

    [Fact]
    public async Task ProcessDonationAsync_ValidDonation_ReturnsDonation()
    {
        // Arrange
        var dto = new ProcessDonationDto
        {
            LiveStreamId = 1,
            Amount = 15.00m,
            Message = "Test donation",
            IsAnonymous = false
        };
        var donorId = "test-user-id";

        // Act
        var result = await _monetizationService.ProcessDonationAsync(dto, donorId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(dto.LiveStreamId, result.LiveStreamId);
        Assert.Equal(dto.Amount, result.Amount);
        Assert.Equal(dto.Message, result.Message);
        Assert.Equal(dto.IsAnonymous, result.IsAnonymous);
        Assert.Equal(donorId, result.DonorId);
        Assert.NotNull(result.CreatedAt);
    }

    [Fact]
    public async Task ProcessDonationAsync_InvalidStream_ThrowsArgumentException()
    {
        // Arrange
        var dto = new ProcessDonationDto
        {
            LiveStreamId = 999,
            Amount = 15.00m,
            Message = "Test donation",
            IsAnonymous = false
        };
        var donorId = "test-user-id";

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => 
            _monetizationService.ProcessDonationAsync(dto, donorId));
    }

    [Fact]
    public async Task ProcessDonationAsync_InvalidAmount_ThrowsArgumentException()
    {
        // Arrange
        var dto = new ProcessDonationDto
        {
            LiveStreamId = 1,
            Amount = -5.00m,
            Message = "Test donation",
            IsAnonymous = false
        };
        var donorId = "test-user-id";

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => 
            _monetizationService.ProcessDonationAsync(dto, donorId));
    }

    [Fact]
    public async Task ProcessSuperChatAsync_ValidSuperChat_ReturnsSuperChat()
    {
        // Arrange
        var dto = new ProcessSuperChatDto
        {
            LiveStreamId = 1,
            Amount = 10.00m,
            Message = "Test super chat",
            Tier = DTOs.SuperChatTier.Tier2
        };
        var userId = "test-user-id";

        // Act
        var result = await _monetizationService.ProcessSuperChatAsync(dto, userId);

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
            Amount = 10.00m,
            Message = "Test super chat",
            Tier = DTOs.SuperChatTier.Tier2
        };
        var userId = "test-user-id";

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => 
            _monetizationService.ProcessSuperChatAsync(dto, userId));
    }

    [Fact]
    public async Task ProcessSuperChatAsync_InvalidAmount_ThrowsArgumentException()
    {
        // Arrange
        var dto = new ProcessSuperChatDto
        {
            LiveStreamId = 1,
            Amount = 0.00m,
            Message = "Test super chat",
            Tier = DTOs.SuperChatTier.Tier1
        };
        var userId = "test-user-id";

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => 
            _monetizationService.ProcessSuperChatAsync(dto, userId));
    }

    [Fact]
    public async Task ProcessSubscriptionAsync_ValidSubscription_ReturnsSubscription()
    {
        // Arrange
        var dto = new ProcessSubscriptionDto
        {
            LiveStreamId = 1,
            Tier = DTOs.SubscriptionTier.Tier2,
            Amount = 9.99m,
            IsGift = false
        };
        var subscriberId = "test-user-id";

        // Act
        var result = await _monetizationService.ProcessSubscriptionAsync(dto, subscriberId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(dto.LiveStreamId, result.LiveStreamId);
        Assert.Equal(dto.Tier, result.Tier);
        Assert.Equal(dto.Amount, result.Amount);
        Assert.Equal(dto.IsGift, result.IsGift);
        Assert.Equal(subscriberId, result.SubscriberId);
        Assert.NotNull(result.CreatedAt);
    }

    [Fact]
    public async Task ProcessSubscriptionAsync_InvalidStream_ThrowsArgumentException()
    {
        // Arrange
        var dto = new ProcessSubscriptionDto
        {
            LiveStreamId = 999,
            Tier = DTOs.SubscriptionTier.Tier2,
            Amount = 9.99m,
            IsGift = false
        };
        var subscriberId = "test-user-id";

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => 
            _monetizationService.ProcessSubscriptionAsync(dto, subscriberId));
    }

    [Fact]
    public async Task ProcessSubscriptionAsync_InvalidAmount_ThrowsArgumentException()
    {
        // Arrange
        var dto = new ProcessSubscriptionDto
        {
            LiveStreamId = 1,
            Tier = DTOs.SubscriptionTier.Tier1,
            Amount = -4.99m,
            IsGift = false
        };
        var subscriberId = "test-user-id";

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => 
            _monetizationService.ProcessSubscriptionAsync(dto, subscriberId));
    }

    [Fact]
    public async Task GetDonationsAsync_ValidStream_ReturnsDonations()
    {
        // Arrange
        var liveStreamId = 1;
        var page = 1;
        var pageSize = 10;

        // Act
        var result = await _monetizationService.GetDonationsAsync(liveStreamId, page, pageSize);

        // Assert
        Assert.NotNull(result);
        Assert.NotEmpty(result);
        Assert.True(result.Count <= pageSize);
        Assert.All(result, donation => Assert.Equal(liveStreamId, donation.LiveStreamId));
    }

    [Fact]
    public async Task GetDonationsAsync_InvalidStream_ReturnsEmptyList()
    {
        // Arrange
        var liveStreamId = 999;
        var page = 1;
        var pageSize = 10;

        // Act
        var result = await _monetizationService.GetDonationsAsync(liveStreamId, page, pageSize);

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);
    }

    [Fact]
    public async Task GetSuperChatsAsync_ValidStream_ReturnsSuperChats()
    {
        // Arrange
        var liveStreamId = 1;
        var page = 1;
        var pageSize = 10;

        // Act
        var result = await _monetizationService.GetSuperChatsAsync(liveStreamId, page, pageSize);

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
        var result = await _monetizationService.GetSuperChatsAsync(liveStreamId, page, pageSize);

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);
    }

    [Fact]
    public async Task GetSubscriptionsAsync_ValidStream_ReturnsSubscriptions()
    {
        // Arrange
        var liveStreamId = 1;
        var page = 1;
        var pageSize = 10;

        // Act
        var result = await _monetizationService.GetSubscriptionsAsync(liveStreamId, page, pageSize);

        // Assert
        Assert.NotNull(result);
        Assert.NotEmpty(result);
        Assert.True(result.Count <= pageSize);
        Assert.All(result, subscription => Assert.Equal(liveStreamId, subscription.LiveStreamId));
    }

    [Fact]
    public async Task GetSubscriptionsAsync_InvalidStream_ReturnsEmptyList()
    {
        // Arrange
        var liveStreamId = 999;
        var page = 1;
        var pageSize = 10;

        // Act
        var result = await _monetizationService.GetSubscriptionsAsync(liveStreamId, page, pageSize);

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);
    }

    [Fact]
    public async Task GetRevenueAsync_ValidStream_ReturnsRevenue()
    {
        // Arrange
        var liveStreamId = 1;
        var startTime = DateTime.UtcNow.AddHours(-1);
        var endTime = DateTime.UtcNow;

        // Act
        var result = await _monetizationService.GetRevenueAsync(liveStreamId, startTime, endTime);

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
        var result = await _monetizationService.GetRevenueAsync(liveStreamId, startTime, endTime);

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
        var result = await _monetizationService.GetRevenueAsync(liveStreamId, startTime, endTime);

        // Assert
        Assert.NotNull(result);
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
        var result = await _monetizationService.GetRevenueAsync(liveStreamId, startTime, endTime, revenueType);

        // Assert
        Assert.NotNull(result);
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
        var result = await _monetizationService.GetRevenueAsync(liveStreamId, startTime, endTime, null, userId);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.TotalRevenue >= 0);
    }

    [Fact]
    public async Task GetRevenueAsync_WithTier_ReturnsFilteredRevenue()
    {
        // Arrange
        var liveStreamId = 1;
        var startTime = DateTime.UtcNow.AddHours(-1);
        var endTime = DateTime.UtcNow;
        var tier = DTOs.SuperChatTier.Tier1;

        // Act
        var result = await _monetizationService.GetRevenueAsync(liveStreamId, startTime, endTime, null, null, tier);

        // Assert
        Assert.NotNull(result);
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
        var result = await _monetizationService.GetRevenueAsync(liveStreamId, startTime, endTime, null, null, null, minAmount);

        // Assert
        Assert.NotNull(result);
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
        var result = await _monetizationService.GetRevenueAsync(liveStreamId, startTime, endTime, null, null, null, null, maxAmount);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.TotalRevenue >= 0);
    }

    [Fact]
    public async Task GetRevenueAsync_WithIsAnonymous_ReturnsFilteredRevenue()
    {
        // Arrange
        var liveStreamId = 1;
        var startTime = DateTime.UtcNow.AddHours(-1);
        var endTime = DateTime.UtcNow;
        var isAnonymous = true;

        // Act
        var result = await _monetizationService.GetRevenueAsync(liveStreamId, startTime, endTime, null, null, null, null, null, isAnonymous);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.TotalRevenue >= 0);
    }

    [Fact]
    public async Task GetRevenueAsync_WithIsGift_ReturnsFilteredRevenue()
    {
        // Arrange
        var liveStreamId = 1;
        var startTime = DateTime.UtcNow.AddHours(-1);
        var endTime = DateTime.UtcNow;
        var isGift = false;

        // Act
        var result = await _monetizationService.GetRevenueAsync(liveStreamId, startTime, endTime, null, null, null, null, null, null, isGift);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.TotalRevenue >= 0);
    }

    [Fact]
    public async Task GetRevenueAsync_WithGiftRecipientId_ReturnsFilteredRevenue()
    {
        // Arrange
        var liveStreamId = 1;
        var startTime = DateTime.UtcNow.AddHours(-1);
        var endTime = DateTime.UtcNow;
        var giftRecipientId = "test-user-id";

        // Act
        var result = await _monetizationService.GetRevenueAsync(liveStreamId, startTime, endTime, null, null, null, null, null, null, null, giftRecipientId);

        // Assert
        Assert.NotNull(result);
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
        var result = await _monetizationService.GetRevenueAsync(liveStreamId, startTime, endTime, null, null, null, null, null, null, null, null, sortBy);

        // Assert
        Assert.NotNull(result);
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
        var result = await _monetizationService.GetRevenueAsync(liveStreamId, startTime, endTime, null, null, null, null, null, null, null, null, sortBy, sortOrder);

        // Assert
        Assert.NotNull(result);
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
        var result = await _monetizationService.GetRevenueAsync(liveStreamId, startTime, endTime, null, null, null, null, null, null, null, null, null, null, page, pageSize);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.TotalRevenue >= 0);
    }

    public void Dispose()
    {
        _context.Dispose();
    }
}
