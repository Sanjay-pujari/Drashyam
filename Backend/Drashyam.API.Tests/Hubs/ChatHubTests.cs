using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using Moq;
using Drashyam.API.Hubs;

namespace Drashyam.API.Tests.Hubs;

public class ChatHubTests
{
    private readonly Mock<IHubCallerClients> _mockClients;
    private readonly Mock<IClientProxy> _mockClientProxy;
    private readonly Mock<IGroupManager> _mockGroups;
    private readonly Mock<HubCallerContext> _mockContext;
    private readonly Mock<ILogger<ChatHub>> _mockLogger;
    private readonly ChatHub _hub;

    public ChatHubTests()
    {
        _mockClients = new Mock<IHubCallerClients>();
        _mockClientProxy = new Mock<IClientProxy>();
        _mockGroups = new Mock<IGroupManager>();
        _mockContext = new Mock<HubCallerContext>();
        _mockLogger = new Mock<ILogger<ChatHub>>();

        _hub = new ChatHub();
        
        // Setup context
        _hub.Context = _mockContext.Object;
        _hub.Clients = _mockClients.Object;
        _hub.Groups = _mockGroups.Object;
    }

    [Fact]
    public async Task JoinChat_ValidLiveStreamId_AddsToGroup()
    {
        // Arrange
        var liveStreamId = "test-stream-123";
        _mockGroups.Setup(g => g.AddToGroupAsync(It.IsAny<string>(), It.IsAny<string>(), default))
            .Returns(Task.CompletedTask);
        _mockClients.Setup(c => c.Group(It.IsAny<string>())).Returns(_mockClientProxy.Object);
        _mockClientProxy.Setup(c => c.SendAsync(It.IsAny<string>(), It.IsAny<object>(), default))
            .Returns(Task.CompletedTask);

        // Act
        await _hub.JoinChat(liveStreamId);

        // Assert
        _mockGroups.Verify(g => g.AddToGroupAsync(It.IsAny<string>(), $"chat_{liveStreamId}", default), Times.Once);
    }

    [Fact]
    public async Task JoinChat_ValidLiveStreamId_NotifiesOtherClients()
    {
        // Arrange
        var liveStreamId = "test-stream-123";
        _mockGroups.Setup(g => g.AddToGroupAsync(It.IsAny<string>(), It.IsAny<string>(), default))
            .Returns(Task.CompletedTask);
        _mockClients.Setup(c => c.Group(It.IsAny<string>())).Returns(_mockClientProxy.Object);
        _mockClientProxy.Setup(c => c.SendAsync(It.IsAny<string>(), It.IsAny<object>(), default))
            .Returns(Task.CompletedTask);

        // Act
        await _hub.JoinChat(liveStreamId);

        // Assert
        _mockClientProxy.Verify(c => c.SendAsync("UserJoinedChat", It.IsAny<object>(), default), Times.Once);
    }

    [Fact]
    public async Task LeaveChat_ValidLiveStreamId_RemovesFromGroup()
    {
        // Arrange
        var liveStreamId = "test-stream-123";
        _mockGroups.Setup(g => g.RemoveFromGroupAsync(It.IsAny<string>(), It.IsAny<string>(), default))
            .Returns(Task.CompletedTask);
        _mockClients.Setup(c => c.Group(It.IsAny<string>())).Returns(_mockClientProxy.Object);
        _mockClientProxy.Setup(c => c.SendAsync(It.IsAny<string>(), It.IsAny<object>(), default))
            .Returns(Task.CompletedTask);

        // Act
        await _hub.LeaveChat(liveStreamId);

        // Assert
        _mockGroups.Verify(g => g.RemoveFromGroupAsync(It.IsAny<string>(), $"chat_{liveStreamId}", default), Times.Once);
    }

    [Fact]
    public async Task LeaveChat_ValidLiveStreamId_NotifiesOtherClients()
    {
        // Arrange
        var liveStreamId = "test-stream-123";
        _mockGroups.Setup(g => g.RemoveFromGroupAsync(It.IsAny<string>(), It.IsAny<string>(), default))
            .Returns(Task.CompletedTask);
        _mockClients.Setup(c => c.Group(It.IsAny<string>())).Returns(_mockClientProxy.Object);
        _mockClientProxy.Setup(c => c.SendAsync(It.IsAny<string>(), It.IsAny<object>(), default))
            .Returns(Task.CompletedTask);

        // Act
        await _hub.LeaveChat(liveStreamId);

        // Assert
        _mockClientProxy.Verify(c => c.SendAsync("UserLeftChat", It.IsAny<object>(), default), Times.Once);
    }

    [Fact]
    public async Task SendMessage_ValidData_SendsToGroup()
    {
        // Arrange
        var liveStreamId = "test-stream-123";
        var user = "testuser";
        var message = "Hello everyone!";
        _mockClients.Setup(c => c.Group(It.IsAny<string>())).Returns(_mockClientProxy.Object);
        _mockClientProxy.Setup(c => c.SendAsync(It.IsAny<string>(), It.IsAny<object>(), It.IsAny<object>(), default))
            .Returns(Task.CompletedTask);

        // Act
        await _hub.SendMessage(liveStreamId, user, message);

        // Assert
        _mockClientProxy.Verify(c => c.SendAsync("MessageReceived", user, message, default), Times.Once);
    }

