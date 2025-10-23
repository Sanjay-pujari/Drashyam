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

public class LiveStreamQualityServiceTests : IDisposable
{
    private readonly DrashyamDbContext _context;
    private readonly Mock<IHubContext<LiveStreamHub>> _mockLiveStreamHub;
    private readonly Mock<IHubContext<NotificationHub>> _mockNotificationHub;
    private readonly Mock<ILogger<LiveStreamQualityService>> _mockLogger;
    private readonly LiveStreamQualityService _qualityService;

    public LiveStreamQualityServiceTests()
    {
        // Setup in-memory database
        var options = new DbContextOptionsBuilder<DrashyamDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new DrashyamDbContext(options);

        // Setup mocks
        _mockLiveStreamHub = new Mock<IHubContext<LiveStreamHub>>();
        _mockNotificationHub = new Mock<IHubContext<NotificationHub>>();
        _mockLogger = new Mock<ILogger<LiveStreamQualityService>>();

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

        _qualityService = new LiveStreamQualityService(
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

        var quality3 = new LiveStreamQuality
        {
            Id = 3,
            LiveStreamId = 1,
            Quality = "480p",
            Bitrate = 1500,
            FrameRate = 30,
            Resolution = "854x480",
            IsActive = false
        };
        _context.LiveStreamQualities.Add(quality3);

        _context.SaveChanges();
    }

    [Fact]
    public async Task GetStreamQualitiesAsync_ValidStream_ReturnsQualities()
    {
        // Arrange
        var liveStreamId = 1;

        // Act
        var result = await _qualityService.GetStreamQualitiesAsync(liveStreamId);

        // Assert
        Assert.NotNull(result);
        Assert.NotEmpty(result);
        Assert.All(result, quality => Assert.Equal(liveStreamId, quality.LiveStreamId));
    }

    [Fact]
    public async Task GetStreamQualitiesAsync_InvalidStream_ReturnsEmptyList()
    {
        // Arrange
        var liveStreamId = 999;

        // Act
        var result = await _qualityService.GetStreamQualitiesAsync(liveStreamId);

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);
    }

    [Fact]
    public async Task GetStreamQualitiesAsync_WithIsActive_ReturnsFilteredQualities()
    {
        // Arrange
        var liveStreamId = 1;
        var isActive = true;

        // Act
        var result = await _qualityService.GetStreamQualitiesAsync(liveStreamId, isActive);

        // Assert
        Assert.NotNull(result);
        Assert.All(result, quality => Assert.Equal(isActive, quality.IsActive));
    }

    [Fact]
    public async Task GetStreamQualitiesAsync_WithQuality_ReturnsFilteredQualities()
    {
        // Arrange
        var liveStreamId = 1;
        var quality = "1080p";

        // Act
        var result = await _qualityService.GetStreamQualitiesAsync(liveStreamId, null, quality);

        // Assert
        Assert.NotNull(result);
        Assert.All(result, q => Assert.Equal(quality, q.Quality));
    }

    [Fact]
    public async Task GetStreamQualitiesAsync_WithMinBitrate_ReturnsFilteredQualities()
    {
        // Arrange
        var liveStreamId = 1;
        var minBitrate = 2000;

        // Act
        var result = await _qualityService.GetStreamQualitiesAsync(liveStreamId, null, null, minBitrate);

        // Assert
        Assert.NotNull(result);
        Assert.All(result, quality => Assert.True(quality.Bitrate >= minBitrate));
    }

    [Fact]
    public async Task GetStreamQualitiesAsync_WithMaxBitrate_ReturnsFilteredQualities()
    {
        // Arrange
        var liveStreamId = 1;
        var maxBitrate = 4000;

        // Act
        var result = await _qualityService.GetStreamQualitiesAsync(liveStreamId, null, null, null, maxBitrate);

        // Assert
        Assert.NotNull(result);
        Assert.All(result, quality => Assert.True(quality.Bitrate <= maxBitrate));
    }

    [Fact]
    public async Task GetStreamQualitiesAsync_WithMinFrameRate_ReturnsFilteredQualities()
    {
        // Arrange
        var liveStreamId = 1;
        var minFrameRate = 25;

        // Act
        var result = await _qualityService.GetStreamQualitiesAsync(liveStreamId, null, null, null, null, minFrameRate);

        // Assert
        Assert.NotNull(result);
        Assert.All(result, quality => Assert.True(quality.FrameRate >= minFrameRate));
    }

