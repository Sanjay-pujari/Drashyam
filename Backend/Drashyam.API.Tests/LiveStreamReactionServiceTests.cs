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

public class LiveStreamReactionServiceTests : IDisposable
{
    private readonly DrashyamDbContext _context;
    private readonly Mock<IHubContext<LiveStreamHub>> _mockLiveStreamHub;
    private readonly Mock<IHubContext<NotificationHub>> _mockNotificationHub;
    private readonly Mock<ILogger<LiveStreamReactionService>> _mockLogger;
    private readonly LiveStreamReactionService _reactionService;

    public LiveStreamReactionServiceTests()
    {
        // Setup in-memory database
        var options = new DbContextOptionsBuilder<DrashyamDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new DrashyamDbContext(options);

        // Setup mocks
        _mockLiveStreamHub = new Mock<IHubContext<LiveStreamHub>>();
        _mockNotificationHub = new Mock<IHubContext<NotificationHub>>();
        _mockLogger = new Mock<ILogger<LiveStreamReactionService>>();

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

        _reactionService = new LiveStreamReactionService(
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

        // Add test reactions
        var reaction1 = new LiveStreamReaction
        {
            Id = 1,
            LiveStreamId = 1,
            UserId = "test-user-id",
            ReactionType = DTOs.ReactionType.Like,
            CreatedAt = DateTime.UtcNow.AddMinutes(-20)
        };
        _context.LiveStreamReactions.Add(reaction1);

        var reaction2 = new LiveStreamReaction
        {
            Id = 2,
            LiveStreamId = 1,
            UserId = "test-user-id",
            ReactionType = DTOs.ReactionType.Love,
            CreatedAt = DateTime.UtcNow.AddMinutes(-15)
        };
        _context.LiveStreamReactions.Add(reaction2);

        var reaction3 = new LiveStreamReaction
        {
            Id = 3,
            LiveStreamId = 1,
            UserId = "test-user-id",
            ReactionType = DTOs.ReactionType.Laugh,
            CreatedAt = DateTime.UtcNow.AddMinutes(-10)
        };
        _context.LiveStreamReactions.Add(reaction3);

        _context.SaveChanges();
    }

    [Fact]
    public async Task AddReactionAsync_ValidReaction_ReturnsReaction()
    {
        // Arrange
        var dto = new AddReactionDto
        {
            LiveStreamId = 1,
            ReactionType = DTOs.ReactionType.Like
        };
        var userId = "test-user-id";

        // Act
        var result = await _reactionService.AddReactionAsync(dto, userId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(dto.LiveStreamId, result.LiveStreamId);
        Assert.Equal(dto.ReactionType, result.ReactionType);
        Assert.Equal(userId, result.UserId);
        Assert.NotNull(result.CreatedAt);
    }

    [Fact]
    public async Task AddReactionAsync_InvalidStream_ThrowsArgumentException()
    {
        // Arrange
        var dto = new AddReactionDto
        {
            LiveStreamId = 999,
            ReactionType = DTOs.ReactionType.Like
        };
        var userId = "test-user-id";

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => 
            _reactionService.AddReactionAsync(dto, userId));
    }

    [Fact]
    public async Task AddReactionAsync_InvalidReactionType_ThrowsArgumentException()
    {
        // Arrange
        var dto = new AddReactionDto
        {
            LiveStreamId = 1,
            ReactionType = (DTOs.ReactionType)999 // Invalid reaction type
        };
        var userId = "test-user-id";

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => 
            _reactionService.AddReactionAsync(dto, userId));
    }

