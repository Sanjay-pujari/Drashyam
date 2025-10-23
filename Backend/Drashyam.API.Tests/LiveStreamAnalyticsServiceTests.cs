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

public class LiveStreamAnalyticsServiceTests : IDisposable
{
    private readonly DrashyamDbContext _context;
    private readonly Mock<IHubContext<LiveStreamHub>> _mockLiveStreamHub;
    private readonly Mock<IHubContext<NotificationHub>> _mockNotificationHub;
    private readonly Mock<ILogger<LiveStreamAnalyticsService>> _mockLogger;
    private readonly LiveStreamAnalyticsService _analyticsService;

    public LiveStreamAnalyticsServiceTests()
    {
        // Setup in-memory database
        var options = new DbContextOptionsBuilder<DrashyamDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new DrashyamDbContext(options);

        // Setup mocks
        _mockLiveStreamHub = new Mock<IHubContext<LiveStreamHub>>();
        _mockNotificationHub = new Mock<IHubContext<NotificationHub>>();
        _mockLogger = new Mock<ILogger<LiveStreamAnalyticsService>>();

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

        _analyticsService = new LiveStreamAnalyticsService(
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

        // Add test analytics
        var analytics1 = new LiveStreamAnalytics
        {
            LiveStreamId = 1,
            ViewerCount = 100,
            PeakViewerCount = 150,
            TotalViewTime = 1800,
            ChatMessageCount = 50,
            ReactionCount = 25,
            ShareCount = 10,
            Revenue = 100.50m,
            Timestamp = DateTime.UtcNow.AddMinutes(-20)
        };
        _context.LiveStreamAnalytics.Add(analytics1);

        var analytics2 = new LiveStreamAnalytics
        {
            LiveStreamId = 1,
            ViewerCount = 120,
            PeakViewerCount = 150,
            TotalViewTime = 2400,
            ChatMessageCount = 75,
            ReactionCount = 40,
            ShareCount = 15,
            Revenue = 150.75m,
            Timestamp = DateTime.UtcNow.AddMinutes(-10)
        };
        _context.LiveStreamAnalytics.Add(analytics2);

        _context.SaveChanges();
    }

    [Fact]
    public async Task GetRealTimeAnalyticsAsync_ValidStream_ReturnsAnalytics()
    {
        // Arrange
        var streamId = 1;

        // Act
        var result = await _analyticsService.GetRealTimeAnalyticsAsync(streamId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(streamId, result.StreamId);
        Assert.True(result.TotalViewers >= 0);
        Assert.True(result.PeakViewers >= 0);
        Assert.True(result.CurrentViewers >= 0);
        Assert.True(result.TotalChatMessages >= 0);
        Assert.True(result.TotalReactions >= 0);
        Assert.True(result.EngagementRate >= 0);
    }

    [Fact]
    public async Task GetRealTimeAnalyticsAsync_InvalidStream_ThrowsArgumentException()
    {
        // Arrange
        var streamId = 999;

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => 
            _analyticsService.GetRealTimeAnalyticsAsync(streamId));
    }

    [Fact]
    public async Task GetStreamHealthAsync_ValidStream_ReturnsHealth()
    {
        // Arrange
        var streamId = 1;

        // Act
        var result = await _analyticsService.GetStreamHealthAsync(streamId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(streamId, result.StreamId);
        Assert.True(result.CpuUsage >= 0);
        Assert.True(result.MemoryUsage >= 0);
        Assert.True(result.NetworkLatency >= 0);
        Assert.True(result.Bitrate >= 0);
        Assert.True(result.Framerate >= 0);
        Assert.True(result.DroppedFrames >= 0);
        Assert.NotNull(result.LastUpdate);
    }

    [Fact]
    public async Task GetStreamHealthAsync_InvalidStream_ThrowsArgumentException()
    {
        // Arrange
        var streamId = 999;

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => 
            _analyticsService.GetStreamHealthAsync(streamId));
    }

    [Fact]
    public async Task GetViewerAnalyticsAsync_ValidStream_ReturnsViewerData()
    {
        // Arrange
        var streamId = 1;
        var startTime = DateTime.UtcNow.AddHours(-1);
        var endTime = DateTime.UtcNow;

        // Act
        var result = await _analyticsService.GetViewerAnalyticsAsync(streamId, startTime, endTime);

        // Assert
        Assert.NotNull(result);
        Assert.NotEmpty(result);
        Assert.All(result, item => Assert.True(item.ViewerCount >= 0));
        Assert.All(result, item => Assert.True(item.NewViewers >= 0));
        Assert.All(result, item => Assert.True(item.ReturningViewers >= 0));
    }

    [Fact]
    public async Task GetViewerAnalyticsAsync_InvalidStream_ReturnsEmptyList()
    {
        // Arrange
        var streamId = 999;
        var startTime = DateTime.UtcNow.AddHours(-1);
        var endTime = DateTime.UtcNow;

        // Act
        var result = await _analyticsService.GetViewerAnalyticsAsync(streamId, startTime, endTime);

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);
    }