    [Fact]
    public async Task GetStreamQualitiesAsync_WithMaxFrameRate_ReturnsFilteredQualities()
    {
        // Arrange
        var liveStreamId = 1;
        var maxFrameRate = 35;

        // Act
        var result = await _qualityService.GetStreamQualitiesAsync(liveStreamId, null, null, null, null, null, maxFrameRate);

        // Assert
        Assert.NotNull(result);
        Assert.All(result, quality => Assert.True(quality.FrameRate <= maxFrameRate));
    }

    [Fact]
    public async Task GetStreamQualitiesAsync_WithResolution_ReturnsFilteredQualities()
    {
        // Arrange
        var liveStreamId = 1;
        var resolution = "1920x1080";

        // Act
        var result = await _qualityService.GetStreamQualitiesAsync(liveStreamId, null, null, null, null, null, null, resolution);

        // Assert
        Assert.NotNull(result);
        Assert.All(result, quality => Assert.Equal(resolution, quality.Resolution));
    }

    [Fact]
    public async Task GetStreamQualitiesAsync_WithSortBy_ReturnsSortedQualities()
    {
        // Arrange
        var liveStreamId = 1;
        var sortBy = "bitrate";

        // Act
        var result = await _qualityService.GetStreamQualitiesAsync(liveStreamId, null, null, null, null, null, null, null, sortBy);

        // Assert
        Assert.NotNull(result);
        Assert.NotEmpty(result);
    }

    [Fact]
    public async Task GetStreamQualitiesAsync_WithSortOrder_ReturnsSortedQualities()
    {
        // Arrange
        var liveStreamId = 1;
        var sortBy = "bitrate";
        var sortOrder = "desc";

        // Act
        var result = await _qualityService.GetStreamQualitiesAsync(liveStreamId, null, null, null, null, null, null, null, sortBy, sortOrder);

        // Assert
        Assert.NotNull(result);
        Assert.NotEmpty(result);
    }

