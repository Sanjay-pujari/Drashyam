using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Drashyam.API.Services;
using Drashyam.API.DTOs;

namespace Drashyam.API.Hubs;

[Authorize]
public class LiveStreamHub : Hub
{
    private readonly ILiveStreamService _liveStreamService;
    private readonly ILiveStreamChatService _chatService;
    private readonly ILogger<LiveStreamHub> _logger;

    public LiveStreamHub(
        ILiveStreamService liveStreamService,
        ILiveStreamChatService chatService,
        ILogger<LiveStreamHub> logger)
    {
        _liveStreamService = liveStreamService;
        _chatService = chatService;
        _logger = logger;
    }

    public async Task JoinStream(int streamId)
    {
        try
        {
            var userId = Context.User?.FindFirst("sub")?.Value ?? Context.User?.FindFirst("id")?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                await Clients.Caller.SendAsync("Error", "User not authenticated");
                return;
            }

            // Add user to stream group
            await Groups.AddToGroupAsync(Context.ConnectionId, $"stream_{streamId}");
            
            // Get current stream info
            var stream = await _liveStreamService.GetLiveStreamByIdAsync(streamId);
            
            // Notify user joined
            await Clients.Group($"stream_{streamId}").SendAsync("UserJoined", new
            {
                UserId = userId,
                ConnectionId = Context.ConnectionId,
                Timestamp = DateTime.UtcNow
            });

            // Send current stream status to the user
            await Clients.Caller.SendAsync("StreamStatus", new
            {
                StreamId = streamId,
                Status = stream.Status.ToString(),
                ViewerCount = stream.ViewerCount,
                IsLive = stream.Status == LiveStreamStatus.Live
            });

            _logger.LogInformation($"User {userId} joined stream {streamId}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error joining stream {streamId}");
            await Clients.Caller.SendAsync("Error", "Failed to join stream");
        }
    }

    public async Task LeaveStream(int streamId)
    {
        try
        {
            var userId = Context.User?.FindFirst("sub")?.Value ?? Context.User?.FindFirst("id")?.Value;
            
            // Remove user from stream group
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"stream_{streamId}");
            
            // Notify user left
            await Clients.Group($"stream_{streamId}").SendAsync("UserLeft", new
            {
                UserId = userId,
                ConnectionId = Context.ConnectionId,
                Timestamp = DateTime.UtcNow
            });

            _logger.LogInformation($"User {userId} left stream {streamId}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error leaving stream {streamId}");
        }
    }

    public async Task StartStream(int streamId)
    {
        try
        {
            var userId = Context.User?.FindFirst("sub")?.Value ?? Context.User?.FindFirst("id")?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                await Clients.Caller.SendAsync("Error", "User not authenticated");
                return;
            }

            // Update stream status to live
            var stream = await _liveStreamService.GetLiveStreamByIdAsync(streamId);
            if (stream.UserId != userId)
            {
                await Clients.Caller.SendAsync("Error", "Unauthorized to start this stream");
                return;
            }

            // Notify all viewers that stream started
            await Clients.Group($"stream_{streamId}").SendAsync("StreamStarted", new
            {
                StreamId = streamId,
                StartedBy = userId,
                StartTime = DateTime.UtcNow,
                StreamUrl = stream.StreamUrl,
                HlsUrl = stream.HlsUrl
            });

            _logger.LogInformation($"Stream {streamId} started by user {userId}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error starting stream {streamId}");
            await Clients.Caller.SendAsync("Error", "Failed to start stream");
        }
    }

    public async Task EndStream(int streamId)
    {
        try
        {
            var userId = Context.User?.FindFirst("sub")?.Value ?? Context.User?.FindFirst("id")?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                await Clients.Caller.SendAsync("Error", "User not authenticated");
                return;
            }

            // Notify all viewers that stream ended
            await Clients.Group($"stream_{streamId}").SendAsync("StreamEnded", new
            {
                StreamId = streamId,
                EndedBy = userId,
                EndTime = DateTime.UtcNow
            });

            _logger.LogInformation($"Stream {streamId} ended by user {userId}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error ending stream {streamId}");
            await Clients.Caller.SendAsync("Error", "Failed to end stream");
        }
    }

    public async Task UpdateViewerCount(int streamId, long viewerCount)
    {
        try
        {
            // Broadcast viewer count update to all stream viewers
            await Clients.Group($"stream_{streamId}").SendAsync("ViewerCountUpdated", new
            {
                StreamId = streamId,
                ViewerCount = viewerCount,
                Timestamp = DateTime.UtcNow
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error updating viewer count for stream {streamId}");
        }
    }

    public async Task SendStreamHealth(int streamId, object healthData)
    {
        try
        {
            var userId = Context.User?.FindFirst("sub")?.Value ?? Context.User?.FindFirst("id")?.Value;
            
            // Send health data to stream owner only
            await Clients.Caller.SendAsync("StreamHealth", new
            {
                StreamId = streamId,
                HealthData = healthData,
                Timestamp = DateTime.UtcNow
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error sending stream health for stream {streamId}");
        }
    }

    public override async Task OnConnectedAsync()
    {
        var userId = Context.User?.FindFirst("sub")?.Value ?? Context.User?.FindFirst("id")?.Value;
        _logger.LogInformation($"User {userId} connected to LiveStreamHub");
        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var userId = Context.User?.FindFirst("sub")?.Value ?? Context.User?.FindFirst("id")?.Value;
        _logger.LogInformation($"User {userId} disconnected from LiveStreamHub");
        await base.OnDisconnectedAsync(exception);
    }
}