    [Fact]
    public async Task GetViewerAnalyticsAsync_WithTimeRange_ReturnsFilteredData()
    {
        // Arrange
        var streamId = 1;
        var startTime = DateTime.UtcNow.AddMinutes(-30);
        var endTime = DateTime.UtcNow;

        // Act
        var result = await _analyticsService.GetViewerAnalyticsAsync(streamId, startTime, endTime);

        // Assert
        Assert.NotNull(result);
        Assert.All(result, item => Assert.True(item.Timestamp >= startTime));
        Assert.All(result, item => Assert.True(item.Timestamp <= endTime));
    }

    [Fact]
    public async Task GetViewerAnalyticsAsync_WithUserId_ReturnsFilteredData()
    {
        // Arrange
        var streamId = 1;
        var startTime = DateTime.UtcNow.AddHours(-1);
        var endTime = DateTime.UtcNow;
        var userId = "test-user-id";

        // Act
        var result = await _analyticsService.GetViewerAnalyticsAsync(streamId, startTime, endTime, userId);

        // Assert
        Assert.NotNull(result);
        Assert.All(result, item => Assert.Equal(userId, item.UserId));
    }

    [Fact]
    public async Task GetViewerAnalyticsAsync_WithSortBy_ReturnsSortedData()
    {
        // Arrange
        var streamId = 1;
        var startTime = DateTime.UtcNow.AddHours(-1);
        var endTime = DateTime.UtcNow;
        var sortBy = "viewerCount";

        // Act
        var result = await _analyticsService.GetViewerAnalyticsAsync(streamId, startTime, endTime, null, sortBy);

        // Assert
        Assert.NotNull(result);
        Assert.NotEmpty(result);
    }

    [Fact]
    public async Task GetViewerAnalyticsAsync_WithSortOrder_ReturnsSortedData()
    {
        // Arrange
        var streamId = 1;
        var startTime = DateTime.UtcNow.AddHours(-1);
        var endTime = DateTime.UtcNow;
        var sortBy = "viewerCount";
        var sortOrder = "desc";

        // Act
        var result = await _analyticsService.GetViewerAnalyticsAsync(streamId, startTime, endTime, null, sortBy, sortOrder);

        // Assert
        Assert.NotNull(result);
        Assert.NotEmpty(result);
    }