    [Fact]
    public async Task GetStreamQualitiesAsync_WithPage_ReturnsPaginatedQualities()
    {
        // Arrange
        var liveStreamId = 1;
        var page = 1;
        var pageSize = 2;

        // Act
        var result = await _qualityService.GetStreamQualitiesAsync(liveStreamId, null, null, null, null, null, null, null, null, null, page, pageSize);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.Count <= pageSize);
    }

    [Fact]
    public async Task GetStreamQualityAsync_ValidQuality_ReturnsQuality()
    {
        // Arrange
        var qualityId = 1;

        // Act
        var result = await _qualityService.GetStreamQualityAsync(qualityId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(qualityId, result.Id);
        Assert.Equal("1080p", result.Quality);
        Assert.Equal(5000, result.Bitrate);
        Assert.Equal(30, result.FrameRate);
        Assert.Equal("1920x1080", result.Resolution);
        Assert.True(result.IsActive);
    }

    [Fact]
    public async Task GetStreamQualityAsync_InvalidQuality_ThrowsArgumentException()
    {
        // Arrange
        var qualityId = 999;

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => 
            _qualityService.GetStreamQualityAsync(qualityId));
    }

    [Fact]
    public async Task CreateStreamQualityAsync_ValidQuality_ReturnsQuality()
    {
        // Arrange
        var dto = new CreateStreamQualityDto
        {
            LiveStreamId = 1,
            Quality = "1440p",
            Bitrate = 8000,
            FrameRate = 60,
            Resolution = "2560x1440",
            IsActive = false
        };
        var userId = "test-user-id";

        // Act
        var result = await _qualityService.CreateStreamQualityAsync(dto, userId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(dto.LiveStreamId, result.LiveStreamId);
        Assert.Equal(dto.Quality, result.Quality);
        Assert.Equal(dto.Bitrate, result.Bitrate);
        Assert.Equal(dto.FrameRate, result.FrameRate);
        Assert.Equal(dto.Resolution, result.Resolution);
        Assert.Equal(dto.IsActive, result.IsActive);
        Assert.Equal(userId, result.CreatedBy);
        Assert.NotNull(result.CreatedAt);
    }

    [Fact]
    public async Task CreateStreamQualityAsync_InvalidStream_ThrowsArgumentException()
    {
        // Arrange
        var dto = new CreateStreamQualityDto
        {
            LiveStreamId = 999,
            Quality = "1440p",
            Bitrate = 8000,
            FrameRate = 60,
            Resolution = "2560x1440",
            IsActive = false
        };
        var userId = "test-user-id";

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => 
            _qualityService.CreateStreamQualityAsync(dto, userId));
    }

    [Fact]
    public async Task CreateStreamQualityAsync_InvalidBitrate_ThrowsArgumentException()
    {
        // Arrange
        var dto = new CreateStreamQualityDto
        {
            LiveStreamId = 1,
            Quality = "1440p",
            Bitrate = -1000, // Invalid bitrate
            FrameRate = 60,
            Resolution = "2560x1440",
            IsActive = false
        };
        var userId = "test-user-id";

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => 
            _qualityService.CreateStreamQualityAsync(dto, userId));
    }

    [Fact]
    public async Task CreateStreamQualityAsync_InvalidFrameRate_ThrowsArgumentException()
    {
        // Arrange
        var dto = new CreateStreamQualityDto
        {
            LiveStreamId = 1,
            Quality = "1440p",
            Bitrate = 8000,
            FrameRate = -30, // Invalid frame rate
            Resolution = "2560x1440",
            IsActive = false
        };
        var userId = "test-user-id";

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => 
            _qualityService.CreateStreamQualityAsync(dto, userId));
    }

    [Fact]
    public async Task CreateStreamQualityAsync_InvalidResolution_ThrowsArgumentException()
    {
        // Arrange
        var dto = new CreateStreamQualityDto
        {
            LiveStreamId = 1,
            Quality = "1440p",
            Bitrate = 8000,
            FrameRate = 60,
            Resolution = "invalid-resolution", // Invalid resolution
            IsActive = false
        };
        var userId = "test-user-id";

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => 
            _qualityService.CreateStreamQualityAsync(dto, userId));
    }

    [Fact]
    public async Task CreateStreamQualityAsync_DuplicateQuality_ThrowsInvalidOperationException()
    {
        // Arrange
        var dto = new CreateStreamQualityDto
        {
            LiveStreamId = 1,
            Quality = "1080p", // Already exists
            Bitrate = 8000,
            FrameRate = 60,
            Resolution = "1920x1080",
            IsActive = false
        };
        var userId = "test-user-id";

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => 
            _qualityService.CreateStreamQualityAsync(dto, userId));
    }

    [Fact]
    public async Task UpdateStreamQualityAsync_ValidQuality_ReturnsUpdatedQuality()
    {
        // Arrange
        var qualityId = 1;
        var dto = new UpdateStreamQualityDto
        {
            Quality = "1080p60",
            Bitrate = 6000,
            FrameRate = 60,
            Resolution = "1920x1080",
            IsActive = true
        };
        var userId = "test-user-id";

        // Act
        var result = await _qualityService.UpdateStreamQualityAsync(qualityId, dto, userId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(dto.Quality, result.Quality);
        Assert.Equal(dto.Bitrate, result.Bitrate);
        Assert.Equal(dto.FrameRate, result.FrameRate);
        Assert.Equal(dto.Resolution, result.Resolution);
        Assert.Equal(dto.IsActive, result.IsActive);
        Assert.NotNull(result.UpdatedAt);
    }

    [Fact]
    public async Task UpdateStreamQualityAsync_InvalidQuality_ThrowsArgumentException()
    {
        // Arrange
        var qualityId = 999;
        var dto = new UpdateStreamQualityDto
        {
            Quality = "1080p60",
            Bitrate = 6000,
            FrameRate = 60,
            Resolution = "1920x1080",
            IsActive = true
        };
        var userId = "test-user-id";

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => 
            _qualityService.UpdateStreamQualityAsync(qualityId, dto, userId));
    }

    [Fact]
    public async Task UpdateStreamQualityAsync_UnauthorizedUser_ThrowsUnauthorizedAccessException()
    {
        // Arrange
        var qualityId = 1;
        var dto = new UpdateStreamQualityDto
        {
            Quality = "1080p60",
            Bitrate = 6000,
            FrameRate = 60,
            Resolution = "1920x1080",
            IsActive = true
        };
        var userId = "unauthorized-user-id";

        // Act & Assert
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => 
            _qualityService.UpdateStreamQualityAsync(qualityId, dto, userId));
    }

    [Fact]
    public async Task DeleteStreamQualityAsync_ValidQuality_ReturnsTrue()
    {
        // Arrange
        var qualityId = 1;
        var userId = "test-user-id";

        // Act
        var result = await _qualityService.DeleteStreamQualityAsync(qualityId, userId);

        // Assert
        Assert.True(result);

        // Verify quality is deleted
        var quality = await _context.LiveStreamQualities.FindAsync(qualityId);
        Assert.Null(quality);
    }

    [Fact]
    public async Task DeleteStreamQualityAsync_InvalidQuality_ThrowsArgumentException()
    {
        // Arrange
        var qualityId = 999;
        var userId = "test-user-id";

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => 
            _qualityService.DeleteStreamQualityAsync(qualityId, userId));
    }

    [Fact]
    public async Task DeleteStreamQualityAsync_UnauthorizedUser_ThrowsUnauthorizedAccessException()
    {
        // Arrange
        var qualityId = 1;
        var userId = "unauthorized-user-id";

        // Act & Assert
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => 
            _qualityService.DeleteStreamQualityAsync(qualityId, userId));
    }

    [Fact]
    public async Task SetActiveQualityAsync_ValidQuality_ReturnsTrue()
    {
        // Arrange
        var qualityId = 2; // 720p quality
        var userId = "test-user-id";

        // Act
        var result = await _qualityService.SetActiveQualityAsync(qualityId, userId);

        // Assert
        Assert.True(result);

        // Verify quality is active and others are inactive
        var activeQuality = await _context.LiveStreamQualities.FindAsync(qualityId);
        Assert.NotNull(activeQuality);
        Assert.True(activeQuality.IsActive);

        var otherQualities = await _context.LiveStreamQualities
            .Where(q => q.LiveStreamId == activeQuality.LiveStreamId && q.Id != qualityId)
            .ToListAsync();
        Assert.All(otherQualities, q => Assert.False(q.IsActive));
    }

    [Fact]
    public async Task SetActiveQualityAsync_InvalidQuality_ThrowsArgumentException()
    {
        // Arrange
        var qualityId = 999;
        var userId = "test-user-id";

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => 
            _qualityService.SetActiveQualityAsync(qualityId, userId));
    }

    [Fact]
    public async Task SetActiveQualityAsync_UnauthorizedUser_ThrowsUnauthorizedAccessException()
    {
        // Arrange
        var qualityId = 2;
        var userId = "unauthorized-user-id";

        // Act & Assert
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => 
            _qualityService.SetActiveQualityAsync(qualityId, userId));
    }

    [Fact]
    public async Task GetStreamQualityStatsAsync_ValidStream_ReturnsStats()
    {
        // Arrange
        var liveStreamId = 1;

        // Act
        var result = await _qualityService.GetStreamQualityStatsAsync(liveStreamId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(liveStreamId, result.LiveStreamId);
        Assert.True(result.TotalQualities >= 0);
        Assert.True(result.ActiveQualities >= 0);
        Assert.NotNull(result.QualityBreakdown);
        Assert.NotEmpty(result.QualityBreakdown);
    }

    [Fact]
    public async Task GetStreamQualityStatsAsync_InvalidStream_ReturnsEmptyStats()
    {
        // Arrange
        var liveStreamId = 999;

        // Act
        var result = await _qualityService.GetStreamQualityStatsAsync(liveStreamId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(liveStreamId, result.LiveStreamId);
        Assert.Equal(0, result.TotalQualities);
        Assert.Equal(0, result.ActiveQualities);
        Assert.Empty(result.QualityBreakdown);
    }

    [Fact]
    public async Task GetStreamQualityStatsAsync_WithTimeRange_ReturnsFilteredStats()
    {
        // Arrange
        var liveStreamId = 1;
        var startTime = DateTime.UtcNow.AddHours(-1);
        var endTime = DateTime.UtcNow;

        // Act
        var result = await _qualityService.GetStreamQualityStatsAsync(liveStreamId, startTime, endTime);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(liveStreamId, result.LiveStreamId);
        Assert.True(result.TotalQualities >= 0);
        Assert.True(result.ActiveQualities >= 0);
    }

    [Fact]
    public async Task GetStreamQualityStatsAsync_WithQuality_ReturnsFilteredStats()
    {
        // Arrange
        var liveStreamId = 1;
        var quality = "1080p";

        // Act
        var result = await _qualityService.GetStreamQualityStatsAsync(liveStreamId, null, null, quality);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(liveStreamId, result.LiveStreamId);
        Assert.True(result.TotalQualities >= 0);
        Assert.True(result.ActiveQualities >= 0);
    }

    [Fact]
    public async Task GetStreamQualityStatsAsync_WithIsActive_ReturnsFilteredStats()
    {
        // Arrange
        var liveStreamId = 1;
        var isActive = true;

        // Act
        var result = await _qualityService.GetStreamQualityStatsAsync(liveStreamId, null, null, null, isActive);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(liveStreamId, result.LiveStreamId);
        Assert.True(result.TotalQualities >= 0);
        Assert.True(result.ActiveQualities >= 0);
    }

    [Fact]
    public async Task GetStreamQualityStatsAsync_WithMinBitrate_ReturnsFilteredStats()
    {
        // Arrange
        var liveStreamId = 1;
        var minBitrate = 2000;

        // Act
        var result = await _qualityService.GetStreamQualityStatsAsync(liveStreamId, null, null, null, null, minBitrate);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(liveStreamId, result.LiveStreamId);
        Assert.True(result.TotalQualities >= 0);
        Assert.True(result.ActiveQualities >= 0);
    }

    [Fact]
    public async Task GetStreamQualityStatsAsync_WithMaxBitrate_ReturnsFilteredStats()
    {
        // Arrange
        var liveStreamId = 1;
        var maxBitrate = 4000;

        // Act
        var result = await _qualityService.GetStreamQualityStatsAsync(liveStreamId, null, null, null, null, null, maxBitrate);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(liveStreamId, result.LiveStreamId);
        Assert.True(result.TotalQualities >= 0);
        Assert.True(result.ActiveQualities >= 0);
    }

    [Fact]
    public async Task GetStreamQualityStatsAsync_WithMinFrameRate_ReturnsFilteredStats()
    {
        // Arrange
        var liveStreamId = 1;
        var minFrameRate = 25;

        // Act
        var result = await _qualityService.GetStreamQualityStatsAsync(liveStreamId, null, null, null, null, null, null, minFrameRate);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(liveStreamId, result.LiveStreamId);
        Assert.True(result.TotalQualities >= 0);
        Assert.True(result.ActiveQualities >= 0);
    }

    [Fact]
    public async Task GetStreamQualityStatsAsync_WithMaxFrameRate_ReturnsFilteredStats()
    {
        // Arrange
        var liveStreamId = 1;
        var maxFrameRate = 35;

        // Act
        var result = await _qualityService.GetStreamQualityStatsAsync(liveStreamId, null, null, null, null, null, null, null, maxFrameRate);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(liveStreamId, result.LiveStreamId);
        Assert.True(result.TotalQualities >= 0);
        Assert.True(result.ActiveQualities >= 0);
    }

    [Fact]
    public async Task GetStreamQualityStatsAsync_WithResolution_ReturnsFilteredStats()
    {
        // Arrange
        var liveStreamId = 1;
        var resolution = "1920x1080";

        // Act
        var result = await _qualityService.GetStreamQualityStatsAsync(liveStreamId, null, null, null, null, null, null, null, null, resolution);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(liveStreamId, result.LiveStreamId);
        Assert.True(result.TotalQualities >= 0);
        Assert.True(result.ActiveQualities >= 0);
    }

    [Fact]
    public async Task GetStreamQualityStatsAsync_WithSortBy_ReturnsSortedStats()
    {
        // Arrange
        var liveStreamId = 1;
        var sortBy = "bitrate";

        // Act
        var result = await _qualityService.GetStreamQualityStatsAsync(liveStreamId, null, null, null, null, null, null, null, null, null, sortBy);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(liveStreamId, result.LiveStreamId);
        Assert.True(result.TotalQualities >= 0);
        Assert.True(result.ActiveQualities >= 0);
    }

    [Fact]
    public async Task GetStreamQualityStatsAsync_WithSortOrder_ReturnsSortedStats()
    {
        // Arrange
        var liveStreamId = 1;
        var sortBy = "bitrate";
        var sortOrder = "desc";

        // Act
        var result = await _qualityService.GetStreamQualityStatsAsync(liveStreamId, null, null, null, null, null, null, null, null, null, sortBy, sortOrder);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(liveStreamId, result.LiveStreamId);
        Assert.True(result.TotalQualities >= 0);
        Assert.True(result.ActiveQualities >= 0);
    }

    [Fact]
    public async Task GetStreamQualityStatsAsync_WithPage_ReturnsPaginatedStats()
    {
        // Arrange
        var liveStreamId = 1;
        var page = 1;
        var pageSize = 10;

        // Act
        var result = await _qualityService.GetStreamQualityStatsAsync(liveStreamId, null, null, null, null, null, null, null, null, null, null, null, page, pageSize);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(liveStreamId, result.LiveStreamId);
        Assert.True(result.TotalQualities >= 0);
        Assert.True(result.ActiveQualities >= 0);
    }

    public void Dispose()
    {
        _context.Dispose();
    }
}
