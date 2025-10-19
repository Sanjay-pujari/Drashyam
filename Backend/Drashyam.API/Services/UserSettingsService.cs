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
        var userSettings = await GetOrCreateUserSettingsAsync(userId);
        
        return new PrivacySettingsDto
        {
            ProfilePublic = userSettings.ProfilePublic,
            ShowEmail = userSettings.ShowEmail,
            AllowDataSharing = userSettings.AllowDataSharing
        };
    }

    public async Task<PrivacySettingsDto> UpdatePrivacySettingsAsync(string userId, PrivacySettingsDto settings)
    {
        var userSettings = await GetOrCreateUserSettingsAsync(userId);
        
        userSettings.ProfilePublic = settings.ProfilePublic;
        userSettings.ShowEmail = settings.ShowEmail;
        userSettings.AllowDataSharing = settings.AllowDataSharing;
        userSettings.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return settings;
    }

    public async Task<NotificationSettingsDto> GetNotificationSettingsAsync(string userId)
    {
        var userSettings = await GetOrCreateUserSettingsAsync(userId);
        
        return new NotificationSettingsDto
        {
            EmailNotifications = userSettings.EmailNotifications,
            PushNotifications = userSettings.PushNotifications,
            NewVideoNotifications = userSettings.NewVideoNotifications,
            CommentNotifications = userSettings.CommentNotifications
        };
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
        var userSettings = await _context.UserSettings
            .FirstOrDefaultAsync(us => us.UserId == userId);

        if (userSettings == null)
        {
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
            await _context.SaveChangesAsync();
        }

        return userSettings;
    }
}