    [Fact]
    public async Task GetViewerAnalyticsAsync_WithPage_ReturnsPaginatedData()
    {
        // Arrange
        var streamId = 1;
        var startTime = DateTime.UtcNow.AddHours(-1);
        var endTime = DateTime.UtcNow;
        var page = 1;
        var pageSize = 10;

        // Act
        var result = await _analyticsService.GetViewerAnalyticsAsync(streamId, startTime, endTime, null, null, null, page, pageSize);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.Count <= pageSize);
    }

    [Fact]
    public async Task GetQualityAnalyticsAsync_ValidStream_ReturnsQualityData()
    {
        // Arrange
        var streamId = 1;
        var startTime = DateTime.UtcNow.AddHours(-1);
        var endTime = DateTime.UtcNow;

        // Act
        var result = await _analyticsService.GetQualityAnalyticsAsync(streamId, startTime, endTime);

        // Assert
        Assert.NotNull(result);
        Assert.NotEmpty(result);
        Assert.Contains(result, q => q.QualityName == "1080p");
        Assert.Contains(result, q => q.QualityName == "720p");
        Assert.Contains(result, q => q.QualityName == "480p");
    }

    [Fact]
    public async Task GetQualityAnalyticsAsync_InvalidStream_ReturnsEmptyList()
    {
        // Arrange
        var streamId = 999;
        var startTime = DateTime.UtcNow.AddHours(-1);
        var endTime = DateTime.UtcNow;

        // Act
        var result = await _analyticsService.GetQualityAnalyticsAsync(streamId, startTime, endTime);

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);
    }

    [Fact]
    public async Task GetQualityAnalyticsAsync_WithTimeRange_ReturnsFilteredData()
    {
        // Arrange
        var streamId = 1;
        var startTime = DateTime.UtcNow.AddMinutes(-30);
        var endTime = DateTime.UtcNow;

        // Act
        var result = await _analyticsService.GetQualityAnalyticsAsync(streamId, startTime, endTime);

        // Assert
        Assert.NotNull(result);
        Assert.All(result, item => Assert.True(item.Timestamp >= startTime));
        Assert.All(result, item => Assert.True(item.Timestamp <= endTime));
    }

    [Fact]
    public async Task GetQualityAnalyticsAsync_WithQuality_ReturnsFilteredData()
    {
        // Arrange
        var streamId = 1;
        var startTime = DateTime.UtcNow.AddHours(-1);
        var endTime = DateTime.UtcNow;
        var quality = "1080p";

        // Act
        var result = await _analyticsService.GetQualityAnalyticsAsync(streamId, startTime, endTime, quality);

        // Assert
        Assert.NotNull(result);
        Assert.All(result, item => Assert.Equal(quality, item.QualityName));
    }

    [Fact]
    public async Task GetQualityAnalyticsAsync_WithUserId_ReturnsFilteredData()
    {
        // Arrange
        var streamId = 1;
        var startTime = DateTime.UtcNow.AddHours(-1);
        var endTime = DateTime.UtcNow;
        var userId = "test-user-id";

        // Act
        var result = await _analyticsService.GetQualityAnalyticsAsync(streamId, startTime, endTime, null, userId);

        // Assert
        Assert.NotNull(result);
        Assert.All(result, item => Assert.Equal(userId, item.UserId));
    }

    [Fact]
    public async Task GetQualityAnalyticsAsync_WithSortBy_ReturnsSortedData()
    {
        // Arrange
        var streamId = 1;
        var startTime = DateTime.UtcNow.AddHours(-1);
        var endTime = DateTime.UtcNow;
        var sortBy = "viewerCount";

        // Act
        var result = await _analyticsService.GetQualityAnalyticsAsync(streamId, startTime, endTime, null, null, sortBy);

        // Assert
        Assert.NotNull(result);
        Assert.NotEmpty(result);
    }

    [Fact]
    public async Task GetQualityAnalyticsAsync_WithSortOrder_ReturnsSortedData()
    {
        // Arrange
        var streamId = 1;
        var startTime = DateTime.UtcNow.AddHours(-1);
        var endTime = DateTime.UtcNow;
        var sortBy = "viewerCount";
        var sortOrder = "desc";

        // Act
        var result = await _analyticsService.GetQualityAnalyticsAsync(streamId, startTime, endTime, null, null, sortBy, sortOrder);

        // Assert
        Assert.NotNull(result);
        Assert.NotEmpty(result);
    }

    [Fact]
    public async Task GetQualityAnalyticsAsync_WithPage_ReturnsPaginatedData()
    {
        // Arrange
        var streamId = 1;
        var startTime = DateTime.UtcNow.AddHours(-1);
        var endTime = DateTime.UtcNow;
        var page = 1;
        var pageSize = 10;

        // Act
        var result = await _analyticsService.GetQualityAnalyticsAsync(streamId, startTime, endTime, null, null, null, null, page, pageSize);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.Count <= pageSize);
    }

    [Fact]
    public async Task RecordViewerJoinAsync_ValidStream_UpdatesAnalytics()
    {
        // Arrange
        var streamId = 1;
        var userId = "test-user-id";

        // Act
        await _analyticsService.RecordViewerJoinAsync(streamId, userId);

        // Assert
        var analytics = await _context.LiveStreamAnalytics
            .FirstOrDefaultAsync(a => a.LiveStreamId == streamId);
        Assert.NotNull(analytics);
        Assert.True(analytics.ViewerCount > 0);
    }

    [Fact]
    public async Task RecordViewerJoinAsync_InvalidStream_ThrowsArgumentException()
    {
        // Arrange
        var streamId = 999;
        var userId = "test-user-id";

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => 
            _analyticsService.RecordViewerJoinAsync(streamId, userId));
    }

    [Fact]
    public async Task RecordViewerLeaveAsync_ValidStream_UpdatesAnalytics()
    {
        // Arrange
        var streamId = 1;
        var userId = "test-user-id";

        // Act
        await _analyticsService.RecordViewerLeaveAsync(streamId, userId);

        // Assert
        var analytics = await _context.LiveStreamAnalytics
            .FirstOrDefaultAsync(a => a.LiveStreamId == streamId);
        Assert.NotNull(analytics);
    }

    [Fact]
    public async Task RecordViewerLeaveAsync_InvalidStream_ThrowsArgumentException()
    {
        // Arrange
        var streamId = 999;
        var userId = "test-user-id";

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => 
            _analyticsService.RecordViewerLeaveAsync(streamId, userId));
    }

    [Fact]
    public async Task RecordChatMessageAsync_ValidStream_UpdatesAnalytics()
    {
        // Arrange
        var streamId = 1;
        var userId = "test-user-id";

        // Act
        await _analyticsService.RecordChatMessageAsync(streamId, userId);

        // Assert
        var analytics = await _context.LiveStreamAnalytics
            .FirstOrDefaultAsync(a => a.LiveStreamId == streamId);
        Assert.NotNull(analytics);
        Assert.True(analytics.ChatMessageCount > 0);
    }

    [Fact]
    public async Task RecordChatMessageAsync_InvalidStream_ThrowsArgumentException()
    {
        // Arrange
        var streamId = 999;
        var userId = "test-user-id";

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => 
            _analyticsService.RecordChatMessageAsync(streamId, userId));
    }

    [Fact]
    public async Task RecordReactionAsync_ValidStream_UpdatesAnalytics()
    {
        // Arrange
        var streamId = 1;
        var userId = "test-user-id";
        var reactionType = "like";

        // Act
        await _analyticsService.RecordReactionAsync(streamId, userId, reactionType);

        // Assert
        var analytics = await _context.LiveStreamAnalytics
            .FirstOrDefaultAsync(a => a.LiveStreamId == streamId);
        Assert.NotNull(analytics);
        Assert.True(analytics.ReactionCount > 0);
    }

    [Fact]
    public async Task RecordReactionAsync_InvalidStream_ThrowsArgumentException()
    {
        // Arrange
        var streamId = 999;
        var userId = "test-user-id";
        var reactionType = "like";

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => 
            _analyticsService.RecordReactionAsync(streamId, userId, reactionType));
    }

    [Fact]
    public async Task RecordStreamEventAsync_ValidStream_UpdatesAnalytics()
    {
        // Arrange
        var streamId = 1;
        var eventType = "share";
        var eventData = new { count = 1 };

        // Act
        await _analyticsService.RecordStreamEventAsync(streamId, eventType, eventData);

        // Assert
        var analytics = await _context.LiveStreamAnalytics
            .FirstOrDefaultAsync(a => a.LiveStreamId == streamId);
        Assert.NotNull(analytics);
    }

    [Fact]
    public async Task RecordStreamEventAsync_InvalidStream_ThrowsArgumentException()
    {
        // Arrange
        var streamId = 999;
        var eventType = "share";
        var eventData = new { count = 1 };

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => 
            _analyticsService.RecordStreamEventAsync(streamId, eventType, eventData));
    }

    [Fact]
    public async Task GenerateStreamReportAsync_ValidStream_ReturnsReport()
    {
        // Arrange
        var streamId = 1;
        var startTime = DateTime.UtcNow.AddHours(-1);
        var endTime = DateTime.UtcNow;

        // Act
        var result = await _analyticsService.GenerateStreamReportAsync(streamId, startTime, endTime);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(streamId, result.StreamId);
        Assert.Equal("Test Stream", result.Title);
        Assert.True(result.TotalViewers >= 0);
        Assert.True(result.PeakViewers >= 0);
        Assert.True(result.TotalChatMessages >= 0);
        Assert.True(result.TotalReactions >= 0);
        Assert.True(result.EngagementRate >= 0);
        Assert.NotEmpty(result.TopCountries);
        Assert.NotEmpty(result.TopDevices);
        Assert.NotEmpty(result.TopBrowsers);
    }

    [Fact]
    public async Task GenerateStreamReportAsync_InvalidStream_ThrowsArgumentException()
    {
        // Arrange
        var streamId = 999;
        var startTime = DateTime.UtcNow.AddHours(-1);
        var endTime = DateTime.UtcNow;

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => 
            _analyticsService.GenerateStreamReportAsync(streamId, startTime, endTime));
    }

    [Fact]
    public async Task GenerateUserStreamReportsAsync_ValidUser_ReturnsReports()
    {
        // Arrange
        var userId = "test-user-id";
        var startTime = DateTime.UtcNow.AddDays(-7);
        var endTime = DateTime.UtcNow;

        // Act
        var result = await _analyticsService.GenerateUserStreamReportsAsync(userId, startTime, endTime);

        // Assert
        Assert.NotNull(result);
        Assert.NotEmpty(result);
        Assert.All(result, report => Assert.Equal("Test Stream", report.Title));
    }

    [Fact]
    public async Task GenerateUserStreamReportsAsync_InvalidUser_ReturnsEmptyList()
    {
        // Arrange
        var userId = "invalid-user-id";
        var startTime = DateTime.UtcNow.AddDays(-7);
        var endTime = DateTime.UtcNow;

        // Act
        var result = await _analyticsService.GenerateUserStreamReportsAsync(userId, startTime, endTime);

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);
    }

    [Fact]
    public async Task CompareStreamsAsync_ValidStreams_ReturnsComparison()
    {
        // Arrange
        var streamIds = new List<int> { 1 };

        // Act
        var result = await _analyticsService.CompareStreamsAsync(streamIds);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(streamIds, result.StreamIds);
        Assert.NotEmpty(result.ComparisonData);
        Assert.True(result.AverageEngagementRate >= 0);
        Assert.True(result.TotalViewers >= 0);
    }

    [Fact]
    public async Task CompareStreamsAsync_InvalidStreams_ThrowsArgumentException()
    {
        // Arrange
        var streamIds = new List<int> { 999 };

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => 
            _analyticsService.CompareStreamsAsync(streamIds));
    }

    [Fact]
    public async Task GetStreamDashboardAsync_ValidStream_ReturnsDashboard()
    {
        // Arrange
        var streamId = 1;

        // Act
        var result = await _analyticsService.GetStreamDashboardAsync(streamId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(streamId, result.Id);
        Assert.Equal("test-user-id", result.UserId);
        Assert.True(result.TotalViews >= 0);
        Assert.True(result.UniqueViews >= 0);
        Assert.True(result.TotalLikes >= 0);
        Assert.True(result.TotalComments >= 0);
        Assert.True(result.EngagementRate >= 0);
    }

    [Fact]
    public async Task GetStreamDashboardAsync_InvalidStream_ThrowsArgumentException()
    {
        // Arrange
        var streamId = 999;

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => 
            _analyticsService.GetStreamDashboardAsync(streamId));
    }

    [Fact]
    public async Task GetUserDashboardAsync_ValidUser_ReturnsDashboard()
    {
        // Arrange
        var userId = "test-user-id";

        // Act
        var result = await _analyticsService.GetUserDashboardAsync(userId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(userId, result.UserId);
        Assert.True(result.TotalViews >= 0);
        Assert.True(result.UniqueViews >= 0);
        Assert.True(result.TotalLikes >= 0);
        Assert.True(result.TotalComments >= 0);
        Assert.True(result.EngagementRate >= 0);
    }

    [Fact]
    public async Task GetUserDashboardAsync_InvalidUser_ReturnsEmptyDashboard()
    {
        // Arrange
        var userId = "invalid-user-id";

        // Act
        var result = await _analyticsService.GetUserDashboardAsync(userId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(userId, result.UserId);
        Assert.Equal(0, result.TotalViews);
        Assert.Equal(0, result.UniqueViews);
        Assert.Equal(0, result.TotalLikes);
        Assert.Equal(0, result.TotalComments);
        Assert.Equal(0, result.EngagementRate);
    }

    [Fact]
    public async Task GetGlobalDashboardAsync_ReturnsDashboard()
    {
        // Act
        var result = await _analyticsService.GetGlobalDashboardAsync();

        // Assert
        Assert.NotNull(result);
        Assert.True(result.TotalViews >= 0);
        Assert.True(result.UniqueViews >= 0);
        Assert.True(result.TotalLikes >= 0);
        Assert.True(result.TotalComments >= 0);
        Assert.True(result.EngagementRate >= 0);
    }

    public void Dispose()
    {
        _context.Dispose();
    }
}
