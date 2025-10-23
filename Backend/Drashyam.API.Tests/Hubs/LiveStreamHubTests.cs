using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using Moq;
using Drashyam.API.Hubs;

namespace Drashyam.API.Tests.Hubs;

public class LiveStreamHubTests
{
    private readonly Mock<IHubCallerClients> _mockClients;
    private readonly Mock<IClientProxy> _mockClientProxy;
    private readonly Mock<IGroupManager> _mockGroups;
    private readonly Mock<HubCallerContext> _mockContext;
    private readonly Mock<ILogger<LiveStreamHub>> _mockLogger;
    private readonly LiveStreamHub _hub;

    public LiveStreamHubTests()
    {
        _mockClients = new Mock<IHubCallerClients>();
        _mockClientProxy = new Mock<IClientProxy>();
        _mockGroups = new Mock<IGroupManager>();
        _mockContext = new Mock<HubCallerContext>();
        _mockLogger = new Mock<ILogger<LiveStreamHub>>();

        _hub = new LiveStreamHub();
        
        // Setup context
        _hub.Context = _mockContext.Object;
        _hub.Clients = _mockClients.Object;
        _hub.Groups = _mockGroups.Object;
    }

    [Fact]
    public async Task JoinStream_ValidStreamId_AddsToGroup()
    {
        // Arrange
        var streamId = "test-stream-123";
        _mockGroups.Setup(g => g.AddToGroupAsync(It.IsAny<string>(), It.IsAny<string>(), default))
            .Returns(Task.CompletedTask);
        _mockClients.Setup(c => c.Group(It.IsAny<string>())).Returns(_mockClientProxy.Object);
        _mockClientProxy.Setup(c => c.SendAsync(It.IsAny<string>(), It.IsAny<object>(), default))
            .Returns(Task.CompletedTask);

        // Act
        await _hub.JoinStream(streamId);

        // Assert
        _mockGroups.Verify(g => g.AddToGroupAsync(It.IsAny<string>(), $"stream_{streamId}", default), Times.Once);
    }

    [Fact]
    public async Task JoinStream_ValidStreamId_NotifiesOtherClients()
    {
        // Arrange
        var streamId = "test-stream-123";
        _mockGroups.Setup(g => g.AddToGroupAsync(It.IsAny<string>(), It.IsAny<string>(), default))
            .Returns(Task.CompletedTask);
        _mockClients.Setup(c => c.Group(It.IsAny<string>())).Returns(_mockClientProxy.Object);
        _mockClientProxy.Setup(c => c.SendAsync(It.IsAny<string>(), It.IsAny<object>(), default))
            .Returns(Task.CompletedTask);

        // Act
        await _hub.JoinStream(streamId);

        // Assert
        _mockClientProxy.Verify(c => c.SendAsync("ViewerJoined", It.IsAny<object>(), default), Times.Once);
    }

    [Fact]
    public async Task LeaveStream_ValidStreamId_RemovesFromGroup()
    {
        // Arrange
        var streamId = "test-stream-123";
        _mockGroups.Setup(g => g.RemoveFromGroupAsync(It.IsAny<string>(), It.IsAny<string>(), default))
            .Returns(Task.CompletedTask);
        _mockClients.Setup(c => c.Group(It.IsAny<string>())).Returns(_mockClientProxy.Object);
        _mockClientProxy.Setup(c => c.SendAsync(It.IsAny<string>(), It.IsAny<object>(), default))
            .Returns(Task.CompletedTask);

        // Act
        await _hub.LeaveStream(streamId);

        // Assert
        _mockGroups.Verify(g => g.RemoveFromGroupAsync(It.IsAny<string>(), $"stream_{streamId}", default), Times.Once);
    }

