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

public class StreamingServiceTests : IDisposable
{
    private readonly DrashyamDbContext _context;
    private readonly Mock<IHubContext<LiveStreamHub>> _mockLiveStreamHub;
    private readonly Mock<ILogger<StreamingService>> _mockLogger;
    private readonly StreamingService _streamingService;

    public StreamingServiceTests()
    {
        // Setup in-memory database
        var options = new DbContextOptionsBuilder<DrashyamDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new DrashyamDbContext(options);

        // Setup mocks
        _mockLiveStreamHub = new Mock<IHubContext<LiveStreamHub>>();
        _mockLogger = new Mock<ILogger<StreamingService>>();

        // Setup hub context mocks
        var mockClients = new Mock<IHubCallerClients>();
        var mockClientProxy = new Mock<IClientProxy>();
        var mockGroupManager = new Mock<IGroupManager>();

        _mockLiveStreamHub.Setup(h => h.Clients).Returns(mockClients.Object);
        _mockLiveStreamHub.Setup(h => h.Groups).Returns(mockGroupManager.Object);

        mockClients.Setup(c => c.Group(It.IsAny<string>())).Returns(mockClientProxy.Object);
        mockClientProxy.Setup(c => c.SendAsync(It.IsAny<string>(), It.IsAny<object>(), default))
            .Returns(Task.CompletedTask);

        _streamingService = new StreamingService(
            _context,
            _mockLogger.Object,
            _mockLiveStreamHub.Object
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

        _context.SaveChanges();
    }

    [Fact]
    public async Task GetStreamConfigurationAsync_ValidStream_ReturnsConfiguration()
    {
        // Arrange
        var liveStreamId = 1;

        // Act
        var result = await _streamingService.GetStreamConfigurationAsync(liveStreamId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(liveStreamId, result.LiveStreamId);
        Assert.Equal("test-stream-key", result.StreamKey);
        Assert.Equal("rtmp://your-ingest-server/live", result.IngestUrl);
        Assert.Equal(DTOs.LiveStreamStatus.Live, result.Status);
        Assert.True(result.IsChatEnabled);
        Assert.True(result.IsMonetizationEnabled);
        Assert.NotEmpty(result.AvailableQualities);
        Assert.Contains("1080p", result.AvailableQualities);
        Assert.Contains("720p", result.AvailableQualities);
    }

    [Fact]
    public async Task GetStreamConfigurationAsync_InvalidStream_ThrowsArgumentException()
    {
        // Arrange
        var liveStreamId = 999;

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => 
            _streamingService.GetStreamConfigurationAsync(liveStreamId));
    }

    [Fact]
    public async Task GetStreamHealthAsync_ValidStream_ReturnsHealth()
    {
        // Arrange
        var liveStreamId = 1;

        // Act
        var result = await _streamingService.GetStreamHealthAsync(liveStreamId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(liveStreamId, result.LiveStreamId);
        Assert.True(result.CpuUsage >= 0);
        Assert.True(result.MemoryUsage >= 0);
        Assert.True(result.NetworkIn >= 0);
        Assert.True(result.NetworkOut >= 0);
        Assert.True(result.Latency >= 0);
        Assert.True(result.FrameRate >= 0);
        Assert.True(result.Bitrate >= 0);
        Assert.NotNull(result.LastUpdated);
    }

    [Fact]
    public async Task GetStreamQualityAsync_ValidStream_ReturnsQuality()
    {
        // Arrange
        var liveStreamId = 1;

        // Act
        var result = await _streamingService.GetStreamQualityAsync(liveStreamId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(liveStreamId, result.LiveStreamId);
        Assert.Equal("1080p", result.Quality);
        Assert.Equal(5000, result.Bitrate);
        Assert.Equal(30, result.FrameRate);
        Assert.Equal("1920x1080", result.Resolution);
        Assert.True(result.IsActive);
    }

    [Fact]
    public async Task StartRecordingAsync_ValidStream_ReturnsRecordingUrl()
    {
        // Arrange
        var liveStreamId = 1;

        // Act
        var result = await _streamingService.StartRecordingAsync(liveStreamId);

        // Assert
        Assert.NotNull(result);
        Assert.Contains("recordings", result);
        Assert.Contains(liveStreamId.ToString(), result);

        // Verify stream is marked as recording
        var stream = await _context.LiveStreams.FindAsync(liveStreamId);
        Assert.True(stream!.IsRecording);
        Assert.NotNull(stream.RecordedUrl);
        Assert.NotNull(stream.ThumbnailUrl);
    }

    [Fact]
    public async Task StartRecordingAsync_InvalidStream_ThrowsArgumentException()
    {
        // Arrange
        var liveStreamId = 999;

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => 
            _streamingService.StartRecordingAsync(liveStreamId));
    }

    [Fact]
    public async Task StartRecordingAsync_NotLiveStream_ThrowsInvalidOperationException()
    {
        // Arrange
        var liveStreamId = 1;
        var stream = await _context.LiveStreams.FindAsync(liveStreamId);
        stream!.Status = DTOs.LiveStreamStatus.Ended;
        await _context.SaveChangesAsync();

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => 
            _streamingService.StartRecordingAsync(liveStreamId));
    }

    [Fact]
    public async Task StopRecordingAsync_ValidStream_ReturnsRecordingUrl()
    {
        // Arrange
        var liveStreamId = 1;
        var stream = await _context.LiveStreams.FindAsync(liveStreamId);
        stream!.IsRecording = true;
        stream.RecordedUrl = "test-recording-url";
        await _context.SaveChangesAsync();

        // Act
        var result = await _streamingService.StopRecordingAsync(liveStreamId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("test-recording-url", result);

        // Verify stream is no longer recording
        var updatedStream = await _context.LiveStreams.FindAsync(liveStreamId);
        Assert.False(updatedStream!.IsRecording);
    }

    [Fact]
    public async Task StopRecordingAsync_InvalidStream_ThrowsArgumentException()
    {
        // Arrange
        var liveStreamId = 999;

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => 
            _streamingService.StopRecordingAsync(liveStreamId));
    }

    [Fact]
    public async Task StopRecordingAsync_NotRecording_ThrowsInvalidOperationException()
    {
        // Arrange
        var liveStreamId = 1;

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => 
            _streamingService.StopRecordingAsync(liveStreamId));
    }

