using Drashyam.API.DTOs;

namespace Drashyam.API.Services;

public interface INotificationService
{
    Task<PagedResult<VideoNotificationDto>> GetUserNotificationsAsync(string userId, int page = 1, int pageSize = 20);
    Task<int> GetUnreadNotificationCountAsync(string userId);
    Task MarkNotificationAsReadAsync(int notificationId, string userId);
    Task MarkAllNotificationsAsReadAsync(string userId);
    Task CreateVideoNotificationAsync(int videoId, int channelId);
    Task DeleteNotificationAsync(int notificationId, string userId);
}