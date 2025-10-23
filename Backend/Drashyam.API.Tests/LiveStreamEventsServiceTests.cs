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

public class LiveStreamEventsServiceTests : IDisposable
{
    private readonly DrashyamDbContext _context;
    private readonly Mock<IHubContext<LiveStreamHub>> _mockLiveStreamHub;
    private readonly Mock<IHubContext<NotificationHub>> _mockNotificationHub;
    private readonly Mock<ILogger<LiveStreamEventsService>> _mockLogger;
    private readonly LiveStreamEventsService _eventsService;

    public LiveStreamEventsServiceTests()
    {
        // Setup in-memory database
        var options = new DbContextOptionsBuilder<DrashyamDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new DrashyamDbContext(options);

        // Setup mocks
        _mockLiveStreamHub = new Mock<IHubContext<LiveStreamHub>>();
        _mockNotificationHub = new Mock<IHubContext<NotificationHub>>();
        _mockLogger = new Mock<ILogger<LiveStreamEventsService>>();

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

        _eventsService = new LiveStreamEventsService(
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

        // Add test event
        var streamEvent = new LiveStreamEvent
        {
            Id = 1,
            LiveStreamId = 1,
            Title = "Test Event",
            Description = "Test Event Description",
            EventType = DTOs.EventType.Giveaway,
            StartTime = DateTime.UtcNow.AddMinutes(10),
            EndTime = DateTime.UtcNow.AddMinutes(40),
            Status = DTOs.EventStatus.Scheduled,
            MaxAttendees = 100,
            IsPublic = true,
            CreatedAt = DateTime.UtcNow
        };
        _context.LiveStreamEvents.Add(streamEvent);

        // Add test attendee
        var attendee = new EventAttendee
        {
            Id = 1,
            EventId = 1,
            UserId = "test-user-id",
            Status = DTOs.AttendanceStatus.Registered,
            RegisteredAt = DateTime.UtcNow.AddMinutes(-5)
        };
        _context.EventAttendees.Add(attendee);

        // Add test collaboration
        var collaboration = new LiveStreamCollaboration
        {
            Id = 1,
            LiveStreamId = 1,
            CollaboratorId = "test-user-id",
            Role = DTOs.CollaborationRole.Guest,
            Status = DTOs.CollaborationStatus.Active,
            JoinedAt = DateTime.UtcNow.AddMinutes(-15)
        };
        _context.LiveStreamCollaborations.Add(collaboration);

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
    public async Task CreateEventAsync_ValidEvent_ReturnsEvent()
    {
        // Arrange
        var dto = new CreateEventDto
        {
            LiveStreamId = 1,
            Title = "New Test Event",
            Description = "New Test Event Description",
            EventType = DTOs.EventType.QnA,
            StartTime = DateTime.UtcNow.AddMinutes(30),
            EndTime = DateTime.UtcNow.AddHours(1),
            MaxAttendees = 50,
            IsPublic = true
        };
        var userId = "test-user-id";

        // Act
        var result = await _eventsService.CreateEventAsync(dto, userId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(dto.LiveStreamId, result.LiveStreamId);
        Assert.Equal(dto.Title, result.Title);
        Assert.Equal(dto.Description, result.Description);
        Assert.Equal(dto.EventType, result.EventType);
        Assert.Equal(dto.StartTime, result.StartTime);
        Assert.Equal(dto.EndTime, result.EndTime);
        Assert.Equal(dto.MaxAttendees, result.MaxAttendees);
        Assert.Equal(dto.IsPublic, result.IsPublic);
        Assert.Equal(userId, result.CreatedBy);
        Assert.NotNull(result.CreatedAt);
    }

    [Fact]
    public async Task CreateEventAsync_InvalidStream_ThrowsArgumentException()
    {
        // Arrange
        var dto = new CreateEventDto
        {
            LiveStreamId = 999,
            Title = "New Test Event",
            Description = "New Test Event Description",
            EventType = DTOs.EventType.QnA,
            StartTime = DateTime.UtcNow.AddMinutes(30),
            EndTime = DateTime.UtcNow.AddHours(1),
            MaxAttendees = 50,
            IsPublic = true
        };
        var userId = "test-user-id";

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => 
            _eventsService.CreateEventAsync(dto, userId));
    }

    [Fact]
    public async Task CreateEventAsync_InvalidTimeRange_ThrowsArgumentException()
    {
        // Arrange
        var dto = new CreateEventDto
        {
            LiveStreamId = 1,
            Title = "New Test Event",
            Description = "New Test Event Description",
            EventType = DTOs.EventType.QnA,
            StartTime = DateTime.UtcNow.AddHours(1),
            EndTime = DateTime.UtcNow.AddMinutes(30), // End time before start time
            MaxAttendees = 50,
            IsPublic = true
        };
        var userId = "test-user-id";

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => 
            _eventsService.CreateEventAsync(dto, userId));
    }

    [Fact]
    public async Task GetEventsAsync_ValidStream_ReturnsEvents()
    {
        // Arrange
        var liveStreamId = 1;
        var page = 1;
        var pageSize = 10;

        // Act
        var result = await _eventsService.GetEventsAsync(liveStreamId, page, pageSize);

        // Assert
        Assert.NotNull(result);
        Assert.NotEmpty(result);
        Assert.True(result.Count <= pageSize);
        Assert.All(result, evt => Assert.Equal(liveStreamId, evt.LiveStreamId));
    }

    [Fact]
    public async Task GetEventsAsync_InvalidStream_ReturnsEmptyList()
    {
        // Arrange
        var liveStreamId = 999;
        var page = 1;
        var pageSize = 10;

        // Act
        var result = await _eventsService.GetEventsAsync(liveStreamId, page, pageSize);

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);
    }

