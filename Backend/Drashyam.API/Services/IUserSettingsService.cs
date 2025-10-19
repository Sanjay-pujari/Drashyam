using Drashyam.API.DTOs;
using Drashyam.API.Models;

namespace Drashyam.API.Services;

public interface IUserSettingsService
{
    Task<PrivacySettingsDto> GetPrivacySettingsAsync(string userId);
    Task<PrivacySettingsDto> UpdatePrivacySettingsAsync(string userId, PrivacySettingsDto settings);
    Task<NotificationSettingsDto> GetNotificationSettingsAsync(string userId);
    Task<NotificationSettingsDto> UpdateNotificationSettingsAsync(string userId, NotificationSettingsDto settings);
    Task<UserSettings> GetOrCreateUserSettingsAsync(string userId);
}
