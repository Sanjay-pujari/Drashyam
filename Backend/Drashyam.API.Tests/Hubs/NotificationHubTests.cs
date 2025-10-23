using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using Moq;
using Drashyam.API.Hubs;

namespace Drashyam.API.Tests.Hubs;

public class NotificationHubTests
{
    private readonly Mock<IHubCallerClients> _mockClients;
    private readonly Mock<IClientProxy> _mockClientProxy;
    private readonly Mock<IGroupManager> _mockGroups;
    private readonly Mock<HubCallerContext> _mockContext;
    private readonly Mock<ILogger<NotificationHub>> _mockLogger;
    private readonly NotificationHub _hub;

    public NotificationHubTests()
    {
        _mockClients = new Mock<IHubCallerClients>();
        _mockClientProxy = new Mock<IClientProxy>();
        _mockGroups = new Mock<IGroupManager>();
        _mockContext = new Mock<HubCallerContext>();
        _mockLogger = new Mock<ILogger<NotificationHub>>();

        _hub = new NotificationHub();
        
        // Setup context
        _hub.Context = _mockContext.Object;
        _hub.Clients = _mockClients.Object;
        _hub.Groups = _mockGroups.Object;
    }

    [Fact]
    public async Task SendNotificationToUser_ValidData_SendsToUser()
    {
        // Arrange
        var userId = "test-user-123";
        var message = "Test notification";
        _mockClients.Setup(c => c.User(userId)).Returns(_mockClientProxy.Object);
        _mockClientProxy.Setup(c => c.SendAsync(It.IsAny<string>(), It.IsAny<object>(), default))
            .Returns(Task.CompletedTask);

        // Act
        await _hub.SendNotificationToUser(userId, message);

        // Assert
        _mockClientProxy.Verify(c => c.SendAsync("ReceiveNotification", message, default), Times.Once);
    }

    [Fact]
    public async Task SendGlobalNotification_ValidData_SendsToAll()
    {
        // Arrange
        var message = "Global notification";
        _mockClients.Setup(c => c.All).Returns(_mockClientProxy.Object);
        _mockClientProxy.Setup(c => c.SendAsync(It.IsAny<string>(), It.IsAny<object>(), default))
            .Returns(Task.CompletedTask);

        // Act
        await _hub.SendGlobalNotification(message);

        // Assert
        _mockClientProxy.Verify(c => c.SendAsync("ReceiveGlobalNotification", message, default), Times.Once);
    }

    [Fact]
    public async Task SubscribeToNotifications_ValidUserId_AddsToGroup()
    {
        // Arrange
        var userId = "test-user-123";
        _mockGroups.Setup(g => g.AddToGroupAsync(It.IsAny<string>(), It.IsAny<string>(), default))
            .Returns(Task.CompletedTask);
        _mockClients.Setup(c => c.Caller).Returns(_mockClientProxy.Object);
        _mockClientProxy.Setup(c => c.SendAsync(It.IsAny<string>(), It.IsAny<object>(), default))
            .Returns(Task.CompletedTask);

        // Act
        await _hub.SubscribeToNotifications(userId);

        // Assert
        _mockGroups.Verify(g => g.AddToGroupAsync(It.IsAny<string>(), $"user_{userId}_notifications", default), Times.Once);
    }

    [Fact]
    public async Task SubscribeToNotifications_ValidUserId_NotifiesCaller()
    {
        // Arrange
        var userId = "test-user-123";
        _mockGroups.Setup(g => g.AddToGroupAsync(It.IsAny<string>(), It.IsAny<string>(), default))
            .Returns(Task.CompletedTask);
        _mockClients.Setup(c => c.Caller).Returns(_mockClientProxy.Object);
        _mockClientProxy.Setup(c => c.SendAsync(It.IsAny<string>(), It.IsAny<object>(), default))
            .Returns(Task.CompletedTask);

        // Act
        await _hub.SubscribeToNotifications(userId);

        // Assert
        _mockClientProxy.Verify(c => c.SendAsync("SubscribedToNotifications", It.IsAny<object>(), default), Times.Once);
    }

