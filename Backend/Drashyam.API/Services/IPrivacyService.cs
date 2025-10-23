using Drashyam.API.DTOs;

namespace Drashyam.API.Services;

public interface IPrivacyService
{
    // Video Privacy
    Task<bool> CanUserAccessVideoAsync(int videoId, string userId);
    Task<bool> CanUserViewVideoAsync(int videoId, string userId);
    Task<bool> IsVideoPublicAsync(int videoId);
    
    // User Profile Privacy
    Task<bool> CanViewUserProfileAsync(string profileUserId, string requestingUserId);
    Task<bool> CanViewUserEmailAsync(string profileUserId, string requestingUserId);
    Task<bool> CanViewUserVideosAsync(string profileUserId, string requestingUserId);
    
    // Channel Privacy
    Task<bool> CanUserAccessChannelAsync(int channelId, string userId);
    Task<bool> CanUserViewChannelAsync(int channelId, string userId);
    Task<bool> IsChannelPublicAsync(int channelId);
    
    // Notification Privacy
    Task<bool> CanSendNotificationToUserAsync(string userId, NotificationType notificationType);
    Task<bool> ShouldReceiveEmailNotificationAsync(string userId);
    Task<bool> ShouldReceivePushNotificationAsync(string userId);
    Task<bool> ShouldReceiveVideoNotificationAsync(string userId);
    Task<bool> ShouldReceiveCommentNotificationAsync(string userId);
    
    // Data Sharing Privacy
    Task<bool> CanShareUserDataAsync(string userId);
    Task<bool> CanUseUserDataForAnalyticsAsync(string userId);
    Task<bool> CanUseUserDataForRecommendationsAsync(string userId);
    
    // General Privacy Checks
    Task<bool> IsUserProfilePublicAsync(string userId);
    Task<PrivacySettingsDto> GetUserPrivacySettingsAsync(string userId);
}