    [Fact]
    public async Task UpdateStreamQualityAsync_ValidStream_ReturnsTrue()
    {
        // Arrange
        var liveStreamId = 1;
        var quality = "720p";

        // Act
        var result = await _streamingService.UpdateStreamQualityAsync(liveStreamId, quality);

        // Assert
        Assert.True(result);

        // Verify quality is updated
        var stream = await _context.LiveStreams
            .Include(ls => ls.StreamQualities)
            .FirstOrDefaultAsync(ls => ls.Id == liveStreamId);
        
        var activeQuality = stream!.StreamQualities.FirstOrDefault(sq => sq.IsActive);
        Assert.NotNull(activeQuality);
        Assert.Equal("720p", activeQuality.Quality);
    }

    [Fact]
    public async Task UpdateStreamQualityAsync_InvalidStream_ThrowsArgumentException()
    {
        // Arrange
        var liveStreamId = 999;
        var quality = "720p";

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => 
            _streamingService.UpdateStreamQualityAsync(liveStreamId, quality));
    }

    [Fact]
    public async Task UpdateStreamQualityAsync_InvalidQuality_ThrowsArgumentException()
    {
        // Arrange
        var liveStreamId = 1;
        var quality = "invalid-quality";

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => 
            _streamingService.UpdateStreamQualityAsync(liveStreamId, quality));
    }

    [Fact]
    public async Task UpdateStreamHealthAsync_ValidStream_ReturnsTrue()
    {
        // Arrange
        var liveStreamId = 1;
        var healthUpdate = new StreamHealthUpdateDto
        {
            Status = StreamHealthStatus.Healthy,
            CpuUsage = 25.5,
            MemoryUsage = 40.2,
            NetworkIn = 10.5,
            NetworkOut = 5.3,
            Latency = 150,
            FrameRate = 30,
            Bitrate = 5000
        };

        // Act
        var result = await _streamingService.UpdateStreamHealthAsync(liveStreamId, healthUpdate);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public async Task PauseStreamAsync_ValidStream_ReturnsTrue()
    {
        // Arrange
        var liveStreamId = 1;

        // Act
        var result = await _streamingService.PauseStreamAsync(liveStreamId);

        // Assert
        Assert.True(result);

        // Verify stream is paused
        var stream = await _context.LiveStreams.FindAsync(liveStreamId);
        Assert.Equal(DTOs.LiveStreamStatus.Paused, stream!.Status);
    }

    [Fact]
    public async Task PauseStreamAsync_InvalidStream_ThrowsArgumentException()
    {
        // Arrange
        var liveStreamId = 999;

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => 
            _streamingService.PauseStreamAsync(liveStreamId));
    }

    [Fact]
    public async Task PauseStreamAsync_NotLiveStream_ThrowsInvalidOperationException()
    {
        // Arrange
        var liveStreamId = 1;
        var stream = await _context.LiveStreams.FindAsync(liveStreamId);
        stream!.Status = DTOs.LiveStreamStatus.Ended;
        await _context.SaveChangesAsync();

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => 
            _streamingService.PauseStreamAsync(liveStreamId));
    }

    [Fact]
    public async Task ResumeStreamAsync_ValidStream_ReturnsTrue()
    {
        // Arrange
        var liveStreamId = 1;
        var stream = await _context.LiveStreams.FindAsync(liveStreamId);
        stream!.Status = DTOs.LiveStreamStatus.Paused;
        await _context.SaveChangesAsync();

        // Act
        var result = await _streamingService.ResumeStreamAsync(liveStreamId);

        // Assert
        Assert.True(result);

        // Verify stream is resumed
        var updatedStream = await _context.LiveStreams.FindAsync(liveStreamId);
        Assert.Equal(DTOs.LiveStreamStatus.Live, updatedStream!.Status);
    }

    [Fact]
    public async Task ResumeStreamAsync_InvalidStream_ThrowsArgumentException()
    {
        // Arrange
        var liveStreamId = 999;

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => 
            _streamingService.ResumeStreamAsync(liveStreamId));
    }

    [Fact]
    public async Task ResumeStreamAsync_NotPausedStream_ThrowsInvalidOperationException()
    {
        // Arrange
        var liveStreamId = 1;

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => 
            _streamingService.ResumeStreamAsync(liveStreamId));
    }

    [Fact]
    public async Task GenerateStreamKeyAsync_ValidStream_ReturnsNewKey()
    {
        // Arrange
        var liveStreamId = 1;

        // Act
        var result = await _streamingService.GenerateStreamKeyAsync(liveStreamId);

        // Assert
        Assert.NotNull(result);
        Assert.NotEqual("test-stream-key", result);
        Assert.True(result.Length > 0);

        // Verify key is updated in database
        var stream = await _context.LiveStreams.FindAsync(liveStreamId);
        Assert.Equal(result, stream!.StreamKey);
    }

    [Fact]
    public async Task GenerateStreamKeyAsync_InvalidStream_ThrowsArgumentException()
    {
        // Arrange
        var liveStreamId = 999;

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => 
            _streamingService.GenerateStreamKeyAsync(liveStreamId));
    }

    [Fact]
    public async Task GetStreamPlaybackUrlAsync_ValidStream_ReturnsUrl()
    {
        // Arrange
        var liveStreamId = 1;

        // Act
        var result = await _streamingService.GetStreamPlaybackUrlAsync(liveStreamId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("https://your-cdn.com/live/test-stream-key", result);
    }

    [Fact]
    public async Task GetStreamPlaybackUrlAsync_InvalidStream_ThrowsArgumentException()
    {
        // Arrange
        var liveStreamId = 999;

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => 
            _streamingService.GetStreamPlaybackUrlAsync(liveStreamId));
    }

    [Fact]
    public async Task GetStreamIngestUrlAsync_ValidStream_ReturnsUrl()
    {
        // Arrange
        var liveStreamId = 1;

        // Act
        var result = await _streamingService.GetStreamIngestUrlAsync(liveStreamId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("rtmp://your-ingest-server/live", result);
    }

    [Fact]
    public async Task GetStreamIngestUrlAsync_InvalidStream_ThrowsArgumentException()
    {
        // Arrange
        var liveStreamId = 999;

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => 
            _streamingService.GetStreamIngestUrlAsync(liveStreamId));
    }

    public void Dispose()
    {
        _context.Dispose();
    }
}