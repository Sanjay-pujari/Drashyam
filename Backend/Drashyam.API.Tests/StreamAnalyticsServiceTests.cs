using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Drashyam.API.Data;
using Drashyam.API.DTOs;
using Drashyam.API.Models;
using Drashyam.API.Services;
using Drashyam.API.Hubs;
using Microsoft.AspNetCore.SignalR;
using AutoMapper;

namespace Drashyam.API.Tests;

public class StreamAnalyticsServiceTests : IDisposable
{
    private readonly DrashyamDbContext _context;
    private readonly Mock<IMapper> _mockMapper;
    private readonly Mock<IHubContext<LiveStreamHub>> _mockLiveStreamHub;
    private readonly Mock<IHubContext<NotificationHub>> _mockNotificationHub;
    private readonly Mock<ILogger<StreamAnalyticsService>> _mockLogger;
    private readonly StreamAnalyticsService _streamAnalyticsService;

    public StreamAnalyticsServiceTests()
    {
        // Setup in-memory database
        var options = new DbContextOptionsBuilder<DrashyamDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new DrashyamDbContext(options);

        // Setup mocks
        _mockMapper = new Mock<IMapper>();
        _mockLiveStreamHub = new Mock<IHubContext<LiveStreamHub>>();
        _mockNotificationHub = new Mock<IHubContext<NotificationHub>>();
        _mockLogger = new Mock<ILogger<StreamAnalyticsService>>();

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

        _streamAnalyticsService = new StreamAnalyticsService(
            _context,
            _mockMapper.Object,
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
        var analytics = new LiveStreamAnalytics
        {
            LiveStreamId = 1,
            ViewerCount = 100,
            PeakViewerCount = 150,
            TotalViewTime = 1800,
            ChatMessageCount = 50,
            ReactionCount = 25,
            ShareCount = 10,
            Revenue = 100.50m,
            Timestamp = DateTime.UtcNow
        };
        _context.LiveStreamAnalytics.Add(analytics);

        // Add test chat messages
        var chatMessage = new LiveStreamChat
        {
            Id = 1,
            LiveStreamId = 1,
            UserId = "test-user-id",
            Message = "Test message",
            MessageType = DTOs.ChatMessageType.Text,
            CreatedAt = DateTime.UtcNow
        };
        _context.LiveStreamChats.Add(chatMessage);

        // Add test reactions
        var reaction = new LiveStreamReaction
        {
            Id = 1,
            LiveStreamId = 1,
            UserId = "test-user-id",
            ReactionType = DTOs.ReactionType.Like,
            CreatedAt = DateTime.UtcNow
        };
        _context.LiveStreamReactions.Add(reaction);

        _context.SaveChanges();
    }

    [Fact]
    public async Task GetRealTimeAnalyticsAsync_ValidStream_ReturnsAnalytics()
    {
        // Arrange
        var streamId = 1;

        // Act
        var result = await _streamAnalyticsService.GetRealTimeAnalyticsAsync(streamId);

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
            _streamAnalyticsService.GetRealTimeAnalyticsAsync(streamId));
    }

