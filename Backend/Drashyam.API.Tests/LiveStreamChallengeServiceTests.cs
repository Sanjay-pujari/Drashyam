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

public class LiveStreamChallengeServiceTests : IDisposable
{
    private readonly DrashyamDbContext _context;
    private readonly Mock<IHubContext<LiveStreamHub>> _mockLiveStreamHub;
    private readonly Mock<IHubContext<NotificationHub>> _mockNotificationHub;
    private readonly Mock<ILogger<LiveStreamChallengeService>> _mockLogger;
    private readonly LiveStreamChallengeService _challengeService;

    public LiveStreamChallengeServiceTests()
    {
        // Setup in-memory database
        var options = new DbContextOptionsBuilder<DrashyamDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new DrashyamDbContext(options);

        // Setup mocks
        _mockLiveStreamHub = new Mock<IHubContext<LiveStreamHub>>();
        _mockNotificationHub = new Mock<IHubContext<NotificationHub>>();
        _mockLogger = new Mock<ILogger<LiveStreamChallengeService>>();

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

        _challengeService = new LiveStreamChallengeService(
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

        // Add test challenge
        var challenge = new LiveStreamChallenge
        {
            Id = 1,
            LiveStreamId = 1,
            Title = "Test Challenge",
            Description = "Test Challenge Description",
            ChallengeType = DTOs.ChallengeType.Interactive,
            Status = DTOs.ChallengeStatus.Active,
            StartTime = DateTime.UtcNow.AddMinutes(-10),
            EndTime = DateTime.UtcNow.AddMinutes(20),
            MaxParticipants = 50,
            IsPublic = true,
            CreatedAt = DateTime.UtcNow.AddMinutes(-10)
        };
        _context.LiveStreamChallenges.Add(challenge);

        _context.SaveChanges();
    }

    [Fact]
    public async Task CreateChallengeAsync_ValidChallenge_ReturnsChallenge()
    {
        // Arrange
        var dto = new CreateChallengeDto
        {
            LiveStreamId = 1,
            Title = "New Test Challenge",
            Description = "New Test Challenge Description",
            ChallengeType = DTOs.ChallengeType.Interactive,
            StartTime = DateTime.UtcNow.AddMinutes(30),
            EndTime = DateTime.UtcNow.AddHours(1),
            MaxParticipants = 100,
            IsPublic = true
        };
        var userId = "test-user-id";

        // Act
        var result = await _challengeService.CreateChallengeAsync(dto, userId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(dto.LiveStreamId, result.LiveStreamId);
        Assert.Equal(dto.Title, result.Title);
        Assert.Equal(dto.Description, result.Description);
        Assert.Equal(dto.ChallengeType, result.ChallengeType);
        Assert.Equal(dto.StartTime, result.StartTime);
        Assert.Equal(dto.EndTime, result.EndTime);
        Assert.Equal(dto.MaxParticipants, result.MaxParticipants);
        Assert.Equal(dto.IsPublic, result.IsPublic);
        Assert.Equal(userId, result.CreatedBy);
        Assert.NotNull(result.CreatedAt);
    }

    [Fact]
    public async Task CreateChallengeAsync_InvalidStream_ThrowsArgumentException()
    {
        // Arrange
        var dto = new CreateChallengeDto
        {
            LiveStreamId = 999,
            Title = "New Test Challenge",
            Description = "New Test Challenge Description",
            ChallengeType = DTOs.ChallengeType.Interactive,
            StartTime = DateTime.UtcNow.AddMinutes(30),
            EndTime = DateTime.UtcNow.AddHours(1),
            MaxParticipants = 100,
            IsPublic = true
        };
        var userId = "test-user-id";

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => 
            _challengeService.CreateChallengeAsync(dto, userId));
    }

    [Fact]
    public async Task CreateChallengeAsync_InvalidTimeRange_ThrowsArgumentException()
    {
        // Arrange
        var dto = new CreateChallengeDto
        {
            LiveStreamId = 1,
            Title = "New Test Challenge",
            Description = "New Test Challenge Description",
            ChallengeType = DTOs.ChallengeType.Interactive,
            StartTime = DateTime.UtcNow.AddHours(1),
            EndTime = DateTime.UtcNow.AddMinutes(30), // End time before start time
            MaxParticipants = 100,
            IsPublic = true
        };
        var userId = "test-user-id";

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => 
            _challengeService.CreateChallengeAsync(dto, userId));
    }