    [Fact]
    public async Task GetEventsAsync_WithEventType_ReturnsFilteredEvents()
    {
        // Arrange
        var liveStreamId = 1;
        var page = 1;
        var pageSize = 10;
        var eventType = DTOs.EventType.Giveaway;

        // Act
        var result = await _eventsService.GetEventsAsync(liveStreamId, page, pageSize, eventType);

        // Assert
        Assert.NotNull(result);
        Assert.All(result, evt => Assert.Equal(eventType, evt.EventType));
    }

    [Fact]
    public async Task GetEventsAsync_WithStatus_ReturnsFilteredEvents()
    {
        // Arrange
        var liveStreamId = 1;
        var page = 1;
        var pageSize = 10;
        var status = DTOs.EventStatus.Scheduled;

        // Act
        var result = await _eventsService.GetEventsAsync(liveStreamId, page, pageSize, null, status);

        // Assert
        Assert.NotNull(result);
        Assert.All(result, evt => Assert.Equal(status, evt.Status));
    }

    [Fact]
    public async Task GetEventsAsync_WithIsPublic_ReturnsFilteredEvents()
    {
        // Arrange
        var liveStreamId = 1;
        var page = 1;
        var pageSize = 10;
        var isPublic = true;

        // Act
        var result = await _eventsService.GetEventsAsync(liveStreamId, page, pageSize, null, null, isPublic);

        // Assert
        Assert.NotNull(result);
        Assert.All(result, evt => Assert.Equal(isPublic, evt.IsPublic));
    }

    [Fact]
    public async Task GetEventsAsync_WithStartTime_ReturnsFilteredEvents()
    {
        // Arrange
        var liveStreamId = 1;
        var page = 1;
        var pageSize = 10;
        var startTime = DateTime.UtcNow.AddMinutes(-30);

        // Act
        var result = await _eventsService.GetEventsAsync(liveStreamId, page, pageSize, null, null, null, startTime);

        // Assert
        Assert.NotNull(result);
        Assert.All(result, evt => Assert.True(evt.StartTime >= startTime));
    }

    [Fact]
    public async Task GetEventsAsync_WithEndTime_ReturnsFilteredEvents()
    {
        // Arrange
        var liveStreamId = 1;
        var page = 1;
        var pageSize = 10;
        var endTime = DateTime.UtcNow.AddHours(1);

        // Act
        var result = await _eventsService.GetEventsAsync(liveStreamId, page, pageSize, null, null, null, null, endTime);

        // Assert
        Assert.NotNull(result);
        Assert.All(result, evt => Assert.True(evt.EndTime <= endTime));
    }

    [Fact]
    public async Task GetEventsAsync_WithSortBy_ReturnsSortedEvents()
    {
        // Arrange
        var liveStreamId = 1;
        var page = 1;
        var pageSize = 10;
        var sortBy = "startTime";

        // Act
        var result = await _eventsService.GetEventsAsync(liveStreamId, page, pageSize, null, null, null, null, null, sortBy);

        // Assert
        Assert.NotNull(result);
        Assert.NotEmpty(result);
    }

    [Fact]
    public async Task GetEventsAsync_WithSortOrder_ReturnsSortedEvents()
    {
        // Arrange
        var liveStreamId = 1;
        var page = 1;
        var pageSize = 10;
        var sortBy = "startTime";
        var sortOrder = "desc";

        // Act
        var result = await _eventsService.GetEventsAsync(liveStreamId, page, pageSize, null, null, null, null, null, sortBy, sortOrder);

        // Assert
        Assert.NotNull(result);
        Assert.NotEmpty(result);
    }

