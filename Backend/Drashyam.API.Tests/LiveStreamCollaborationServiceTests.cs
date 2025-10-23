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

public class LiveStreamCollaborationServiceTests : IDisposable
{
    private readonly DrashyamDbContext _context;
    private readonly Mock<IHubContext<LiveStreamHub>> _mockLiveStreamHub;
    private readonly Mock<IHubContext<NotificationHub>> _mockNotificationHub;
    private readonly Mock<ILogger<LiveStreamCollaborationService>> _mockLogger;
    private readonly LiveStreamCollaborationService _collaborationService;

    public LiveStreamCollaborationServiceTests()
    {
        // Setup in-memory database
        var options = new DbContextOptionsBuilder<DrashyamDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new DrashyamDbContext(options);

        // Setup mocks
        _mockLiveStreamHub = new Mock<IHubContext<LiveStreamHub>>();
        _mockNotificationHub = new Mock<IHubContext<NotificationHub>>();
        _mockLogger = new Mock<ILogger<LiveStreamCollaborationService>>();

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

        _collaborationService = new LiveStreamCollaborationService(
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

        // Add test collaborator
        var collaborator = new ApplicationUser
        {
            Id = "collaborator-user-id",
            UserName = "collaborator",
            Email = "collaborator@example.com",
            FirstName = "Collaborator",
            LastName = "User"
        };
        _context.Users.Add(collaborator);

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

        // Add test collaboration
        var collaboration = new LiveStreamCollaboration
        {
            Id = 1,
            LiveStreamId = 1,
            CollaboratorId = "collaborator-user-id",
            Role = DTOs.CollaborationRole.Guest,
            Status = DTOs.CollaborationStatus.Active,
            JoinedAt = DateTime.UtcNow.AddMinutes(-15)
        };
        _context.LiveStreamCollaborations.Add(collaboration);

        _context.SaveChanges();
    }

    [Fact]
    public async Task InviteCollaboratorAsync_ValidInvitation_ReturnsCollaboration()
    {
        // Arrange
        var dto = new InviteCollaboratorDto
        {
            LiveStreamId = 1,
            CollaboratorId = "collaborator-user-id",
            Role = DTOs.CollaborationRole.Guest,
            Message = "Join my stream!"
        };
        var userId = "test-user-id";

        // Act
        var result = await _collaborationService.InviteCollaboratorAsync(dto, userId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(dto.LiveStreamId, result.LiveStreamId);
        Assert.Equal(dto.CollaboratorId, result.CollaboratorId);
        Assert.Equal(dto.Role, result.Role);
        Assert.Equal(DTOs.CollaborationStatus.Pending, result.Status);
        Assert.Equal(userId, result.InvitedBy);
        Assert.NotNull(result.InvitedAt);
    }

    [Fact]
    public async Task InviteCollaboratorAsync_InvalidStream_ThrowsArgumentException()
    {
        // Arrange
        var dto = new InviteCollaboratorDto
        {
            LiveStreamId = 999,
            CollaboratorId = "collaborator-user-id",
            Role = DTOs.CollaborationRole.Guest,
            Message = "Join my stream!"
        };
        var userId = "test-user-id";

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => 
            _collaborationService.InviteCollaboratorAsync(dto, userId));
    }

    [Fact]
    public async Task InviteCollaboratorAsync_UnauthorizedUser_ThrowsUnauthorizedAccessException()
    {
        // Arrange
        var dto = new InviteCollaboratorDto
        {
            LiveStreamId = 1,
            CollaboratorId = "collaborator-user-id",
            Role = DTOs.CollaborationRole.Guest,
            Message = "Join my stream!"
        };
        var userId = "unauthorized-user-id";

        // Act & Assert
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => 
            _collaborationService.InviteCollaboratorAsync(dto, userId));
    }

