using AutoMapper;
using Drashyam.API.Data;
using Drashyam.API.DTOs;
using Drashyam.API.Models;
using Microsoft.EntityFrameworkCore;

namespace Drashyam.API.Services;

public class PrivacyService : IPrivacyService
{
    private readonly DrashyamDbContext _context;
    private readonly IMapper _mapper;
    private readonly ILogger<PrivacyService> _logger;

    public PrivacyService(DrashyamDbContext context, IMapper mapper, ILogger<PrivacyService> logger)
    {
        _context = context;
        _mapper = mapper;
        _logger = logger;
    }

    #region Video Privacy

    public async Task<bool> CanUserAccessVideoAsync(int videoId, string userId)
    {
        try
        {
            var video = await _context.Videos
                .Include(v => v.User)
                .FirstOrDefaultAsync(v => v.Id == videoId);

            if (video == null) return false;

            // Owner can always access their own videos
            if (video.UserId == userId) return true;

            // Check video visibility
            return video.Visibility switch
            {
                Models.VideoVisibility.Public => true,
                Models.VideoVisibility.Private => false,
                Models.VideoVisibility.Unlisted => await CanUserViewUnlistedVideoAsync(videoId, userId),
                _ => false
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking video access for video {VideoId} and user {UserId}", videoId, userId);
            return false;
        }
    }

    public async Task<bool> CanUserViewVideoAsync(int videoId, string userId)
    {
        return await CanUserAccessVideoAsync(videoId, userId);
    }

    public async Task<bool> IsVideoPublicAsync(int videoId)
    {
        var video = await _context.Videos
            .FirstOrDefaultAsync(v => v.Id == videoId);

        return video?.Visibility == Models.VideoVisibility.Public;
    }

    private async Task<bool> CanUserViewUnlistedVideoAsync(int videoId, string userId)
    {
        // For unlisted videos, only the owner can view them
        // In a real implementation, you might have a separate table for shared unlisted videos
        var video = await _context.Videos
            .FirstOrDefaultAsync(v => v.Id == videoId);

        return video?.UserId == userId;
    }

    #endregion

    #region User Profile Privacy

    public async Task<bool> CanViewUserProfileAsync(string profileUserId, string requestingUserId)
    {
        try
        {
            // Users can always view their own profile
            if (profileUserId == requestingUserId) return true;

            var userSettings = await GetUserPrivacySettingsAsync(profileUserId);
            return userSettings.ProfilePublic;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking profile access for user {ProfileUserId} by {RequestingUserId}", profileUserId, requestingUserId);
            return false;
        }
    }

    public async Task<bool> CanViewUserEmailAsync(string profileUserId, string requestingUserId)
    {
        try
        {
            // Users can always view their own email
            if (profileUserId == requestingUserId) return true;

            var userSettings = await GetUserPrivacySettingsAsync(profileUserId);
            return userSettings.ShowEmail;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking email visibility for user {ProfileUserId} by {RequestingUserId}", profileUserId, requestingUserId);
            return false;
        }
    }

    public async Task<bool> CanViewUserVideosAsync(string profileUserId, string requestingUserId)
    {
        try
        {
            // Users can always view their own videos
            if (profileUserId == requestingUserId) return true;

            var userSettings = await GetUserPrivacySettingsAsync(profileUserId);
            return userSettings.ProfilePublic;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking video visibility for user {ProfileUserId} by {RequestingUserId}", profileUserId, requestingUserId);
            return false;
        }
    }

    #endregion

    #region Channel Privacy

    public async Task<bool> CanUserAccessChannelAsync(int channelId, string userId)
    {
        try
        {
            var channel = await _context.Channels
                .FirstOrDefaultAsync(c => c.Id == channelId);

            if (channel == null) return false;

            // Channel owner can always access their channel
            if (channel.UserId == userId) return true;

            // For now, all channels are public
            // In the future, you might add channel privacy settings
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking channel access for channel {ChannelId} and user {UserId}", channelId, userId);
            return false;
        }
    }

    public async Task<bool> CanUserViewChannelAsync(int channelId, string userId)
    {
        return await CanUserAccessChannelAsync(channelId, userId);
    }

    public async Task<bool> IsChannelPublicAsync(int channelId)
    {
        var channel = await _context.Channels
            .FirstOrDefaultAsync(c => c.Id == channelId);

        return channel != null;
    }

    #endregion

    #region Notification Privacy

    public async Task<bool> CanSendNotificationToUserAsync(string userId, NotificationType notificationType)
    {
        try
        {
            var userSettings = await _context.UserSettings
                .FirstOrDefaultAsync(us => us.UserId == userId);

            if (userSettings == null)
            {
                // Create default settings if they don't exist
                userSettings = new UserSettings
                {
                    UserId = userId,
                    ProfilePublic = true,
                    ShowEmail = false,
                    AllowDataSharing = true,
                    EmailNotifications = true,
                    PushNotifications = true,
                    NewVideoNotifications = true,
                    CommentNotifications = true
                };

                _context.UserSettings.Add(userSettings);
                await _context.SaveChangesAsync();
            }

            return notificationType switch
            {
                NotificationType.Email => userSettings.EmailNotifications,
                NotificationType.Push => userSettings.PushNotifications,
                NotificationType.Video => userSettings.NewVideoNotifications,
                NotificationType.Comment => userSettings.CommentNotifications,
                NotificationType.Subscription => userSettings.EmailNotifications,
                NotificationType.System => true, // System notifications are always allowed
                _ => false
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking notification permission for user {UserId} and type {NotificationType}", userId, notificationType);
            return false;
        }
    }

    public async Task<bool> ShouldReceiveEmailNotificationAsync(string userId)
    {
        var userSettings = await _context.UserSettings
            .FirstOrDefaultAsync(us => us.UserId == userId);
        return userSettings?.EmailNotifications ?? true;
    }

    public async Task<bool> ShouldReceivePushNotificationAsync(string userId)
    {
        var userSettings = await _context.UserSettings
            .FirstOrDefaultAsync(us => us.UserId == userId);
        return userSettings?.PushNotifications ?? true;
    }

    public async Task<bool> ShouldReceiveVideoNotificationAsync(string userId)
    {
        var userSettings = await _context.UserSettings
            .FirstOrDefaultAsync(us => us.UserId == userId);
        return userSettings?.NewVideoNotifications ?? true;
    }

    public async Task<bool> ShouldReceiveCommentNotificationAsync(string userId)
    {
        var userSettings = await _context.UserSettings
            .FirstOrDefaultAsync(us => us.UserId == userId);
        return userSettings?.CommentNotifications ?? true;
    }

    #endregion

    #region Data Sharing Privacy

    public async Task<bool> CanShareUserDataAsync(string userId)
    {
        var userSettings = await GetUserPrivacySettingsAsync(userId);
        return userSettings.AllowDataSharing;
    }

    public async Task<bool> CanUseUserDataForAnalyticsAsync(string userId)
    {
        var userSettings = await GetUserPrivacySettingsAsync(userId);
        return userSettings.AllowDataSharing;
    }

    public async Task<bool> CanUseUserDataForRecommendationsAsync(string userId)
    {
        var userSettings = await GetUserPrivacySettingsAsync(userId);
        return userSettings.AllowDataSharing;
    }

    #endregion

    #region General Privacy Checks

    public async Task<bool> IsUserProfilePublicAsync(string userId)
    {
        var userSettings = await GetUserPrivacySettingsAsync(userId);
        return userSettings.ProfilePublic;
    }

    public async Task<PrivacySettingsDto> GetUserPrivacySettingsAsync(string userId)
    {
        try
        {
            var userSettings = await _context.UserSettings
                .FirstOrDefaultAsync(us => us.UserId == userId);

            if (userSettings == null)
            {
                // Create default settings if they don't exist
                userSettings = new UserSettings
                {
                    UserId = userId,
                    ProfilePublic = true,
                    ShowEmail = false,
                    AllowDataSharing = true,
                    EmailNotifications = true,
                    PushNotifications = true,
                    NewVideoNotifications = true,
                    CommentNotifications = true
                };

                _context.UserSettings.Add(userSettings);
                await _context.SaveChangesAsync();
            }

            return new PrivacySettingsDto
            {
                ProfilePublic = userSettings.ProfilePublic,
                ShowEmail = userSettings.ShowEmail,
                AllowDataSharing = userSettings.AllowDataSharing
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting privacy settings for user {UserId}", userId);
            
            // Return default settings on error
            return new PrivacySettingsDto
            {
                ProfilePublic = true,
                ShowEmail = false,
                AllowDataSharing = true
            };
        }
    }

    #endregion
}
