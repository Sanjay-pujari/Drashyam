using Microsoft.AspNetCore.SignalR;
using Drashyam.API.Hubs;
using Drashyam.API.DTOs;

namespace Drashyam.API.Services;

public class NotificationService : INotificationService
{
    private readonly IHubContext<NotificationHub> _hubContext;
    private readonly ILogger<NotificationService> _logger;

    public NotificationService(
        IHubContext<NotificationHub> hubContext,
        ILogger<NotificationService> logger)
    {
        _hubContext = hubContext;
        _logger = logger;
    }

    public async Task SendInviteNotificationAsync(string userId, string message, NotificationType type, object? data = null)
    {
        try
        {
            var notification = new
            {
                Type = type.ToString(),
                Message = message,
                Timestamp = DateTime.UtcNow,
                Data = data
            };

            await _hubContext.Clients.Group($"user_{userId}").SendAsync("InviteNotification", notification);
            _logger.LogInformation("Sent invite notification to user {UserId}: {Message}", userId, message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending invite notification to user {UserId}", userId);
        }
    }

    public async Task SendReferralNotificationAsync(string userId, string message, NotificationType type, object? data = null)
    {
        try
        {
            var notification = new
            {
                Type = type.ToString(),
                Message = message,
                Timestamp = DateTime.UtcNow,
                Data = data
            };

            await _hubContext.Clients.Group($"user_{userId}").SendAsync("ReferralNotification", notification);
            _logger.LogInformation("Sent referral notification to user {UserId}: {Message}", userId, message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending referral notification to user {UserId}", userId);
        }
    }

    public async Task SendInviteAcceptedNotificationAsync(string inviterId, string inviteeName, string inviteeEmail)
    {
        var message = $"🎉 {inviteeName} ({inviteeEmail}) accepted your invitation!";
        var data = new { InviteeName = inviteeName, InviteeEmail = inviteeEmail };
        
        await SendInviteNotificationAsync(inviterId, message, NotificationType.Success, data);
    }

    public async Task SendReferralCompletedNotificationAsync(string referrerId, string referredUserName)
    {
        var message = $"🎉 {referredUserName} completed your referral! You've earned a reward.";
        var data = new { ReferredUserName = referredUserName };
        
        await SendReferralNotificationAsync(referrerId, message, NotificationType.Success, data);
    }

    public async Task SendRewardEarnedNotificationAsync(string userId, decimal amount, string rewardType)
    {
        var message = $"💰 You earned {amount:C} in {rewardType}! Check your rewards to claim it.";
        var data = new { Amount = amount, RewardType = rewardType };
        
        await SendReferralNotificationAsync(userId, message, NotificationType.Reward, data);
    }

    public async Task SendRewardClaimedNotificationAsync(string userId, decimal amount, string rewardType)
    {
        var message = $"✅ You successfully claimed {amount:C} in {rewardType}!";
        var data = new { Amount = amount, RewardType = rewardType };
        
        await SendReferralNotificationAsync(userId, message, NotificationType.Success, data);
    }

    public async Task SendInviteExpiredNotificationAsync(string userId, string inviteeEmail)
    {
        var message = $"⏰ Your invitation to {inviteeEmail} has expired.";
        var data = new { InviteeEmail = inviteeEmail };
        
        await SendInviteNotificationAsync(userId, message, NotificationType.Warning, data);
    }

    public async Task SendReferralCodeUsedNotificationAsync(string userId, string code, string usedBy)
    {
        var message = $"🔗 Your referral code '{code}' was used by {usedBy}!";
        var data = new { Code = code, UsedBy = usedBy };
        
        await SendReferralNotificationAsync(userId, message, NotificationType.Referral, data);
    }

    public async Task SendBulkInviteNotificationAsync(string userId, int successCount, int failureCount)
    {
        var message = $"📧 Bulk invite completed: {successCount} sent successfully, {failureCount} failed.";
        var data = new { SuccessCount = successCount, FailureCount = failureCount };
        
        await SendInviteNotificationAsync(userId, message, 
            failureCount > 0 ? NotificationType.Warning : NotificationType.Success, data);
    }

    public async Task SendReferralStatsUpdateAsync(string userId, ReferralStatsDto stats)
    {
        var message = $"📊 Your referral stats updated: {stats.CompletedReferrals} completed, {stats.TotalRewards:C} earned";
        var data = new { Stats = stats };
        
        await SendReferralNotificationAsync(userId, message, NotificationType.Stats, data);
    }

    public async Task SendInviteStatsUpdateAsync(string userId, InviteStatsDto stats)
    {
        var message = $"📊 Your invite stats updated: {stats.AcceptedInvites} accepted, {stats.ConversionRate:F1}% conversion rate";
        var data = new { Stats = stats };
        
        await SendInviteNotificationAsync(userId, message, NotificationType.Stats, data);
    }
}
