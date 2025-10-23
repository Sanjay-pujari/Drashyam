using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Drashyam.API.Services;
using Drashyam.API.DTOs;

namespace Drashyam.API.Hubs;

[Authorize]
public class NotificationHub : Hub
{
    private readonly INotificationService _notificationService;
    private readonly ILogger<NotificationHub> _logger;

    public NotificationHub(INotificationService notificationService, ILogger<NotificationHub> logger)
    {
        _notificationService = notificationService;
        _logger = logger;
    }

    public async Task JoinNotificationGroup(string groupName)
    {
        try
        {
            var userId = Context.User?.FindFirst("sub")?.Value ?? Context.User?.FindFirst("id")?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                await Clients.Caller.SendAsync("Error", "User not authenticated");
                return;
            }

            // Add user to notification group
            await Groups.AddToGroupAsync(Context.ConnectionId, groupName);
            
            _logger.LogInformation($"User {userId} joined notification group {groupName}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error joining notification group {groupName}");
            await Clients.Caller.SendAsync("Error", "Failed to join notification group");
        }
    }

    public async Task LeaveNotificationGroup(string groupName)
    {
        try
        {
            var userId = Context.User?.FindFirst("sub")?.Value ?? Context.User?.FindFirst("id")?.Value;
            
            // Remove user from notification group
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, groupName);
            
            _logger.LogInformation($"User {userId} left notification group {groupName}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error leaving notification group {groupName}");
        }
    }

    public async Task SubscribeToUser(string targetUserId)
    {
        try
        {
            var userId = Context.User?.FindFirst("sub")?.Value ?? Context.User?.FindFirst("id")?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                await Clients.Caller.SendAsync("Error", "User not authenticated");
                return;
            }

            // Add user to target user's notification group
            await Groups.AddToGroupAsync(Context.ConnectionId, $"user_{targetUserId}");
            
            _logger.LogInformation($"User {userId} subscribed to notifications for user {targetUserId}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error subscribing to user {targetUserId}");
            await Clients.Caller.SendAsync("Error", "Failed to subscribe to user");
        }
    }

    public async Task UnsubscribeFromUser(string targetUserId)
    {
        try
        {
            var userId = Context.User?.FindFirst("sub")?.Value ?? Context.User?.FindFirst("id")?.Value;
            
            // Remove user from target user's notification group
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"user_{targetUserId}");
            
            _logger.LogInformation($"User {userId} unsubscribed from notifications for user {targetUserId}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error unsubscribing from user {targetUserId}");
        }
    }

    public async Task SubscribeToStream(int streamId)
    {
        try
        {
            var userId = Context.User?.FindFirst("sub")?.Value ?? Context.User?.FindFirst("id")?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                await Clients.Caller.SendAsync("Error", "User not authenticated");
                return;
            }

            // Add user to stream notification group
            await Groups.AddToGroupAsync(Context.ConnectionId, $"stream_{streamId}");
            
            _logger.LogInformation($"User {userId} subscribed to notifications for stream {streamId}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error subscribing to stream {streamId}");
            await Clients.Caller.SendAsync("Error", "Failed to subscribe to stream");
        }
    }

    public async Task UnsubscribeFromStream(int streamId)
    {
        try
        {
            var userId = Context.User?.FindFirst("sub")?.Value ?? Context.User?.FindFirst("id")?.Value;
            
            // Remove user from stream notification group
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"stream_{streamId}");
            
            _logger.LogInformation($"User {userId} unsubscribed from notifications for stream {streamId}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error unsubscribing from stream {streamId}");
        }
    }

    public async Task MarkNotificationAsRead(int notificationId)
    {
        try
        {
            var userId = Context.User?.FindFirst("sub")?.Value ?? Context.User?.FindFirst("id")?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                await Clients.Caller.SendAsync("Error", "User not authenticated");
                return;
            }

            // Mark notification as read in database
            await _notificationService.MarkAsReadAsync(notificationId, userId);

            _logger.LogInformation($"Notification {notificationId} marked as read by user {userId}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error marking notification {notificationId} as read");
            await Clients.Caller.SendAsync("Error", "Failed to mark notification as read");
        }
    }

    public async Task MarkAllNotificationsAsRead()
    {
        try
        {
            var userId = Context.User?.FindFirst("sub")?.Value ?? Context.User?.FindFirst("id")?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                await Clients.Caller.SendAsync("Error", "User not authenticated");
                return;
            }

            // Mark all notifications as read in database
            await _notificationService.MarkAllAsReadAsync(userId);

            _logger.LogInformation($"All notifications marked as read by user {userId}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error marking all notifications as read");
            await Clients.Caller.SendAsync("Error", "Failed to mark all notifications as read");
        }
    }

    public async Task GetUnreadCount()
    {
        try
        {
            var userId = Context.User?.FindFirst("sub")?.Value ?? Context.User?.FindFirst("id")?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                await Clients.Caller.SendAsync("Error", "User not authenticated");
                return;
            }

            // Get unread notification count
            var unreadCount = await _notificationService.GetUnreadCountAsync(userId);

            // Send unread count to user
            await Clients.Caller.SendAsync("UnreadCount", new
            {
                Count = unreadCount,
                Timestamp = DateTime.UtcNow
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting unread count");
            await Clients.Caller.SendAsync("Error", "Failed to get unread count");
        }
    }

    public override async Task OnConnectedAsync()
    {
        var userId = Context.User?.FindFirst("sub")?.Value ?? Context.User?.FindFirst("id")?.Value;
        _logger.LogInformation($"User {userId} connected to NotificationHub");
        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var userId = Context.User?.FindFirst("sub")?.Value ?? Context.User?.FindFirst("id")?.Value;
        _logger.LogInformation($"User {userId} disconnected from NotificationHub");
        await base.OnDisconnectedAsync(exception);
    }
}