    [Fact]
    public async Task GetChallengesAsync_ValidStream_ReturnsChallenges()
    {
        // Arrange
        var liveStreamId = 1;
        var page = 1;
        var pageSize = 10;

        // Act
        var result = await _challengeService.GetChallengesAsync(liveStreamId, page, pageSize);

        // Assert
        Assert.NotNull(result);
        Assert.NotEmpty(result);
        Assert.True(result.Count <= pageSize);
        Assert.All(result, challenge => Assert.Equal(liveStreamId, challenge.LiveStreamId));
    }

    [Fact]
    public async Task GetChallengesAsync_InvalidStream_ReturnsEmptyList()
    {
        // Arrange
        var liveStreamId = 999;
        var page = 1;
        var pageSize = 10;

        // Act
        var result = await _challengeService.GetChallengesAsync(liveStreamId, page, pageSize);

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);
    }

    [Fact]
    public async Task GetChallengesAsync_WithChallengeType_ReturnsFilteredChallenges()
    {
        // Arrange
        var liveStreamId = 1;
        var page = 1;
        var pageSize = 10;
        var challengeType = DTOs.ChallengeType.Interactive;

        // Act
        var result = await _challengeService.GetChallengesAsync(liveStreamId, page, pageSize, challengeType);

        // Assert
        Assert.NotNull(result);
        Assert.All(result, challenge => Assert.Equal(challengeType, challenge.ChallengeType));
    }

    [Fact]
    public async Task GetChallengesAsync_WithStatus_ReturnsFilteredChallenges()
    {
        // Arrange
        var liveStreamId = 1;
        var page = 1;
        var pageSize = 10;
        var status = DTOs.ChallengeStatus.Active;

        // Act
        var result = await _challengeService.GetChallengesAsync(liveStreamId, page, pageSize, null, status);

        // Assert
        Assert.NotNull(result);
        Assert.All(result, challenge => Assert.Equal(status, challenge.Status));
    }

    [Fact]
    public async Task GetChallengesAsync_WithIsPublic_ReturnsFilteredChallenges()
    {
        // Arrange
        var liveStreamId = 1;
        var page = 1;
        var pageSize = 10;
        var isPublic = true;

        // Act
        var result = await _challengeService.GetChallengesAsync(liveStreamId, page, pageSize, null, null, isPublic);

        // Assert
        Assert.NotNull(result);
        Assert.All(result, challenge => Assert.Equal(isPublic, challenge.IsPublic));
    }

    [Fact]
    public async Task GetChallengesAsync_WithStartTime_ReturnsFilteredChallenges()
    {
        // Arrange
        var liveStreamId = 1;
        var page = 1;
        var pageSize = 10;
        var startTime = DateTime.UtcNow.AddMinutes(-30);

        // Act
        var result = await _challengeService.GetChallengesAsync(liveStreamId, page, pageSize, null, null, null, startTime);

        // Assert
        Assert.NotNull(result);
        Assert.All(result, challenge => Assert.True(challenge.StartTime >= startTime));
    }

    [Fact]
    public async Task GetChallengesAsync_WithEndTime_ReturnsFilteredChallenges()
    {
        // Arrange
        var liveStreamId = 1;
        var page = 1;
        var pageSize = 10;
        var endTime = DateTime.UtcNow.AddHours(1);

        // Act
        var result = await _challengeService.GetChallengesAsync(liveStreamId, page, pageSize, null, null, null, null, endTime);

        // Assert
        Assert.NotNull(result);
        Assert.All(result, challenge => Assert.True(challenge.EndTime <= endTime));
    }

    [Fact]
    public async Task GetChallengesAsync_WithSortBy_ReturnsSortedChallenges()
    {
        // Arrange
        var liveStreamId = 1;
        var page = 1;
        var pageSize = 10;
        var sortBy = "startTime";

        // Act
        var result = await _challengeService.GetChallengesAsync(liveStreamId, page, pageSize, null, null, null, null, null, sortBy);

        // Assert
        Assert.NotNull(result);
        Assert.NotEmpty(result);
    }

    [Fact]
    public async Task GetChallengesAsync_WithSortOrder_ReturnsSortedChallenges()
    {
        // Arrange
        var liveStreamId = 1;
        var page = 1;
        var pageSize = 10;
        var sortBy = "startTime";
        var sortOrder = "desc";

        // Act
        var result = await _challengeService.GetChallengesAsync(liveStreamId, page, pageSize, null, null, null, null, null, sortBy, sortOrder);

        // Assert
        Assert.NotNull(result);
        Assert.NotEmpty(result);
    }

    [Fact]
    public async Task GetChallengeAsync_ValidChallenge_ReturnsChallenge()
    {
        // Arrange
        var challengeId = 1;

        // Act
        var result = await _challengeService.GetChallengeAsync(challengeId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(challengeId, result.Id);
        Assert.Equal("Test Challenge", result.Title);
        Assert.Equal("Test Challenge Description", result.Description);
        Assert.Equal(DTOs.ChallengeType.Interactive, result.ChallengeType);
        Assert.Equal(DTOs.ChallengeStatus.Active, result.Status);
    }

    [Fact]
    public async Task GetChallengeAsync_InvalidChallenge_ThrowsArgumentException()
    {
        // Arrange
        var challengeId = 999;

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => 
            _challengeService.GetChallengeAsync(challengeId));
    }

    [Fact]
    public async Task UpdateChallengeAsync_ValidChallenge_ReturnsUpdatedChallenge()
    {
        // Arrange
        var challengeId = 1;
        var dto = new UpdateChallengeDto
        {
            Title = "Updated Test Challenge",
            Description = "Updated Test Challenge Description",
            StartTime = DateTime.UtcNow.AddMinutes(20),
            EndTime = DateTime.UtcNow.AddMinutes(50),
            MaxParticipants = 75,
            IsPublic = false
        };
        var userId = "test-user-id";

        // Act
        var result = await _challengeService.UpdateChallengeAsync(challengeId, dto, userId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(dto.Title, result.Title);
        Assert.Equal(dto.Description, result.Description);
        Assert.Equal(dto.StartTime, result.StartTime);
        Assert.Equal(dto.EndTime, result.EndTime);
        Assert.Equal(dto.MaxParticipants, result.MaxParticipants);
        Assert.Equal(dto.IsPublic, result.IsPublic);
        Assert.NotNull(result.UpdatedAt);
    }

    [Fact]
    public async Task UpdateChallengeAsync_InvalidChallenge_ThrowsArgumentException()
    {
        // Arrange
        var challengeId = 999;
        var dto = new UpdateChallengeDto
        {
            Title = "Updated Test Challenge",
            Description = "Updated Test Challenge Description",
            StartTime = DateTime.UtcNow.AddMinutes(20),
            EndTime = DateTime.UtcNow.AddMinutes(50),
            MaxParticipants = 75,
            IsPublic = false
        };
        var userId = "test-user-id";

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => 
            _challengeService.UpdateChallengeAsync(challengeId, dto, userId));
    }

    [Fact]
    public async Task UpdateChallengeAsync_UnauthorizedUser_ThrowsUnauthorizedAccessException()
    {
        // Arrange
        var challengeId = 1;
        var dto = new UpdateChallengeDto
        {
            Title = "Updated Test Challenge",
            Description = "Updated Test Challenge Description",
            StartTime = DateTime.UtcNow.AddMinutes(20),
            EndTime = DateTime.UtcNow.AddMinutes(50),
            MaxParticipants = 75,
            IsPublic = false
        };
        var userId = "unauthorized-user-id";

        // Act & Assert
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => 
            _challengeService.UpdateChallengeAsync(challengeId, dto, userId));
    }

    [Fact]
    public async Task DeleteChallengeAsync_ValidChallenge_ReturnsTrue()
    {
        // Arrange
        var challengeId = 1;
        var userId = "test-user-id";

        // Act
        var result = await _challengeService.DeleteChallengeAsync(challengeId, userId);

        // Assert
        Assert.True(result);

        // Verify challenge is deleted
        var challenge = await _context.LiveStreamChallenges.FindAsync(challengeId);
        Assert.Null(challenge);
    }

    [Fact]
    public async Task DeleteChallengeAsync_InvalidChallenge_ThrowsArgumentException()
    {
        // Arrange
        var challengeId = 999;
        var userId = "test-user-id";

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => 
            _challengeService.DeleteChallengeAsync(challengeId, userId));
    }

    [Fact]
    public async Task DeleteChallengeAsync_UnauthorizedUser_ThrowsUnauthorizedAccessException()
    {
        // Arrange
        var challengeId = 1;
        var userId = "unauthorized-user-id";

        // Act & Assert
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => 
            _challengeService.DeleteChallengeAsync(challengeId, userId));
    }

    [Fact]
    public async Task JoinChallengeAsync_ValidChallenge_ReturnsParticipation()
    {
        // Arrange
        var challengeId = 1;
        var userId = "test-user-id";

        // Act
        var result = await _challengeService.JoinChallengeAsync(challengeId, userId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(challengeId, result.ChallengeId);
        Assert.Equal(userId, result.UserId);
        Assert.Equal(DTOs.ParticipationStatus.Active, result.Status);
        Assert.NotNull(result.JoinedAt);
    }

    [Fact]
    public async Task JoinChallengeAsync_InvalidChallenge_ThrowsArgumentException()
    {
        // Arrange
        var challengeId = 999;
        var userId = "test-user-id";

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => 
            _challengeService.JoinChallengeAsync(challengeId, userId));
    }

    [Fact]
    public async Task JoinChallengeAsync_AlreadyJoined_ThrowsInvalidOperationException()
    {
        // Arrange
        var challengeId = 1;
        var userId = "test-user-id";

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => 
            _challengeService.JoinChallengeAsync(challengeId, userId));
    }

    [Fact]
    public async Task LeaveChallengeAsync_ValidChallenge_ReturnsTrue()
    {
        // Arrange
        var challengeId = 1;
        var userId = "test-user-id";

        // Act
        var result = await _challengeService.LeaveChallengeAsync(challengeId, userId);

        // Assert
        Assert.True(result);

        // Verify participation is removed
        var participation = await _context.ChallengeParticipants
            .FirstOrDefaultAsync(p => p.ChallengeId == challengeId && p.UserId == userId);
        Assert.Null(participation);
    }

    [Fact]
    public async Task LeaveChallengeAsync_InvalidChallenge_ThrowsArgumentException()
    {
        // Arrange
        var challengeId = 999;
        var userId = "test-user-id";

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => 
            _challengeService.LeaveChallengeAsync(challengeId, userId));
    }

    [Fact]
    public async Task LeaveChallengeAsync_NotJoined_ThrowsInvalidOperationException()
    {
        // Arrange
        var challengeId = 1;
        var userId = "not-joined-user-id";

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => 
            _challengeService.LeaveChallengeAsync(challengeId, userId));
    }

    [Fact]
    public async Task GetChallengeParticipantsAsync_ValidChallenge_ReturnsParticipants()
    {
        // Arrange
        var challengeId = 1;
        var page = 1;
        var pageSize = 10;

        // Act
        var result = await _challengeService.GetChallengeParticipantsAsync(challengeId, page, pageSize);

        // Assert
        Assert.NotNull(result);
        Assert.NotEmpty(result);
        Assert.True(result.Count <= pageSize);
        Assert.All(result, participant => Assert.Equal(challengeId, participant.ChallengeId));
    }

    [Fact]
    public async Task GetChallengeParticipantsAsync_InvalidChallenge_ReturnsEmptyList()
    {
        // Arrange
        var challengeId = 999;
        var page = 1;
        var pageSize = 10;

        // Act
        var result = await _challengeService.GetChallengeParticipantsAsync(challengeId, page, pageSize);

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);
    }

    [Fact]
    public async Task GetChallengeParticipantsAsync_WithStatus_ReturnsFilteredParticipants()
    {
        // Arrange
        var challengeId = 1;
        var page = 1;
        var pageSize = 10;
        var status = DTOs.ParticipationStatus.Active;

        // Act
        var result = await _challengeService.GetChallengeParticipantsAsync(challengeId, page, pageSize, status);

        // Assert
        Assert.NotNull(result);
        Assert.All(result, participant => Assert.Equal(status, participant.Status));
    }

    [Fact]
    public async Task GetChallengeParticipantsAsync_WithUserId_ReturnsFilteredParticipants()
    {
        // Arrange
        var challengeId = 1;
        var page = 1;
        var pageSize = 10;
        var userId = "test-user-id";

        // Act
        var result = await _challengeService.GetChallengeParticipantsAsync(challengeId, page, pageSize, null, userId);

        // Assert
        Assert.NotNull(result);
        Assert.All(result, participant => Assert.Equal(userId, participant.UserId));
    }

    [Fact]
    public async Task GetChallengeParticipantsAsync_WithSortBy_ReturnsSortedParticipants()
    {
        // Arrange
        var challengeId = 1;
        var page = 1;
        var pageSize = 10;
        var sortBy = "joinedAt";

        // Act
        var result = await _challengeService.GetChallengeParticipantsAsync(challengeId, page, pageSize, null, null, sortBy);

        // Assert
        Assert.NotNull(result);
        Assert.NotEmpty(result);
    }

    [Fact]
    public async Task GetChallengeParticipantsAsync_WithSortOrder_ReturnsSortedParticipants()
    {
        // Arrange
        var challengeId = 1;
        var page = 1;
        var pageSize = 10;
        var sortBy = "joinedAt";
        var sortOrder = "desc";

        // Act
        var result = await _challengeService.GetChallengeParticipantsAsync(challengeId, page, pageSize, null, null, sortBy, sortOrder);

        // Assert
        Assert.NotNull(result);
        Assert.NotEmpty(result);
    }

    [Fact]
    public async Task GetChallengeStatsAsync_ValidChallenge_ReturnsStats()
    {
        // Arrange
        var challengeId = 1;

        // Act
        var result = await _challengeService.GetChallengeStatsAsync(challengeId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(challengeId, result.ChallengeId);
        Assert.True(result.TotalParticipants >= 0);
        Assert.True(result.ActiveParticipants >= 0);
        Assert.True(result.CompletedParticipants >= 0);
        Assert.NotNull(result.ParticipationBreakdown);
        Assert.NotNull(result.RecentParticipants);
    }

    [Fact]
    public async Task GetChallengeStatsAsync_InvalidChallenge_ReturnsEmptyStats()
    {
        // Arrange
        var challengeId = 999;

        // Act
        var result = await _challengeService.GetChallengeStatsAsync(challengeId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(challengeId, result.ChallengeId);
        Assert.Equal(0, result.TotalParticipants);
        Assert.Equal(0, result.ActiveParticipants);
        Assert.Equal(0, result.CompletedParticipants);
        Assert.Empty(result.ParticipationBreakdown);
        Assert.Empty(result.RecentParticipants);
    }

    [Fact]
    public async Task GetChallengeStatsAsync_WithTimeRange_ReturnsFilteredStats()
    {
        // Arrange
        var challengeId = 1;
        var startTime = DateTime.UtcNow.AddHours(-1);
        var endTime = DateTime.UtcNow;

        // Act
        var result = await _challengeService.GetChallengeStatsAsync(challengeId, startTime, endTime);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(challengeId, result.ChallengeId);
        Assert.True(result.TotalParticipants >= 0);
        Assert.True(result.ActiveParticipants >= 0);
    }

    [Fact]
    public async Task GetChallengeStatsAsync_WithStatus_ReturnsFilteredStats()
    {
        // Arrange
        var challengeId = 1;
        var status = DTOs.ParticipationStatus.Active;

        // Act
        var result = await _challengeService.GetChallengeStatsAsync(challengeId, null, null, status);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(challengeId, result.ChallengeId);
        Assert.True(result.TotalParticipants >= 0);
        Assert.True(result.ActiveParticipants >= 0);
    }

    [Fact]
    public async Task GetChallengeStatsAsync_WithUserId_ReturnsFilteredStats()
    {
        // Arrange
        var challengeId = 1;
        var userId = "test-user-id";

        // Act
        var result = await _challengeService.GetChallengeStatsAsync(challengeId, null, null, null, userId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(challengeId, result.ChallengeId);
        Assert.True(result.TotalParticipants >= 0);
        Assert.True(result.ActiveParticipants >= 0);
    }

    [Fact]
    public async Task GetChallengeStatsAsync_WithSortBy_ReturnsSortedStats()
    {
        // Arrange
        var challengeId = 1;
        var sortBy = "joinedAt";

        // Act
        var result = await _challengeService.GetChallengeStatsAsync(challengeId, null, null, null, null, sortBy);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(challengeId, result.ChallengeId);
        Assert.True(result.TotalParticipants >= 0);
        Assert.True(result.ActiveParticipants >= 0);
    }

    [Fact]
    public async Task GetChallengeStatsAsync_WithSortOrder_ReturnsSortedStats()
    {
        // Arrange
        var challengeId = 1;
        var sortBy = "joinedAt";
        var sortOrder = "desc";

        // Act
        var result = await _challengeService.GetChallengeStatsAsync(challengeId, null, null, null, null, sortBy, sortOrder);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(challengeId, result.ChallengeId);
        Assert.True(result.TotalParticipants >= 0);
        Assert.True(result.ActiveParticipants >= 0);
    }

    [Fact]
    public async Task GetChallengeStatsAsync_WithPage_ReturnsPaginatedStats()
    {
        // Arrange
        var challengeId = 1;
        var page = 1;
        var pageSize = 10;

        // Act
        var result = await _challengeService.GetChallengeStatsAsync(challengeId, null, null, null, null, null, null, page, pageSize);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(challengeId, result.ChallengeId);
        Assert.True(result.TotalParticipants >= 0);
        Assert.True(result.ActiveParticipants >= 0);
    }

    public void Dispose()
    {
        _context.Dispose();
    }
}
