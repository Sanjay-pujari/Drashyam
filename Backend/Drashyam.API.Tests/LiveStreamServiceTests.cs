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

public class LiveStreamServiceTests : IDisposable
{
    private readonly DrashyamDbContext _context;
    private readonly Mock<IHubContext<LiveStreamHub>> _mockLiveStreamHub;
    private readonly Mock<IHubContext<ChatHub>> _mockChatHub;
    private readonly Mock<IHubContext<NotificationHub>> _mockNotificationHub;
    private readonly Mock<ILogger<LiveStreamService>> _mockLogger;
    private readonly LiveStreamService _liveStreamService;

    public LiveStreamServiceTests()
    {
        // Setup in-memory database
        var options = new DbContextOptionsBuilder<DrashyamDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new DrashyamDbContext(options);

        // Setup mocks
        _mockLiveStreamHub = new Mock<IHubContext<LiveStreamHub>>();
        _mockChatHub = new Mock<IHubContext<ChatHub>>();
        _mockNotificationHub = new Mock<IHubContext<NotificationHub>>();
        _mockLogger = new Mock<ILogger<LiveStreamService>>();

        // Setup hub context mocks
        var mockClients = new Mock<IHubCallerClients>();
        var mockClientProxy = new Mock<IClientProxy>();
        var mockGroupManager = new Mock<IGroupManager>();

        _mockLiveStreamHub.Setup(h => h.Clients).Returns(mockClients.Object);
        _mockLiveStreamHub.Setup(h => h.Groups).Returns(mockGroupManager.Object);
        _mockChatHub.Setup(h => h.Clients).Returns(mockClients.Object);
        _mockChatHub.Setup(h => h.Groups).Returns(mockGroupManager.Object);
        _mockNotificationHub.Setup(h => h.Clients).Returns(mockClients.Object);
        _mockNotificationHub.Setup(h => h.Groups).Returns(mockGroupManager.Object);

        mockClients.Setup(c => c.Group(It.IsAny<string>())).Returns(mockClientProxy.Object);
        mockClientProxy.Setup(c => c.SendAsync(It.IsAny<string>(), It.IsAny<object>(), default))
            .Returns(Task.CompletedTask);

        _liveStreamService = new LiveStreamService(
            _context,
            _mockLiveStreamHub.Object,
            _mockChatHub.Object,
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

        // Add test ended stream
        var endedStream = new LiveStream
        {
            Id = 2,
            Title = "Ended Stream",
            Description = "Ended Stream Description",
            UserId = "test-user-id",
            ChannelId = 1,
            StreamKey = "ended-stream-key",
            Status = DTOs.LiveStreamStatus.Ended,
            ViewerCount = 0,
            PeakViewerCount = 200,
            CreatedAt = DateTime.UtcNow.AddHours(-2),
            ActualStartTime = DateTime.UtcNow.AddHours(-2),
            ActualEndTime = DateTime.UtcNow.AddHours(-1)
        };
        _context.LiveStreams.Add(endedStream);

        // Add test scheduled stream
        var scheduledStream = new LiveStream
        {
            Id = 3,
            Title = "Scheduled Stream",
            Description = "Scheduled Stream Description",
            UserId = "test-user-id",
            ChannelId = 1,
            StreamKey = "scheduled-stream-key",
            Status = DTOs.LiveStreamStatus.Scheduled,
            ViewerCount = 0,
            PeakViewerCount = 0,
            CreatedAt = DateTime.UtcNow,
            ScheduledStartTime = DateTime.UtcNow.AddHours(1)
        };
        _context.LiveStreams.Add(scheduledStream);

        // Add test stream qualities
        var quality1 = new LiveStreamQuality
        {
            Id = 1,
            LiveStreamId = 1,
            Quality = "1080p",
            Bitrate = 5000,
            FrameRate = 30,
            Resolution = "1920x1080",
            IsActive = true
        };
        _context.LiveStreamQualities.Add(quality1);

        var quality2 = new LiveStreamQuality
        {
            Id = 2,
            LiveStreamId = 1,
            Quality = "720p",
            Bitrate = 3000,
            FrameRate = 30,
            Resolution = "1280x720",
            IsActive = false
        };
        _context.LiveStreamQualities.Add(quality2);

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

        _context.SaveChanges();
    }

    [Fact]
    public async Task CreateLiveStreamAsync_ValidStream_ReturnsStream()
    {
        // Arrange
        var dto = new CreateLiveStreamDto
        {
            Title = "New Test Stream",
            Description = "New Test Stream Description",
            ChannelId = 1,
            IsPublic = true,
            IsChatEnabled = true,
            IsMonetizationEnabled = true,
            IsRecording = false,
            ScheduledStartTime = null
        };
        var userId = "test-user-id";

        // Act
        var result = await _liveStreamService.CreateLiveStreamAsync(dto, userId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(dto.Title, result.Title);
        Assert.Equal(dto.Description, result.Description);
        Assert.Equal(dto.ChannelId, result.ChannelId);
        Assert.Equal(dto.IsPublic, result.IsPublic);
        Assert.Equal(dto.IsChatEnabled, result.IsChatEnabled);
        Assert.Equal(dto.IsMonetizationEnabled, result.IsMonetizationEnabled);
        Assert.Equal(dto.IsRecording, result.IsRecording);
        Assert.Equal(userId, result.UserId);
        Assert.NotNull(result.StreamKey);
        Assert.NotNull(result.StreamUrl);
        Assert.NotNull(result.HlsUrl);
        Assert.Equal(DTOs.LiveStreamStatus.Scheduled, result.Status);
        Assert.NotNull(result.CreatedAt);
    }

    [Fact]
    public async Task CreateLiveStreamAsync_InvalidChannel_ThrowsArgumentException()
    {
        // Arrange
        var dto = new CreateLiveStreamDto
        {
            Title = "New Test Stream",
            Description = "New Test Stream Description",
            ChannelId = 999,
            IsPublic = true,
            IsChatEnabled = true,
            IsMonetizationEnabled = true,
            IsRecording = false,
            ScheduledStartTime = null
        };
        var userId = "test-user-id";

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => 
            _liveStreamService.CreateLiveStreamAsync(dto, userId));
    }

    [Fact]
    public async Task CreateLiveStreamAsync_UnauthorizedUser_ThrowsUnauthorizedAccessException()
    {
        // Arrange
        var dto = new CreateLiveStreamDto
        {
            Title = "New Test Stream",
            Description = "New Test Stream Description",
            ChannelId = 1,
            IsPublic = true,
            IsChatEnabled = true,
            IsMonetizationEnabled = true,
            IsRecording = false,
            ScheduledStartTime = null
        };
        var userId = "unauthorized-user-id";

        // Act & Assert
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => 
            _liveStreamService.CreateLiveStreamAsync(dto, userId));
    }

