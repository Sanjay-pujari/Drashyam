using Drashyam.API.DTOs;

namespace Drashyam.API.Services;

public interface INotificationService
{
    Task SendInviteNotificationAsync(string userId, string message, NotificationType type, object? data = null);
    Task SendReferralNotificationAsync(string userId, string message, NotificationType type, object? data = null);
    Task SendInviteAcceptedNotificationAsync(string inviterId, string inviteeName, string inviteeEmail);
    Task SendReferralCompletedNotificationAsync(string referrerId, string referredUserName);
    Task SendRewardEarnedNotificationAsync(string userId, decimal amount, string rewardType);
    Task SendRewardClaimedNotificationAsync(string userId, decimal amount, string rewardType);
    Task SendInviteExpiredNotificationAsync(string userId, string inviteeEmail);
    Task SendReferralCodeUsedNotificationAsync(string userId, string code, string usedBy);
    Task SendBulkInviteNotificationAsync(string userId, int successCount, int failureCount);
    Task SendReferralStatsUpdateAsync(string userId, ReferralStatsDto stats);
    Task SendInviteStatsUpdateAsync(string userId, InviteStatsDto stats);
}

public enum NotificationType
{
    Info,
    Success,
    Warning,
    Error,
    Invite,
    Referral,
    Reward,
    Stats
}
