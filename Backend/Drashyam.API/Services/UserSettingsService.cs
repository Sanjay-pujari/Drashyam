using AutoMapper;
using Drashyam.API.Data;
using Drashyam.API.DTOs;
using Drashyam.API.Models;
using Microsoft.EntityFrameworkCore;

namespace Drashyam.API.Services;

public class UserSettingsService : IUserSettingsService
{
    private readonly DrashyamDbContext _context;
    private readonly IMapper _mapper;

    public UserSettingsService(DrashyamDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<PrivacySettingsDto> GetPrivacySettingsAsync(string userId)
    {
        Console.WriteLine($"UserSettingsService - Getting privacy settings for user: {userId}");
        
        var userSettings = await GetOrCreateUserSettingsAsync(userId);
        Console.WriteLine($"UserSettingsService - Retrieved settings: ProfilePublic={userSettings.ProfilePublic}, ShowEmail={userSettings.ShowEmail}, AllowDataSharing={userSettings.AllowDataSharing}");
        
        var result = new PrivacySettingsDto
        {
            ProfilePublic = userSettings.ProfilePublic,
            ShowEmail = userSettings.ShowEmail,
            AllowDataSharing = userSettings.AllowDataSharing
        };
        
        Console.WriteLine($"UserSettingsService - Returning privacy settings: ProfilePublic={result.ProfilePublic}, ShowEmail={result.ShowEmail}, AllowDataSharing={result.AllowDataSharing}");
        return result;
    }

    public async Task<PrivacySettingsDto> UpdatePrivacySettingsAsync(string userId, PrivacySettingsDto settings)
    {
        Console.WriteLine($"UserSettingsService - Updating privacy settings for user: {userId}");
        Console.WriteLine($"UserSettingsService - Received settings: ProfilePublic={settings.ProfilePublic}, ShowEmail={settings.ShowEmail}, AllowDataSharing={settings.AllowDataSharing}");
        
        var userSettings = await GetOrCreateUserSettingsAsync(userId);
        Console.WriteLine($"UserSettingsService - Found existing settings: ProfilePublic={userSettings.ProfilePublic}, ShowEmail={userSettings.ShowEmail}, AllowDataSharing={userSettings.AllowDataSharing}");
        
        userSettings.ProfilePublic = settings.ProfilePublic;
        userSettings.ShowEmail = settings.ShowEmail;
        userSettings.AllowDataSharing = settings.AllowDataSharing;
        userSettings.UpdatedAt = DateTime.UtcNow;

        Console.WriteLine($"UserSettingsService - Updated settings: ProfilePublic={userSettings.ProfilePublic}, ShowEmail={userSettings.ShowEmail}, AllowDataSharing={userSettings.AllowDataSharing}");
        
        var changesSaved = await _context.SaveChangesAsync();
        Console.WriteLine($"UserSettingsService - SaveChangesAsync returned: {changesSaved}");
        
        return settings;
    }

    public async Task<NotificationSettingsDto> GetNotificationSettingsAsync(string userId)
    {
        Console.WriteLine($"UserSettingsService - Getting notification settings for user: {userId}");
        
        var userSettings = await GetOrCreateUserSettingsAsync(userId);
        Console.WriteLine($"UserSettingsService - Retrieved notification settings: EmailNotifications={userSettings.EmailNotifications}, PushNotifications={userSettings.PushNotifications}, NewVideoNotifications={userSettings.NewVideoNotifications}, CommentNotifications={userSettings.CommentNotifications}");
        
        var result = new NotificationSettingsDto
        {
            EmailNotifications = userSettings.EmailNotifications,
            PushNotifications = userSettings.PushNotifications,
            NewVideoNotifications = userSettings.NewVideoNotifications,
            CommentNotifications = userSettings.CommentNotifications
        };
        
        Console.WriteLine($"UserSettingsService - Returning notification settings: EmailNotifications={result.EmailNotifications}, PushNotifications={result.PushNotifications}, NewVideoNotifications={result.NewVideoNotifications}, CommentNotifications={result.CommentNotifications}");
        return result;
    }

    public async Task<NotificationSettingsDto> UpdateNotificationSettingsAsync(string userId, NotificationSettingsDto settings)
    {
        var userSettings = await GetOrCreateUserSettingsAsync(userId);
        
        userSettings.EmailNotifications = settings.EmailNotifications;
        userSettings.PushNotifications = settings.PushNotifications;
        userSettings.NewVideoNotifications = settings.NewVideoNotifications;
        userSettings.CommentNotifications = settings.CommentNotifications;
        userSettings.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return settings;
    }

    public async Task<UserSettings> GetOrCreateUserSettingsAsync(string userId)
    {
        Console.WriteLine($"UserSettingsService - Getting or creating settings for user: {userId}");
        
        var userSettings = await _context.UserSettings
            .FirstOrDefaultAsync(us => us.UserId == userId);

        if (userSettings == null)
        {
            Console.WriteLine($"UserSettingsService - No existing settings found, creating new ones for user: {userId}");
            userSettings = new UserSettings
            {
                UserId = userId,
                ProfilePublic = true,
                ShowEmail = false,
                AllowDataSharing = true,
                EmailNotifications = true,
                PushNotifications = true,
                NewVideoNotifications = true,
                CommentNotifications = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.UserSettings.Add(userSettings);
            var createResult = await _context.SaveChangesAsync();
            Console.WriteLine($"UserSettingsService - Created new settings, SaveChangesAsync returned: {createResult}");
        }
        else
        {
            Console.WriteLine($"UserSettingsService - Found existing settings for user: {userId}");
        }

        return userSettings;
    }
}