    [Fact]
    public async Task SendReaction_ValidData_SendsToGroup()
    {
        // Arrange
        var liveStreamId = "test-stream-123";
        var user = "testuser";
        var reactionType = "like";
        _mockClients.Setup(c => c.Group(It.IsAny<string>())).Returns(_mockClientProxy.Object);
        _mockClientProxy.Setup(c => c.SendAsync(It.IsAny<string>(), It.IsAny<object>(), It.IsAny<object>(), default))
            .Returns(Task.CompletedTask);

        // Act
        await _hub.SendReaction(liveStreamId, user, reactionType);

        // Assert
        _mockClientProxy.Verify(c => c.SendAsync("ReactionReceived", user, reactionType, default), Times.Once);
    }

    [Fact]
    public async Task JoinChat_EmptyLiveStreamId_ThrowsArgumentException()
    {
        // Arrange
        var liveStreamId = "";

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => _hub.JoinChat(liveStreamId));
    }

    [Fact]
    public async Task JoinChat_NullLiveStreamId_ThrowsArgumentException()
    {
        // Arrange
        string liveStreamId = null!;

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => _hub.JoinChat(liveStreamId));
    }

    [Fact]
    public async Task LeaveChat_EmptyLiveStreamId_ThrowsArgumentException()
    {
        // Arrange
        var liveStreamId = "";

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => _hub.LeaveChat(liveStreamId));
    }

    [Fact]
    public async Task LeaveChat_NullLiveStreamId_ThrowsArgumentException()
    {
        // Arrange
        string liveStreamId = null!;

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => _hub.LeaveChat(liveStreamId));
    }

    [Fact]
    public async Task SendMessage_EmptyLiveStreamId_ThrowsArgumentException()
    {
        // Arrange
        var liveStreamId = "";
        var user = "testuser";
        var message = "Hello everyone!";

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => _hub.SendMessage(liveStreamId, user, message));
    }

    [Fact]
    public async Task SendMessage_NullLiveStreamId_ThrowsArgumentException()
    {
        // Arrange
        string liveStreamId = null!;
        var user = "testuser";
        var message = "Hello everyone!";

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => _hub.SendMessage(liveStreamId, user, message));
    }

    [Fact]
    public async Task SendMessage_EmptyUser_ThrowsArgumentException()
    {
        // Arrange
        var liveStreamId = "test-stream-123";
        var user = "";
        var message = "Hello everyone!";

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => _hub.SendMessage(liveStreamId, user, message));
    }

    [Fact]
    public async Task SendMessage_NullUser_ThrowsArgumentException()
    {
        // Arrange
        var liveStreamId = "test-stream-123";
        string user = null!;
        var message = "Hello everyone!";

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => _hub.SendMessage(liveStreamId, user, message));
    }

    [Fact]
    public async Task SendMessage_EmptyMessage_ThrowsArgumentException()
    {
        // Arrange
        var liveStreamId = "test-stream-123";
        var user = "testuser";
        var message = "";

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => _hub.SendMessage(liveStreamId, user, message));
    }

    [Fact]
    public async Task SendMessage_NullMessage_ThrowsArgumentException()
    {
        // Arrange
        var liveStreamId = "test-stream-123";
        var user = "testuser";
        string message = null!;

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => _hub.SendMessage(liveStreamId, user, message));
    }

    [Fact]
    public async Task SendReaction_EmptyLiveStreamId_ThrowsArgumentException()
    {
        // Arrange
        var liveStreamId = "";
        var user = "testuser";
        var reactionType = "like";

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => _hub.SendReaction(liveStreamId, user, reactionType));
    }

    [Fact]
    public async Task SendReaction_NullLiveStreamId_ThrowsArgumentException()
    {
        // Arrange
        string liveStreamId = null!;
        var user = "testuser";
        var reactionType = "like";

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => _hub.SendReaction(liveStreamId, user, reactionType));
    }

    [Fact]
    public async Task SendReaction_EmptyUser_ThrowsArgumentException()
    {
        // Arrange
        var liveStreamId = "test-stream-123";
        var user = "";
        var reactionType = "like";

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => _hub.SendReaction(liveStreamId, user, reactionType));
    }

    [Fact]
    public async Task SendReaction_NullUser_ThrowsArgumentException()
    {
        // Arrange
        var liveStreamId = "test-stream-123";
        string user = null!;
        var reactionType = "like";

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => _hub.SendReaction(liveStreamId, user, reactionType));
    }

    [Fact]
    public async Task SendReaction_EmptyReactionType_ThrowsArgumentException()
    {
        // Arrange
        var liveStreamId = "test-stream-123";
        var user = "testuser";
        var reactionType = "";

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => _hub.SendReaction(liveStreamId, user, reactionType));
    }

    [Fact]
    public async Task SendReaction_NullReactionType_ThrowsArgumentException()
    {
        // Arrange
        var liveStreamId = "test-stream-123";
        var user = "testuser";
        string reactionType = null!;

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => _hub.SendReaction(liveStreamId, user, reactionType));
    }
}