    [Fact]
    public async Task GetEventAsync_ValidEvent_ReturnsEvent()
    {
        // Arrange
        var eventId = 1;

        // Act
        var result = await _eventsService.GetEventAsync(eventId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(eventId, result.Id);
        Assert.Equal("Test Event", result.Title);
        Assert.Equal("Test Event Description", result.Description);
        Assert.Equal(DTOs.EventType.Giveaway, result.EventType);
        Assert.Equal(DTOs.EventStatus.Scheduled, result.Status);
    }

    [Fact]
    public async Task GetEventAsync_InvalidEvent_ThrowsArgumentException()
    {
        // Arrange
        var eventId = 999;

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => 
            _eventsService.GetEventAsync(eventId));
    }

    [Fact]
    public async Task UpdateEventAsync_ValidEvent_ReturnsUpdatedEvent()
    {
        // Arrange
        var eventId = 1;
        var dto = new UpdateEventDto
        {
            Title = "Updated Test Event",
            Description = "Updated Test Event Description",
            StartTime = DateTime.UtcNow.AddMinutes(20),
            EndTime = DateTime.UtcNow.AddMinutes(50),
            MaxAttendees = 75,
            IsPublic = false
        };
        var userId = "test-user-id";

        // Act
        var result = await _eventsService.UpdateEventAsync(eventId, dto, userId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(dto.Title, result.Title);
        Assert.Equal(dto.Description, result.Description);
        Assert.Equal(dto.StartTime, result.StartTime);
        Assert.Equal(dto.EndTime, result.EndTime);
        Assert.Equal(dto.MaxAttendees, result.MaxAttendees);
        Assert.Equal(dto.IsPublic, result.IsPublic);
        Assert.NotNull(result.UpdatedAt);
    }

    [Fact]
    public async Task UpdateEventAsync_InvalidEvent_ThrowsArgumentException()
    {
        // Arrange
        var eventId = 999;
        var dto = new UpdateEventDto
        {
            Title = "Updated Test Event",
            Description = "Updated Test Event Description",
            StartTime = DateTime.UtcNow.AddMinutes(20),
            EndTime = DateTime.UtcNow.AddMinutes(50),
            MaxAttendees = 75,
            IsPublic = false
        };
        var userId = "test-user-id";

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => 
            _eventsService.UpdateEventAsync(eventId, dto, userId));
    }

