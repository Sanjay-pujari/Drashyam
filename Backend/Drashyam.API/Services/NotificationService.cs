using AutoMapper;
using Drashyam.API.Data;
using Drashyam.API.DTOs;
using Drashyam.API.Models;
using Microsoft.EntityFrameworkCore;

namespace Drashyam.API.Services;

public class NotificationService : INotificationService
{
    private readonly DrashyamDbContext _context;
    private readonly IMapper _mapper;

    public NotificationService(DrashyamDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<PagedResult<VideoNotificationDto>> GetUserNotificationsAsync(string userId, int page = 1, int pageSize = 20)
    {
        var query = _context.Notifications
            .Where(n => n.UserId == userId)
            .Include(n => n.RelatedEntityId)
            .OrderByDescending(n => n.CreatedAt);

        var totalCount = await query.CountAsync();
        var notifications = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        var videoNotifications = new List<VideoNotificationDto>();

        foreach (var notification in notifications)
        {
            if (notification.RelatedEntityType == "Video" && int.TryParse(notification.RelatedEntityId, out int videoId))
            {
                var video = await _context.Videos
                    .Include(v => v.Channel)
                    .Include(v => v.User)
                    .FirstOrDefaultAsync(v => v.Id == videoId);

                if (video != null)
                {
                    videoNotifications.Add(new VideoNotificationDto
                    {
                        Id = notification.Id,
                        VideoId = video.Id,
                        VideoTitle = video.Title,
                        VideoThumbnailUrl = video.ThumbnailUrl ?? string.Empty,
                        VideoDuration = video.Duration,
                        VideoViewCount = video.ViewCount,
                        ChannelId = video.ChannelId ?? 0,
                        ChannelName = video.Channel?.Name ?? $"{video.User.FirstName} {video.User.LastName}",
                        ChannelProfilePictureUrl = video.Channel?.ProfilePictureUrl ?? video.User.ProfilePictureUrl ?? string.Empty,
                        CreatedAt = notification.CreatedAt,
                        IsRead = notification.IsRead,
                        ReadAt = notification.ReadAt,
                        NotificationType = "new_video",
                        Message = $"New video: {video.Title}"
                    });
                }
            }
        }

        return new PagedResult<VideoNotificationDto>
        {
            Items = videoNotifications,
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        };
    }

    public async Task<int> GetUnreadNotificationCountAsync(string userId)
    {
        return await _context.Notifications
            .CountAsync(n => n.UserId == userId && !n.IsRead);
    }

    public async Task MarkNotificationAsReadAsync(int notificationId, string userId)
    {
        var notification = await _context.Notifications
            .FirstOrDefaultAsync(n => n.Id == notificationId && n.UserId == userId);

        if (notification != null)
        {
            notification.IsRead = true;
            notification.ReadAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
        }
    }

    public async Task MarkAllNotificationsAsReadAsync(string userId)
    {
        var notifications = await _context.Notifications
            .Where(n => n.UserId == userId && !n.IsRead)
            .ToListAsync();

        foreach (var notification in notifications)
        {
            notification.IsRead = true;
            notification.ReadAt = DateTime.UtcNow;
        }

        await _context.SaveChangesAsync();
    }

    public async Task CreateVideoNotificationAsync(int videoId, int channelId)
    {
        // Get all subscribers who have notifications enabled for this channel
        var subscribers = await _context.ChannelSubscriptions
            .Where(cs => cs.ChannelId == channelId && cs.IsActive && cs.NotificationsEnabled)
            .Select(cs => cs.UserId)
            .ToListAsync();

        var video = await _context.Videos
            .Include(v => v.Channel)
            .FirstOrDefaultAsync(v => v.Id == videoId);

        if (video == null) return;

        var notifications = subscribers.Select(userId => new Notification
        {
            UserId = userId,
            Title = "New Video",
            Message = $"New video uploaded: {video.Title}",
            Type = NotificationType.VideoUploaded,
            IsRead = false,
            CreatedAt = DateTime.UtcNow,
            RelatedEntityId = videoId.ToString(),
            RelatedEntityType = "Video",
            ActionUrl = $"/videos/{videoId}"
        }).ToList();

        _context.Notifications.AddRange(notifications);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteNotificationAsync(int notificationId, string userId)
    {
        var notification = await _context.Notifications
            .FirstOrDefaultAsync(n => n.Id == notificationId && n.UserId == userId);

        if (notification != null)
        {
            _context.Notifications.Remove(notification);
            await _context.SaveChangesAsync();
        }
    }
}