    [Fact]
    public async Task InviteCollaboratorAsync_InvalidCollaborator_ThrowsArgumentException()
    {
        // Arrange
        var dto = new InviteCollaboratorDto
        {
            LiveStreamId = 1,
            CollaboratorId = "invalid-user-id",
            Role = DTOs.CollaborationRole.Guest,
            Message = "Join my stream!"
        };
        var userId = "test-user-id";

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => 
            _collaborationService.InviteCollaboratorAsync(dto, userId));
    }

    [Fact]
    public async Task InviteCollaboratorAsync_AlreadyCollaborating_ThrowsInvalidOperationException()
    {
        // Arrange
        var dto = new InviteCollaboratorDto
        {
            LiveStreamId = 1,
            CollaboratorId = "collaborator-user-id",
            Role = DTOs.CollaborationRole.Guest,
            Message = "Join my stream!"
        };
        var userId = "test-user-id";

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => 
            _collaborationService.InviteCollaboratorAsync(dto, userId));
    }

    [Fact]
    public async Task AcceptInvitationAsync_ValidInvitation_ReturnsCollaboration()
    {
        // Arrange
        var collaborationId = 1;
        var userId = "collaborator-user-id";

        // Act
        var result = await _collaborationService.AcceptInvitationAsync(collaborationId, userId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(collaborationId, result.Id);
        Assert.Equal(DTOs.CollaborationStatus.Active, result.Status);
        Assert.NotNull(result.JoinedAt);
    }

    [Fact]
    public async Task AcceptInvitationAsync_InvalidCollaboration_ThrowsArgumentException()
    {
        // Arrange
        var collaborationId = 999;
        var userId = "collaborator-user-id";

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => 
            _collaborationService.AcceptInvitationAsync(collaborationId, userId));
    }

    [Fact]
    public async Task AcceptInvitationAsync_UnauthorizedUser_ThrowsUnauthorizedAccessException()
    {
        // Arrange
        var collaborationId = 1;
        var userId = "unauthorized-user-id";

        // Act & Assert
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => 
            _collaborationService.AcceptInvitationAsync(collaborationId, userId));
    }

    [Fact]
    public async Task AcceptInvitationAsync_NotPending_ThrowsInvalidOperationException()
    {
        // Arrange
        var collaborationId = 1;
        var userId = "collaborator-user-id";

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => 
            _collaborationService.AcceptInvitationAsync(collaborationId, userId));
    }

    [Fact]
    public async Task RejectInvitationAsync_ValidInvitation_ReturnsTrue()
    {
        // Arrange
        var collaborationId = 1;
        var userId = "collaborator-user-id";

        // Act
        var result = await _collaborationService.RejectInvitationAsync(collaborationId, userId);

        // Assert
        Assert.True(result);

        // Verify collaboration is removed
        var collaboration = await _context.LiveStreamCollaborations.FindAsync(collaborationId);
        Assert.Null(collaboration);
    }

    [Fact]
    public async Task RejectInvitationAsync_InvalidCollaboration_ThrowsArgumentException()
    {
        // Arrange
        var collaborationId = 999;
        var userId = "collaborator-user-id";

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => 
            _collaborationService.RejectInvitationAsync(collaborationId, userId));
    }

    [Fact]
    public async Task RejectInvitationAsync_UnauthorizedUser_ThrowsUnauthorizedAccessException()
    {
        // Arrange
        var collaborationId = 1;
        var userId = "unauthorized-user-id";

        // Act & Assert
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => 
            _collaborationService.RejectInvitationAsync(collaborationId, userId));
    }

    [Fact]
    public async Task RejectInvitationAsync_NotPending_ThrowsInvalidOperationException()
    {
        // Arrange
        var collaborationId = 1;
        var userId = "collaborator-user-id";

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => 
            _collaborationService.RejectInvitationAsync(collaborationId, userId));
    }

    [Fact]
    public async Task RemoveCollaboratorAsync_ValidCollaboration_ReturnsTrue()
    {
        // Arrange
        var collaborationId = 1;
        var userId = "test-user-id";

        // Act
        var result = await _collaborationService.RemoveCollaboratorAsync(collaborationId, userId);

        // Assert
        Assert.True(result);

        // Verify collaboration is removed
        var collaboration = await _context.LiveStreamCollaborations.FindAsync(collaborationId);
        Assert.Null(collaboration);
    }

    [Fact]
    public async Task RemoveCollaboratorAsync_InvalidCollaboration_ThrowsArgumentException()
    {
        // Arrange
        var collaborationId = 999;
        var userId = "test-user-id";

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => 
            _collaborationService.RemoveCollaboratorAsync(collaborationId, userId));
    }

    [Fact]
    public async Task RemoveCollaboratorAsync_UnauthorizedUser_ThrowsUnauthorizedAccessException()
    {
        // Arrange
        var collaborationId = 1;
        var userId = "unauthorized-user-id";

        // Act & Assert
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => 
            _collaborationService.RemoveCollaboratorAsync(collaborationId, userId));
    }

    [Fact]
    public async Task LeaveCollaborationAsync_ValidCollaboration_ReturnsTrue()
    {
        // Arrange
        var collaborationId = 1;
        var userId = "collaborator-user-id";

        // Act
        var result = await _collaborationService.LeaveCollaborationAsync(collaborationId, userId);

        // Assert
        Assert.True(result);

        // Verify collaboration is removed
        var collaboration = await _context.LiveStreamCollaborations.FindAsync(collaborationId);
        Assert.Null(collaboration);
    }

    [Fact]
    public async Task LeaveCollaborationAsync_InvalidCollaboration_ThrowsArgumentException()
    {
        // Arrange
        var collaborationId = 999;
        var userId = "collaborator-user-id";

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => 
            _collaborationService.LeaveCollaborationAsync(collaborationId, userId));
    }

    [Fact]
    public async Task LeaveCollaborationAsync_UnauthorizedUser_ThrowsUnauthorizedAccessException()
    {
        // Arrange
        var collaborationId = 1;
        var userId = "unauthorized-user-id";

        // Act & Assert
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => 
            _collaborationService.LeaveCollaborationAsync(collaborationId, userId));
    }

    [Fact]
    public async Task GetCollaborationsAsync_ValidStream_ReturnsCollaborations()
    {
        // Arrange
        var liveStreamId = 1;
        var page = 1;
        var pageSize = 10;

        // Act
        var result = await _collaborationService.GetCollaborationsAsync(liveStreamId, page, pageSize);

        // Assert
        Assert.NotNull(result);
        Assert.NotEmpty(result);
        Assert.True(result.Count <= pageSize);
        Assert.All(result, collaboration => Assert.Equal(liveStreamId, collaboration.LiveStreamId));
    }

    [Fact]
    public async Task GetCollaborationsAsync_InvalidStream_ReturnsEmptyList()
    {
        // Arrange
        var liveStreamId = 999;
        var page = 1;
        var pageSize = 10;

        // Act
        var result = await _collaborationService.GetCollaborationsAsync(liveStreamId, page, pageSize);

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);
    }

    [Fact]
    public async Task GetCollaborationsAsync_WithStatus_ReturnsFilteredCollaborations()
    {
        // Arrange
        var liveStreamId = 1;
        var page = 1;
        var pageSize = 10;
        var status = DTOs.CollaborationStatus.Active;

        // Act
        var result = await _collaborationService.GetCollaborationsAsync(liveStreamId, page, pageSize, status);

        // Assert
        Assert.NotNull(result);
        Assert.All(result, collaboration => Assert.Equal(status, collaboration.Status));
    }

    [Fact]
    public async Task GetCollaborationsAsync_WithRole_ReturnsFilteredCollaborations()
    {
        // Arrange
        var liveStreamId = 1;
        var page = 1;
        var pageSize = 10;
        var role = DTOs.CollaborationRole.Guest;

        // Act
        var result = await _collaborationService.GetCollaborationsAsync(liveStreamId, page, pageSize, null, role);

        // Assert
        Assert.NotNull(result);
        Assert.All(result, collaboration => Assert.Equal(role, collaboration.Role));
    }

    [Fact]
    public async Task GetCollaborationsAsync_WithCollaboratorId_ReturnsFilteredCollaborations()
    {
        // Arrange
        var liveStreamId = 1;
        var page = 1;
        var pageSize = 10;
        var collaboratorId = "collaborator-user-id";

        // Act
        var result = await _collaborationService.GetCollaborationsAsync(liveStreamId, page, pageSize, null, null, collaboratorId);

        // Assert
        Assert.NotNull(result);
        Assert.All(result, collaboration => Assert.Equal(collaboratorId, collaboration.CollaboratorId));
    }

    [Fact]
    public async Task GetCollaborationsAsync_WithSortBy_ReturnsSortedCollaborations()
    {
        // Arrange
        var liveStreamId = 1;
        var page = 1;
        var pageSize = 10;
        var sortBy = "joinedAt";

        // Act
        var result = await _collaborationService.GetCollaborationsAsync(liveStreamId, page, pageSize, null, null, null, sortBy);

        // Assert
        Assert.NotNull(result);
        Assert.NotEmpty(result);
    }

    [Fact]
    public async Task GetCollaborationsAsync_WithSortOrder_ReturnsSortedCollaborations()
    {
        // Arrange
        var liveStreamId = 1;
        var page = 1;
        var pageSize = 10;
        var sortBy = "joinedAt";
        var sortOrder = "desc";

        // Act
        var result = await _collaborationService.GetCollaborationsAsync(liveStreamId, page, pageSize, null, null, null, sortBy, sortOrder);

        // Assert
        Assert.NotNull(result);
        Assert.NotEmpty(result);
    }

    [Fact]
    public async Task GetCollaborationAsync_ValidCollaboration_ReturnsCollaboration()
    {
        // Arrange
        var collaborationId = 1;

        // Act
        var result = await _collaborationService.GetCollaborationAsync(collaborationId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(collaborationId, result.Id);
        Assert.Equal(1, result.LiveStreamId);
        Assert.Equal("collaborator-user-id", result.CollaboratorId);
        Assert.Equal(DTOs.CollaborationRole.Guest, result.Role);
        Assert.Equal(DTOs.CollaborationStatus.Active, result.Status);
    }

    [Fact]
    public async Task GetCollaborationAsync_InvalidCollaboration_ThrowsArgumentException()
    {
        // Arrange
        var collaborationId = 999;

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => 
            _collaborationService.GetCollaborationAsync(collaborationId));
    }

    [Fact]
    public async Task UpdateCollaborationAsync_ValidCollaboration_ReturnsUpdatedCollaboration()
    {
        // Arrange
        var collaborationId = 1;
        var dto = new UpdateCollaborationDto
        {
            Role = DTOs.CollaborationRole.CoHost,
            Status = DTOs.CollaborationStatus.Active
        };
        var userId = "test-user-id";

        // Act
        var result = await _collaborationService.UpdateCollaborationAsync(collaborationId, dto, userId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(dto.Role, result.Role);
        Assert.Equal(dto.Status, result.Status);
        Assert.NotNull(result.UpdatedAt);
    }

    [Fact]
    public async Task UpdateCollaborationAsync_InvalidCollaboration_ThrowsArgumentException()
    {
        // Arrange
        var collaborationId = 999;
        var dto = new UpdateCollaborationDto
        {
            Role = DTOs.CollaborationRole.CoHost,
            Status = DTOs.CollaborationStatus.Active
        };
        var userId = "test-user-id";

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => 
            _collaborationService.UpdateCollaborationAsync(collaborationId, dto, userId));
    }

    [Fact]
    public async Task UpdateCollaborationAsync_UnauthorizedUser_ThrowsUnauthorizedAccessException()
    {
        // Arrange
        var collaborationId = 1;
        var dto = new UpdateCollaborationDto
        {
            Role = DTOs.CollaborationRole.CoHost,
            Status = DTOs.CollaborationStatus.Active
        };
        var userId = "unauthorized-user-id";

        // Act & Assert
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => 
            _collaborationService.UpdateCollaborationAsync(collaborationId, dto, userId));
    }

    [Fact]
    public async Task GetCollaborationStatsAsync_ValidStream_ReturnsStats()
    {
        // Arrange
        var liveStreamId = 1;

        // Act
        var result = await _collaborationService.GetCollaborationStatsAsync(liveStreamId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(liveStreamId, result.LiveStreamId);
        Assert.True(result.TotalCollaborations >= 0);
        Assert.True(result.ActiveCollaborations >= 0);
        Assert.True(result.PendingCollaborations >= 0);
        Assert.NotNull(result.RoleBreakdown);
        Assert.NotNull(result.RecentCollaborations);
    }

    [Fact]
    public async Task GetCollaborationStatsAsync_InvalidStream_ReturnsEmptyStats()
    {
        // Arrange
        var liveStreamId = 999;

        // Act
        var result = await _collaborationService.GetCollaborationStatsAsync(liveStreamId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(liveStreamId, result.LiveStreamId);
        Assert.Equal(0, result.TotalCollaborations);
        Assert.Equal(0, result.ActiveCollaborations);
        Assert.Equal(0, result.PendingCollaborations);
        Assert.Empty(result.RoleBreakdown);
        Assert.Empty(result.RecentCollaborations);
    }

    [Fact]
    public async Task GetCollaborationStatsAsync_WithTimeRange_ReturnsFilteredStats()
    {
        // Arrange
        var liveStreamId = 1;
        var startTime = DateTime.UtcNow.AddHours(-1);
        var endTime = DateTime.UtcNow;

        // Act
        var result = await _collaborationService.GetCollaborationStatsAsync(liveStreamId, startTime, endTime);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(liveStreamId, result.LiveStreamId);
        Assert.True(result.TotalCollaborations >= 0);
        Assert.True(result.ActiveCollaborations >= 0);
    }

    [Fact]
    public async Task GetCollaborationStatsAsync_WithStatus_ReturnsFilteredStats()
    {
        // Arrange
        var liveStreamId = 1;
        var status = DTOs.CollaborationStatus.Active;

        // Act
        var result = await _collaborationService.GetCollaborationStatsAsync(liveStreamId, null, null, status);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(liveStreamId, result.LiveStreamId);
        Assert.True(result.TotalCollaborations >= 0);
        Assert.True(result.ActiveCollaborations >= 0);
    }

    [Fact]
    public async Task GetCollaborationStatsAsync_WithRole_ReturnsFilteredStats()
    {
        // Arrange
        var liveStreamId = 1;
        var role = DTOs.CollaborationRole.Guest;

        // Act
        var result = await _collaborationService.GetCollaborationStatsAsync(liveStreamId, null, null, null, role);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(liveStreamId, result.LiveStreamId);
        Assert.True(result.TotalCollaborations >= 0);
        Assert.True(result.ActiveCollaborations >= 0);
    }

    [Fact]
    public async Task GetCollaborationStatsAsync_WithCollaboratorId_ReturnsFilteredStats()
    {
        // Arrange
        var liveStreamId = 1;
        var collaboratorId = "collaborator-user-id";

        // Act
        var result = await _collaborationService.GetCollaborationStatsAsync(liveStreamId, null, null, null, null, collaboratorId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(liveStreamId, result.LiveStreamId);
        Assert.True(result.TotalCollaborations >= 0);
        Assert.True(result.ActiveCollaborations >= 0);
    }

    [Fact]
    public async Task GetCollaborationStatsAsync_WithSortBy_ReturnsSortedStats()
    {
        // Arrange
        var liveStreamId = 1;
        var sortBy = "joinedAt";

        // Act
        var result = await _collaborationService.GetCollaborationStatsAsync(liveStreamId, null, null, null, null, null, sortBy);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(liveStreamId, result.LiveStreamId);
        Assert.True(result.TotalCollaborations >= 0);
        Assert.True(result.ActiveCollaborations >= 0);
    }

    [Fact]
    public async Task GetCollaborationStatsAsync_WithSortOrder_ReturnsSortedStats()
    {
        // Arrange
        var liveStreamId = 1;
        var sortBy = "joinedAt";
        var sortOrder = "desc";

        // Act
        var result = await _collaborationService.GetCollaborationStatsAsync(liveStreamId, null, null, null, null, null, sortBy, sortOrder);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(liveStreamId, result.LiveStreamId);
        Assert.True(result.TotalCollaborations >= 0);
        Assert.True(result.ActiveCollaborations >= 0);
    }

    [Fact]
    public async Task GetCollaborationStatsAsync_WithPage_ReturnsPaginatedStats()
    {
        // Arrange
        var liveStreamId = 1;
        var page = 1;
        var pageSize = 10;

        // Act
        var result = await _collaborationService.GetCollaborationStatsAsync(liveStreamId, null, null, null, null, null, null, null, page, pageSize);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(liveStreamId, result.LiveStreamId);
        Assert.True(result.TotalCollaborations >= 0);
        Assert.True(result.ActiveCollaborations >= 0);
    }

    public void Dispose()
    {
        _context.Dispose();
    }
}