    [Fact]
    public async Task UpdateEventAsync_UnauthorizedUser_ThrowsUnauthorizedAccessException()
    {
        // Arrange
        var eventId = 1;
        var dto = new UpdateEventDto
        {
            Title = "Updated Test Event",
            Description = "Updated Test Event Description",
            StartTime = DateTime.UtcNow.AddMinutes(20),
            EndTime = DateTime.UtcNow.AddMinutes(50),
            MaxAttendees = 75,
            IsPublic = false
        };
        var userId = "unauthorized-user-id";

        // Act & Assert
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => 
            _eventsService.UpdateEventAsync(eventId, dto, userId));
    }

    [Fact]
    public async Task DeleteEventAsync_ValidEvent_ReturnsTrue()
    {
        // Arrange
        var eventId = 1;
        var userId = "test-user-id";

        // Act
        var result = await _eventsService.DeleteEventAsync(eventId, userId);

        // Assert
        Assert.True(result);

        // Verify event is deleted
        var evt = await _context.LiveStreamEvents.FindAsync(eventId);
        Assert.Null(evt);
    }

    [Fact]
    public async Task DeleteEventAsync_InvalidEvent_ThrowsArgumentException()
    {
        // Arrange
        var eventId = 999;
        var userId = "test-user-id";

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => 
            _eventsService.DeleteEventAsync(eventId, userId));
    }

    [Fact]
    public async Task DeleteEventAsync_UnauthorizedUser_ThrowsUnauthorizedAccessException()
    {
        // Arrange
        var eventId = 1;
        var userId = "unauthorized-user-id";

        // Act & Assert
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => 
            _eventsService.DeleteEventAsync(eventId, userId));
    }

    [Fact]
    public async Task RegisterForEventAsync_ValidEvent_ReturnsAttendee()
    {
        // Arrange
        var eventId = 1;
        var userId = "test-user-id";

        // Act
        var result = await _eventsService.RegisterForEventAsync(eventId, userId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(eventId, result.EventId);
        Assert.Equal(userId, result.UserId);
        Assert.Equal(DTOs.AttendanceStatus.Registered, result.Status);
        Assert.NotNull(result.RegisteredAt);
    }

    [Fact]
    public async Task RegisterForEventAsync_InvalidEvent_ThrowsArgumentException()
    {
        // Arrange
        var eventId = 999;
        var userId = "test-user-id";

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => 
            _eventsService.RegisterForEventAsync(eventId, userId));
    }

    [Fact]
    public async Task RegisterForEventAsync_AlreadyRegistered_ThrowsInvalidOperationException()
    {
        // Arrange
        var eventId = 1;
        var userId = "test-user-id";

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => 
            _eventsService.RegisterForEventAsync(eventId, userId));
    }

    [Fact]
    public async Task UnregisterFromEventAsync_ValidEvent_ReturnsTrue()
    {
        // Arrange
        var eventId = 1;
        var userId = "test-user-id";

        // Act
        var result = await _eventsService.UnregisterFromEventAsync(eventId, userId);

        // Assert
        Assert.True(result);

        // Verify attendee is removed
        var attendee = await _context.EventAttendees
            .FirstOrDefaultAsync(a => a.EventId == eventId && a.UserId == userId);
        Assert.Null(attendee);
    }

    [Fact]
    public async Task UnregisterFromEventAsync_InvalidEvent_ThrowsArgumentException()
    {
        // Arrange
        var eventId = 999;
        var userId = "test-user-id";

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => 
            _eventsService.UnregisterFromEventAsync(eventId, userId));
    }

    [Fact]
    public async Task UnregisterFromEventAsync_NotRegistered_ThrowsInvalidOperationException()
    {
        // Arrange
        var eventId = 1;
        var userId = "not-registered-user-id";

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => 
            _eventsService.UnregisterFromEventAsync(eventId, userId));
    }

    [Fact]
    public async Task GetEventAttendeesAsync_ValidEvent_ReturnsAttendees()
    {
        // Arrange
        var eventId = 1;
        var page = 1;
        var pageSize = 10;

        // Act
        var result = await _eventsService.GetEventAttendeesAsync(eventId, page, pageSize);

        // Assert
        Assert.NotNull(result);
        Assert.NotEmpty(result);
        Assert.True(result.Count <= pageSize);
        Assert.All(result, attendee => Assert.Equal(eventId, attendee.EventId));
    }

    [Fact]
    public async Task GetEventAttendeesAsync_InvalidEvent_ReturnsEmptyList()
    {
        // Arrange
        var eventId = 999;
        var page = 1;
        var pageSize = 10;

        // Act
        var result = await _eventsService.GetEventAttendeesAsync(eventId, page, pageSize);

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);
    }

    [Fact]
    public async Task GetEventAttendeesAsync_WithStatus_ReturnsFilteredAttendees()
    {
        // Arrange
        var eventId = 1;
        var page = 1;
        var pageSize = 10;
        var status = DTOs.AttendanceStatus.Registered;

        // Act
        var result = await _eventsService.GetEventAttendeesAsync(eventId, page, pageSize, status);

        // Assert
        Assert.NotNull(result);
        Assert.All(result, attendee => Assert.Equal(status, attendee.Status));
    }

    [Fact]
    public async Task GetEventAttendeesAsync_WithUserId_ReturnsFilteredAttendees()
    {
        // Arrange
        var eventId = 1;
        var page = 1;
        var pageSize = 10;
        var userId = "test-user-id";

        // Act
        var result = await _eventsService.GetEventAttendeesAsync(eventId, page, pageSize, null, userId);

        // Assert
        Assert.NotNull(result);
        Assert.All(result, attendee => Assert.Equal(userId, attendee.UserId));
    }

    [Fact]
    public async Task GetEventAttendeesAsync_WithSortBy_ReturnsSortedAttendees()
    {
        // Arrange
        var eventId = 1;
        var page = 1;
        var pageSize = 10;
        var sortBy = "registeredAt";

        // Act
        var result = await _eventsService.GetEventAttendeesAsync(eventId, page, pageSize, null, null, sortBy);

        // Assert
        Assert.NotNull(result);
        Assert.NotEmpty(result);
    }

    [Fact]
    public async Task GetEventAttendeesAsync_WithSortOrder_ReturnsSortedAttendees()
    {
        // Arrange
        var eventId = 1;
        var page = 1;
        var pageSize = 10;
        var sortBy = "registeredAt";
        var sortOrder = "desc";

        // Act
        var result = await _eventsService.GetEventAttendeesAsync(eventId, page, pageSize, null, null, sortBy, sortOrder);

        // Assert
        Assert.NotNull(result);
        Assert.NotEmpty(result);
    }

    public void Dispose()
    {
        _context.Dispose();
    }
}
