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

public class LiveStreamChatServiceTests : IDisposable
{
    private readonly DrashyamDbContext _context;
    private readonly Mock<IHubContext<ChatHub>> _mockChatHub;
    private readonly Mock<ILogger<LiveStreamChatService>> _mockLogger;
    private readonly LiveStreamChatService _chatService;

    public LiveStreamChatServiceTests()
    {
        // Setup in-memory database
        var options = new DbContextOptionsBuilder<DrashyamDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new DrashyamDbContext(options);

        // Setup mocks
        _mockChatHub = new Mock<IHubContext<ChatHub>>();
        _mockLogger = new Mock<ILogger<LiveStreamChatService>>();

        // Setup hub context mocks
        var mockClients = new Mock<IHubCallerClients>();
        var mockClientProxy = new Mock<IClientProxy>();
        var mockGroupManager = new Mock<IGroupManager>();

        _mockChatHub.Setup(h => h.Clients).Returns(mockClients.Object);
        _mockChatHub.Setup(h => h.Groups).Returns(mockGroupManager.Object);

        mockClients.Setup(c => c.Group(It.IsAny<string>())).Returns(mockClientProxy.Object);
        mockClientProxy.Setup(c => c.SendAsync(It.IsAny<string>(), It.IsAny<object>(), default))
            .Returns(Task.CompletedTask);

        _chatService = new LiveStreamChatService(
            _context,
            _mockChatHub.Object,
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

        // Add test chat messages
        var chatMessage1 = new LiveStreamChat
        {
            Id = 1,
            LiveStreamId = 1,
            UserId = "test-user-id",
            Message = "Hello everyone!",
            MessageType = DTOs.ChatMessageType.Text,
            CreatedAt = DateTime.UtcNow.AddMinutes(-10)
        };
        _context.LiveStreamChats.Add(chatMessage1);

        var chatMessage2 = new LiveStreamChat
        {
            Id = 2,
            LiveStreamId = 1,
            UserId = "test-user-id",
            Message = "Great stream!",
            MessageType = DTOs.ChatMessageType.Text,
            CreatedAt = DateTime.UtcNow.AddMinutes(-5)
        };
        _context.LiveStreamChats.Add(chatMessage2);

        // Add test reactions
        var reaction1 = new LiveStreamReaction
        {
            Id = 1,
            LiveStreamId = 1,
            UserId = "test-user-id",
            ReactionType = DTOs.ReactionType.Like,
            CreatedAt = DateTime.UtcNow.AddMinutes(-8)
        };
        _context.LiveStreamReactions.Add(reaction1);

        var reaction2 = new LiveStreamReaction
        {
            Id = 2,
            LiveStreamId = 1,
            UserId = "test-user-id",
            ReactionType = DTOs.ReactionType.Love,
            CreatedAt = DateTime.UtcNow.AddMinutes(-3)
        };
        _context.LiveStreamReactions.Add(reaction2);

        _context.SaveChanges();
    }

    [Fact]
    public async Task SendMessageAsync_ValidMessage_ReturnsMessage()
    {
        // Arrange
        var dto = new SendChatMessageDto
        {
            LiveStreamId = 1,
            Message = "Test message",
            MessageType = DTOs.ChatMessageType.Text
        };
        var userId = "test-user-id";

        // Act
        var result = await _chatService.SendMessageAsync(dto, userId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(dto.LiveStreamId, result.LiveStreamId);
        Assert.Equal(dto.Message, result.Message);
        Assert.Equal(dto.MessageType, result.MessageType);
        Assert.Equal(userId, result.UserId);
        Assert.NotNull(result.CreatedAt);
    }

    [Fact]
    public async Task SendMessageAsync_InvalidStream_ThrowsArgumentException()
    {
        // Arrange
        var dto = new SendChatMessageDto
        {
            LiveStreamId = 999,
            Message = "Test message",
            MessageType = DTOs.ChatMessageType.Text
        };
        var userId = "test-user-id";

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => 
            _chatService.SendMessageAsync(dto, userId));
    }

    [Fact]
    public async Task GetChatMessagesAsync_ValidStream_ReturnsMessages()
    {
        // Arrange
        var liveStreamId = 1;
        var page = 1;
        var pageSize = 10;

        // Act
        var result = await _chatService.GetChatMessagesAsync(liveStreamId, page, pageSize);

        // Assert
        Assert.NotNull(result);
        Assert.NotEmpty(result);
        Assert.True(result.Count <= pageSize);
        Assert.All(result, message => Assert.Equal(liveStreamId, message.LiveStreamId));
    }

    [Fact]
    public async Task GetChatMessagesAsync_InvalidStream_ReturnsEmptyList()
    {
        // Arrange
        var liveStreamId = 999;
        var page = 1;
        var pageSize = 10;

        // Act
        var result = await _chatService.GetChatMessagesAsync(liveStreamId, page, pageSize);

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);
    }