    [Fact]
    public async Task LeaveStream_ValidStreamId_NotifiesOtherClients()
    {
        // Arrange
        var streamId = "test-stream-123";
        _mockGroups.Setup(g => g.RemoveFromGroupAsync(It.IsAny<string>(), It.IsAny<string>(), default))
            .Returns(Task.CompletedTask);
        _mockClients.Setup(c => c.Group(It.IsAny<string>())).Returns(_mockClientProxy.Object);
        _mockClientProxy.Setup(c => c.SendAsync(It.IsAny<string>(), It.IsAny<object>(), default))
            .Returns(Task.CompletedTask);

        // Act
        await _hub.LeaveStream(streamId);

        // Assert
        _mockClientProxy.Verify(c => c.SendAsync("ViewerLeft", It.IsAny<object>(), default), Times.Once);
    }

    [Fact]
    public async Task SendStreamUpdate_ValidData_SendsToGroup()
    {
        // Arrange
        var streamId = "test-stream-123";
        var updateData = new { viewerCount = 100, status = "live" };
        _mockClients.Setup(c => c.Group(It.IsAny<string>())).Returns(_mockClientProxy.Object);
        _mockClientProxy.Setup(c => c.SendAsync(It.IsAny<string>(), It.IsAny<object>(), default))
            .Returns(Task.CompletedTask);

        // Act
        await _hub.SendStreamUpdate(streamId, updateData);

        // Assert
        _mockClientProxy.Verify(c => c.SendAsync("StreamUpdateReceived", updateData, default), Times.Once);
    }

    [Fact]
    public async Task UpdateViewerCount_ValidData_SendsToGroup()
    {
        // Arrange
        var streamId = "test-stream-123";
        var viewerCount = 150;
        _mockClients.Setup(c => c.Group(It.IsAny<string>())).Returns(_mockClientProxy.Object);
        _mockClientProxy.Setup(c => c.SendAsync(It.IsAny<string>(), It.IsAny<object>(), default))
            .Returns(Task.CompletedTask);

        // Act
        await _hub.UpdateViewerCount(streamId, viewerCount);

        // Assert
        _mockClientProxy.Verify(c => c.SendAsync("ViewerCountUpdated", viewerCount, default), Times.Once);
    }

    [Fact]
    public async Task JoinStream_EmptyStreamId_ThrowsArgumentException()
    {
        // Arrange
        var streamId = "";

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => _hub.JoinStream(streamId));
    }

    [Fact]
    public async Task JoinStream_NullStreamId_ThrowsArgumentException()
    {
        // Arrange
        string streamId = null!;

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => _hub.JoinStream(streamId));
    }

    [Fact]
    public async Task LeaveStream_EmptyStreamId_ThrowsArgumentException()
    {
        // Arrange
        var streamId = "";

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => _hub.LeaveStream(streamId));
    }

    [Fact]
    public async Task LeaveStream_NullStreamId_ThrowsArgumentException()
    {
        // Arrange
        string streamId = null!;

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => _hub.LeaveStream(streamId));
    }

    [Fact]
    public async Task SendStreamUpdate_EmptyStreamId_ThrowsArgumentException()
    {
        // Arrange
        var streamId = "";
        var updateData = new { viewerCount = 100, status = "live" };

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => _hub.SendStreamUpdate(streamId, updateData));
    }

    [Fact]
    public async Task SendStreamUpdate_NullStreamId_ThrowsArgumentException()
    {
        // Arrange
        string streamId = null!;
        var updateData = new { viewerCount = 100, status = "live" };

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => _hub.SendStreamUpdate(streamId, updateData));
    }

    [Fact]
    public async Task UpdateViewerCount_EmptyStreamId_ThrowsArgumentException()
    {
        // Arrange
        var streamId = "";
        var viewerCount = 150;

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => _hub.UpdateViewerCount(streamId, viewerCount));
    }

    [Fact]
    public async Task UpdateViewerCount_NullStreamId_ThrowsArgumentException()
    {
        // Arrange
        string streamId = null!;
        var viewerCount = 150;

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => _hub.UpdateViewerCount(streamId, viewerCount));
    }

    [Fact]
    public async Task UpdateViewerCount_NegativeViewerCount_ThrowsArgumentException()
    {
        // Arrange
        var streamId = "test-stream-123";
        var viewerCount = -1;

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => _hub.UpdateViewerCount(streamId, viewerCount));
    }
}