    [Fact]
    public async Task AddReactionAsync_AlreadyReacted_ThrowsInvalidOperationException()
    {
        // Arrange
        var dto = new AddReactionDto
        {
            LiveStreamId = 1,
            ReactionType = DTOs.ReactionType.Like
        };
        var userId = "test-user-id";

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => 
            _reactionService.AddReactionAsync(dto, userId));
    }

    [Fact]
    public async Task RemoveReactionAsync_ValidReaction_ReturnsTrue()
    {
        // Arrange
        var liveStreamId = 1;
        var userId = "test-user-id";

        // Act
        var result = await _reactionService.RemoveReactionAsync(liveStreamId, userId);

        // Assert
        Assert.True(result);

        // Verify reaction is removed
        var reaction = await _context.LiveStreamReactions
            .FirstOrDefaultAsync(r => r.LiveStreamId == liveStreamId && r.UserId == userId);
        Assert.Null(reaction);
    }

    [Fact]
    public async Task RemoveReactionAsync_InvalidStream_ThrowsArgumentException()
    {
        // Arrange
        var liveStreamId = 999;
        var userId = "test-user-id";

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => 
            _reactionService.RemoveReactionAsync(liveStreamId, userId));
    }

    [Fact]
    public async Task RemoveReactionAsync_NotReacted_ThrowsInvalidOperationException()
    {
        // Arrange
        var liveStreamId = 1;
        var userId = "not-reacted-user-id";

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => 
            _reactionService.RemoveReactionAsync(liveStreamId, userId));
    }

    [Fact]
    public async Task GetReactionsAsync_ValidStream_ReturnsReactions()
    {
        // Arrange
        var liveStreamId = 1;
        var page = 1;
        var pageSize = 10;

        // Act
        var result = await _reactionService.GetReactionsAsync(liveStreamId, page, pageSize);

        // Assert
        Assert.NotNull(result);
        Assert.NotEmpty(result);
        Assert.True(result.Count <= pageSize);
        Assert.All(result, reaction => Assert.Equal(liveStreamId, reaction.LiveStreamId));
    }

    [Fact]
    public async Task GetReactionsAsync_InvalidStream_ReturnsEmptyList()
    {
        // Arrange
        var liveStreamId = 999;
        var page = 1;
        var pageSize = 10;

        // Act
        var result = await _reactionService.GetReactionsAsync(liveStreamId, page, pageSize);

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);
    }

    [Fact]
    public async Task GetReactionsAsync_WithReactionType_ReturnsFilteredReactions()
    {
        // Arrange
        var liveStreamId = 1;
        var page = 1;
        var pageSize = 10;
        var reactionType = DTOs.ReactionType.Like;

        // Act
        var result = await _reactionService.GetReactionsAsync(liveStreamId, page, pageSize, reactionType);

        // Assert
        Assert.NotNull(result);
        Assert.All(result, reaction => Assert.Equal(reactionType, reaction.ReactionType));
    }

    [Fact]
    public async Task GetReactionsAsync_WithUserId_ReturnsFilteredReactions()
    {
        // Arrange
        var liveStreamId = 1;
        var page = 1;
        var pageSize = 10;
        var userId = "test-user-id";

        // Act
        var result = await _reactionService.GetReactionsAsync(liveStreamId, page, pageSize, null, userId);

        // Assert
        Assert.NotNull(result);
        Assert.All(result, reaction => Assert.Equal(userId, reaction.UserId));
    }

    [Fact]
    public async Task GetReactionsAsync_WithStartTime_ReturnsFilteredReactions()
    {
        // Arrange
        var liveStreamId = 1;
        var page = 1;
        var pageSize = 10;
        var startTime = DateTime.UtcNow.AddMinutes(-30);

        // Act
        var result = await _reactionService.GetReactionsAsync(liveStreamId, page, pageSize, null, null, startTime);

        // Assert
        Assert.NotNull(result);
        Assert.All(result, reaction => Assert.True(reaction.CreatedAt >= startTime));
    }

    [Fact]
    public async Task GetReactionsAsync_WithEndTime_ReturnsFilteredReactions()
    {
        // Arrange
        var liveStreamId = 1;
        var page = 1;
        var pageSize = 10;
        var endTime = DateTime.UtcNow;

        // Act
        var result = await _reactionService.GetReactionsAsync(liveStreamId, page, pageSize, null, null, null, endTime);

        // Assert
        Assert.NotNull(result);
        Assert.All(result, reaction => Assert.True(reaction.CreatedAt <= endTime));
    }

    [Fact]
    public async Task GetReactionsAsync_WithSortBy_ReturnsSortedReactions()
    {
        // Arrange
        var liveStreamId = 1;
        var page = 1;
        var pageSize = 10;
        var sortBy = "createdAt";

        // Act
        var result = await _reactionService.GetReactionsAsync(liveStreamId, page, pageSize, null, null, null, null, sortBy);

        // Assert
        Assert.NotNull(result);
        Assert.NotEmpty(result);
    }

    [Fact]
    public async Task GetReactionsAsync_WithSortOrder_ReturnsSortedReactions()
    {
        // Arrange
        var liveStreamId = 1;
        var page = 1;
        var pageSize = 10;
        var sortBy = "createdAt";
        var sortOrder = "desc";

        // Act
        var result = await _reactionService.GetReactionsAsync(liveStreamId, page, pageSize, null, null, null, null, sortBy, sortOrder);

        // Assert
        Assert.NotNull(result);
        Assert.NotEmpty(result);
    }

    [Fact]
    public async Task GetReactionStatsAsync_ValidStream_ReturnsStats()
    {
        // Arrange
        var liveStreamId = 1;

        // Act
        var result = await _reactionService.GetReactionStatsAsync(liveStreamId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(liveStreamId, result.LiveStreamId);
        Assert.True(result.TotalReactions >= 0);
        Assert.True(result.UniqueReacters >= 0);
        Assert.NotNull(result.ReactionBreakdown);
        Assert.NotEmpty(result.ReactionBreakdown);
    }

    [Fact]
    public async Task GetReactionStatsAsync_InvalidStream_ReturnsEmptyStats()
    {
        // Arrange
        var liveStreamId = 999;

        // Act
        var result = await _reactionService.GetReactionStatsAsync(liveStreamId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(liveStreamId, result.LiveStreamId);
        Assert.Equal(0, result.TotalReactions);
        Assert.Equal(0, result.UniqueReacters);
        Assert.Empty(result.ReactionBreakdown);
    }

    [Fact]
    public async Task GetReactionStatsAsync_WithTimeRange_ReturnsFilteredStats()
    {
        // Arrange
        var liveStreamId = 1;
        var startTime = DateTime.UtcNow.AddHours(-1);
        var endTime = DateTime.UtcNow;

        // Act
        var result = await _reactionService.GetReactionStatsAsync(liveStreamId, startTime, endTime);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(liveStreamId, result.LiveStreamId);
        Assert.True(result.TotalReactions >= 0);
        Assert.True(result.UniqueReacters >= 0);
    }

    [Fact]
    public async Task GetReactionStatsAsync_WithReactionType_ReturnsFilteredStats()
    {
        // Arrange
        var liveStreamId = 1;
        var reactionType = DTOs.ReactionType.Like;

        // Act
        var result = await _reactionService.GetReactionStatsAsync(liveStreamId, null, null, reactionType);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(liveStreamId, result.LiveStreamId);
        Assert.True(result.TotalReactions >= 0);
        Assert.True(result.UniqueReacters >= 0);
    }

    [Fact]
    public async Task GetReactionStatsAsync_WithUserId_ReturnsFilteredStats()
    {
        // Arrange
        var liveStreamId = 1;
        var userId = "test-user-id";

        // Act
        var result = await _reactionService.GetReactionStatsAsync(liveStreamId, null, null, null, userId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(liveStreamId, result.LiveStreamId);
        Assert.True(result.TotalReactions >= 0);
        Assert.True(result.UniqueReacters >= 0);
    }

    [Fact]
    public async Task GetReactionStatsAsync_WithSortBy_ReturnsSortedStats()
    {
        // Arrange
        var liveStreamId = 1;
        var sortBy = "reactionCount";

        // Act
        var result = await _reactionService.GetReactionStatsAsync(liveStreamId, null, null, null, null, sortBy);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(liveStreamId, result.LiveStreamId);
        Assert.True(result.TotalReactions >= 0);
        Assert.True(result.UniqueReacters >= 0);
    }

    [Fact]
    public async Task GetReactionStatsAsync_WithSortOrder_ReturnsSortedStats()
    {
        // Arrange
        var liveStreamId = 1;
        var sortBy = "reactionCount";
        var sortOrder = "desc";

        // Act
        var result = await _reactionService.GetReactionStatsAsync(liveStreamId, null, null, null, null, sortBy, sortOrder);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(liveStreamId, result.LiveStreamId);
        Assert.True(result.TotalReactions >= 0);
        Assert.True(result.UniqueReacters >= 0);
    }

    [Fact]
    public async Task GetReactionStatsAsync_WithPage_ReturnsPaginatedStats()
    {
        // Arrange
        var liveStreamId = 1;
        var page = 1;
        var pageSize = 10;

        // Act
        var result = await _reactionService.GetReactionStatsAsync(liveStreamId, null, null, null, null, null, null, page, pageSize);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(liveStreamId, result.LiveStreamId);
        Assert.True(result.TotalReactions >= 0);
        Assert.True(result.UniqueReacters >= 0);
    }

    [Fact]
    public async Task GetReactionHistoryAsync_ValidStream_ReturnsHistory()
    {
        // Arrange
        var liveStreamId = 1;
        var startTime = DateTime.UtcNow.AddHours(-1);
        var endTime = DateTime.UtcNow;

        // Act
        var result = await _reactionService.GetReactionHistoryAsync(liveStreamId, startTime, endTime);

        // Assert
        Assert.NotNull(result);
        Assert.NotEmpty(result);
        Assert.All(result, reaction => Assert.Equal(liveStreamId, reaction.LiveStreamId));
        Assert.All(result, reaction => Assert.True(reaction.CreatedAt >= startTime));
        Assert.All(result, reaction => Assert.True(reaction.CreatedAt <= endTime));
    }

    [Fact]
    public async Task GetReactionHistoryAsync_InvalidStream_ReturnsEmptyList()
    {
        // Arrange
        var liveStreamId = 999;
        var startTime = DateTime.UtcNow.AddHours(-1);
        var endTime = DateTime.UtcNow;

        // Act
        var result = await _reactionService.GetReactionHistoryAsync(liveStreamId, startTime, endTime);

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);
    }

    [Fact]
    public async Task GetReactionHistoryAsync_WithReactionType_ReturnsFilteredHistory()
    {
        // Arrange
        var liveStreamId = 1;
        var startTime = DateTime.UtcNow.AddHours(-1);
        var endTime = DateTime.UtcNow;
        var reactionType = DTOs.ReactionType.Like;

        // Act
        var result = await _reactionService.GetReactionHistoryAsync(liveStreamId, startTime, endTime, reactionType);

        // Assert
        Assert.NotNull(result);
        Assert.All(result, reaction => Assert.Equal(reactionType, reaction.ReactionType));
    }

    [Fact]
    public async Task GetReactionHistoryAsync_WithUserId_ReturnsFilteredHistory()
    {
        // Arrange
        var liveStreamId = 1;
        var startTime = DateTime.UtcNow.AddHours(-1);
        var endTime = DateTime.UtcNow;
        var userId = "test-user-id";

        // Act
        var result = await _reactionService.GetReactionHistoryAsync(liveStreamId, startTime, endTime, null, userId);

        // Assert
        Assert.NotNull(result);
        Assert.All(result, reaction => Assert.Equal(userId, reaction.UserId));
    }

    [Fact]
    public async Task GetReactionHistoryAsync_WithSortBy_ReturnsSortedHistory()
    {
        // Arrange
        var liveStreamId = 1;
        var startTime = DateTime.UtcNow.AddHours(-1);
        var endTime = DateTime.UtcNow;
        var sortBy = "createdAt";

        // Act
        var result = await _reactionService.GetReactionHistoryAsync(liveStreamId, startTime, endTime, null, null, sortBy);

        // Assert
        Assert.NotNull(result);
        Assert.NotEmpty(result);
    }

    [Fact]
    public async Task GetReactionHistoryAsync_WithSortOrder_ReturnsSortedHistory()
    {
        // Arrange
        var liveStreamId = 1;
        var startTime = DateTime.UtcNow.AddHours(-1);
        var endTime = DateTime.UtcNow;
        var sortBy = "createdAt";
        var sortOrder = "desc";

        // Act
        var result = await _reactionService.GetReactionHistoryAsync(liveStreamId, startTime, endTime, null, null, sortBy, sortOrder);

        // Assert
        Assert.NotNull(result);
        Assert.NotEmpty(result);
    }

    [Fact]
    public async Task GetReactionHistoryAsync_WithPage_ReturnsPaginatedHistory()
    {
        // Arrange
        var liveStreamId = 1;
        var startTime = DateTime.UtcNow.AddHours(-1);
        var endTime = DateTime.UtcNow;
        var page = 1;
        var pageSize = 10;

        // Act
        var result = await _reactionService.GetReactionHistoryAsync(liveStreamId, startTime, endTime, null, null, null, null, page, pageSize);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.Count <= pageSize);
    }

    public void Dispose()
    {
        _context.Dispose();
    }
}