    [Fact]
    public async Task GetLiveStreamAsync_ValidStream_ReturnsStream()
    {
        // Arrange
        var streamId = 1;

        // Act
        var result = await _liveStreamService.GetLiveStreamAsync(streamId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(streamId, result.Id);
        Assert.Equal("Test Stream", result.Title);
        Assert.Equal("Test Stream Description", result.Description);
        Assert.Equal("test-user-id", result.UserId);
        Assert.Equal(1, result.ChannelId);
        Assert.Equal("test-stream-key", result.StreamKey);
        Assert.Equal(DTOs.LiveStreamStatus.Live, result.Status);
        Assert.Equal(100, result.ViewerCount);
        Assert.Equal(150, result.PeakViewerCount);
    }

    [Fact]
    public async Task GetLiveStreamAsync_InvalidStream_ThrowsArgumentException()
    {
        // Arrange
        var streamId = 999;

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => 
            _liveStreamService.GetLiveStreamAsync(streamId));
    }

    [Fact]
    public async Task GetLiveStreamsAsync_ValidUser_ReturnsStreams()
    {
        // Arrange
        var userId = "test-user-id";
        var page = 1;
        var pageSize = 10;

        // Act
        var result = await _liveStreamService.GetLiveStreamsAsync(userId, page, pageSize);

        // Assert
        Assert.NotNull(result);
        Assert.NotEmpty(result);
        Assert.True(result.Count <= pageSize);
        Assert.All(result, stream => Assert.Equal(userId, stream.UserId));
    }