    [Fact]
    public async Task UnsubscribeFromNotifications_ValidUserId_RemovesFromGroup()
    {
        // Arrange
        var userId = "test-user-123";
        _mockGroups.Setup(g => g.RemoveFromGroupAsync(It.IsAny<string>(), It.IsAny<string>(), default))
            .Returns(Task.CompletedTask);
        _mockClients.Setup(c => c.Caller).Returns(_mockClientProxy.Object);
        _mockClientProxy.Setup(c => c.SendAsync(It.IsAny<string>(), It.IsAny<object>(), default))
            .Returns(Task.CompletedTask);

        // Act
        await _hub.UnsubscribeFromNotifications(userId);

        // Assert
        _mockGroups.Verify(g => g.RemoveFromGroupAsync(It.IsAny<string>(), $"user_{userId}_notifications", default), Times.Once);
    }

    [Fact]
    public async Task UnsubscribeFromNotifications_ValidUserId_NotifiesCaller()
    {
        // Arrange
        var userId = "test-user-123";
        _mockGroups.Setup(g => g.RemoveFromGroupAsync(It.IsAny<string>(), It.IsAny<string>(), default))
            .Returns(Task.CompletedTask);
        _mockClients.Setup(c => c.Caller).Returns(_mockClientProxy.Object);
        _mockClientProxy.Setup(c => c.SendAsync(It.IsAny<string>(), It.IsAny<object>(), default))
            .Returns(Task.CompletedTask);

        // Act
        await _hub.UnsubscribeFromNotifications(userId);

        // Assert
        _mockClientProxy.Verify(c => c.SendAsync("UnsubscribedFromNotifications", It.IsAny<object>(), default), Times.Once);
    }

    [Fact]
    public async Task SendNotificationToUser_EmptyUserId_ThrowsArgumentException()
    {
        // Arrange
        var userId = "";
        var message = "Test notification";

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => _hub.SendNotificationToUser(userId, message));
    }

    [Fact]
    public async Task SendNotificationToUser_NullUserId_ThrowsArgumentException()
    {
        // Arrange
        string userId = null!;
        var message = "Test notification";

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => _hub.SendNotificationToUser(userId, message));
    }

    [Fact]
    public async Task SendNotificationToUser_EmptyMessage_ThrowsArgumentException()
    {
        // Arrange
        var userId = "test-user-123";
        var message = "";

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => _hub.SendNotificationToUser(userId, message));
    }

    [Fact]
    public async Task SendNotificationToUser_NullMessage_ThrowsArgumentException()
    {
        // Arrange
        var userId = "test-user-123";
        string message = null!;

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => _hub.SendNotificationToUser(userId, message));
    }

    [Fact]
    public async Task SendGlobalNotification_EmptyMessage_ThrowsArgumentException()
    {
        // Arrange
        var message = "";

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => _hub.SendGlobalNotification(message));
    }

    [Fact]
    public async Task SendGlobalNotification_NullMessage_ThrowsArgumentException()
    {
        // Arrange
        string message = null!;

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => _hub.SendGlobalNotification(message));
    }

    [Fact]
    public async Task SubscribeToNotifications_EmptyUserId_ThrowsArgumentException()
    {
        // Arrange
        var userId = "";

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => _hub.SubscribeToNotifications(userId));
    }

    [Fact]
    public async Task SubscribeToNotifications_NullUserId_ThrowsArgumentException()
    {
        // Arrange
        string userId = null!;

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => _hub.SubscribeToNotifications(userId));
    }

    [Fact]
    public async Task UnsubscribeFromNotifications_EmptyUserId_ThrowsArgumentException()
    {
        // Arrange
        var userId = "";

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => _hub.UnsubscribeFromNotifications(userId));
    }

    [Fact]
    public async Task UnsubscribeFromNotifications_NullUserId_ThrowsArgumentException()
    {
        // Arrange
        string userId = null!;

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => _hub.UnsubscribeFromNotifications(userId));
    }
}