    [Fact]
    public async Task GetChatMessagesAsync_WithPagination_ReturnsCorrectPage()
    {
        // Arrange
        var liveStreamId = 1;
        var page = 1;
        var pageSize = 1;

        // Act
        var result = await _chatService.GetChatMessagesAsync(liveStreamId, page, pageSize);

        // Assert
        Assert.NotNull(result);
        Assert.Single(result);
    }

    [Fact]
    public async Task GetChatReactionsAsync_ValidStream_ReturnsReactions()
    {
        // Arrange
        var liveStreamId = 1;

        // Act
        var result = await _chatService.GetChatReactionsAsync(liveStreamId);

        // Assert
        Assert.NotNull(result);
        Assert.NotEmpty(result);
        Assert.All(result, reaction => Assert.Equal(liveStreamId, reaction.LiveStreamId));
    }

    [Fact]
    public async Task GetChatReactionsAsync_InvalidStream_ReturnsEmptyList()
    {
        // Arrange
        var liveStreamId = 999;

        // Act
        var result = await _chatService.GetChatReactionsAsync(liveStreamId);

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);
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
        var result = await _chatService.AddReactionAsync(dto, userId);

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
            _chatService.AddReactionAsync(dto, userId));
    }

    [Fact]
    public async Task RemoveReactionAsync_ValidReaction_ReturnsTrue()
    {
        // Arrange
        var liveStreamId = 1;
        var userId = "test-user-id";

        // Act
        var result = await _chatService.RemoveReactionAsync(liveStreamId, userId);

        // Assert
        Assert.True(result);

        // Verify reaction is removed
        var reaction = await _context.LiveStreamReactions
            .FirstOrDefaultAsync(r => r.LiveStreamId == liveStreamId && r.UserId == userId);
        Assert.Null(reaction);
    }

    [Fact]
    public async Task RemoveReactionAsync_InvalidStream_ReturnsFalse()
    {
        // Arrange
        var liveStreamId = 999;
        var userId = "test-user-id";

        // Act
        var result = await _chatService.RemoveReactionAsync(liveStreamId, userId);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task GetChatStatsAsync_ValidStream_ReturnsStats()
    {
        // Arrange
        var liveStreamId = 1;

        // Act
        var result = await _chatService.GetChatStatsAsync(liveStreamId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(liveStreamId, result.LiveStreamId);
        Assert.True(result.TotalMessages >= 0);
        Assert.True(result.TotalReactions >= 0);
        Assert.True(result.ActiveUsers >= 0);
        Assert.NotNull(result.TopReactions);
        Assert.NotNull(result.RecentMessages);
    }

    [Fact]
    public async Task GetChatStatsAsync_InvalidStream_ReturnsEmptyStats()
    {
        // Arrange
        var liveStreamId = 999;

        // Act
        var result = await _chatService.GetChatStatsAsync(liveStreamId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(liveStreamId, result.LiveStreamId);
        Assert.Equal(0, result.TotalMessages);
        Assert.Equal(0, result.TotalReactions);
        Assert.Equal(0, result.ActiveUsers);
        Assert.Empty(result.TopReactions);
        Assert.Empty(result.RecentMessages);
    }

    [Fact]
    public async Task GetChatHistoryAsync_ValidStream_ReturnsHistory()
    {
        // Arrange
        var liveStreamId = 1;
        var startTime = DateTime.UtcNow.AddHours(-1);
        var endTime = DateTime.UtcNow;

        // Act
        var result = await _chatService.GetChatHistoryAsync(liveStreamId, startTime, endTime);

        // Assert
        Assert.NotNull(result);
        Assert.NotEmpty(result);
        Assert.All(result, message => Assert.Equal(liveStreamId, message.LiveStreamId));
        Assert.All(result, message => Assert.True(message.CreatedAt >= startTime));
        Assert.All(result, message => Assert.True(message.CreatedAt <= endTime));
    }

    [Fact]
    public async Task GetChatHistoryAsync_InvalidStream_ReturnsEmptyList()
    {
        // Arrange
        var liveStreamId = 999;
        var startTime = DateTime.UtcNow.AddHours(-1);
        var endTime = DateTime.UtcNow;

        // Act
        var result = await _chatService.GetChatHistoryAsync(liveStreamId, startTime, endTime);

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);
    }

    [Fact]
    public async Task GetChatHistoryAsync_WithTimeRange_ReturnsFilteredMessages()
    {
        // Arrange
        var liveStreamId = 1;
        var startTime = DateTime.UtcNow.AddMinutes(-15);
        var endTime = DateTime.UtcNow;

        // Act
        var result = await _chatService.GetChatHistoryAsync(liveStreamId, startTime, endTime);

        // Assert
        Assert.NotNull(result);
        Assert.All(result, message => Assert.True(message.CreatedAt >= startTime));
        Assert.All(result, message => Assert.True(message.CreatedAt <= endTime));
    }

    [Fact]
    public async Task GetChatHistoryAsync_WithMessageType_ReturnsFilteredMessages()
    {
        // Arrange
        var liveStreamId = 1;
        var startTime = DateTime.UtcNow.AddHours(-1);
        var endTime = DateTime.UtcNow;
        var messageType = DTOs.ChatMessageType.Text;

        // Act
        var result = await _chatService.GetChatHistoryAsync(liveStreamId, startTime, endTime, messageType);

        // Assert
        Assert.NotNull(result);
        Assert.All(result, message => Assert.Equal(messageType, message.MessageType));
    }

    [Fact]
    public async Task GetChatHistoryAsync_WithUserId_ReturnsFilteredMessages()
    {
        // Arrange
        var liveStreamId = 1;
        var startTime = DateTime.UtcNow.AddHours(-1);
        var endTime = DateTime.UtcNow;
        var userId = "test-user-id";

        // Act
        var result = await _chatService.GetChatHistoryAsync(liveStreamId, startTime, endTime, null, userId);

        // Assert
        Assert.NotNull(result);
        Assert.All(result, message => Assert.Equal(userId, message.UserId));
    }

    [Fact]
    public async Task GetChatHistoryAsync_WithSearchTerm_ReturnsFilteredMessages()
    {
        // Arrange
        var liveStreamId = 1;
        var startTime = DateTime.UtcNow.AddHours(-1);
        var endTime = DateTime.UtcNow;
        var searchTerm = "Great";

        // Act
        var result = await _chatService.GetChatHistoryAsync(liveStreamId, startTime, endTime, null, null, searchTerm);

        // Assert
        Assert.NotNull(result);
        Assert.All(result, message => Assert.Contains(searchTerm, message.Message, StringComparison.OrdinalIgnoreCase));
    }

    public void Dispose()
    {
        _context.Dispose();
    }
}