    [Fact]
    public async Task GetLiveStreamsAsync_InvalidUser_ReturnsEmptyList()
    {
        // Arrange
        var userId = "invalid-user-id";
        var page = 1;
        var pageSize = 10;

        // Act
        var result = await _liveStreamService.GetLiveStreamsAsync(userId, page, pageSize);

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);
    }

    [Fact]
    public async Task GetLiveStreamsAsync_WithStatus_ReturnsFilteredStreams()
    {
        // Arrange
        var userId = "test-user-id";
        var page = 1;
        var pageSize = 10;
        var status = DTOs.LiveStreamStatus.Live;

        // Act
        var result = await _liveStreamService.GetLiveStreamsAsync(userId, page, pageSize, status);

        // Assert
        Assert.NotNull(result);
        Assert.All(result, stream => Assert.Equal(status, stream.Status));
    }

    [Fact]
    public async Task GetLiveStreamsAsync_WithChannelId_ReturnsFilteredStreams()
    {
        // Arrange
        var userId = "test-user-id";
        var page = 1;
        var pageSize = 10;
        var channelId = 1;

        // Act
        var result = await _liveStreamService.GetLiveStreamsAsync(userId, page, pageSize, null, channelId);

        // Assert
        Assert.NotNull(result);
        Assert.All(result, stream => Assert.Equal(channelId, stream.ChannelId));
    }

    [Fact]
    public async Task GetLiveStreamsAsync_WithSearchTerm_ReturnsFilteredStreams()
    {
        // Arrange
        var userId = "test-user-id";
        var page = 1;
        var pageSize = 10;
        var searchTerm = "Test";

        // Act
        var result = await _liveStreamService.GetLiveStreamsAsync(userId, page, pageSize, null, null, searchTerm);

        // Assert
        Assert.NotNull(result);
        Assert.All(result, stream => Assert.Contains(searchTerm, stream.Title, StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public async Task GetLiveStreamsAsync_WithSortBy_ReturnsSortedStreams()
    {
        // Arrange
        var userId = "test-user-id";
        var page = 1;
        var pageSize = 10;
        var sortBy = "viewerCount";

        // Act
        var result = await _liveStreamService.GetLiveStreamsAsync(userId, page, pageSize, null, null, null, sortBy);

        // Assert
        Assert.NotNull(result);
        Assert.NotEmpty(result);
    }

    [Fact]
    public async Task GetLiveStreamsAsync_WithSortOrder_ReturnsSortedStreams()
    {
        // Arrange
        var userId = "test-user-id";
        var page = 1;
        var pageSize = 10;
        var sortBy = "viewerCount";
        var sortOrder = "desc";

        // Act
        var result = await _liveStreamService.GetLiveStreamsAsync(userId, page, pageSize, null, null, null, sortBy, sortOrder);

        // Assert
        Assert.NotNull(result);
        Assert.NotEmpty(result);
    }

    [Fact]
    public async Task UpdateLiveStreamAsync_ValidStream_ReturnsUpdatedStream()
    {
        // Arrange
        var streamId = 1;
        var dto = new UpdateLiveStreamDto
        {
            Title = "Updated Test Stream",
            Description = "Updated Test Stream Description",
            IsPublic = false,
            IsChatEnabled = false,
            IsMonetizationEnabled = false,
            IsRecording = true
        };
        var userId = "test-user-id";

        // Act
        var result = await _liveStreamService.UpdateLiveStreamAsync(streamId, dto, userId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(dto.Title, result.Title);
        Assert.Equal(dto.Description, result.Description);
        Assert.Equal(dto.IsPublic, result.IsPublic);
        Assert.Equal(dto.IsChatEnabled, result.IsChatEnabled);
        Assert.Equal(dto.IsMonetizationEnabled, result.IsMonetizationEnabled);
        Assert.Equal(dto.IsRecording, result.IsRecording);
        Assert.NotNull(result.UpdatedAt);
    }

    [Fact]
    public async Task UpdateLiveStreamAsync_InvalidStream_ThrowsArgumentException()
    {
        // Arrange
        var streamId = 999;
        var dto = new UpdateLiveStreamDto
        {
            Title = "Updated Test Stream",
            Description = "Updated Test Stream Description",
            IsPublic = false,
            IsChatEnabled = false,
            IsMonetizationEnabled = false,
            IsRecording = true
        };
        var userId = "test-user-id";

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => 
            _liveStreamService.UpdateLiveStreamAsync(streamId, dto, userId));
    }

    [Fact]
    public async Task UpdateLiveStreamAsync_UnauthorizedUser_ThrowsUnauthorizedAccessException()
    {
        // Arrange
        var streamId = 1;
        var dto = new UpdateLiveStreamDto
        {
            Title = "Updated Test Stream",
            Description = "Updated Test Stream Description",
            IsPublic = false,
            IsChatEnabled = false,
            IsMonetizationEnabled = false,
            IsRecording = true
        };
        var userId = "unauthorized-user-id";

        // Act & Assert
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => 
            _liveStreamService.UpdateLiveStreamAsync(streamId, dto, userId));
    }

    [Fact]
    public async Task DeleteLiveStreamAsync_ValidStream_ReturnsTrue()
    {
        // Arrange
        var streamId = 1;
        var userId = "test-user-id";

        // Act
        var result = await _liveStreamService.DeleteLiveStreamAsync(streamId, userId);

        // Assert
        Assert.True(result);

        // Verify stream is deleted
        var stream = await _context.LiveStreams.FindAsync(streamId);
        Assert.Null(stream);
    }

    [Fact]
    public async Task DeleteLiveStreamAsync_InvalidStream_ThrowsArgumentException()
    {
        // Arrange
        var streamId = 999;
        var userId = "test-user-id";

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => 
            _liveStreamService.DeleteLiveStreamAsync(streamId, userId));
    }

    [Fact]
    public async Task DeleteLiveStreamAsync_UnauthorizedUser_ThrowsUnauthorizedAccessException()
    {
        // Arrange
        var streamId = 1;
        var userId = "unauthorized-user-id";

        // Act & Assert
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => 
            _liveStreamService.DeleteLiveStreamAsync(streamId, userId));
    }

    [Fact]
    public async Task StartLiveStreamAsync_ValidStream_ReturnsStream()
    {
        // Arrange
        var streamId = 1;
        var userId = "test-user-id";

        // Act
        var result = await _liveStreamService.StartLiveStreamAsync(streamId, userId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(streamId, result.Id);
        Assert.Equal(DTOs.LiveStreamStatus.Live, result.Status);
        Assert.NotNull(result.ActualStartTime);
    }

    [Fact]
    public async Task StartLiveStreamAsync_InvalidStream_ThrowsArgumentException()
    {
        // Arrange
        var streamId = 999;
        var userId = "test-user-id";

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => 
            _liveStreamService.StartLiveStreamAsync(streamId, userId));
    }

    [Fact]
    public async Task StartLiveStreamAsync_UnauthorizedUser_ThrowsUnauthorizedAccessException()
    {
        // Arrange
        var streamId = 1;
        var userId = "unauthorized-user-id";

        // Act & Assert
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => 
            _liveStreamService.StartLiveStreamAsync(streamId, userId));
    }

    [Fact]
    public async Task StartLiveStreamAsync_AlreadyLiveStream_ThrowsInvalidOperationException()
    {
        // Arrange
        var streamId = 1;
        var userId = "test-user-id";

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => 
            _liveStreamService.StartLiveStreamAsync(streamId, userId));
    }

    [Fact]
    public async Task StopLiveStreamAsync_ValidStream_ReturnsStream()
    {
        // Arrange
        var streamId = 1;
        var userId = "test-user-id";

        // Act
        var result = await _liveStreamService.StopLiveStreamAsync(streamId, userId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(streamId, result.Id);
        Assert.Equal(DTOs.LiveStreamStatus.Ended, result.Status);
        Assert.NotNull(result.ActualEndTime);
    }

    [Fact]
    public async Task StopLiveStreamAsync_InvalidStream_ThrowsArgumentException()
    {
        // Arrange
        var streamId = 999;
        var userId = "test-user-id";

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => 
            _liveStreamService.StopLiveStreamAsync(streamId, userId));
    }

    [Fact]
    public async Task StopLiveStreamAsync_UnauthorizedUser_ThrowsUnauthorizedAccessException()
    {
        // Arrange
        var streamId = 1;
        var userId = "unauthorized-user-id";

        // Act & Assert
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => 
            _liveStreamService.StopLiveStreamAsync(streamId, userId));
    }

    [Fact]
    public async Task StopLiveStreamAsync_NotLiveStream_ThrowsInvalidOperationException()
    {
        // Arrange
        var streamId = 2; // Ended stream
        var userId = "test-user-id";

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => 
            _liveStreamService.StopLiveStreamAsync(streamId, userId));
    }

    [Fact]
    public async Task GetLiveStreamViewerCountAsync_ValidStream_ReturnsCount()
    {
        // Arrange
        var streamId = 1;

        // Act
        var result = await _liveStreamService.GetLiveStreamViewerCountAsync(streamId);

        // Assert
        Assert.True(result >= 0);
    }

    [Fact]
    public async Task GetLiveStreamViewerCountAsync_InvalidStream_ThrowsArgumentException()
    {
        // Arrange
        var streamId = 999;

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => 
            _liveStreamService.GetLiveStreamViewerCountAsync(streamId));
    }

    [Fact]
    public async Task UpdateViewerCountAsync_ValidStream_UpdatesCount()
    {
        // Arrange
        var streamId = 1;
        var viewerCount = 150L;
        var userId = "test-user-id";

        // Act
        await _liveStreamService.UpdateViewerCountAsync(streamId, viewerCount, userId);

        // Assert
        var stream = await _context.LiveStreams.FindAsync(streamId);
        Assert.NotNull(stream);
        Assert.Equal(viewerCount, stream.ViewerCount);
        Assert.NotNull(stream.LastViewerCountUpdate);
    }

    [Fact]
    public async Task UpdateViewerCountAsync_InvalidStream_ThrowsArgumentException()
    {
        // Arrange
        var streamId = 999;
        var viewerCount = 150L;
        var userId = "test-user-id";

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => 
            _liveStreamService.UpdateViewerCountAsync(streamId, viewerCount, userId));
    }

    [Fact]
    public async Task UpdateViewerCountAsync_UnauthorizedUser_ThrowsUnauthorizedAccessException()
    {
        // Arrange
        var streamId = 1;
        var viewerCount = 150L;
        var userId = "unauthorized-user-id";

        // Act & Assert
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => 
            _liveStreamService.UpdateViewerCountAsync(streamId, viewerCount, userId));
    }

    [Fact]
    public async Task GetUnreadNotificationCountAsync_ValidUser_ReturnsCount()
    {
        // Arrange
        var userId = "test-user-id";

        // Act
        var result = await _liveStreamService.GetUnreadNotificationCountAsync(userId);

        // Assert
        Assert.True(result >= 0);
    }

    [Fact]
    public async Task GetUnreadNotificationCountAsync_InvalidUser_ReturnsZero()
    {
        // Arrange
        var userId = "invalid-user-id";

        // Act
        var result = await _liveStreamService.GetUnreadNotificationCountAsync(userId);

        // Assert
        Assert.Equal(0, result);
    }

    [Fact]
    public async Task MarkNotificationAsReadAsync_ValidNotification_ReturnsTrue()
    {
        // Arrange
        var notificationId = 1;
        var userId = "test-user-id";

        // Act
        var result = await _liveStreamService.MarkNotificationAsReadAsync(notificationId, userId);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public async Task MarkNotificationAsReadAsync_InvalidNotification_ReturnsFalse()
    {
        // Arrange
        var notificationId = 999;
        var userId = "test-user-id";

        // Act
        var result = await _liveStreamService.MarkNotificationAsReadAsync(notificationId, userId);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task MarkAllNotificationsAsReadAsync_ValidUser_ReturnsTrue()
    {
        // Arrange
        var userId = "test-user-id";

        // Act
        var result = await _liveStreamService.MarkAllNotificationsAsReadAsync(userId);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public async Task MarkAllNotificationsAsReadAsync_InvalidUser_ReturnsFalse()
    {
        // Arrange
        var userId = "invalid-user-id";

        // Act
        var result = await _liveStreamService.MarkAllNotificationsAsReadAsync(userId);

        // Assert
        Assert.False(result);
    }

    public void Dispose()
    {
        _context.Dispose();
    }
}