    [Fact]
    public async Task GetStreamHealthAsync_ValidStream_ReturnsHealth()
    {
        // Arrange
        var streamId = 1;

        // Act
        var result = await _streamAnalyticsService.GetStreamHealthAsync(streamId);

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
    public async Task GetViewerAnalyticsAsync_ValidStream_ReturnsViewerData()
    {
        // Arrange
        var streamId = 1;
        var startTime = DateTime.UtcNow.AddHours(-1);
        var endTime = DateTime.UtcNow;

        // Act
        var result = await _streamAnalyticsService.GetViewerAnalyticsAsync(streamId, startTime, endTime);

        // Assert
        Assert.NotNull(result);
        Assert.NotEmpty(result);
        Assert.All(result, item => Assert.True(item.ViewerCount >= 0));
        Assert.All(result, item => Assert.True(item.NewViewers >= 0));
        Assert.All(result, item => Assert.True(item.ReturningViewers >= 0));
    }

    [Fact]
    public async Task GetQualityAnalyticsAsync_ValidStream_ReturnsQualityData()
    {
        // Arrange
        var streamId = 1;
        var startTime = DateTime.UtcNow.AddHours(-1);
        var endTime = DateTime.UtcNow;

        // Act
        var result = await _streamAnalyticsService.GetQualityAnalyticsAsync(streamId, startTime, endTime);

        // Assert
        Assert.NotNull(result);
        Assert.NotEmpty(result);
        Assert.Contains(result, q => q.QualityName == "1080p");
        Assert.Contains(result, q => q.QualityName == "720p");
        Assert.Contains(result, q => q.QualityName == "480p");
    }

    [Fact]
    public async Task RecordViewerJoinAsync_ValidStream_UpdatesAnalytics()
    {
        // Arrange
        var streamId = 1;
        var userId = "test-user-id";

        // Act
        await _streamAnalyticsService.RecordViewerJoinAsync(streamId, userId);

        // Assert
        var analytics = await _context.LiveStreamAnalytics
            .FirstOrDefaultAsync(a => a.LiveStreamId == streamId);
        Assert.NotNull(analytics);
        Assert.True(analytics.ViewerCount > 0);
    }

    [Fact]
    public async Task RecordViewerLeaveAsync_ValidStream_UpdatesAnalytics()
    {
        // Arrange
        var streamId = 1;
        var userId = "test-user-id";

        // Act
        await _streamAnalyticsService.RecordViewerLeaveAsync(streamId, userId);

        // Assert
        var analytics = await _context.LiveStreamAnalytics
            .FirstOrDefaultAsync(a => a.LiveStreamId == streamId);
        Assert.NotNull(analytics);
    }

    [Fact]
    public async Task RecordChatMessageAsync_ValidStream_UpdatesAnalytics()
    {
        // Arrange
        var streamId = 1;
        var userId = "test-user-id";

        // Act
        await _streamAnalyticsService.RecordChatMessageAsync(streamId, userId);

        // Assert
        var analytics = await _context.LiveStreamAnalytics
            .FirstOrDefaultAsync(a => a.LiveStreamId == streamId);
        Assert.NotNull(analytics);
        Assert.True(analytics.ChatMessageCount > 0);
    }

    [Fact]
    public async Task RecordReactionAsync_ValidStream_UpdatesAnalytics()
    {
        // Arrange
        var streamId = 1;
        var userId = "test-user-id";
        var reactionType = "like";

        // Act
        await _streamAnalyticsService.RecordReactionAsync(streamId, userId, reactionType);

        // Assert
        var analytics = await _context.LiveStreamAnalytics
            .FirstOrDefaultAsync(a => a.LiveStreamId == streamId);
        Assert.NotNull(analytics);
        Assert.True(analytics.ReactionCount > 0);
    }

    [Fact]
    public async Task RecordStreamEventAsync_ValidStream_UpdatesAnalytics()
    {
        // Arrange
        var streamId = 1;
        var eventType = "share";
        var eventData = new { count = 1 };

        // Act
        await _streamAnalyticsService.RecordStreamEventAsync(streamId, eventType, eventData);

        // Assert
        var analytics = await _context.LiveStreamAnalytics
            .FirstOrDefaultAsync(a => a.LiveStreamId == streamId);
        Assert.NotNull(analytics);
    }

    [Fact]
    public async Task GenerateStreamReportAsync_ValidStream_ReturnsReport()
    {
        // Arrange
        var streamId = 1;
        var startTime = DateTime.UtcNow.AddHours(-1);
        var endTime = DateTime.UtcNow;

        // Act
        var result = await _streamAnalyticsService.GenerateStreamReportAsync(streamId, startTime, endTime);

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
    public async Task GenerateUserStreamReportsAsync_ValidUser_ReturnsReports()
    {
        // Arrange
        var userId = "test-user-id";
        var startTime = DateTime.UtcNow.AddDays(-7);
        var endTime = DateTime.UtcNow;

        // Act
        var result = await _streamAnalyticsService.GenerateUserStreamReportsAsync(userId, startTime, endTime);

        // Assert
        Assert.NotNull(result);
        Assert.NotEmpty(result);
        Assert.All(result, report => Assert.Equal("Test Stream", report.Title));
    }

    [Fact]
    public async Task CompareStreamsAsync_ValidStreams_ReturnsComparison()
    {
        // Arrange
        var streamIds = new List<int> { 1 };

        // Act
        var result = await _streamAnalyticsService.CompareStreamsAsync(streamIds);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(streamIds, result.StreamIds);
        Assert.NotEmpty(result.ComparisonData);
        Assert.True(result.AverageEngagementRate >= 0);
        Assert.True(result.TotalViewers >= 0);
    }

    [Fact]
    public async Task GetStreamDashboardAsync_ValidStream_ReturnsDashboard()
    {
        // Arrange
        var streamId = 1;

        // Act
        var result = await _streamAnalyticsService.GetStreamDashboardAsync(streamId);

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
    public async Task GetUserDashboardAsync_ValidUser_ReturnsDashboard()
    {
        // Arrange
        var userId = "test-user-id";

        // Act
        var result = await _streamAnalyticsService.GetUserDashboardAsync(userId);

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
    public async Task GetGlobalDashboardAsync_ReturnsDashboard()
    {
        // Act
        var result = await _streamAnalyticsService.GetGlobalDashboardAsync();

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
